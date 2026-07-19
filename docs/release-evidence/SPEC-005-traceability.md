# SPEC-005 Completed Traceability Evidence

**Recorded:** 2026-07-19  
**Owner:** Ahmed ELbamby  
**Release scope:** non-production design-capability demo  
**Fixture status:** PASS — no SPEC-005 fixture is skipped, deferred, design-only, or missing executable evidence

## Functional requirements

| Requirement | Acceptance / edge coverage | Executable evidence | Result |
|---|---|---|---|
| FR-1 — Code First migrations, SQL Server 2022/160, migration-first isolated lifecycle | AC-4, EC-1 | `CodeFirstOwnershipMapTests`; `Spec008SqlServerTestDatabaseBootstrapperTests.Testing_database_migrates_then_seeds_is_idempotent_ready_and_disposable`; migration tests | PASS |
| FR-2 — uniqueness guards | AC-1, AC-6 | `RelationalInvariantContractTests`; `S6RegistrationMigrationTests`; `SqlSeatAllocatorConcurrencyTests`; `RegistrationIdempotencyFailureTests` | PASS |
| FR-3 — capacity and temporal checks | AC-2, EC-4 | `RelationalInvariantContractTests`; `GroupCapacityRaceTests.Capacity_below_enrollment_changes_nothing`; real-SQL counter fixture | PASS |
| FR-4 — rowversion mutable roots | AC-4, AC-6, EC-3 | `RelationalInvariantContractTests`; `RegistrationPlanModelConfigurationTests.Real_sql_round_trips_plan_values_and_enforces_unique_scope_and_offering`; SPEC-005 EC-3 | PASS |
| FR-5 — immutable historical meaning and correction chains | AC-3 | `RelationalInvariantContractTests`; AC-3 executable delivery binding; owner policy/submission tests | PASS |
| FR-6 — offering/group referential integrity | AC-4 | `RelationalInvariantContractTests`; `RegistrationPlanModelConfigurationTests.Real_sql_round_trips_plan_values_and_enforces_unique_scope_and_offering` | PASS |
| FR-7 — controlled migrations, no startup migration | AC-5, EC-1 | `MigrationBundleTests`; `Spec005MigrationRehearsalTests`; `SPEC-005-NFR-2-rehearsal.json` | PASS |
| FR-8 — provenance and synthetic-only non-production data | AC-4, EC-1, EC-2 | `ImportedProvenanceContractTests`; `Spec008SqlServerTestDatabaseBootstrapperTests`; SPEC-005 EC-2 | PASS |
| FR-9 — shared student-term boundary and sole durable submission idempotency record | AC-6 | `RegistrationGuardSchemaContractTests`; `RegistrationIdempotencyFailureTests.Accepted_and_rejected_results_replay_payload_bound_and_cross_term_is_independent` | PASS |

## Non-functional requirements

| Requirement | Executable evidence | Measured result | Result |
|---|---|---|---|
| NFR-1 — actual plans, p95, bounded scans, indexes | `NFR_1EvidenceTests`; `Spec005QueryPlanRehearsalTests`; `SPEC-005-NFR-1-plans.json` | 25k accounts/students/submissions, 75k enrollments, 100k audits; seven actual plan hashes; p95 6.251–20.611 ms; only CQ-05 uses approved expiring exception | PASS |
| NFR-2 — <=80% approved window and tested rollback | `NFR_2EvidenceTests`; `Spec005MigrationRehearsalTests`; `MigrationBundleTests`; `SPEC-005-NFR-2-rehearsal.json` | 600 s approved POC window; <=480 s gate; seven applied EF migrations; verified backup, model parity and prior-state restore | PASS |
| NFR-3 — SPEC-018 RPO/RTO | `NFR_3EvidenceTests`; `RecoveryRehearsalTests.Real_sql_backup_restore_rehearsal_meets_rpo_rto_and_integrity_gates`; `SPEC-018-NFR-7-recovery.json` | RPO 1 s <=300; RTO 2 s <=3,600; DBCC/migration/invariant checks pass | PASS |
| NFR-4 — sensitive-data minimization | `NFR_4EvidenceTests`; SQL bootstrap hash inspection; lifecycle cleanup tests | Password hashes only; no plaintext generated credential/full profile; local artifacts expire within 7 days | PASS |

## Acceptance and edge fixtures

