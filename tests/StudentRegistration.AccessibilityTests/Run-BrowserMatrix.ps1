[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [string] $TestFilter = 'FullyQualifiedName~StudentRegistration.AccessibilityTests.Routes',

    [string[]] $TargetNames = @(
        'Google Chrome',
        'Microsoft Edge',
        'Mozilla Firefox',
        'Playwright WebKit'),

    [switch] $SkipBuild,

    [switch] $SkipBrowserInstall
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$project = Join-Path $PSScriptRoot 'StudentRegistration.AccessibilityTests.csproj'
$matrixPath = Join-Path $repositoryRoot 'tests/StudentRegistration.E2ETests/browser-matrix.json'
$runId = 'SPEC-018-NFR-8-{0}' -f (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$resultDirectory = Join-Path $repositoryRoot ".local/accessibility-evidence/$runId/browser-matrix"
New-Item -ItemType Directory -Force -Path $resultDirectory | Out-Null

function Invoke-DotNetChecked {
    param([string[]] $Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Get-InstalledBrowserVersion {
    param([string] $Name, [object] $Target)

    $knownPath = switch ($Name) {
        'Google Chrome' { Join-Path $env:ProgramFiles 'Google/Chrome/Application/chrome.exe' }
        'Microsoft Edge' { Join-Path ${env:ProgramFiles(x86)} 'Microsoft/Edge/Application/msedge.exe' }
        'Mozilla Firefox' { Join-Path $env:ProgramFiles 'Mozilla Firefox/firefox.exe' }
        default { $null }
    }

    if ($null -ne $knownPath -and (Test-Path -LiteralPath $knownPath -PathType Leaf)) {
        return (Get-Item -LiteralPath $knownPath).VersionInfo.ProductVersion
    }

    if ($Name -eq 'Playwright WebKit') {
        return "WebKit $($Target.browserBuild) via Microsoft.Playwright $($Target.playwrightVersion)"
    }

    return 'not-observed'
}

Push-Location $repositoryRoot
try {
    if (-not $SkipBuild) {
        Invoke-DotNetChecked @(
            'build',
            'src/StudentRegistration.Client/StudentRegistration.Client.csproj',
            '--configuration', $Configuration)
        Invoke-DotNetChecked @('build', $project, '--configuration', $Configuration)
    }

    if (-not $SkipBrowserInstall) {
        $playwright = Get-ChildItem `
            -Path (Join-Path $PSScriptRoot "bin/$Configuration") `
            -Filter 'playwright.ps1' `
            -Recurse | Select-Object -First 1
        if ($null -eq $playwright) {
            throw 'The pinned Playwright installer was not produced by the accessibility build.'
        }

        & $playwright.FullName install firefox webkit
        if ($LASTEXITCODE -ne 0) {
            throw "Pinned Firefox/WebKit installation failed with exit code $LASTEXITCODE."
        }
    }

    $matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
    $required = @($matrix.targets | Where-Object status -EQ 'required')
    $expectedNames = @(
        'Google Chrome',
        'Microsoft Edge',
        'Mozilla Firefox',
        'Playwright WebKit')
    if ((@($required.name) -join '|') -ne ($expectedNames -join '|')) {
        throw 'The governed required browser matrix is incomplete or has changed order.'
    }
    $unknownNames = @($TargetNames | Where-Object { $_ -notin $expectedNames })
    if ($unknownNames.Count -ne 0) {
        throw "Unknown browser target(s): $($unknownNames -join ', ')."
    }
    $selected = @($required | Where-Object { $_.name -in $TargetNames })
    if ($selected.Count -eq 0) {
        throw 'At least one required browser target must be selected.'
    }

    $results = [System.Collections.Generic.List[object]]::new()
    foreach ($target in $selected) {
        $slug = switch ($target.name) {
            'Google Chrome' { 'chrome-stable' }
            'Microsoft Edge' { 'edge-stable' }
            'Mozilla Firefox' { 'firefox-release' }
            'Playwright WebKit' { 'playwright-webkit' }
        }
        $logPath = Join-Path $resultDirectory "$slug.log"
        $started = (Get-Date).ToUniversalTime()
        $env:SRS_BROWSER_TARGET = $target.name
        $env:SRS_ACCESSIBILITY_REQUIRE_RUNTIME = 'true'

        & dotnet test $project `
            --configuration $Configuration `
            --no-build `
            --no-restore `
            --filter $TestFilter `
            --blame-hang-timeout 3m `
            --blame-hang-dump-type none `
            --logger "trx;LogFileName=$slug.trx" `
            --results-directory $resultDirectory 2>&1 | Tee-Object -FilePath $logPath
        $exitCode = $LASTEXITCODE
        $finished = (Get-Date).ToUniversalTime()
        $trxPath = Join-Path $resultDirectory "$slug.trx"
        $executedCount = 0
        $skippedCount = 0
        if (-not (Test-Path -LiteralPath $trxPath -PathType Leaf)) {
            $exitCode = 1
            Add-Content -LiteralPath $logPath -Value 'FAIL-CLOSED: the test run produced no TRX result.'
        }
        else {
            [xml] $trx = Get-Content -LiteralPath $trxPath -Raw
            $counters = $trx.SelectSingleNode("//*[local-name()='Counters']")
            if ($null -eq $counters) {
                $exitCode = 1
                Add-Content -LiteralPath $logPath -Value 'FAIL-CLOSED: the TRX result contains no counters.'
            }
            else {
                $executedCount = [int] $counters.executed
                $skippedCount = [int] $counters.notExecuted + [int] $counters.inconclusive + [int] $counters.notRunnable
                if ($executedCount -eq 0) {
                    $exitCode = 1
                    Add-Content -LiteralPath $logPath -Value 'FAIL-CLOSED: the filter executed zero tests.'
                }
                if ($skippedCount -ne 0) {
                    $exitCode = 1
                    Add-Content -LiteralPath $logPath -Value "FAIL-CLOSED: $skippedCount test(s) were skipped or not executed."
                }
            }
        }
        $results.Add([ordered]@{
            name = $target.name
            label = $target.label
            configuredVersion = $target.browserBuild
            observedVersion = Get-InstalledBrowserVersion $target.name $target
            result = if ($exitCode -eq 0) { 'passed' } else { 'failed' }
            exitCode = $exitCode
            executedCount = $executedCount
            skippedCount = $skippedCount
            startedUtc = $started.ToString('O')
            finishedUtc = $finished.ToString('O')
            durationSeconds = [math]::Round(($finished - $started).TotalSeconds, 3)
            trx = "$slug.trx"
            trxSha256 = if (Test-Path -LiteralPath $trxPath -PathType Leaf) {
                (Get-FileHash -LiteralPath $trxPath -Algorithm SHA256).Hash.ToLowerInvariant()
            } else { $null }
            log = "$slug.log"
            logSha256 = if (Test-Path -LiteralPath $logPath -PathType Leaf) {
                (Get-FileHash -LiteralPath $logPath -Algorithm SHA256).Hash.ToLowerInvariant()
            } else { $null }
        })

        if ($exitCode -ne 0) {
            break
        }
    }

    $gitHead = (& git rev-parse HEAD).Trim()
    $gitDirty = -not [string]::IsNullOrWhiteSpace((& git status --short) -join "`n")
    $summary = [ordered]@{
        schemaVersion = 'spec018-accessibility-browser-matrix/1.0.0'
        runId = $runId
        requirement = 'SPEC-018/NFR-8'
        scope = $TestFilter
        executedOnUtc = (Get-Date).ToUniversalTime().ToString('O')
        operatingSystem = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
        gitCommit = $gitHead
        dirtyWorkingTree = $gitDirty
        selectedTargets = @($selected.name)
        results = $results
        allRequiredPassed = ($results.Count -eq $expectedNames.Count -and
            @($results | Where-Object result -NE 'passed').Count -eq 0)
        allSelectedPassed = ($results.Count -eq $selected.Count -and
            @($results | Where-Object result -NE 'passed').Count -eq 0)
        safari = 'deferred; Playwright WebKit was not represented as Safari'
    }
    $summaryPath = Join-Path $resultDirectory 'summary.json'
    $summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $summaryPath -Encoding utf8
    Write-Output "Browser-matrix evidence: $summaryPath"

    if (-not $summary.allSelectedPassed) {
        throw 'One or more selected accessibility browser targets did not pass.'
    }
}
finally {
    Remove-Item Env:SRS_BROWSER_TARGET -ErrorAction SilentlyContinue
    Remove-Item Env:SRS_ACCESSIBILITY_REQUIRE_RUNTIME -ErrorAction SilentlyContinue
    Pop-Location
}
