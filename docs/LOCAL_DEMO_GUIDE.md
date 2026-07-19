# Local demo startup and login guide

This guide explains how to run the Student Registration System locally on
Windows with PowerShell and how each demo role signs in.

> [!IMPORTANT]
> The credentials in this file are for the local Development demo database
> only. They are intentionally recorded here at the demo owner's request and
> must never be reused for production, institutional, or personal accounts.

## Prerequisites

- .NET SDK `10.0.301` (the version pinned in `global.json`)
- Docker Desktop with Linux containers
- PowerShell 7 or Windows PowerShell 5.1
- A trusted ASP.NET Core HTTPS development certificate

Run all commands from the repository root:

```powershell
Set-Location 'C:\Users\Ahmed\Documents\Student Registeration System'
dotnet --version
docker --version
dotnet dev-certs https --trust
```

`dotnet --version` should print `10.0.301` or a compatible patch selected by
`global.json`.

## 1. Start SQL Server

Choose a strong local-only SQL password. Do not commit it to Git.

```powershell
$env:SRS_SQL_SA_PASSWORD = 'LocalDemo-Only-Change-Me-2026!'
$env:SRS_SQL_PORT = '1433'

docker compose -f .\infra\docker\compose.development.yml up -d
docker compose -f .\infra\docker\compose.development.yml ps
```

Wait until the `sqlserver` service reports `healthy`:

```powershell
docker compose -f .\infra\docker\compose.development.yml ps --format json
```

To inspect SQL Server startup failures:

```powershell
docker compose -f .\infra\docker\compose.development.yml logs sqlserver
```

## 2. Configure the Development process

The API fails closed unless it receives a SQL connection string and an external
PFX certificate used to protect the shared ASP.NET Core Data Protection keys.

Create a local, Git-ignored certificate:

```powershell
$certificatePasswordText = 'Local-Pfx-Only-Change-Me-2026!'
$certificatePassword = ConvertTo-SecureString $certificatePasswordText -AsPlainText -Force
$certificate = New-SelfSignedCertificate `
    -Subject 'CN=StudentRegistration Local Data Protection' `
    -CertStoreLocation 'Cert:\CurrentUser\My' `
    -KeyAlgorithm RSA `
    -KeyLength 3072 `
    -KeyExportPolicy Exportable `
    -NotAfter (Get-Date).AddYears(2)

$certificatePath = Join-Path (Get-Location) '.local\secrets\data-protection.pfx'
New-Item (Split-Path $certificatePath) -ItemType Directory -Force | Out-Null
Export-PfxCertificate `
    -Cert $certificate `
    -FilePath $certificatePath `
    -Password $certificatePassword | Out-Null
```

Set the required configuration in the same PowerShell window that will run the
application:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__StudentRegistration = "Server=localhost,$env:SRS_SQL_PORT;Database=StudentRegistration_Development;User ID=sa;Password=$env:SRS_SQL_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True"
$env:DataProtection__ApplicationName = 'AASTMT.StudentRegistration'
$env:DataProtection__Repository = 'SqlServer'
$env:DataProtection__Encryption = 'ExternalCertificate'
$env:DataProtection__CertificatePath = $certificatePath
$env:DataProtection__CertificatePassword = $certificatePasswordText
$env:DemoDatabase__StudentCount = '25'
```

PowerShell environment variables are scoped to the current terminal process.
If this window is closed, repeat this configuration block in the new window
before running EF or the API. The fact that the Docker container is still
running does not restore these PowerShell variables.

The double underscore in an environment-variable name maps to a colon in .NET
configuration. For example,
`ConnectionStrings__StudentRegistration` supplies
`ConnectionStrings:StudentRegistration`.

## 3. Create or update the database schema

Restore tools and packages, then apply the EF Core migrations:

