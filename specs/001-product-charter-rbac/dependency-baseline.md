# SPEC-001 Dependency Baseline

**Recorded:** 2026-07-13
**Feature:** SPEC-001 Product Charter and RBAC
**Dependency role:** Root

SPEC-001 has no upstream feature dependency. It is the root product-scope and
authorization-vocabulary contract consumed by later specifications.

## Governing baseline

- Constitution: `.specify/memory/constitution.md`, version 1.1.0.
- Product plan: `docs/PROJECT_PLAN.md`, Gate A approved 2026-07-13.
- Technology: .NET 10, ASP.NET Core, Blazor WebAssembly, EF Core, LINQ, and
  SQL Server Code First.
- Architecture: one deployable modular monolith with server-authoritative
  authorization and no direct browser-to-database access.
- Human authority: Ahmed ELbamby is the sole developer and sole demo approval
  authority.

## Ownership boundary

SPEC-001 owns only the governed role vocabulary, permission definitions,
RBAC matrix, and their conformance evidence. SPEC-007 owns runtime identity,
role assignments, claims, sessions, and executable authorization policies.
SPEC-001 owns no route, endpoint, Razor component, database entity, or
migration.
