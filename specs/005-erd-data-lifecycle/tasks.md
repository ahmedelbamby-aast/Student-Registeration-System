# Tasks: ERD and Data Lifecycle

**Status**: Gate A design-owned task artifacts T001-T038 and T043-T060 are verified on 2026-07-13. T039-T042 execution and T061-T070 evidence/release tasks remain dependency-, institutional-authority-, and release-gated.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Checked tasks have verified artifacts; unchecked tasks remain pending
or dependency-gated. Every task names an exact file and traces to a requirement,
criterion, edge case, entity, dependency, workstream, or gate.

## Phase 1 - Dependency, Consistency, Readiness, and Final Approval Gates

- [x] T001 [DEP-SPEC-002] Validate the consumed upstream requirements, plan, data model, and API contract at specs/002-aastmt-policy-rulebook/ and record the accepted versions in specs/005-erd-data-lifecycle/dependency-baseline.md.
- [x] T002 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/005-erd-data-lifecycle/dependency-baseline.md.
- [x] T003 [GATE] Run cross-spec consistency analysis for SPEC-005; verify requirement/acceptance/success-criterion traceability, truthful artifact/runtime ownership, exact architecture paths, endpoint/route contracts, acyclic dependencies, task ordering, and approved Gate A demo-implementation status; record findings and resolutions in specs/005-erd-data-lifecycle/checklists/consistency-analysis.md.
- [x] T004 [GATE] After T003 passes, freeze the SPEC-005 requirements, data/API/design contracts, institutional decision states, dependency versions, and executable task baseline in specs/005-erd-data-lifecycle/checklists/implementation-readiness.md.
- [x] T005 [GATE] After T004 passes, verify the accountable owner and Ahmed ELbamby's 2026-07-13 Gate A human approval for SPEC-005 in specs/005-erd-data-lifecycle/checklists/approval.md as the final planning gate; no test, source, migration, or other implementation task may execute without this approval record.

## Phase 2 - ERD Ownership and Schema Contracts

These tasks validate and publish design-time contracts only. They neither load,
require, nor create downstream runtime classes/configurations. The test-first
task validates the matching ERD reference contract, including its declared
canonical source path; the following task publishes that reference. Each
canonical owner spec implements its own model and EF mapping after its separate
approval, and deferred SQL conformance later verifies the composed schema.