```powershell
dotnet tool restore
dotnet restore .\StudentRegistration.slnx

if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__StudentRegistration)) {
    throw 'Set ConnectionStrings__StudentRegistration as shown in step 2.'
}

dotnet ef database update `
    --project .\src\StudentRegistration.Infrastructure.SqlServer\StudentRegistration.Infrastructure.SqlServer.csproj `
    --connection "$env:ConnectionStrings__StudentRegistration"
```

If the guard throws, stop there and repeat step 2. Do not run the EF command
with an empty `--connection`: an empty value lets EF fall back to
`StudentRegistration_DesignTime`, which uses Windows authentication and can
surface a misleading SSL error such as `The target principal name is
incorrect`.

This creates or updates only the database named
`StudentRegistration_Development`. The SQL infrastructure project is used
directly because it owns both `Microsoft.EntityFrameworkCore.Design` and the
`IDesignTimeDbContextFactory`. Do not select `StudentRegistration.Api` as the
startup project for this command. The explicit `--connection` value overrides
the factory's harmless `StudentRegistration_DesignTime` fallback and prevents
the migration from targeting the wrong local database. This command does not
create demo identities.

## 4. Seed the Development demo database

The seed command is explicit, Development-only, migration-first, and exits
when seeding finishes. Run it from the repository root in the same PowerShell
window configured in step 2:

```powershell
dotnet run `
    --project .\src\StudentRegistration.Api\StudentRegistration.Api.csproj `
    --configuration Release `
    --no-launch-profile `
    -- --initialize-demo-database
```

The default configuration creates 25 students and four staff accounts. On a
fresh database it also creates a Git-ignored credential artifact under:

```text
src/StudentRegistration.Api/.local/credentials/imports/import-<id>.json
```

The command is idempotent. Existing identities are not recreated and their
passwords are not rotated when the command is run again.

## 5. Start the web application

The API hosts both the API endpoints and the Blazor WebAssembly client. Only
one application process is required.

```powershell
dotnet run `
    --project .\src\StudentRegistration.Api\StudentRegistration.Api.csproj `
    --urls 'https://localhost:7078'
```

