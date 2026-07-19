[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [string] $NvdaExecutable
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if ([string]::IsNullOrWhiteSpace($NvdaExecutable)) {
    $NvdaExecutable = Join-Path $repositoryRoot '.local/nvda-2026.1.1/nvda.exe'
}
$NvdaExecutable = (Resolve-Path -LiteralPath $NvdaExecutable).Path
$project = Join-Path $PSScriptRoot 'StudentRegistration.AccessibilityTests.csproj'
$runId = 'SPEC-018-NFR-8-NVDA-{0}' -f (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$resultDirectory = Join-Path $repositoryRoot ".local/accessibility-evidence/$runId/nvda-probe"
$configDirectory = Join-Path $resultDirectory 'nvda-config'
$nvdaLog = Join-Path $resultDirectory 'nvda.log'
$testLog = Join-Path $resultDirectory 'keyboard-probe.log'
New-Item -ItemType Directory -Force -Path $configDirectory | Out-Null

function Invoke-DotNetChecked {
    param([string[]] $Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Start-Nvda {
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new($NvdaExecutable)
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    foreach ($argument in @(
        '--minimal',
        '--disable-addons',
        "--config-path=$configDirectory",
        "--log-file=$nvdaLog",
        '--log-level=12')) {
        [void] $startInfo.ArgumentList.Add($argument)
    }

    return [System.Diagnostics.Process]::Start($startInfo)
}

function Stop-Nvda {
    $stopInfo = [System.Diagnostics.ProcessStartInfo]::new($NvdaExecutable)
    $stopInfo.UseShellExecute = $false
    $stopInfo.CreateNoWindow = $true
    [void] $stopInfo.ArgumentList.Add('--quit')
    [void] $stopInfo.ArgumentList.Add("--config-path=$configDirectory")
    $stop = [System.Diagnostics.Process]::Start($stopInfo)
    if ($null -ne $stop) {
        $stop.WaitForExit(10000) | Out-Null
        $stop.Dispose()
    }
}

$preExistingNvda = @(Get-Process -Name 'nvda' -ErrorAction SilentlyContinue)
if ($preExistingNvda.Count -ne 0) {
    throw 'NVDA is already running. Stop it before this isolated evidence probe so process provenance is unambiguous.'
}

$startedUtc = (Get-Date).ToUniversalTime()
$nvdaProcess = $null
$nvdaProcessEvidence = @()
$testExitCode = -1
Push-Location $repositoryRoot
try {
    Invoke-DotNetChecked @(
        'build',
        'src/StudentRegistration.Client/StudentRegistration.Client.csproj',
        '--configuration', $Configuration)
    Invoke-DotNetChecked @('build', $project, '--configuration', $Configuration)

    $nvdaProcess = Start-Nvda
    if ($null -eq $nvdaProcess) {
        throw 'The official NVDA process could not be started.'
    }

    $deadline = (Get-Date).AddSeconds(30)
    do {
        Start-Sleep -Milliseconds 250
        $running = @(Get-Process -Name 'nvda' -ErrorAction SilentlyContinue)
    } while ($running.Count -eq 0 -and (Get-Date) -lt $deadline)
    if ($running.Count -eq 0) {
        throw 'NVDA did not remain running for the browser integration probe.'
    }
    $nvdaProcessEvidence = @($running | ForEach-Object {
        [ordered]@{
            processId = $_.Id
            processName = $_.ProcessName
            startTimeUtc = $_.StartTime.ToUniversalTime().ToString('O')
            executable = $_.Path
        }
    })

    $nvdaVersion = (Get-Item -LiteralPath $NvdaExecutable).VersionInfo.ProductVersion
    $env:SRS_BROWSER_TARGET = 'Google Chrome'
    $env:SRS_ACCESSIBILITY_REQUIRE_RUNTIME = 'true'
    $env:SRS_ACCESSIBILITY_HEADED = 'true'
    $env:SRS_NVDA_PROBE = 'true'
    $env:SRS_NVDA_EXECUTABLE = $NvdaExecutable
    $env:SRS_NVDA_VERSION = $nvdaVersion
    $env:SRS_NVDA_LOG_PATH = $nvdaLog

    & dotnet test $project `
        --configuration $Configuration `
        --no-build `
        --no-restore `
        --filter 'Category=NvdaIntegrationProbe' `
        --logger 'trx;LogFileName=nvda-keyboard-probe.trx' `
        --results-directory $resultDirectory 2>&1 | Tee-Object -FilePath $testLog
    $testExitCode = $LASTEXITCODE
}
finally {
    foreach ($name in @(
        'SRS_BROWSER_TARGET',
        'SRS_ACCESSIBILITY_REQUIRE_RUNTIME',
        'SRS_ACCESSIBILITY_HEADED',
        'SRS_NVDA_PROBE',
        'SRS_NVDA_EXECUTABLE',
        'SRS_NVDA_VERSION',
        'SRS_NVDA_LOG_PATH')) {
        Remove-Item "Env:$name" -ErrorAction SilentlyContinue
    }

    try {
        Stop-Nvda
    }
    finally {
        Start-Sleep -Seconds 1
        Get-Process -Name 'nvda' -ErrorAction SilentlyContinue |
            Where-Object { $_.StartTime.ToUniversalTime() -ge $startedUtc } |
            Stop-Process -Force -ErrorAction SilentlyContinue
        if ($null -ne $nvdaProcess) {
            $nvdaProcess.Dispose()
        }
        Pop-Location
    }
}

$finishedUtc = (Get-Date).ToUniversalTime()
$temporaryNvdaLog = Join-Path $env:TEMP 'nvda.log'
if (-not (Test-Path -LiteralPath $nvdaLog -PathType Leaf) -and
    (Test-Path -LiteralPath $temporaryNvdaLog -PathType Leaf) -and
    (Get-Item -LiteralPath $temporaryNvdaLog).LastWriteTimeUtc -ge $startedUtc) {
    Copy-Item -LiteralPath $temporaryNvdaLog -Destination $nvdaLog
}
$nvdaLogExists = Test-Path -LiteralPath $nvdaLog -PathType Leaf
$nvdaLogText = if ($nvdaLogExists) { Get-Content -LiteralPath $nvdaLog -Raw } else { '' }
$speechEventCount = [regex]::Matches(
    $nvdaLogText,
    '(?im)^.*(?:speech|speak).*$',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
$focusEventCount = [regex]::Matches(
    $nvdaLogText,
    '(?im)^.*(?:focus|foreground).*$',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
$summary = [ordered]@{
    schemaVersion = 'spec018-nvda-integration-probe/1.0.0'
    runId = $runId
    requirement = 'SPEC-018/NFR-8'
    executedOnUtc = $startedUtc.ToString('O')
    finishedOnUtc = $finishedUtc.ToString('O')
    operatingSystem = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
    nvdaExecutable = $NvdaExecutable
    nvdaVersion = (Get-Item -LiteralPath $NvdaExecutable).VersionInfo.ProductVersion
    nvdaExecutableSha256 = (Get-FileHash -LiteralPath $NvdaExecutable -Algorithm SHA256).Hash.ToLowerInvariant()
    nvdaProcesses = $nvdaProcessEvidence
    nvdaLog = 'nvda.log'
    nvdaLogSha256 = if ($nvdaLogExists) {
        (Get-FileHash -LiteralPath $nvdaLog -Algorithm SHA256).Hash.ToLowerInvariant()
    } else { $null }
    nvdaInternalLogStatus = if ($nvdaLogExists) { 'captured' } else { 'not-emitted-by-portable-runtime' }
    speechEventCount = if ($nvdaLogExists) { $speechEventCount } else { $null }
    focusEventCount = if ($nvdaLogExists) { $focusEventCount } else { $null }
    keyboardProbeLog = 'keyboard-probe.log'
    keyboardProbeLogSha256 = (Get-FileHash -LiteralPath $testLog -Algorithm SHA256).Hash.ToLowerInvariant()
    keyboardProbeTrx = 'nvda-keyboard-probe.trx'
    keyboardProbeTrxSha256 = if (Test-Path -LiteralPath (Join-Path $resultDirectory 'nvda-keyboard-probe.trx')) {
        (Get-FileHash -LiteralPath (Join-Path $resultDirectory 'nvda-keyboard-probe.trx') -Algorithm SHA256).Hash.ToLowerInvariant()
    } else { $null }
    keyboardProbeExitCode = $testExitCode
    integrationProbeResult = if ($testExitCode -eq 0 -and $nvdaProcessEvidence.Count -gt 0) {
        'passed'
    } else { 'failed' }
    evidenceBoundary = 'Objective automated NVDA/Windows integration probe only; no human usability verdict.'
    manualTester = 'not-performed'
    uxQaSignOff = 'unsigned'
    releaseGate = 'blocked'
}
$summaryPath = Join-Path $resultDirectory 'summary.json'
$summary | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $summaryPath -Encoding utf8
Write-Output "NVDA integration evidence: $summaryPath"

if ($summary.integrationProbeResult -ne 'passed') {
    throw 'The real NVDA/Windows integration probe did not complete successfully.'
}
