# SPEC-005 Scope Review

**Verified:** 2026-07-16  
**Result: PASS.**

This review covers source, contracts, migrations, routes, and tests at the
current bounded MVP evidence boundary. It does not approve production
retention, topology, migration execution, or deferred catalogue runtime.

## OS-1 — Database-per-module or read replica

Remains excluded. The application uses the single
`StudentRegistrationDbContext` owned by `Infrastructure.SqlServer`;
`PersistenceBoundaryTests` rejects additional source contexts. No source
configuration or route introduces a read-replica connection or treats a cache
or replica as authoritative for registration decisions.

## OS-2 — Hard deletion or production retention schedule

Remains excluded. `immutable-history.md` requires append/supersession behavior
and states that hard deletion and retention for real or production data remain
unapproved and fail closed. The seven-day cleanup applies only to local,
Git-ignored artifacts and does not create an institutional retention policy.

## OS-3 — Automatic correction of invalid curriculum imports

Remains excluded. `import-provenance-contract.md` requires missing
prerequisites to reject preview and keep publication blocked. The relational
invariant contract rejects invalid values rather than clamping or
auto-correcting them. No route publishes invalid imported curriculum data.

## OS-4 — Direct production schema mutation outside migrations

Remains excluded. The controlled-migrations contract prohibits startup
migration and requires a reviewed, pinned migration artifact and separate
operator authority. The only source call to `MigrateAsync` is inside the
explicit, Development-only `DemoDatabaseInitializer`, which validates the
exact Development environment and database before mutation. No non-migration
source file contains direct schema DDL.

## Executable evidence

- `tests/StudentRegistration.QualityTests/Specs/Spec005/ScopeReviewEvidenceTests.cs`
- `tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs`
- `tests/StudentRegistration.SpecificationTests/Spec005/ImportedProvenanceContractTests.cs`
- `tests/StudentRegistration.SpecificationTests/Spec005/RelationalInvariantContractTests.cs`
- `src/StudentRegistration.Infrastructure.SqlServer/Migrations/README.md`