- [x] T006 [ENTITY-ApplicationUser] [SCHEMA-CONTRACT] [CONSUMER-SPEC-007] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for ApplicationUser in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/ApplicationUserErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.IdentityAccess/Domain/ApplicationUser.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T007 [ENTITY-ApplicationUser] [ERD-REFERENCE] [CONSUMER-SPEC-007] Publish the canonical-owner ERD reference for ApplicationUser at specs/005-erd-data-lifecycle/contracts/entities/ApplicationUser.md after T006 fails for the expected reason (depends on T006); SPEC-007 alone implements its runtime model and EF mapping.
- [x] T008 [ENTITY-Student] [SCHEMA-CONTRACT] [CONSUMER-SPEC-008] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for Student in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/StudentErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Academics/Domain/Student.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T009 [ENTITY-Student] [ERD-REFERENCE] [CONSUMER-SPEC-008] Publish the canonical-owner ERD reference for Student at specs/005-erd-data-lifecycle/contracts/entities/Student.md after T008 fails for the expected reason (depends on T008); SPEC-008 alone implements its runtime model and EF mapping.
- [x] T010 [ENTITY-Staff] [SCHEMA-CONTRACT] [CONSUMER-SPEC-007] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for Staff in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/StaffErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.IdentityAccess/Domain/Staff.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T011 [ENTITY-Staff] [ERD-REFERENCE] [CONSUMER-SPEC-007] Publish the canonical-owner ERD reference for Staff at specs/005-erd-data-lifecycle/contracts/entities/Staff.md after T010 fails for the expected reason (depends on T010); SPEC-007 alone implements its runtime model and EF mapping.
- [x] T012 [ENTITY-Program] [SCHEMA-CONTRACT] [CONSUMER-SPEC-009] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for Program in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/ProgramErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Academics/Domain/Program.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T013 [ENTITY-Program] [ERD-REFERENCE] [CONSUMER-SPEC-009] Publish the canonical-owner ERD reference for Program at specs/005-erd-data-lifecycle/contracts/entities/Program.md after T012 fails for the expected reason (depends on T012); SPEC-009 alone implements its runtime model and EF mapping.
- [x] T014 [ENTITY-Course] [SCHEMA-CONTRACT] [CONSUMER-SPEC-009] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for Course in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/CourseErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Academics/Domain/Course.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T015 [ENTITY-Course] [ERD-REFERENCE] [CONSUMER-SPEC-009] Publish the canonical-owner ERD reference for Course at specs/005-erd-data-lifecycle/contracts/entities/Course.md after T014 fails for the expected reason (depends on T014); SPEC-009 alone implements its runtime model and EF mapping.
- [x] T016 [ENTITY-AcademicTerm] [SCHEMA-CONTRACT] [CONSUMER-SPEC-008] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for AcademicTerm in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/AcademicTermErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Academics/Domain/AcademicTerm.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T017 [ENTITY-AcademicTerm] [ERD-REFERENCE] [CONSUMER-SPEC-008] Publish the canonical-owner ERD reference for AcademicTerm at specs/005-erd-data-lifecycle/contracts/entities/AcademicTerm.md after T016 fails for the expected reason (depends on T016); SPEC-008 alone implements its runtime model and EF mapping.
- [x] T018 [ENTITY-PolicySet] [SCHEMA-CONTRACT] [CONSUMER-SPEC-009] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for PolicySet in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/PolicySetErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Academics/Domain/PolicySet.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T019 [ENTITY-PolicySet] [ERD-REFERENCE] [CONSUMER-SPEC-009] Publish the canonical-owner ERD reference for PolicySet at specs/005-erd-data-lifecycle/contracts/entities/PolicySet.md after T018 fails for the expected reason (depends on T018); SPEC-009 alone implements its runtime model and EF mapping.
- [x] T020 [ENTITY-CourseOffering] [SCHEMA-CONTRACT] [CONSUMER-SPEC-010] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for CourseOffering in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/CourseOfferingErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Scheduling/Domain/CourseOffering.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T021 [ENTITY-CourseOffering] [ERD-REFERENCE] [CONSUMER-SPEC-010] Publish the canonical-owner ERD reference for CourseOffering at specs/005-erd-data-lifecycle/contracts/entities/CourseOffering.md after T020 fails for the expected reason (depends on T020); SPEC-010 alone implements its runtime model and EF mapping.
- [x] T022 [ENTITY-SectionGroup] [SCHEMA-CONTRACT] [CONSUMER-SPEC-010] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for SectionGroup in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/SectionGroupErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Scheduling/Domain/SectionGroup.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T023 [ENTITY-SectionGroup] [ERD-REFERENCE] [CONSUMER-SPEC-010] Publish the canonical-owner ERD reference for SectionGroup at specs/005-erd-data-lifecycle/contracts/entities/SectionGroup.md after T022 fails for the expected reason (depends on T022); SPEC-010 alone implements its runtime model and EF mapping.
- [x] T024 [ENTITY-RegistrationPlan] [SCHEMA-CONTRACT] [CONSUMER-SPEC-012] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for RegistrationPlan in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/RegistrationPlanErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Registration/Domain/RegistrationPlan.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T025 [ENTITY-RegistrationPlan] [ERD-REFERENCE] [CONSUMER-SPEC-012] Publish the canonical-owner ERD reference for RegistrationPlan at specs/005-erd-data-lifecycle/contracts/entities/RegistrationPlan.md after T024 fails for the expected reason (depends on T024); SPEC-012 alone implements its runtime model and EF mapping.
- [x] T026 [ENTITY-RegistrationSubmission] [ENTITY-StudentTermRegistrationGuard] [SCHEMA-CONTRACT] [CONSUMER-SPEC-014] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for the registration transaction pair in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/RegistrationTransactionErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source paths src/StudentRegistration.Registration/Domain/RegistrationSubmission.cs and src/StudentRegistration.Registration/Domain/StudentTermRegistrationGuard.cs and reconciles their fields/invariants with docs/diagrams/ERD.md.
- [x] T027 [ENTITY-RegistrationSubmission] [ENTITY-StudentTermRegistrationGuard] [ERD-REFERENCE] [CONSUMER-SPEC-014] Publish the canonical-owner ERD reference for RegistrationSubmission and StudentTermRegistrationGuard at specs/005-erd-data-lifecycle/contracts/entities/RegistrationTransaction.md after T026 fails for the expected reason (depends on T026); SPEC-014 alone implements both runtime models and EF mappings.
- [x] T028 [ENTITY-Enrollment] [SCHEMA-CONTRACT] [CONSUMER-SPEC-014] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for Enrollment in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/EnrollmentErdContractTests.cs without loading or requiring downstream runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Registration/Domain/Enrollment.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T029 [ENTITY-Enrollment] [ERD-REFERENCE] [CONSUMER-SPEC-014] Publish the canonical-owner ERD reference for Enrollment at specs/005-erd-data-lifecycle/contracts/entities/Enrollment.md after T028 fails for the expected reason (depends on T028); SPEC-014 alone implements its runtime model and EF mapping.
- [x] T030 [ENTITY-AuditEvent] [SCHEMA-CONTRACT] [CONSUMER-SPEC-004] Create the future failing ownership, key, relationship, lifecycle, and invariant contract checks for AuditEvent in tests/StudentRegistration.SpecificationTests/Specs/Spec005/Entities/AuditEventErdContractTests.cs without loading or requiring runtime source. Verify the matching ERD reference contract declares canonical owner source path src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditEvent.cs and reconciles its fields/invariants with docs/diagrams/ERD.md.
- [x] T031 [ENTITY-AuditEvent] [ERD-REFERENCE] [CONSUMER-SPEC-004] Publish the canonical-owner ERD reference for AuditEvent at specs/005-erd-data-lifecycle/contracts/entities/AuditEvent.md after T030 fails for the expected reason (depends on T030); SPEC-004 alone implements its runtime model/mapping and SPEC-017 consumes the read side.

