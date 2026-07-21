#requires -Version 7.0

[CmdletBinding()]
param(
    [ValidateRange(1, 65535)]
    [int]$SqlPort = 1433,

    [ValidateRange(1, 25000)]
    [int]$StudentCount = 25,

    [ValidatePattern('^https://localhost:\d+/?$')]
    [string]$Url = 'https://localhost:7078',

    [switch]$SkipBuild,
    [switch]$PrepareOnly,
    [switch]$ResetDatabase,
    [switch]$TrustHttpsCertificate,
    [switch]$PerformanceRuntime
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$solution = Join-Path $repositoryRoot 'StudentRegistration.slnx'
$apiProject = Join-Path $repositoryRoot 'src\StudentRegistration.Api\StudentRegistration.Api.csproj'
$clientProject = Join-Path $repositoryRoot 'src\StudentRegistration.Client\StudentRegistration.Client.csproj'
$sqlProject = Join-Path $repositoryRoot 'src\StudentRegistration.Infrastructure.SqlServer\StudentRegistration.Infrastructure.SqlServer.csproj'
$composeFile = Join-Path $repositoryRoot 'infra\docker\compose.development.yml'
$certificatePath = Join-Path $repositoryRoot '.local\secrets\data-protection.pfx'
$performancePublishPath = Join-Path $repositoryRoot '.local\performance-app'
$performanceClientPublishPath = Join-Path $repositoryRoot '.local\performance-client'
$performanceArtifactsPath = Join-Path $repositoryRoot '.local\performance-build'

function Invoke-Checked {
    param(
        [Parameter(Mandatory)] [string]$FilePath,
        [Parameter(Mandatory)] [string[]]$ArgumentList
    )

    & $FilePath @ArgumentList
    if ($LASTEXITCODE -ne 0) {
        # Arguments can contain the local connection string. Never echo them.
        throw "Command failed with exit code $LASTEXITCODE`: $FilePath"
    }
}

function Get-LocalDemoSecrets {
    $output = (& dotnet user-secrets list --json --project $apiProject 2>&1 | Out-String)
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to read the API project user secrets.'
    }

    $match = [regex]::Match($output, '(?s)//BEGIN\s*(.*?)\s*//END')
    if (-not $match.Success -or [string]::IsNullOrWhiteSpace($match.Groups[1].Value)) {
        return @{}
    }

    $parsed = $match.Groups[1].Value | ConvertFrom-Json -AsHashtable
    if ($null -eq $parsed) {
        return @{}
    }

    return $parsed
}

function New-LocalSecret {
    $random = [Security.Cryptography.RandomNumberGenerator]::GetBytes(36)
    return "Srs!9Aa$([Convert]::ToBase64String($random))"
}

function Test-DataProtectionCertificate {
    param(
        [Parameter(Mandatory)] [string]$Path,
        [Parameter(Mandatory)] [string]$Password
    )

    try {
        $certificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new(
            $Path,
            $Password,
            [Security.Cryptography.X509Certificates.X509KeyStorageFlags]::EphemeralKeySet)
        try {
            return $certificate.HasPrivateKey
        }
        finally {
            $certificate.Dispose()
        }
    }
    catch {
        return $false
    }
}

function New-DataProtectionCertificate {
    param(
        [Parameter(Mandatory)] [string]$Path,
        [Parameter(Mandatory)] [string]$Password
    )

    $parent = Split-Path $Path -Parent
    New-Item $parent -ItemType Directory -Force | Out-Null
    $securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
    $certificate = New-SelfSignedCertificate `
        -Subject 'CN=StudentRegistration Local Data Protection' `
        -CertStoreLocation 'Cert:\CurrentUser\My' `
        -KeyAlgorithm RSA `
        -KeyLength 3072 `
        -KeyExportPolicy Exportable `
        -NotAfter (Get-Date).AddYears(2)

    try {
        Export-PfxCertificate `
            -Cert $certificate `
            -FilePath $Path `
            -Password $securePassword `
            -Force | Out-Null
    }
    finally {
        Remove-Item "Cert:\CurrentUser\My\$($certificate.Thumbprint)" -Force -ErrorAction SilentlyContinue
        $certificate.Dispose()
    }
}

