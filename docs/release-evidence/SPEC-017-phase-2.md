# SPEC-017 Phase 2 Evidence

**Recorded:** 2026-07-17
**Phase 1 baseline:** `95aa6b70781913b302b1e592ba56082580e278a1`
**Scope:** T013-T031 only
**Result:** PASS with four intentional expected-red deliveries deferred to
T052/T056/T070/T091

## Task evidence

| Tasks | Evidence | Result |
|---|---|---|
| T013 | `AuditSourceConsumptionTests.cs` | Canonical AuditEvent, SecurityEvent, and AdminSecurityGuard assemblies/paths verified; no StaffAdministration redefinition or writer; green |
| T014 | `AuditSourceMergeContractTests.cs` | Compiles and fails only because `AuditEventQueries.cs` is deferred to T070 |
| T015 | `ImportBatchModelTests.cs` | Canonical SPEC-009 ownership and no redefinition; green |
| T016-T017 | `ExportJobModelTests.cs`, `ExportJob.cs` | Compile-safe pre-delivery reflection red recorded; final domain and real-SQL lifecycle/constraint/concurrency tests green |
| T018 | `OperationalMetricModelTests.cs` | Canonical SPEC-018 contract, observed time, and bounded dimensions; green |
| T019-T020 | `IdentityAdminDelegationTests.cs`, `identity-admin-delegation.md` | Page-to-Identity delegation, absent StaffAdministration writer/facade, and real-SQL two-replica FINAL_ADMIN_REQUIRED race; green |
| T021-T022 | mapping/migration tests, configuration, migration, snapshot | Three compile-safe expected reds before delivery; metadata, fresh/upgrade/rollback/idempotent SQL, constraints, rowversion, ownership, and snapshot parity green |
| T023-T024 | metrics API contract and Endpoint01 tests | Complete 200/400/401/403/404/429/500/503, scope/freshness/degraded behavior; green |
| T025 | Endpoint01 behavior tests | Compiles and fails only for absent T052 query and T091 handler |
| T026-T027 | audit API contract and Endpoint02 tests | Complete paging/filter/scope/redaction/order/error behavior; green |
| T028 | Endpoint02 behavior tests | Compiles and fails only for absent T070 query and T091 handler |
| T029-T030 | export lifecycle contract and Endpoint03-05 tests | Complete create/status/download, new-key terminal retry, authorization, idempotency, lease, expiry, audit, rate-limit, and safe-error behavior; green |
| T031 | export behavior tests | Compiles and fails only for absent T056 service/worker and T091 handlers |

## Red-before evidence

### T016 ExportJob

A detached temporary worktree at the Phase 1 baseline used the named
`ExportJobModelTests.cs` path with reflection so the test compiled without the
future type:

```text
dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --filter "FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec017.ExportJobModelTests" --logger "console;verbosity=normal" -m:1 -p:BuildInParallel=false
```

The project compiled, one test executed, and it failed only because
`StudentRegistration.StaffAdministration.Domain.ExportJob` was null. The
temporary worktree was then verified to be under the designated visualization
directory and removed.

### T021 mapping and migration

Before T022 delivery:

```text
dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --filter "(FullyQualifiedName~AdministrationAuditModelConfigurationTests|FullyQualifiedName~S7StaffAdminOperationsMigrationTests)&Dependency!=Docker" --no-restore
```

The project compiled and 3/3 tests failed only because ExportJob was absent
from the EF model and the exact S7 migration file did not exist.

### T014 and T025/T028/T031 deferred behavior

```text
dotnet test tests/StudentRegistration.ContractTests/StudentRegistration.ContractTests.csproj --no-build --filter "FullyQualifiedName~AuditSourceMergeContractTests" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

Result: 1/1 expected failure at the explicit missing T070 projection assertion.

```text
dotnet test tests/StudentRegistration.ApplicationTests/StudentRegistration.ApplicationTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.ApplicationTests.Specs.Spec017" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

Result: 7/7 expected failures, each at the explicit missing T052, T056, T070,
or T091 source assertion; there were no build, fixture, environment, or
unrelated failures.

## Green evidence

```text
dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --no-build --filter "(FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec017|FullyQualifiedName~AdministrationAuditModelConfigurationTests|FullyQualifiedName~S7StaffAdminOperationsMigrationTests)&Dependency!=Docker" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

Result: 16/16 passed.

```text
dotnet test tests/StudentRegistration.ContractTests/StudentRegistration.ContractTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.ContractTests.Specs.Spec017.Endpoint" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

Result: 15/15 passed.

The following focused Docker/SQL tests each passed:

- ExportJob persisted uniqueness, invalid lease/result rejection, unique
  ArtifactId, and rowversion conflict: 1/1.
- AdministrationAudit mapping and canonical upstream table ownership: 1/1.
- S7 fresh migration, S6 upgrade, rollback, idempotent replay, and snapshot
  parity: 1/1.
- Identity final-Admin two-replica serialization and audit atomicity: 1/1.

The integration test project builds with 0 warnings and 0 errors.

## Scoped architecture note

The repository-wide architecture suite currently has one pre-existing failure
in SPEC-016's `Spec016Endpoints.cs`, which references Academics.Application and
Scheduling.Domain rather than their port namespaces. A direct scan found no
such violation in any new SPEC-017 file. This phase does not edit SPEC-016 and
does not represent the repository-wide suite as green.
