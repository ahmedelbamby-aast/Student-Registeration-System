# Development Environment

## Installed and verified on 13 July 2026

| Tool | State |
|---|---|
| Git | 2.53.0.windows.2 |
| .NET SDK | 10.0.301, user-local at C:/Users/Ahmed/AppData/Local/Microsoft/dotnet |
| .NET runtime | 10.0.9 in user-local SDK installation |
| dotnet-ef | 10.0.9 repository-local tool restored from .config/dotnet-tools.json |
| Docker | 29.4.2 |
| Node.js | 24.14.0 |
| PowerShell | 7.6.3 |

The user-local .NET SDK directory was added to the user PATH; the EF tool is
invoked from the repository manifest and needs no global-tool PATH entry:

- C:/Users/Ahmed/AppData/Local/Microsoft/dotnet

Open a new terminal if the current shell does not see the updated PATH. The
repository pins SDK 10.0.301 in global.json and rolls forward only to a newer
patch in the same feature band.

## Baseline commands

```powershell
dotnet --info
dotnet tool run dotnet-ef --version
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
planning. DEC-14 requires Data/Operations approval of the version,
compatibility level, benchmark hardware/topology/dataset, container licensing,
secrets, storage, and CI strategy before migration/load evidence is accepted.

## Planned solution creation after approval

No source projects are created until Gate A. The approved scaffold will:

1. Create the solution and module/test projects targeting net10.0.
2. Reference EF Core SQL Server packages at the approved 10.0 patch.
3. Configure same-origin Blazor WebAssembly assets and ASP.NET Core API.
4. Create a local-only SQL Server configuration with secrets outside Git.
5. Generate and rehearse the slice migration declared in
   `.specify/persistence-manifest.json`: S1 initial schema, then S2, S4, S6,
   and S7 incremental migrations. SPEC-004 owns the single DbContext;
   SPEC-005 governs schema/lifecycle; each declared slice owner is the sole
   writer of its migration file and shared snapshot update.
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
