# SPEC-004 NFR-4 Release Evidence

**Requirement:** SPEC-004/NFR-4 - business-module Domain code references no ASP.NET, Blazor, EF Core, or SQL Server type or namespace.
**Evidence date:** 2026-07-13
**Gate result:** PASS for all five business modules, with a controlled negative fixture proving that the scanner detects every prohibited framework category.

## Automated gate

`NFR-4EvidenceTests` discovers every `*.cs` file recursively beneath each
governed `Domain` root on every run. Missing Domain directories are treated as
empty module shells, while any Domain source added by a later feature is
automatically included without changing the test.

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --filter "FullyQualifiedName~Specs.Spec004.NFR_4EvidenceTests"
```

## Measured scan boundary

| Business module | Recursively scanned Domain root |
|---|---|
| `StudentRegistration.IdentityAccess` | `src/StudentRegistration.IdentityAccess/Domain` |
| `StudentRegistration.Academics` | `src/StudentRegistration.Academics/Domain` |
| `StudentRegistration.Scheduling` | `src/StudentRegistration.Scheduling/Domain` |
| `StudentRegistration.Registration` | `src/StudentRegistration.Registration/Domain` |
| `StudentRegistration.StaffAdministration` | `src/StudentRegistration.StaffAdministration/Domain` |

Measured module count: **5**. Each canonical project must exist before its
Domain root is evaluated, preventing a deleted module from producing a false
pass.

## Prohibited dependency rules

| Category | Rejected namespace/type examples |
|---|---|
| ASP.NET and Blazor | `Microsoft.AspNetCore.*`, `ComponentBase`, `RenderFragment`, `ControllerBase`, `HttpContext`, `IActionResult` |
| EF Core | `Microsoft.EntityFrameworkCore.*`, `DbContext`, `DbSet`, `IEntityTypeConfiguration`, `EntityTypeBuilder`, `UseSqlServer` |
| SQL Server | `Microsoft.Data.SqlClient.*`, `System.Data.SqlClient.*`, `SqlConnection`, `SqlCommand`, `SqlParameter`, `SqlDataReader`, `SqlTransaction` |

The controlled negative fixture injects ASP.NET/Blazor, EF Core, and SQL Server
namespaces and types together. The gate must report all three categories. A
plain C# domain value-object sample must report none, proving the validator is
both active and bounded to the declared framework dependencies.

ASP.NET composition stays in `StudentRegistration.Api`, Blazor presentation
stays in `StudentRegistration.Client`, and EF Core/SQL Server persistence stays
in `StudentRegistration.Infrastructure.SqlServer`. Business Domain code
therefore remains portable plain C# with no dependency on those delivery or
persistence frameworks.
