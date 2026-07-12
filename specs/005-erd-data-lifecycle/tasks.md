# Tasks: ERD and Data Lifecycle

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-005 in specs/005-erd-data-lifecycle/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-002] Validate the consumed upstream requirements, plan, data model, and API contract at specs/002-aastmt-policy-rulebook/ and record the accepted versions in specs/005-erd-data-lifecycle/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/005-erd-data-lifecycle/dependency-baseline.md.
- [ ] T004 [GATE] Freeze SPEC-005 requirements, API, data-model, policy approvals, and dependency versions in specs/005-erd-data-lifecycle/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T005 [P] [ENTITY-ApplicationUser] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for ApplicationUser in tests/StudentRegistration.IntegrationTests/Specs/Spec005/ApplicationUserModelTests.cs against the canonical domain model owned by SPEC-007.
- [ ] T006 [ENTITY-ApplicationUser] [PERSISTENCE-MAPPING] Map the canonical ApplicationUser model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/ApplicationUserConfiguration.cs after T005 fails for the expected reason (depends on T005).
- [ ] T007 [P] [ENTITY-Student] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for Student in tests/StudentRegistration.IntegrationTests/Specs/Spec005/StudentModelTests.cs against the canonical domain model owned by SPEC-008.
- [ ] T008 [ENTITY-Student] [PERSISTENCE-MAPPING] Map the canonical Student model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/StudentConfiguration.cs after T007 fails for the expected reason (depends on T007).
- [ ] T009 [P] [ENTITY-Staff] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for Staff in tests/StudentRegistration.IntegrationTests/Specs/Spec005/StaffModelTests.cs against the canonical domain model owned by SPEC-007.
- [ ] T010 [ENTITY-Staff] [PERSISTENCE-MAPPING] Map the canonical Staff model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/StaffConfiguration.cs after T009 fails for the expected reason (depends on T009).
- [ ] T011 [P] [ENTITY-Program] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for Program in tests/StudentRegistration.IntegrationTests/Specs/Spec005/ProgramModelTests.cs against the canonical domain model owned by SPEC-009.
- [ ] T012 [ENTITY-Program] [PERSISTENCE-MAPPING] Map the canonical Program model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/ProgramConfiguration.cs after T011 fails for the expected reason (depends on T011).
- [ ] T013 [P] [ENTITY-Course] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for Course in tests/StudentRegistration.IntegrationTests/Specs/Spec005/CourseModelTests.cs against the canonical domain model owned by SPEC-009.
- [ ] T014 [ENTITY-Course] [PERSISTENCE-MAPPING] Map the canonical Course model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/CourseConfiguration.cs after T013 fails for the expected reason (depends on T013).
- [ ] T015 [P] [ENTITY-AcademicTerm] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for AcademicTerm in tests/StudentRegistration.IntegrationTests/Specs/Spec005/AcademicTermModelTests.cs against the canonical domain model owned by SPEC-008.
- [ ] T016 [ENTITY-AcademicTerm] [PERSISTENCE-MAPPING] Map the canonical AcademicTerm model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/AcademicTermConfiguration.cs after T015 fails for the expected reason (depends on T015).
- [ ] T017 [P] [ENTITY-PolicySet] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for PolicySet in tests/StudentRegistration.IntegrationTests/Specs/Spec005/PolicySetModelTests.cs against the canonical domain model owned by SPEC-009.
- [ ] T018 [ENTITY-PolicySet] [PERSISTENCE-MAPPING] Map the canonical PolicySet model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/PolicySetConfiguration.cs after T017 fails for the expected reason (depends on T017).
- [ ] T019 [P] [ENTITY-CourseOffering] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for CourseOffering in tests/StudentRegistration.IntegrationTests/Specs/Spec005/CourseOfferingModelTests.cs against the canonical domain model owned by SPEC-010.
- [ ] T020 [ENTITY-CourseOffering] [PERSISTENCE-MAPPING] Map the canonical CourseOffering model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/CourseOfferingConfiguration.cs after T019 fails for the expected reason (depends on T019).
- [ ] T021 [P] [ENTITY-SectionGroup] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for SectionGroup in tests/StudentRegistration.IntegrationTests/Specs/Spec005/SectionGroupModelTests.cs against the canonical domain model owned by SPEC-010.
- [ ] T022 [ENTITY-SectionGroup] [PERSISTENCE-MAPPING] Map the canonical SectionGroup model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/SectionGroupConfiguration.cs after T021 fails for the expected reason (depends on T021).
- [ ] T023 [P] [ENTITY-RegistrationPlan] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for RegistrationPlan in tests/StudentRegistration.IntegrationTests/Specs/Spec005/RegistrationPlanModelTests.cs against the canonical domain model owned by SPEC-012.
- [ ] T024 [ENTITY-RegistrationPlan] [PERSISTENCE-MAPPING] Map the canonical RegistrationPlan model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/RegistrationPlanConfiguration.cs after T023 fails for the expected reason (depends on T023).
- [ ] T025 [P] [ENTITY-RegistrationSubmission] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for RegistrationSubmission in tests/StudentRegistration.IntegrationTests/Specs/Spec005/RegistrationSubmissionModelTests.cs against the canonical domain model owned by SPEC-014.
- [ ] T026 [ENTITY-RegistrationSubmission] [PERSISTENCE-MAPPING] Map the canonical RegistrationSubmission model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/RegistrationSubmissionConfiguration.cs after T025 fails for the expected reason (depends on T025).
- [ ] T027 [P] [ENTITY-Enrollment] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for Enrollment in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EnrollmentModelTests.cs against the canonical domain model owned by SPEC-014.
- [ ] T028 [ENTITY-Enrollment] [PERSISTENCE-MAPPING] Map the canonical Enrollment model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/EnrollmentConfiguration.cs after T027 fails for the expected reason (depends on T027).
- [ ] T029 [P] [ENTITY-AuditEvent] [PERSISTENCE-MAPPING] Create the future failing SQL mapping/constraint test for AuditEvent in tests/StudentRegistration.IntegrationTests/Specs/Spec005/AuditEventModelTests.cs against the canonical domain model owned by SPEC-017.
- [ ] T030 [ENTITY-AuditEvent] [PERSISTENCE-MAPPING] Map the canonical AuditEvent model without redefining it at src/StudentRegistration.Infrastructure/Persistence/Configurations/AuditEventConfiguration.cs after T029 fails for the expected reason (depends on T029).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Duplicate enrollment guard (FR-2) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T030.
- [ ] T031 [P] [AC-1] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-1Tests.cs for AC-1: Duplicate enrollment guard (FR-2): Given a student already has an enrollment for an offering When another concurrent insert uses the same student/offering Then the database rejects the duplicate And the API maps it to the stable conflict response.
### US2 - Invalid capacity (FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T030.
- [ ] T032 [P] [AC-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-2Tests.cs for AC-2: Invalid capacity (FR-3): Given a group has 20 active enrollments When capacity is changed to 19 Then the database/application command rejects the change.
### US3 - Historical policy (FR-5) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T030.
- [ ] T033 [P] [AC-3] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-3Tests.cs for AC-3: Historical policy (FR-5): Given policy version 2026.1 is published When version 2026.2 supersedes it Then 2026.1 remains immutable and queryable by historical submission.
### US4 - Code First invariant model (FR-1, FR-4, FR-6, FR-8) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T030.
- [ ] T034 [P] [AC-4] [FR-1] [FR-4] [FR-6] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-4Tests.cs for AC-4: Code First invariant model (FR-1, FR-4, FR-6, FR-8): Given the approved Code First model is migrated to an empty SQL Server When schema inspection and seed import tests run Then rowversion and offering/group referential constraints match the ERD And imported academic rows retain source provenance.
### US5 - Controlled production migration (FR-7) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T030.
- [ ] T035 [P] [AC-5] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-5Tests.cs for AC-5: Controlled production migration (FR-7): Given a reviewed migration bundle and production-like backup When deployment rehearsal runs Then migration is applied as a controlled step rather than app startup And rollback instructions restore the prior verified state.
### US6 - Database-backed registration and idempotency guards (FR-2, FR-4, FR-9) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T030.
- [ ] T036 [P] [AC-6] [FR-2] [FR-4] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-6Tests.cs for AC-6: Database-backed registration and idempotency guards (FR-2, FR-4, FR-9): Given the Code First model is migrated to SQL Server When parallel transactions claim one student-term guard and one idempotency key Then database uniqueness and concurrency controls permit one canonical owner and payload And a different payload cannot reuse that key And the stored deterministic result survives application-process restart.
### US7 - Data operational quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of ERD and Data Lifecycle.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T030.
- [ ] T037 [P] [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec005/AC-7Tests.cs for AC-7: Data operational quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given a production-like database, reviewed migration bundle, backup, critical query plans, and privacy-safe logging fixture When the data release gate executes Then critical tables above 10,000 rows have reviewed indexed plans And migration rehearsal completes inside the approved deployment window And restore meets SPEC-018 RPO/RTO And sensitive fields are absent from unsafe logs.
- [ ] T038 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EdgeCases/EC-1Tests.cs and assert: Migration fails partway -> deployment stops and follows tested rollback.
- [ ] T039 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EdgeCases/EC-2Tests.cs and assert: Import references missing prerequisite -> preview rejects row and publish remains blocked.
- [ ] T040 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EdgeCases/EC-3Tests.cs and assert: rowversion is stale -> return 409 with current version, no lost update.
- [ ] T041 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec005/EdgeCases/EC-4Tests.cs and assert: Enrollment counter mismatch -> alert and reconcile through controlled operation; do not silently alter history.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T042 [P] [FR-1] [WORKSTREAM-CODE-FIRST-MODEL] Create the future failing FR-1 checks in tests/StudentRegistration.IntegrationTests/Persistence/CodeFirstModelTests.cs. Test focus: empty SQL Server migration matches the approved ERD. Prove the requirement against its linked AC/EC fixtures: EF Core Code First migrations MUST define the approved ERD.
- [ ] T043 [FR-1] [WORKSTREAM-CODE-FIRST-MODEL] Deliver FR-1 through the bounded Code First model workstream at src/StudentRegistration.Infrastructure/Persistence/StudentRegistrationDbContext.cs only after T042 fails for the expected reason (depends on T042): EF Core Code First migrations MUST define the approved ERD.
- [ ] T044 [P] [FR-2] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-2 checks in tests/StudentRegistration.IntegrationTests/Persistence/RelationalInvariantTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history constraints on real SQL Server. Prove the requirement against its linked AC/EC fixtures: Student University ID, course/program/term codes, group codes, active enrollment, and submission idempotency MUST have database uniqueness guards.
- [ ] T045 [FR-2] [WORKSTREAM-RELATIONAL-INVARIANTS] Deliver FR-2 through the bounded Relational invariants workstream at src/StudentRegistration.Infrastructure/Persistence/Configurations/ModelInvariantConfiguration.cs only after T044 fails for the expected reason (depends on T044): Student University ID, course/program/term codes, group codes, active enrollment, and submission idempotency MUST have database uniqueness guards.
- [ ] T046 [P] [FR-3] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Persistence/RelationalInvariantTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history constraints on real SQL Server. Prove the requirement against its linked AC/EC fixtures: Capacity and time/date bounds MUST have database check constraints.
- [ ] T047 [FR-3] [WORKSTREAM-RELATIONAL-INVARIANTS] Deliver FR-3 through the bounded Relational invariants workstream at src/StudentRegistration.Infrastructure/Persistence/Configurations/ModelInvariantConfiguration.cs only after T046 fails for the expected reason (depends on T046): Capacity and time/date bounds MUST have database check constraints.
- [ ] T048 [P] [FR-4] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-4 checks in tests/StudentRegistration.IntegrationTests/Persistence/RelationalInvariantTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history constraints on real SQL Server. Prove the requirement against its linked AC/EC fixtures: Mutable aggregate roots MUST use SQL Server rowversion where specified.
- [ ] T049 [FR-4] [WORKSTREAM-RELATIONAL-INVARIANTS] Deliver FR-4 through the bounded Relational invariants workstream at src/StudentRegistration.Infrastructure/Persistence/Configurations/ModelInvariantConfiguration.cs only after T048 fails for the expected reason (depends on T048): Mutable aggregate roots MUST use SQL Server rowversion where specified.
- [ ] T050 [P] [FR-5] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-5 checks in tests/StudentRegistration.IntegrationTests/Persistence/RelationalInvariantTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history constraints on real SQL Server. Prove the requirement against its linked AC/EC fixtures: Transcript attempts, published policies, decision snapshots, and audit events MUST preserve historical meaning.
- [ ] T051 [FR-5] [WORKSTREAM-RELATIONAL-INVARIANTS] Deliver FR-5 through the bounded Relational invariants workstream at src/StudentRegistration.Infrastructure/Persistence/Configurations/ModelInvariantConfiguration.cs only after T050 fails for the expected reason (depends on T050): Transcript attempts, published policies, decision snapshots, and audit events MUST preserve historical meaning.
- [ ] T052 [P] [FR-6] [WORKSTREAM-RELATIONAL-INVARIANTS] Create the future failing FR-6 checks in tests/StudentRegistration.IntegrationTests/Persistence/RelationalInvariantTests.cs. Test focus: unique, check, alternate-key, foreign-key, rowversion and history constraints on real SQL Server. Prove the requirement against its linked AC/EC fixtures: Enrollment/group references MUST guarantee the group belongs to the selected offering.
- [ ] T053 [FR-6] [WORKSTREAM-RELATIONAL-INVARIANTS] Deliver FR-6 through the bounded Relational invariants workstream at src/StudentRegistration.Infrastructure/Persistence/Configurations/ModelInvariantConfiguration.cs only after T052 fails for the expected reason (depends on T052): Enrollment/group references MUST guarantee the group belongs to the selected offering.
- [ ] T054 [P] [FR-7] [WORKSTREAM-CONTROLLED-MIGRATIONS] Create the future failing FR-7 checks in tests/StudentRegistration.MigrationTests/MigrationBundleTests.cs. Test focus: reviewed bundle, no startup migration, rehearsal and rollback. Prove the requirement against its linked AC/EC fixtures: Production migrations MUST be reviewed scripts/bundles, not automatic startup migrations.
- [ ] T055 [FR-7] [WORKSTREAM-CONTROLLED-MIGRATIONS] Deliver FR-7 through the bounded Controlled migrations workstream at src/StudentRegistration.Infrastructure/Migrations/README.md only after T054 fails for the expected reason (depends on T054): Production migrations MUST be reviewed scripts/bundles, not automatic startup migrations.
- [ ] T056 [P] [FR-8] [WORKSTREAM-IMPORTED-PROVENANCE] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Persistence/ImportedProvenanceTests.cs. Test focus: required source, batch, actor and timestamp survive import and query. Prove the requirement against its linked AC/EC fixtures: Data provenance MUST be recorded for imported academic/catalogue data.
- [ ] T057 [FR-8] [WORKSTREAM-IMPORTED-PROVENANCE] Deliver FR-8 through the bounded Imported provenance workstream at src/StudentRegistration.Infrastructure/Persistence/Configurations/ImportedRecordConfiguration.cs only after T056 fails for the expected reason (depends on T056): Data provenance MUST be recorded for imported academic/catalogue data.
- [ ] T058 [P] [FR-9] [WORKSTREAM-REGISTRATION-GUARD-AND-IDEMPOTENCY-PERSISTENCE] Create the future failing FR-9 checks in tests/StudentRegistration.IntegrationTests/Persistence/RegistrationGuardSchemaTests.cs. Test focus: student-term guard and owner/scope/key/payload/state/result/timestamp uniqueness and durability. Prove the requirement against its linked AC/EC fixtures: The ERD MUST model a unique student-term registration guard and an idempotency record containing owner/scope, canonical payload hash, processing state, immutable deterministic result, created/updated/completed timestamps, and uniqueness on owner/scope/key.
- [ ] T059 [FR-9] [WORKSTREAM-REGISTRATION-GUARD-AND-IDEMPOTENCY-PERSISTENCE] Deliver FR-9 through the bounded Registration guard and idempotency persistence workstream at src/StudentRegistration.Infrastructure/Persistence/Configurations/RegistrationSubmissionConfiguration.cs only after T058 fails for the expected reason (depends on T058): The ERD MUST model a unique student-term registration guard and an idempotency record containing owner/scope, canonical payload hash, processing state, immutable deterministic result, created/updated/completed timestamps, and uniqueness on owner/scope/key.

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T060 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec005/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-005-NFR-1.md: No query on a table expected above 10,000 rows MAY rely on an unreviewed full scan in a critical path.
- [ ] T061 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec005/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-005-NFR-2.md: A production-like migration rehearsal MUST complete inside the approved deployment window with rollback instructions.
- [ ] T062 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec005/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-005-NFR-3.md: Backup/restore MUST meet SPEC-018 RPO/RTO.
- [ ] T063 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec005/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-005-NFR-4.md: Sensitive fields MUST be minimized and excluded from unsafe logs.

## Phase 7 - Scope and Release Evidence

- [ ] T064 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-005-scope-review.md that OS-1 remains excluded: Database-per-module or read replica in MVP.
- [ ] T065 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-005-scope-review.md that OS-2 remains excluded: Hard deletion/retention schedule until AASTMT privacy approval.
- [ ] T066 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-005-scope-review.md that OS-3 remains excluded: Automatically resolving invalid imported curriculum data.
- [ ] T067 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-005-scope-review.md that OS-4 remains excluded: Direct production schema mutation outside migrations.
- [ ] T068 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-005-traceability.md and reject release if any row lacks passing evidence.
- [ ] T069 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-005 in docs/release-evidence/SPEC-005-release-approval.md.

No task is complete and no implementation file has been created.
