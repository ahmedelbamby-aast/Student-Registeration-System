#requires -Version 7.0

[CmdletBinding()]
param(
    [switch]$ResetDatabase
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$composeFile = Join-Path $repositoryRoot 'infra\docker\compose.development.yml'

$hadSqlPassword = Test-Path Env:SRS_SQL_SA_PASSWORD
$originalSqlPassword = $env:SRS_SQL_SA_PASSWORD

# Compose validates this required interpolation even though `down` never uses
# the password. Restore the caller's environment before returning so a later
# start in the same terminal cannot inherit this teardown-only placeholder.
$env:SRS_SQL_SA_PASSWORD = 'not-used-by-compose-down'

$arguments = @('compose', '-f', $composeFile, 'down', '--remove-orphans')
if ($ResetDatabase) {
    $arguments += '--volumes'
}

Push-Location $repositoryRoot
try {
    & docker @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose teardown failed with exit code $LASTEXITCODE."
    }

    if ($ResetDatabase) {
        Write-Host 'The local Development database volume was deleted.' -ForegroundColor Yellow
    }
    else {
        Write-Host 'Local SQL Server stopped; the Development database volume was preserved.' -ForegroundColor Green
    }
}
finally {
    if ($hadSqlPassword) {
        $env:SRS_SQL_SA_PASSWORD = $originalSqlPassword
    }
    else {
        Remove-Item Env:SRS_SQL_SA_PASSWORD -ErrorAction SilentlyContinue
    }
    Pop-Location
}