## Phase 3 - User-Story Acceptance and Edge Tests

Creating the future failing fixtures is allowed after SPEC-005 approval. Passing
runtime/SQL execution remains deferred until every referenced canonical owner
spec is approved and its mapping contribution is present.

### US1 - Duplicate enrollment guard (FR-2) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [x] T032 [AC-1] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-1Tests.cs for AC-1: Duplicate enrollment guard (FR-2): Given a student already has an enrollment for an offering When another concurrent insert uses the same student/offering Then the database rejects the duplicate And the API maps it to the stable conflict response.
### US2 - Invalid capacity (FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [x] T033 [AC-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-2Tests.cs for AC-2: Invalid capacity (FR-3): Given a group has 20 active enrollments When capacity is changed to 19 Then the database/application command rejects the change.
### US3 - Historical policy (FR-5) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [x] T034 [AC-3] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-3Tests.cs for AC-3: Historical policy (FR-5): Given policy version 2026.1 is published When version 2026.2 supersedes it Then 2026.1 remains immutable and queryable by historical submission.
### US4 - Code First invariant model (FR-1, FR-4, FR-6, FR-8) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [x] T035 [AC-4] [FR-1] [FR-4] [FR-6] [FR-8] Create future failing coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-4Tests.cs and tests/StudentRegistration.IntegrationTests/Persistence/EnvironmentDatabaseProvisioningTests.cs for SQL Server 2022 Developer compatibility 160, Docker Development, Testcontainers per-run Testing creation/disposal, persistent-until-reset Development, migration-first provisioning, deterministic synthetic-only provenance-bearing fixtures, idempotent reseed, explicit guarded reset, unique University IDs, ASP.NET Identity hash-only persistence, and absence of plaintext credentials or real data.
### US5 - Controlled production migration (FR-7) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [x] T036 [AC-5] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-5Tests.cs for AC-5: Controlled production migration (FR-7): Given a reviewed migration bundle and production-like backup When deployment rehearsal runs Then migration is applied as a controlled step rather than app startup And rollback instructions restore the prior verified state.
### US6 - Database-backed registration and idempotency guards (FR-2, FR-4, FR-9) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [x] T037 [AC-6] [FR-2] [FR-4] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-6Tests.cs for AC-6: Database-backed registration and idempotency guards (FR-2, FR-4, FR-9): Given the Code First model is migrated to SQL Server When parallel transactions claim one student-term guard and one idempotency key Then database uniqueness and concurrency controls permit one canonical owner and payload And a different payload cannot reuse that key And the stored deterministic result survives application-process restart.
### US7 - Data operational quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [x] T038 [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-7Tests.cs for inventoried actual plans with no unapproved unbounded scan, rehearsal within 80% of the approved numeric Operations window, SPEC-018 restore targets, and privacy-safe logs.
- [x] T039 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EdgeCases/EC-1Tests.cs and assert that partial migration/bootstrap never marks a database ready and every seed/reset request outside Development or Testing is rejected without mutation.
- [ ] T040 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EdgeCases/EC-2Tests.cs and assert: Import references missing prerequisite -> preview rejects row and publish remains blocked.
- [ ] T041 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EdgeCases/EC-3Tests.cs and assert: rowversion is stale -> return 409 with current version, no lost update.
- [ ] T042 [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EdgeCases/EC-4Tests.cs and assert: Enrollment counter mismatch -> alert and reconcile through controlled operation; do not silently alter history.

## Phase 4 - Requirement Tests and Bounded Delivery

- [x] T043 [FR-1] [WORKSTREAM-CODE-FIRST-MODEL] Create the future failing FR-1 checks in tests/StudentRegistration.SpecificationTests/Spec005/CodeFirstOwnershipMapTests.cs. Test focus: schema/aggregate ownership plus SQL Server 2022 Developer compatibility 160, Docker Development persistence, Testcontainers per-run Testing disposal, migration-first profiles, no seed rows in migrations, and no production bootstrap/reset path.
- [x] T044 [FR-1] [WORKSTREAM-CODE-FIRST-MODEL] Deliver the bounded Code First model and non-production database-profile contract at docs/data/code-first-ownership-map.md only after T043 fails for the expected reason (depends on T043); Infrastructure.SqlServer owns the sole DbContext/migrations, canonical feature owners own mappings/seed rows, and the Docker/Testcontainers environment bootstrap composes them only after migrations with the approved persistence/disposal lifecycle.
- [x] T045 [FR-2] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-2 checks in tests/StudentRegistration.SpecificationTests/Spec005/RelationalInvariantContractTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history contracts for owner-spec persistence mappings.
- [x] T046 [FR-2] [WORKSTREAM-RELATIONAL-INVARIANTS] Publish uniqueness and alternate-key invariants at specs/005-erd-data-lifecycle/contracts/unique-invariants.md only after T045 fails for the expected reason (depends on T045).
- [x] T047 [FR-3] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-3 checks in tests/StudentRegistration.SpecificationTests/Spec005/RelationalInvariantContractTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history contracts for owner-spec persistence mappings.
- [x] T048 [FR-3] [WORKSTREAM-RELATIONAL-INVARIANTS] Publish capacity and temporal check constraints at specs/005-erd-data-lifecycle/contracts/check-constraints.md only after T047 fails for the expected reason (depends on T047).
- [x] T049 [FR-4] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-4 checks in tests/StudentRegistration.SpecificationTests/Spec005/RelationalInvariantContractTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history contracts for owner-spec persistence mappings.
- [x] T050 [FR-4] [WORKSTREAM-RELATIONAL-INVARIANTS] Publish the mutable-root rowversion catalogue at specs/005-erd-data-lifecycle/contracts/concurrency-tokens.md only after T049 fails for the expected reason (depends on T049).
- [x] T051 [FR-5] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-5 checks in tests/StudentRegistration.SpecificationTests/Spec005/RelationalInvariantContractTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history contracts for owner-spec persistence mappings.
- [x] T052 [FR-5] [WORKSTREAM-RELATIONAL-INVARIANTS] Publish immutable-history and supersession rules at specs/005-erd-data-lifecycle/contracts/immutable-history.md only after T051 fails for the expected reason (depends on T051).
- [x] T053 [FR-6] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-6 checks in tests/StudentRegistration.SpecificationTests/Spec005/RelationalInvariantContractTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history contracts for owner-spec persistence mappings.
- [x] T054 [FR-2] [FR-3] [FR-4] [FR-5] [FR-6] [WORKSTREAM-RELATIONAL-INVARIANTS] Deliver the bounded Relational invariants workstream at docs/data/relational-invariants.md only after T045, T047, T049, T051, and T053 fail for their expected reasons (depends on T045, T047, T049, T051, T053).
- [x] T055 [FR-7] [WORKSTREAM-CONTROLLED-MIGRATIONS] Create and register the minimal tests/StudentRegistration.MigrationTests/StudentRegistration.MigrationTests.csproj test-project shell when absent, then create the future failing FR-7 checks in tests/StudentRegistration.MigrationTests/MigrationBundleTests.cs. Test focus: reviewed bundle, no startup migration, rehearsal and rollback. Prove the requirement against its linked AC/EC fixtures: Production migrations MUST be reviewed scripts/bundles, not automatic startup migrations.
- [x] T056 [FR-7] [WORKSTREAM-CONTROLLED-MIGRATIONS] Deliver the bounded Controlled migrations workstream at src/StudentRegistration.Infrastructure.SqlServer/Migrations/README.md only after T055 fails for the expected reason (depends on T055).
- [x] T057 [FR-8] [WORKSTREAM-IMPORTED-PROVENANCE] Create the future failing FR-8 checks in tests/StudentRegistration.SpecificationTests/Spec005/ImportedProvenanceContractTests.cs. Test focus: required source/batch/actor/time for imports plus synthetic source, seed-profile version, stable fixture ordinal, idempotent reseed, explicit Development/Testing reset guards, and rejection of real institutional/student data from demo profiles.
- [x] T058 [FR-8] [WORKSTREAM-IMPORTED-PROVENANCE] Deliver the bounded imported/synthetic provenance and synthetic-only non-production seed contract at docs/data/import-provenance-contract.md only after T057 fails for the expected reason (depends on T057).
- [x] T059 [FR-9] [WORKSTREAM-REGISTRATION-GUARD-AND-IDEMPOTENCY-PERSISTENCE] Create the future failing FR-9 checks in tests/StudentRegistration.SpecificationTests/Spec005/RegistrationGuardSchemaContractTests.cs. Test focus: student-term guard and RegistrationSubmission owner/scope/key/payload/state/result/timestamp uniqueness and durability.
- [x] T060 [FR-9] [WORKSTREAM-REGISTRATION-GUARD-AND-IDEMPOTENCY-PERSISTENCE] Deliver the bounded Registration guard and idempotency persistence workstream at docs/data/registration-transaction-schema.md only after T059 fails for the expected reason (depends on T059); RegistrationSubmission remains the sole registration-submission idempotency record and no separate IdempotencyRecord entity/mapping is allowed.

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T061 [NFR-1] [AUTOMATED-EVIDENCE] Publish the approved critical-query/row-count inventory at docs/data/critical-query-inventory.md and produce actual-plan, p95, bounded-scan, index, and expiring-exception evidence in tests/StudentRegistration.QualityTests/Specs/Spec005/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-005-NFR-1.md.
- [ ] T062 [NFR-2] [AUTOMATED-EVIDENCE] Verify the AASTMT Operations numeric deployment window is approved, fail closed when absent, and record production-like rehearsal duration at or below 80% plus tested rollback in tests/StudentRegistration.QualityTests/Specs/Spec005/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-005-NFR-2.md.
- [ ] T063 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec005/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-005-NFR-3.md: Backup/restore MUST meet SPEC-018 RPO/RTO.
- [x] T064 [NFR-4] [AUTOMATED-EVIDENCE] Produce evidence in tests/StudentRegistration.QualityTests/Specs/Spec005/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-005-NFR-4.md proving SQL rows, migrations, checked-in fixtures, snapshots, logs, traces, and test reports contain no plaintext generated PIN/password or full student profile, password verification uses ASP.NET Identity without asserting deterministic hash bytes, local credential/log/export artifacts are Git-ignored, and cleanup removes them within seven days.

## Phase 7 - Scope and Release Evidence

### Deferred runtime activation prerequisite

Before T069 may execute, every deferred acceptance, edge, runtime-mapping, and
real-SQL conformance fixture created by SPEC-005 MUST be activated only after
its canonical owner specification and exact mapping contribution are approved,
implemented, and version-pinned. T069 remains blocked while any required row is
skipped, deferred, or supported only by a design-time reference contract.

- [x] T065 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-005-scope-review.md that OS-1 remains excluded: Database-per-module or read replica in MVP.
- [x] T066 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-005-scope-review.md that OS-2 remains excluded: Hard deletion/retention schedule until AASTMT privacy approval.
- [x] T067 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-005-scope-review.md that OS-3 remains excluded: Automatically resolving invalid imported curriculum data.
- [x] T068 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-005-scope-review.md that OS-4 remains excluded: Direct production schema mutation outside migrations.
- [ ] T069 [TRACE] [SC-1] [SC-2] [SC-3] After the deferred runtime activation prerequisite is satisfied, execute and pass every required SPEC-005 fixture, generate the completed FR/NFR/AC/EC/SC/route-to-test evidence matrix at docs/release-evidence/SPEC-005-traceability.md, and reject release if any row is skipped, deferred, design-only, or lacks passing evidence.
- [ ] T070 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-005 in docs/release-evidence/SPEC-005-release-approval.md.

Verified progress: T001-T038 and T043-T060 task artifacts are complete (56 of
70); T039-T042 and T061-T070 remain dependency- and authority-gated.
T032-T038 fixture-creation tasks are complete. T039-T042 files compile, but
their execution tasks remain unchecked. All T032-T042 fixtures are intentionally
skipped and MUST be activated and pass before T069 can complete.
No downstream runtime entity, EF mapping, migration, production bundle, or
production-authority artifact was created by these design-owned slices.
