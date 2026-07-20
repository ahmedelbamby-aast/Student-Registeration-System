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
- PowerShell 7
- A trusted ASP.NET Core HTTPS development certificate

## 1. Build, configure, migrate, seed, and run

Run one command from the repository root:

```powershell
Set-Location 'C:\Users\Ahmed\Documents\Student Registeration System'
.\ops\scripts\Start-LocalDemo.ps1 -TrustHttpsCertificate
```

The launcher checks the tools, creates random local-only secrets, stores them
outside Git in .NET User Secrets, creates the external PFX, waits for SQL
health, restores/builds, applies EF migrations, runs the idempotent synthetic
seed, and starts the API-hosted Blazor client. No environment variables need
to be copied between terminals.

Useful variants:

```powershell
.\ops\scripts\Start-LocalDemo.ps1 -SkipBuild
.\ops\scripts\Start-LocalDemo.ps1 -PrepareOnly
.\ops\scripts\Start-LocalDemo.ps1 -ResetDatabase
```

`-ResetDatabase` explicitly deletes only the local Docker Development volume
before recreating it. The default preserves data. Use it once if the retained
volume predates the complete manual-test seed. The idempotent seed creates 25
students and three staff accounts, a published 19-course three-credit roadmap,
the 12/18/21-credit policy boundaries, and 19 published staffed offerings with
rooms and meeting times. Newly issued credentials are written under:

```text
src/StudentRegistration.Api/.local/credentials/imports/import-<id>.json
```

The current profile version is `synthetic-fixture/2.0`. The initializer safely
upgrades an intact v1 synthetic profile in place while preserving stable
student and term identities. It replaces only verified seed-owned transcript
and hold rows. If local academic fixture rows were manually changed, the seed
returns `ACADEMIC_SEED_UPGRADE_UNSAFE`; preserve that data, or rerun with
`-ResetDatabase` only when a destructive local reset is acceptable.

Open [https://localhost:7078](https://localhost:7078). The root page is the
role gateway for the hosted web application.

Useful routes:

| Purpose | URL |
|---|---|
| Role gateway | `https://localhost:7078/` |
| Student login | `https://localhost:7078/student/login` |
| Staff login | `https://localhost:7078/staff/login` |
| Student activation | `https://localhost:7078/student/activate` |
| Account recovery | `https://localhost:7078/account/recovery` |
| OpenAPI document (Development only) | `https://localhost:7078/openapi/v1.json` |

## 2. Inspect the generated credential artifact

The Development seed uses simple, deterministic demo-only passwords based on
each institutional-style login ID. After the initializer has successfully
seeded a fresh database, it also writes a Git-ignored JSON file under the API
content root.

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
$login = 'ADM-0001'
($credentials.accounts |
    Where-Object loginIdentifier -eq $login |
    Select-Object -First 1).secret
```

Treat this artifact like a secret. It is excluded by `.gitignore` and expires
after seven days.

## 7. Verified local demo credentials

### Complete test-account matrix

These credentials were verified against the live HTTPS authentication
endpoints on 20 July 2026.

| Test role | Login page | Username / University ID | Password |
|---|---|---|---|
| Student (after first-use activation) | `/student/login` | `AI2600001` | `DemoLogin@2026!!` |
| Administrator | `/staff/login` | `ADM-0001` | `Demo@2026-ADM-0001` |
| Lecturer (teacher) | `/staff/login` | `LEC-0001` | `Demo@2026-LEC-0001` |
| Teaching Assistant | `/staff/login` | `TA-0001` | `Demo@2026-TA-0001` |

After `-ResetDatabase`, activate the student through the supported first-use flow before using the login row above. Its
initial activation password is `Demo@2026-AI2600001`; its current login
password is `DemoLogin@2026!!`. Staff sign in directly with their role/staff
ID and matching ID-based password.

### Student

1. On a fresh/reset database, open `https://localhost:7078/student/activate`, use `AI2600001` with `Demo@2026-AI2600001`, and choose `DemoLogin@2026!!`.
2. Open `https://localhost:7078/student/login`.
3. Enter `AI2600001` and `DemoLogin@2026!!`.
4. Select **Sign in**. A successful login opens `/student`.

The default seed creates 25 students: `AI2600001` through `AI2600025`.
Changing `DemoDatabase__StudentCount` changes the upper number, up to 25,000.
`AI2600001` is the first-program-term persona. `AI2600002` and later provide
multiple term-2+ academic states. Normal personas have no active blocking
hold; `AI2600007` is the intentionally blocked persona.

### Administrator

1. Open `https://localhost:7078/staff/login`.
2. Enter staff ID `ADM-0001`.
3. Enter `Demo@2026-ADM-0001`.
4. Select **Sign in**. The Admin role opens `/admin`.

### Lecturer (teacher)

1. Open `https://localhost:7078/staff/login`.
2. Enter staff ID `LEC-0001`.
3. Enter `Demo@2026-LEC-0001`.
4. Select **Sign in**. The Lecturer role opens `/staff`.

### Teaching Assistant

1. Open `https://localhost:7078/staff/login`.
2. Enter staff ID `TA-0001`.
3. Enter `Demo@2026-TA-0001`.
4. Select **Sign in**. The Teaching Assistant role opens `/staff`.

The staff form never asks the user to claim a role. Roles come from the server.
Each enabled demo account has exactly one of the four supported roles: Student,
Admin, Lecturer, or Teaching Assistant.

## Stop or reset local infrastructure

First press `Ctrl+C` in the web-app terminal. Then stop SQL Server while
preserving its Docker volume:

```powershell
.\ops\scripts\Stop-LocalDemo.ps1
```

Delete the local Development database volume as well:

```powershell
.\ops\scripts\Stop-LocalDemo.ps1 -ResetDatabase
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
| SQL login or connection failure | Re-run the launcher. If it reports an older volume/password mismatch, use `-ResetDatabase` only when deleting local demo data is intended. |
| `The target principal name is incorrect` | Use the launcher so EF receives the generated SQL-authentication connection through User Secrets and the process environment. |
| Browser rejects the certificate | Re-run with `-TrustHttpsCertificate`, close the browser, and start it again. |
| `MSB3027` or `MSB3021` says `StudentRegistration.Api.dll` is locked | Stop the already-running web app with `Ctrl+C`. The launcher also reports the PID when port 7078 is occupied. |
| Login returns invalid credentials on a fresh database | Run the launcher without `-PrepareOnly`, then use the verified credentials below. |
| No credential JSON exists | The initializer did not provision new accounts, or the artifact expired after seven days. |
| `ACADEMIC_SEED_UPGRADE_UNSAFE` | The retained v1 synthetic academic rows were modified and are not safe to overwrite. Preserve them, or run `Start-LocalDemo.ps1 -ResetDatabase` if deleting local demo data is intended. |
