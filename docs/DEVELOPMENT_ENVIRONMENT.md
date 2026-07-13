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

Gate A is approved. Pin these during the applicable implementation slice in
repository manifests/configuration:

- SQL Server 2022 Developer container at compatibility level 160.
- xUnit for unit and integration tests.
- Testcontainers for real SQL Server concurrency tests.
- Microsoft Playwright for Blazor browser tests.
- k6 for registration-window load tests.
- NetArchTest or ArchUnitNET for module dependency tests.
- Built-in ASP.NET Core OpenAPI, health checks, rate limiting.
- OpenTelemetry SDK and an exporter selected for the deployment environment.

Docker is installed. DEC-14 approves SQL Server 2022 Developer compatibility
level 160 for local Docker/Testcontainers evidence; no production
edition/topology or licensing conclusion is implied.

## Approved solution creation

Gate A was approved by Ahmed ELbamby on 13 July 2026. The scaffold will:

1. Create the solution and module/test projects targeting net10.0.
2. Reference EF Core SQL Server packages at the approved 10.0 patch.
3. Configure same-origin Blazor WebAssembly assets and ASP.NET Core API.
4. Create a local-only SQL Server configuration with secrets outside Git.
5. Generate and rehearse the slice migration declared in
   `.specify/persistence-manifest.json`: S1 initial schema, then S2, S4, S6,
   and S7 incremental migrations. SPEC-004 owns the single DbContext;
   SPEC-005 governs schema/lifecycle; each declared slice owner is the sole
   writer of its migration file and shared snapshot update.
6. Provision `StudentRegistration_Development` or an isolated
   `StudentRegistration_Test_{runId}` only after migrations, using the
   environment-guarded seed profiles in `.specify/persistence-manifest.json`.
    Development emits generated credentials once to a restricted Git-ignored
    local artifact; Testing keeps deterministic fixture credentials in memory.
   SQL receives only ASP.NET Core Identity password hashes. Production,
    non-demo connection strings, and implicit destructive reset are rejected.
    Testing databases are disposed after each run; Development persists until
    explicit guarded reset. Git-ignored local credentials, logs, and exports
    expire within seven days.
7. Run build, architecture, unit, real-SQL integration, and E2E smoke tests.

POC browser evidence covers current stable Chrome, Edge, and Firefox plus
Playwright WebKit. Actual Safari/macOS validation is deferred and WebKit is not
reported as Safari. Secrets use .NET User Secrets or environment variables;
two-replica tests use a SQL-backed shared Data Protection key ring protected by
a generated local certificate stored outside Git.

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
