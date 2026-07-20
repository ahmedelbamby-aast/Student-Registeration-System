[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$visualProject = Join-Path $PSScriptRoot 'StudentRegistration.VisualTests.csproj'
$baselineRoot = Join-Path $PSScriptRoot 'Baselines/Spec003'
$globalManifestPath = Join-Path $PSScriptRoot 'Baselines/baseline-manifest.json'
$browserMatrixPath = Join-Path $repositoryRoot 'tests/StudentRegistration.E2ETests/browser-matrix.json'

if (-not $baselineRoot.StartsWith($repositoryRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Baseline output escaped the repository root: $baselineRoot"
}

$routes = @(
    'STU-02', 'STU-03', 'STU-04', 'STU-05', 'STU-06', 'STU-07',
    'ADM-01', 'ADM-05', 'ADM-06', 'ADM-07', 'ADM-08', 'ADM-09',
    'STF-01', 'STF-02', 'STF-03', 'STF-04'
)
$filterClasses = @(
    'SubjectDiscoveryPageVisualTests', 'SubjectDetailsPageVisualTests',
    'ScheduleBuilderPageVisualTests', 'RegistrationReviewPageVisualTests',
    'RegistrationResultPageVisualTests', 'RegistrationHistoryPageVisualTests',
    'CatalogueAdministrationPageVisualTests', 'OfferingAdministrationPageVisualTests',
    'ResourceAdministrationPageVisualTests', 'StaffDashboardPageVisualTests',
    'StaffTimetablePageVisualTests', 'StaffRosterPageVisualTests',
    'StaffAvailabilityPageVisualTests', 'RequestedAdminRouteBaselineTests'
)
$filter = ($filterClasses | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'

dotnet build $visualProject --configuration $Configuration --no-restore --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw 'The visual test project did not build.' }

$priorBaselineApprover = $env:SPEC003_ROUTE_BASELINE_APPROVER
$priorBrowserConfiguration = $env:STUDENTREGISTRATION_BROWSER_CONFIGURATION
$env:SPEC003_ROUTE_BASELINE_APPROVER = 'Ahmed ELbamby'
$env:STUDENTREGISTRATION_BROWSER_CONFIGURATION = $Configuration
try {
    dotnet test $visualProject --configuration $Configuration --no-build --no-restore `
        --verbosity minimal --filter $filter
    if ($LASTEXITCODE -ne 0) {
        throw 'The deterministic requested-route baseline capture did not pass.'
    }
}
finally {
    if ($null -eq $priorBaselineApprover) {
        Remove-Item Env:SPEC003_ROUTE_BASELINE_APPROVER -ErrorAction SilentlyContinue
    }
    else {
        $env:SPEC003_ROUTE_BASELINE_APPROVER = $priorBaselineApprover
    }
    if ($null -eq $priorBrowserConfiguration) {
        Remove-Item Env:STUDENTREGISTRATION_BROWSER_CONFIGURATION -ErrorAction SilentlyContinue
    }
    else {
        $env:STUDENTREGISTRATION_BROWSER_CONFIGURATION = $priorBrowserConfiguration
    }
}

$matrix = Get-Content -Raw $browserMatrixPath | ConvertFrom-Json
$browserDefinitions = [ordered]@{
    chrome = $matrix.targets | Where-Object name -eq 'Google Chrome'
    edge = $matrix.targets | Where-Object name -eq 'Microsoft Edge'
    firefox = $matrix.targets | Where-Object name -eq 'Mozilla Firefox'
    webkit = $matrix.targets | Where-Object name -eq 'Playwright WebKit'
}
$browserDefinitions.firefox.label =
    'Playwright Firefox 151.0 visual capture; Mozilla Firefox 152.0.6 current-stable smoke'
$browserDefinitions.firefox.observedLocalVersion = '151.0 visual; 152.0.6 stable smoke'
$browserDefinitions.firefox.engine = 'Gecko 151 visual; Gecko 152.0.6 stable smoke'
$widths = @(375, 768, 1280, 1920)
$newGlobalRecords = @()

foreach ($routeId in $routes) {
    $routeDirectory = Join-Path $baselineRoot $routeId
    $targets = @()
    foreach ($browser in $browserDefinitions.Keys) {
        $definition = $browserDefinitions[$browser]
        foreach ($width in $widths) {
            $file = "$browser-$width-denied.png"
            $artifactPath = Join-Path $routeDirectory $file
            if (-not (Test-Path -LiteralPath $artifactPath -PathType Leaf)) {
                throw "Missing captured baseline: $artifactPath"
            }
            $hash = (Get-FileHash -LiteralPath $artifactPath -Algorithm SHA256).Hash.ToLowerInvariant()
            $targets += [ordered]@{
                browser = $browser
                browserName = $definition.label
                browserBuild = $definition.observedLocalVersion
                engine = $definition.engine
                viewport = $width
                file = $file
                sha256 = $hash
                artifactSha256 = $hash
            }
        }
    }

    $targetManifest = [ordered]@{
        version = "$($routeId.ToLowerInvariant())-visual-baselines/1.0.0"
        routeId = $routeId
        state = 'denied'
        fixtureVersion = 'frontend-fixture/1.0'
        tokenVersion = 'design-token-contract/1.0'
        status = 'approved'
        automaticReplacementAllowed = $false
        approvalAuthority = 'Ahmed ELbamby'
        approvedBy = 'Ahmed ELbamby'
        approvedOn = '2026-07-19'
        osImage = $matrix.executionOsImage
        targets = $targets
    }
    $targetManifest | ConvertTo-Json -Depth 8 |
        Set-Content -LiteralPath (Join-Path $routeDirectory 'baseline-targets.json') -Encoding utf8

    $combinedText = ($targets | ForEach-Object { "$($_.file):$($_.sha256)" }) -join "`n"
    $combinedHash = [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($combinedText))
    ).ToLowerInvariant()
    $newGlobalRecords += [ordered]@{
        routeId = $routeId
        state = 'denied'
        browserName = 'Google Chrome Stable; Microsoft Edge Stable; Playwright Firefox 151.0 visual capture + Mozilla Firefox 152.0.6 current-stable smoke; Playwright WebKit (not Safari)'
        browserBuild = (($browserDefinitions.Values | ForEach-Object observedLocalVersion) -join '; ')
        engine = (($browserDefinitions.Values | ForEach-Object engine) -join '; ')
        osImage = $matrix.executionOsImage
        viewport = $widths
        tokenVersion = 'design-token-contract/1.0'
        fixtureVersion = 'frontend-fixture/1.0'
        artifactSha256 = $combinedHash
        approvedBy = 'Ahmed ELbamby'
        approvedOn = '2026-07-19'
        targetManifest = "Spec003/$routeId/baseline-targets.json"
        targetCount = 16
    }
}

$global = Get-Content -Raw $globalManifestPath | ConvertFrom-Json
$preserved = @($global.baselines | Where-Object { $_.routeId -notin $routes })
$global.baselines = @($preserved + $newGlobalRecords)
$global.status = 'partially-approved-routes'
$global | ConvertTo-Json -Depth 10 |
    Set-Content -LiteralPath $globalManifestPath -Encoding utf8

Write-Output "Approved $($routes.Count) requested routes and $($routes.Count * 16) baseline targets."
