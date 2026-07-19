[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [string[]] $TargetNames = @(
        'Google Chrome',
        'Microsoft Edge',
        'Mozilla Firefox',
        'Playwright WebKit'),

    [string[]] $RouteIds = @(
        'AUTH-02',
        'AUTH-04',
        'STU-02',
        'STU-04',
        'STU-05',
        'STU-06',
        'STF-01',
        'STF-03',
        'STF-04'),

    [switch] $SkipBrowserInstall
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$project = Join-Path $PSScriptRoot 'StudentRegistration.AccessibilityTests.csproj'
$browserRunner = Join-Path $PSScriptRoot 'Run-BrowserMatrix.ps1'
$runId = 'SPEC-018-NFR-8-CRITICAL-{0}' -f (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$resultDirectory = Join-Path $repositoryRoot ".local/accessibility-evidence/$runId/critical-route-matrix"
New-Item -ItemType Directory -Force -Path $resultDirectory | Out-Null

$allRoutes = [ordered]@{
    'AUTH-02' = 'StudentLoginPageAccessibilityTests'
    'AUTH-04' = 'StaffLoginPageAccessibilityTests'
    'STU-02' = 'SubjectDiscoveryPageAccessibilityTests'
    'STU-04' = 'ScheduleBuilderPageAccessibilityTests'
    'STU-05' = 'RegistrationReviewPageAccessibilityTests'
    'STU-06' = 'RegistrationRecordsPageAccessibilityTests'
    'STF-01' = 'StaffDashboardPageAccessibilityTests'
    'STF-03' = 'StaffRosterPageAccessibilityTests'
    'STF-04' = 'StaffAvailabilityPageAccessibilityTests'
}
$unknownRouteIds = @($RouteIds | Where-Object { -not $allRoutes.Contains($_) })
if ($unknownRouteIds.Count -ne 0) {
    throw "Unknown critical route ID(s): $($unknownRouteIds -join ', ')."
}
$routes = [ordered]@{}
foreach ($routeId in $RouteIds) {
    $routes[$routeId] = $allRoutes[$routeId]
}

Push-Location $repositoryRoot
try {
    & dotnet build 'src/StudentRegistration.Client/StudentRegistration.Client.csproj' `
        --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'The client build failed.' }
    & dotnet build $project --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'The accessibility build failed.' }

    if (-not $SkipBrowserInstall) {
        $playwright = Get-ChildItem `
            -Path (Join-Path $PSScriptRoot "bin/$Configuration") `
            -Filter 'playwright.ps1' `
            -Recurse | Select-Object -First 1
        if ($null -eq $playwright) { throw 'The pinned Playwright installer is absent.' }
        & $playwright.FullName install firefox webkit
        if ($LASTEXITCODE -ne 0) { throw 'Pinned Firefox/WebKit installation failed.' }
    }

    $results = [System.Collections.Generic.List[object]]::new()
    foreach ($target in $TargetNames) {
        foreach ($entry in $routes.GetEnumerator()) {
            $started = (Get-Date).ToUniversalTime()
            $result = 'failed'
            $evidenceSummary = $null
            $failure = $null
            try {
                & $browserRunner `
                    -Configuration $Configuration `
                    -SkipBuild `
                    -SkipBrowserInstall `
                    -TargetNames $target `
                    -TestFilter "FullyQualifiedName~$($entry.Value)"
                $result = 'passed'
                $candidate = Get-ChildItem `
                    -Path (Join-Path $repositoryRoot '.local/accessibility-evidence') `
                    -Filter 'summary.json' `
                    -File `
                    -Recurse |
                    Where-Object {
                        $_.FullName -like '*browser-matrix*' -and
                        $_.LastWriteTimeUtc -ge $started
                    } |
                    Sort-Object LastWriteTimeUtc -Descending |
                    Select-Object -First 1
                if ($null -ne $candidate) {
                    $evidenceSummary = $candidate.FullName.Substring($repositoryRoot.Length + 1)
                }
            }
            catch {
                $failure = $_.Exception.Message
            }

            $finished = (Get-Date).ToUniversalTime()
            $results.Add([ordered]@{
                browser = $target
                routeId = $entry.Key
                testClass = $entry.Value
                result = $result
                startedUtc = $started.ToString('O')
                finishedUtc = $finished.ToString('O')
                durationSeconds = [math]::Round(($finished - $started).TotalSeconds, 3)
                evidenceSummary = $evidenceSummary
                failure = $failure
            })
        }
    }

    $summary = [ordered]@{
        schemaVersion = 'spec018-critical-route-browser-matrix/1.0.0'
        runId = $runId
        requirement = 'SPEC-018/NFR-8'
        targets = $TargetNames
        routes = @($routes.Keys)
        results = $results
        allPassed = (@($results).Count -eq ($TargetNames.Count * $routes.Count) -and
            @($results | Where-Object result -NE 'passed').Count -eq 0)
        evidenceBoundary = 'Automated Playwright/axe and keyboard-focus evidence; not manual NVDA sign-off.'
    }
    $summaryPath = Join-Path $resultDirectory 'summary.json'
    $summary | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath $summaryPath -Encoding utf8
    Write-Output "Critical-route matrix evidence: $summaryPath"
    if (-not $summary.allPassed) {
        throw 'One or more required critical route/browser combinations failed.'
    }
}
finally {
    Pop-Location
}