| ID | Required observable outcome | Primary fixture | Direct runtime / measured proof | Result |
|---|---|---|---|---|
| AC-1 | Duplicate active enrollment rejected with stable conflict and no second row | `AcceptanceTests/Specs/Spec005/AC-1Tests.cs` | S6 migration uniqueness, real SQL allocator/concurrency fixture, production endpoint mapper | PASS |
| AC-2 | Capacity below active count rejected without group mutation | `AcceptanceTests/Specs/Spec005/AC-2Tests.cs` | `GroupCapacityRaceTests`; SQL conditional capacity update | PASS |
| AC-3 | Superseded policy remains historically meaningful | `AcceptanceTests/Specs/Spec005/AC-3Tests.cs` | Policy lifecycle plus durable registration decision snapshot | PASS |
| AC-4 | Migrated isolated database enforces ERD and synthetic hash-safe seed lifecycle | `AcceptanceTests/Specs/Spec005/AC-4Tests.cs` | real SQL bootstrap/disposal plus schema/model tests | PASS |
| AC-5 | Reviewed controlled migration and verified rollback | `AcceptanceTests/Specs/Spec005/AC-5Tests.cs` | measured NFR-2 script/backup/restore rehearsal | PASS |
| AC-6 | Shared serialization, payload-bound claim and restart replay | `AcceptanceTests/Specs/Spec005/AC-6Tests.cs` | real SQL `RegistrationIdempotencyFailureTests` and coordinator/store | PASS |
| AC-7 | Complete data operations release gate | `AcceptanceTests/Specs/Spec005/AC-7Tests.cs` | NFR-1 through NFR-4 executable evidence | PASS |
| EC-1 | Partial bootstrap never ready; forbidden seed/reset rejected | `IntegrationTests/Specs/Spec005/EdgeCases/EC-1Tests.cs` | real migrated-unseeded SQL database plus target guard | PASS |
| EC-2 | Missing prerequisite rejects preview and blocks publish | `IntegrationTests/Specs/Spec005/EdgeCases/EC-2Tests.cs` | production `CataloguePublicationService` state/error assertions | PASS |
| EC-3 | Stale rowversion returns HTTP 409/current version with no lost update | `IntegrationTests/Specs/Spec005/EdgeCases/EC-3Tests.cs` | two real SQL editors, persisted winner, production HTTP result mapper | PASS |
| EC-4 | Counter mismatch alerts/pauses; authorized audited repair preserves enrollment history | `IntegrationTests/Specs/Spec005/EdgeCases/EC-4Tests.cs` | real SQL fault injection, permission denial, alert/repair audits, before/after enrollment IDs | PASS |

## Success criteria and route matrix

| ID | Evidence | Result |
|---|---|---|
| SC-1 — all uniqueness, capacity, relationship and history invariants modeled | FR-2 through FR-6 contract/model/SQL evidence above | PASS |
| SC-2 — provenance retained and deterministic reseed | FR-8, AC-4, EC-2, imported-provenance and SQL bootstrap evidence | PASS |
| SC-3 — migration, backup and rollback requirements documented before schema/release | controlled migration contract, recovery runbook, NFR-2 and NFR-3 executed evidence | PASS |
| Frontend route ownership | SPEC-005 owns no route. `spec.md` explicitly delegates any later exposure to a SPEC-003 route-manifest amendment. No frontend route fixture is applicable or omitted. | PASS / NOT APPLICABLE |

## Execution record

All SPEC-005 acceptance, integration edge, specification, quality, migration,
and inherited recovery fixtures are executed in Release configuration. The
actual-plan rehearsal and migration rehearsal are explicit environment-gated
generation runs; ordinary validation consumes their immutable aggregate JSON.
No SPEC-005 test source contains `Skip =`, `NotImplementedException`, or a
deferred placeholder.

| Executed gate | Result |
|---|---|
| SPEC-005 acceptance fixtures | 12 passed, 0 failed, 0 skipped |
| SPEC-005 integration edge fixtures | 7 passed, 0 failed, 0 skipped |
| SPEC-005 specification/ERD fixtures | 22 passed, 0 failed, 0 skipped |
| SPEC-005 quality/evidence fixtures | 19 passed, 0 failed, 0 skipped |
| Controlled migration contract | 1 passed, 0 failed, 0 skipped |
| Inherited recovery objective | 1 passed, 0 failed, 0 skipped |
| Direct owner real-SQL replay/bootstrap/migration fixtures | 4 passed, 0 failed, 0 skipped |
| Production-like actual-plan generation | 1 passed, 0 failed, 0 skipped |
| Production-like migration/rollback generation | 1 passed, 0 failed, 0 skipped |

**Overall result: PASS.**
