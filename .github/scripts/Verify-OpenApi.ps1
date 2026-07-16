[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$project = Join-Path $repositoryRoot "tests/StudentRegistration.ContractTests/StudentRegistration.ContractTests.csproj"

$arguments = @(
    "test",
    $project,
    "--configuration",
    $Configuration,
    "--filter",
    "FullyQualifiedName~StudentRegistration.ContractTests.OpenApi.OpenApiBaselineTests"
)
if ($NoRestore) {
    $arguments += "--no-restore"
}

& dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Generated OpenAPI differs semantically from the approved baseline."
}