Push-Location $repositoryRoot
try {
    Write-Host 'Checking the local toolchain...'
    Invoke-Checked dotnet @('--version')
    Invoke-Checked docker @('version', '--format', '{{.Client.Version}}')
    Invoke-Checked docker @('compose', 'version')

    $webPort = ([uri]$Url).Port
    $listeners = @(Get-NetTCPConnection `
        -LocalPort $webPort `
        -State Listen `
        -ErrorAction SilentlyContinue)
    if ($listeners.Count -gt 0) {
        $ownerId = $listeners[0].OwningProcess
        $owner = Get-Process -Id $ownerId -ErrorAction SilentlyContinue
        $ownerName = if ($null -eq $owner) { 'unknown process' } else { $owner.ProcessName }
        throw "Web port $webPort is already used by $ownerName (PID $ownerId). Stop the existing app with Ctrl+C, or stop that PID after verifying it, then retry."
    }

    $httpsCheckOutput = & dotnet dev-certs https --check --trust 2>&1
    if ($LASTEXITCODE -ne 0) {
        if ($TrustHttpsCertificate) {
            Invoke-Checked dotnet @('dev-certs', 'https', '--trust')
        }
        else {
            Write-Warning 'The ASP.NET Core HTTPS certificate is not trusted. Re-run with -TrustHttpsCertificate before browser testing.'
        }
    }

    $secrets = Get-LocalDemoSecrets
    $sqlPassword = if (-not [string]::IsNullOrWhiteSpace($env:SRS_SQL_SA_PASSWORD)) {
        $env:SRS_SQL_SA_PASSWORD
    }
    elseif ($secrets.ContainsKey('LocalDemo:SqlSaPassword')) {
        [string]$secrets['LocalDemo:SqlSaPassword']
    }
    else {
        New-LocalSecret
    }

    $certificatePassword = if (-not [string]::IsNullOrWhiteSpace($env:SRS_DATA_PROTECTION_CERTIFICATE_PASSWORD)) {
        $env:SRS_DATA_PROTECTION_CERTIFICATE_PASSWORD
    }
    elseif ($secrets.ContainsKey('DataProtection:CertificatePassword')) {
        [string]$secrets['DataProtection:CertificatePassword']
    }
    else {
        New-LocalSecret
    }

    if (Test-Path $certificatePath) {
        if (-not (Test-DataProtectionCertificate $certificatePath $certificatePassword)) {
            if ($ResetDatabase) {
                Remove-Item $certificatePath -Force
                New-DataProtectionCertificate $certificatePath $certificatePassword
            }
            else {
                throw 'The local Data Protection certificate does not match the saved password. Set SRS_DATA_PROTECTION_CERTIFICATE_PASSWORD to its original password or use -ResetDatabase to replace the local database and certificate together.'
            }
        }
    }
    else {
        Write-Host 'Creating the local Data Protection certificate...'
        New-DataProtectionCertificate $certificatePath $certificatePassword
    }

    $connectionString = "Server=localhost,$SqlPort;Database=StudentRegistration_Development;User ID=sa;Password=$sqlPassword;Encrypt=True;TrustServerCertificate=True"
    $storedSettings = @{
        'LocalDemo:SqlSaPassword' = $sqlPassword
        'ConnectionStrings:StudentRegistration' = $connectionString
        'DataProtection:ApplicationName' = 'AASTMT.StudentRegistration'
        'DataProtection:Repository' = 'SqlServer'
        'DataProtection:Encryption' = 'ExternalCertificate'
        'DataProtection:CertificatePath' = $certificatePath
        'DataProtection:CertificatePassword' = $certificatePassword
        'DemoDatabase:StudentCount' = [string]$StudentCount
    }
    $storedSettings | ConvertTo-Json -Compress |
        & dotnet user-secrets set --project $apiProject | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to save the local Development settings in .NET User Secrets.'
    }

    $env:SRS_SQL_SA_PASSWORD = $sqlPassword
    $env:SRS_SQL_PORT = [string]$SqlPort
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:DOTNET_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = $Url
    $env:ConnectionStrings__StudentRegistration = $connectionString
    $env:DataProtection__ApplicationName = 'AASTMT.StudentRegistration'
    $env:DataProtection__Repository = 'SqlServer'
    $env:DataProtection__Encryption = 'ExternalCertificate'
    $env:DataProtection__CertificatePath = $certificatePath
    $env:DataProtection__CertificatePassword = $certificatePassword
    $env:DemoDatabase__StudentCount = [string]$StudentCount
    $env:Logging__LogLevel__Microsoft = 'Warning'

    if ($ResetDatabase) {
        Write-Host 'Deleting the local Development SQL volume...'
        Invoke-Checked docker @('compose', '-f', $composeFile, 'down', '--volumes', '--remove-orphans')
    }

    Write-Host 'Starting SQL Server and waiting for readiness...'
    try {
        Invoke-Checked docker @(
            'compose', '-f', $composeFile, 'up', '-d',
            '--wait', '--wait-timeout', '180')
    }
    catch {
        & docker compose -f $composeFile ps
        & docker compose -f $composeFile logs --tail 80 sqlserver
        throw 'SQL Server did not become healthy. If this volume was created with a different password, re-run with -ResetDatabase.'
    }

    if (-not $SkipBuild) {
        Write-Host 'Restoring tools and building the solution...'
        Invoke-Checked dotnet @('tool', 'restore')
        Invoke-Checked dotnet @('restore', $solution)
        Invoke-Checked dotnet @('build', $solution, '--configuration', 'Release', '--no-restore')
    }

    Write-Host 'Applying database migrations...'
    Invoke-Checked dotnet @(
        'ef', 'database', 'update',
        '--no-build',
        '--project', $sqlProject,
        '--configuration', 'Release',
        '--connection', $connectionString)

    Write-Host 'Creating or verifying the synthetic Development seed...'
    Invoke-Checked dotnet @(
        'run', '--project', $apiProject,
        '--configuration', 'Release',
        '--no-build', '--no-launch-profile', '--',
        '--initialize-demo-database')

    Write-Host ''
    Write-Host 'Local demo preparation succeeded.' -ForegroundColor Green
    Write-Host "SQL Server: localhost:$SqlPort (healthy)"
    Write-Host "Web application: $Url"
    Write-Host "Health check: $($Url.TrimEnd('/'))/api/health"

    if ($PrepareOnly) {
        Write-Host 'Preparation-only mode complete; the web application was not started.'
        return
    }

    if ($PerformanceRuntime) {
        # Preparation and deterministic seeding intentionally run in Development.
        # Publish and switch only the long-lived web process so performance
        # checks exercise precompressed static assets and omit WASM debugging.
        Write-Host 'Publishing the optimized local performance build...'
        Invoke-Checked dotnet @(
            'publish', $apiProject,
            '--configuration', 'Release',
            '--artifacts-path', $performanceArtifactsPath,
            '--output', $performancePublishPath)
        Invoke-Checked dotnet @(
            'publish', $clientProject,
            '--configuration', 'Release',
            '--artifacts-path', $performanceArtifactsPath,
            '--output', $performanceClientPublishPath)
        Copy-Item `
            -Path (Join-Path $performanceClientPublishPath 'wwwroot\*') `
            -Destination (Join-Path $performancePublishPath 'wwwroot') `
            -Recurse `
            -Force
        $env:ASPNETCORE_ENVIRONMENT = 'Testing'
        $env:DOTNET_ENVIRONMENT = 'Testing'
        Write-Host 'Starting the web application with the optimized local performance profile.'
    }

    Write-Host 'Press Ctrl+C to stop the web application; the SQL volume is preserved.'

    if ($PerformanceRuntime) {
        Invoke-Checked dotnet @(
            (Join-Path $performancePublishPath 'StudentRegistration.Api.dll'),
            '--contentRoot', $performancePublishPath)
    }
    else {
        Invoke-Checked dotnet @(
            'run', '--project', $apiProject,
            '--configuration', 'Release',
            '--no-build', '--no-launch-profile')
    }
}
finally {
    Pop-Location
}
