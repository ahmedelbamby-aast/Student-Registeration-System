# Development Environment

## Installed and verified on 12 July 2026

| Tool | State |
|---|---|
| Git | 2.53.0.windows.2 |
| .NET SDK | 10.0.301, user-local at C:/Users/Ahmed/.dotnet |
| .NET runtime | 10.0.9 in user-local SDK installation |
| dotnet-ef | 10.0.9 global .NET tool |
| Docker | 29.4.2 |
| Node.js | 24.14.0 |
| PowerShell | 7.6.3 |

The .NET and global-tool directories were added to the user PATH:

- C:/Users/Ahmed/.dotnet
- C:/Users/Ahmed/.dotnet/tools

Open a new terminal if the current shell does not see the updated PATH. The
repository pins SDK 10.0.301 in global.json and rolls forward only to a newer
patch in the same feature band.

## Baseline commands

```powershell
dotnet --info
dotnet-ef --version
docker --version
git status
```

## Tools introduced by implementation specs

Install/pin these only after Gate A, in repository manifests/configuration:

- SQL Server Developer container compatible with the approved production
  version.
- xUnit for unit and integration tests.
- Testcontainers for real SQL Server concurrency tests.
- Microsoft Playwright for Blazor browser tests.
- k6 for registration-window load tests.
- NetArchTest or ArchUnitNET for module dependency tests.
- Built-in ASP.NET Core OpenAPI, health checks, rate limiting.
- OpenTelemetry SDK and an exporter selected for the deployment environment.

Docker is installed, but a SQL Server image is intentionally not pulled during
planning. SPEC-005 and SPEC-018 must first approve the production SQL Server
version/compatibility level, container licensing, secrets, storage, and CI
strategy.

## Planned solution creation after approval

No source projects are created until Gate A. The approved scaffold will:

1. Create the solution and module/test projects targeting net10.0.
2. Reference EF Core SQL Server packages at the approved 10.0 patch.
3. Configure same-origin Blazor WebAssembly assets and ASP.NET Core API.
4. Create a local-only SQL Server configuration with secrets outside Git.
5. Create the initial Code First migration from SPEC-005.
6. Run build, architecture, unit, real-SQL integration, and E2E smoke tests.

## CI quality order

1. Restore from lock files.
2. Formatting and warnings-as-errors build.
3. Unit and architecture tests.
4. SQL Server integration/concurrency tests.
5. Migration idempotency/rollback validation.
6. Playwright critical-path and accessibility smoke tests.
7. Dependency/secret/security scans.
8. k6 gate on scheduled or release pipelines.

Production migrations are reviewed scripts or migration bundles applied as a
controlled deployment step. The application does not auto-migrate production
on startup.