Open [https://localhost:7078](https://localhost:7078). The root page is the
role gateway.

Useful routes:

| Purpose | URL |
|---|---|
| Role gateway | `https://localhost:7078/` |
| Student login | `https://localhost:7078/student/login` |
| Staff login | `https://localhost:7078/staff/login` |
| Student activation | `https://localhost:7078/student/activate` |
| Account recovery | `https://localhost:7078/account/recovery` |
| OpenAPI document (Development only) | `https://localhost:7078/openapi/v1.json` |

## 6. Inspect the generated credential artifact

Passwords are intentionally random; there is no shared hard-coded demo
password. After the Development initializer has successfully seeded a fresh
database, it writes a reveal-once, Git-ignored JSON file under the API content
root.

Show the newest credential file:

```powershell
$credentialFile = Get-ChildItem '.\src\StudentRegistration.Api\.local\credentials\imports\import-*.json' |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if ($null -eq $credentialFile) {
    throw 'No demo credential artifact exists. Run the seed command in step 4.'
}

$credentials = Get-Content $credentialFile.FullName -Raw | ConvertFrom-Json
$credentials.accounts |
    Select-Object loginIdentifier, secret |
    Format-Table -AutoSize
```

To retrieve one password without printing every account:

```powershell
$login = 'admin.demo'
($credentials.accounts |
    Where-Object loginIdentifier -eq $login |
    Select-Object -First 1).secret
```

Treat this artifact like a secret. It is excluded by `.gitignore` and expires
after seven days.

## 7. Verified local demo credentials

### Complete test-account matrix

These credentials were verified against the live HTTPS authentication
endpoints on 19 July 2026.

| Test role | Login page | Username / University ID | Password |
|---|---|---|---|
| Student | `/student/login` | `AI2600001` | `N7v!q2L#p9R@x4T-m8K` |
| Administrator | `/staff/login` | `admin.demo` | `RkV74K5RYb2vsio48SwAhqQa` |
| Lecturer (teacher) | `/staff/login` | `lecturer.demo` | `-xHooVRUPPWKCKAzCkz-atYu` |
| Teaching Assistant | `/staff/login` | `ta.demo` | `7McgZ_LNUgTUbniNpd-cbaQn` |
| Lecturer + Teaching Assistant | `/staff/login` | `dual.demo` | `CaYKEpxkgfVMhY3hUHqCABzB` |

The student account was activated through the supported first-use flow, so
its login password differs from the generated initial password retained in
the artifact. The four staff passwords match their generated seed values.

### Student

1. Open `https://localhost:7078/student/login`.
2. Enter a seeded University ID, such as `AI2600001`.
3. Enter `N7v!q2L#p9R@x4T-m8K`.
4. Select **Sign in**. A successful login opens `/student`.

The default seed creates 25 students: `AI2600001` through `AI2600025`.
Changing `DemoDatabase__StudentCount` changes the upper number, up to 25,000.

### Administrator

1. Open `https://localhost:7078/staff/login`.
2. Enter username `admin.demo`.
3. Enter `RkV74K5RYb2vsio48SwAhqQa`.
4. Select **Sign in**. The Admin role opens `/admin`.

### Lecturer (teacher)

1. Open `https://localhost:7078/staff/login`.
2. Enter username `lecturer.demo`.
3. Enter `-xHooVRUPPWKCKAzCkz-atYu`.
4. Select **Sign in**. The Lecturer role opens `/staff`.

### Teaching Assistant

1. Open `https://localhost:7078/staff/login`.
2. Enter username `ta.demo`.
3. Enter `7McgZ_LNUgTUbniNpd-cbaQn`.
4. Select **Sign in**. The Teaching Assistant role opens `/staff`.

### User with Lecturer and Teaching Assistant roles

1. Open `https://localhost:7078/staff/login`.
2. Enter username `dual.demo` and password `CaYKEpxkgfVMhY3hUHqCABzB`.
3. After authentication, choose either **Lecturer** or
   **Teaching Assistant** when the authorized-role selector appears.

The staff form never asks the user to claim a role. Roles come from the server.
Only `dual.demo` needs to select between more than one authorized staff role.

## Stop or reset local infrastructure

Stop SQL Server while preserving its Docker volume:

```powershell
docker compose -f .\infra\docker\compose.development.yml down
```

Delete the local Development database volume as well:

```powershell
docker compose -f .\infra\docker\compose.development.yml down --volumes
```

The second command permanently deletes the local demo database. It does not
delete the Git-ignored credential artifact, which should be removed separately
when it is no longer needed.

## Common failures

| Message or symptom | Resolution |
|---|---|
| `DATABASE_CONNECTION_REQUIRED` | Set `ConnectionStrings__StudentRegistration` in the process that runs the API. |
| `POC_SECURITY_INPUT_REQUIRED` | Set both Data Protection certificate variables. |
| `DATA_PROTECTION_CONFIGURATION_INVALID` | Confirm the PFX path is absolute, exists, has a private key, and uses the supplied password. |
| SQL login or connection failure | Wait for Docker health, then verify the port and that the connection-string password matches `SRS_SQL_SA_PASSWORD`. |
| `The target principal name is incorrect` after the connection guard failed | Do not continue after the guard. Restore `ConnectionStrings__StudentRegistration` using step 2 so EF uses SQL authentication and `TrustServerCertificate=True`. |
| Browser rejects the certificate | Run `dotnet dev-certs https --trust`, close the browser, and start it again. |
| API startup project does not reference `Microsoft.EntityFrameworkCore.Design` | Remove `--startup-project StudentRegistration.Api` and use the migration command in step 3. The Design package belongs to the SQL infrastructure project. |
| Login returns invalid credentials on a fresh database | Run the explicit seed command in step 4, then use the verified credentials in step 7. |
| No credential JSON exists | The initializer did not provision new accounts, or the artifact expired after seven days. |
