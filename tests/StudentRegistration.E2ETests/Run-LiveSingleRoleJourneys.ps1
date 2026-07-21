[CmdletBinding()]
param(
    [switch]$RequireStudent,
    [switch]$NoBuild,
    [string]$BaseOutputPath,
    [string]$ResultsDirectory = (Join-Path $PSScriptRoot 'TestResults\LiveComposed')
)

$ErrorActionPreference = 'Stop'

function Assert-EnvironmentVariable([string]$Name) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($Name, 'Process'))) {
        throw "Required live-test environment variable '$Name' is not set."
    }
}

Assert-EnvironmentVariable 'SRS_LIVE_DEMO_BASE_URL'
Assert-EnvironmentVariable 'SRS_LIVE_ADMIN_PASSWORD'
Assert-EnvironmentVariable 'SRS_LIVE_LECTURER_PASSWORD'
Assert-EnvironmentVariable 'SRS_LIVE_TA_PASSWORD'

$studentConfigured = -not [string]::IsNullOrWhiteSpace($env:SRS_LIVE_STUDENT_ID) -and
    -not [string]::IsNullOrWhiteSpace($env:SRS_LIVE_STUDENT_PASSWORD)

if ($RequireStudent) {
    Assert-EnvironmentVariable 'SRS_LIVE_STUDENT_ID'
    Assert-EnvironmentVariable 'SRS_LIVE_STUDENT_PASSWORD'
    $env:SRS_LIVE_REQUIRE_STUDENT = '1'
}

$expectedTests = if ($studentConfigured) { 4 } else { 3 }
$classFilter = 'FullyQualifiedName~StudentRegistration.E2ETests.Composed.LiveSingleRoleJourneys'
$filter = if ($studentConfigured -or $RequireStudent) {
    $classFilter
} else {
    "$classFilter&FullyQualifiedName!~Student_live_journey"
}

New-Item -ItemType Directory -Path $ResultsDirectory -Force | Out-Null
$trxName = 'live-single-role-journeys.trx'
$trxPath = Join-Path $ResultsDirectory $trxName
Remove-Item -LiteralPath $trxPath -Force -ErrorAction SilentlyContinue

$dotnetArguments = @(
    'test'
    (Join-Path $PSScriptRoot 'StudentRegistration.E2ETests.csproj')
    '--filter', $filter
    '--logger', "trx;LogFileName=$trxName"
    '--results-directory', $ResultsDirectory
)
if ($NoBuild) {
    $dotnetArguments += '--no-build'
}
if (-not [string]::IsNullOrWhiteSpace($BaseOutputPath)) {
    $dotnetArguments += "-p:BaseOutputPath=$BaseOutputPath"
}

& dotnet @dotnetArguments

if ($LASTEXITCODE -ne 0) {
    throw "Live composed journeys failed with exit code $LASTEXITCODE."
}
if (-not (Test-Path -LiteralPath $trxPath)) {
    throw "The test runner did not produce the expected TRX evidence."
}

[xml]$trx = Get-Content -LiteralPath $trxPath
$counters = $trx.TestRun.ResultSummary.Counters
$total = [int]$counters.total
$executed = [int]$counters.executed
$passed = [int]$counters.passed
$failed = [int]$counters.failed
$notExecuted = [int]$counters.notExecuted

if ($total -eq 0 -or $executed -eq 0) {
    throw 'The live composed filter matched zero executed tests.'
}
if ($total -ne $expectedTests) {
    throw "Expected $expectedTests live composed tests, but the TRX recorded $total."
}
if ($failed -ne 0 -or $notExecuted -ne 0 -or $passed -ne $expectedTests) {
    throw "Live composed evidence is not release-clean: passed=$passed failed=$failed skipped=$notExecuted total=$total."
}

Write-Host "Live composed journeys passed: $passed/$total. TRX: $trxPath"
