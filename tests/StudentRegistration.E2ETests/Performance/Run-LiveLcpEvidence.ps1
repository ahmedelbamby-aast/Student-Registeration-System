[CmdletBinding()]
param(
    [string] $BaseUrl = 'https://localhost:7078',
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    [switch] $SkipBuild
)

$ErrorActionPreference = 'Stop'
$required = @('SRS_LIVE_STUDENT_ID', 'SRS_LIVE_STUDENT_PASSWORD')
$missing = @($required | Where-Object { [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($_)) })
if ($missing.Count -ne 0) {
    throw "Live LCP evidence requires activated Student credentials in: $($missing -join ', '). Values are never logged."
}
if (-not [Uri]::IsWellFormedUriString($BaseUrl, [UriKind]::Absolute) -or -not $BaseUrl.StartsWith('https://', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'BaseUrl must be an absolute HTTPS URL.'
}

$project = Join-Path $PSScriptRoot '../StudentRegistration.E2ETests.csproj'
if (-not $SkipBuild) {
    & dotnet build $project --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "E2E build failed with exit code $LASTEXITCODE." }
}

$env:SRS_RUN_LIVE_LCP = '1'
$env:SRS_LIVE_BASE_URL = $BaseUrl.TrimEnd('/')
try {
    & dotnet test $project --configuration $Configuration --no-build --no-restore `
        --filter 'FullyQualifiedName~Spec003LiveLcpEvidenceTests.Stu_02_04_05_live_authenticated_routes_meet_p75_budget' `
        --logger 'console;verbosity=normal'
    if ($LASTEXITCODE -ne 0) { throw "Live LCP evidence failed with exit code $LASTEXITCODE." }
}
finally {
    Remove-Item Env:SRS_RUN_LIVE_LCP -ErrorAction SilentlyContinue
    Remove-Item Env:SRS_LIVE_BASE_URL -ErrorAction SilentlyContinue
}
