# Tasks: Student Registration Records

**Status**: Approved for non-production demo implementation by Ahmed ELbamby on 2026-07-13; tasks remain unstarted.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Planning Baseline and Recorded Gate A Verification

- [ ] T001 [GATE] Run and record the constitution-compliance review for SPEC-015 in specs/015-student-registration-records/checklists/approval.md; this is planning analysis and does not authorize implementation.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/015-student-registration-records/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-008] Validate the consumed upstream requirements, plan, data model, and API contract at specs/008-academic-term-student-profile/ and record the accepted versions in specs/015-student-registration-records/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-014] Validate the consumed upstream requirements, plan, data model, and API contract at specs/014-registration-capacity-concurrency/ and record the accepted versions in specs/015-student-registration-records/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/015-student-registration-records/dependency-baseline.md.
- [ ] T006 [GATE] Complete dependency validation, cross-spec consistency analysis, model/API/policy/task trace review, and verify the approved SPEC-015 baseline in specs/015-student-registration-records/checklists/implementation-readiness.md; record pass/fail and return the package to In Review if this gate fails.
- [ ] T007 [GATE] Before any later model, test, source, migration, page, or deployment task, verify Ahmed ELbamby's 2026-07-13 Gate A demo approval recorded in specs/015-student-registration-records/clarifications.md remains current; a superseding baseline change returns the package to In Review.

## Phase 2 - Models and API Contracts

- [ ] T008 [ENTITY-RegistrationReceipt] [OWNER-SPEC-015] Create future failing projection/schema tests in tests/StudentRegistration.IntegrationTests/Specs/Spec015/RegistrationReceiptModelTests.cs proving RegistrationReceiptDto reads the canonical SPEC-014 Reference/ReceiptSnapshot, creates no receipt table/write, and preserves original meaning.
- [ ] T009 [ENTITY-RegistrationReceipt] [OWNER-SPEC-015] Deliver the canonical immutable RegistrationReceipt read value at src/StudentRegistration.Registration/Domain/RegistrationReceipt.cs after T008 fails; it projects SPEC-014 persistence and is not mapped as a second EF table (depends on T008).
- [ ] T010 [ENTITY-RegistrationSubmission] [CONSUMER-SPEC-014] Verify SPEC-015 consumes the canonical RegistrationSubmission at src/StudentRegistration.Registration/Domain/RegistrationSubmission.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec015/RegistrationSubmissionModelTests.cs.
- [ ] T011 [ENTITY-Enrollment] [CONSUMER-SPEC-014] Verify SPEC-015 consumes the canonical Enrollment at src/StudentRegistration.Registration/Domain/Enrollment.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EnrollmentModelTests.cs.
- [ ] T012 [ENTITY-DecisionSnapshot] [CONSUMER-SPEC-014] Verify SPEC-015 consumes the canonical DecisionSnapshot at src/StudentRegistration.Registration/Domain/DecisionSnapshot.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec015/DecisionSnapshotModelTests.cs.
- [ ] T013 [PERSISTENCE-MAPPING] [ENTITY-RegistrationReceipt] [MIGRATION-S6Registration] Create future failing real-SQL EF mapping tests in tests/StudentRegistration.IntegrationTests/Persistence/RegistrationReceiptModelConfigurationTests.cs proving unique Reference and immutable ReceiptSnapshot persist on the canonical SPEC-014 submission mapping, project as RegistrationReceipt, and create no second receipt table/write path; also create incremental-migration/update/rollback/snapshot parity tests in tests/StudentRegistration.IntegrationTests/Persistence/S6RegistrationMigrationTests.cs.
- [ ] T014 [PERSISTENCE-MAPPING] [ENTITY-RegistrationReceipt] [MIGRATION-S6Registration] Deliver the bounded receipt projection/mapping contribution at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationReceiptModelConfiguration.cs, generate src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713060000_Registration.cs, and update src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs after T013 and SPEC-014 mapping pass; SPEC-004 remains the sole DbContext writer and SPEC-014 owns the transaction/table (depends on T013).
- [ ] T015 [API-Endpoint01] [API-Endpoint04] [OWNER-SPEC-015] Finalize bounded student-self and RegistrationRecords.Read Admin list contracts for GET /api/student/registrations and GET /api/admin/students/{studentId}/terms/{termId}/registrations in specs/015-student-registration-records/contracts/api.md, including stable sort, paging, audit, and privacy-safe scope failures.
- [ ] T016 [API-Endpoint01] [API-Endpoint04] Verify GET /api/student/registrations in tests/StudentRegistration.ContractTests/Specs/Spec015/Endpoint01ContractTests.cs and GET /api/admin/students/{studentId}/terms/{termId}/registrations in tests/StudentRegistration.ContractTests/Specs/Spec015/Endpoint04ContractTests.cs, including every page boundary, authorization, audit-metadata, and no-PII-overreach outcome.
- [ ] T017 [API-Endpoint01] [API-Endpoint04] [FR-4] [FR-5] Create future failing student/Admin list query behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec015/Endpoint01And04BehaviorTests.cs; handler delivery is deferred until all linked ownership, projection, AC, and FR tests fail for expected reasons.
- [ ] T018 [API-Endpoint02] [API-Endpoint05] [OWNER-SPEC-015] Finalize student-self and RegistrationRecords.Read Admin detail contracts for GET /api/student/registrations/{submissionId} and GET /api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId} in specs/015-student-registration-records/contracts/api.md.
- [ ] T019 [API-Endpoint02] [API-Endpoint05] Verify GET /api/student/registrations/{submissionId} in tests/StudentRegistration.ContractTests/Specs/Spec015/Endpoint02ContractTests.cs and GET /api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId} in tests/StudentRegistration.ContractTests/Specs/Spec015/Endpoint05ContractTests.cs, including mismatched-scope 404, permission denial, immutable snapshot, and inspection-audit outcomes.
- [ ] T020 [API-Endpoint02] [API-Endpoint05] [FR-1] [FR-2] [FR-5] [FR-7] Create future failing self/Admin detail query behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec015/Endpoint02And05BehaviorTests.cs; handler delivery is deferred until all linked projection, scope, AC, and FR tests fail.
- [ ] T021 [API-Endpoint03] [OWNER-SPEC-015] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/student/registrations/current/timetable in specs/015-student-registration-records/contracts/api.md.
- [ ] T022 [API-Endpoint03] Verify every documented response and authorization outcome for GET /api/student/registrations/current/timetable in tests/StudentRegistration.ContractTests/Specs/Spec015/Endpoint03ContractTests.cs.
- [ ] T023 [API-Endpoint03] [FR-4] [FR-6] Create future failing current-timetable ownership, calendar/list equivalence, archived-term, and empty-state behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec015/Endpoint03BehaviorTests.cs; handler delivery is deferred until linked AC/FR tests fail.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Accepted receipt (FR-1, FR-2) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T023.
- [ ] T024 [SC-1] [AC-1] [FR-1] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-1Tests.cs for AC-1: Accepted receipt (FR-1, FR-2): Given a registration commits successfully When the result page loads Then it shows a unique reference and every registered group with staff, location, day/time, credits, policy version, and server timestamp.
### US2 - Atomic rejection (FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T023.
- [ ] T025 [SC-2] [AC-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-2Tests.cs for AC-2: Atomic rejection (FR-3): Given one selected group became full and transaction rolled back When the rejection is shown Then GROUP_FULL and resolution action are displayed And the message explicitly says no subjects were partially registered.
### US3 - Ownership (FR-5, NFR-2) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T023.
- [ ] T026 [AC-3] [FR-5] [NFR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-3Tests.cs for AC-3: Ownership (FR-5, NFR-2): Given Student A knows Student B's submission identifier When Student A requests it Then the API returns 403/404 according to security policy And no Student B data is returned.
### US4 - Historical snapshot (FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T023.
- [ ] T027 [SC-3] [AC-4] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-4Tests.cs for AC-4: Historical snapshot (FR-7): Given a room/group display name changes after registration When the original receipt is inspected Then the stored historical snapshot remains available with original details.
### US5 - Current/history views without unapproved actions (FR-4, FR-6, FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T023.
- [ ] T028 [AC-5] [FR-4] [FR-6] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-5Tests.cs for AC-5: Current/history views without unapproved actions (FR-4, FR-6, FR-8): Given a student has current and historical registrations When both timetable views are opened Then calendar and accessible list/table show equivalent authorized records And no drop/correction action appears before its workflow is approved.
### US6 - Registration-record quality gate (NFR-1, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T023.
- [ ] T029 [AC-6] [NFR-1] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-6Tests.cs for AC-6: Registration-record quality gate (NFR-1, NFR-3, NFR-4): Given the approved read-load dataset, accessible print/export checks, and records spanning the full retention fixture When record quality tests execute Then receipt retrieval is at most 300 ms p95 And printed/exported views meet accessibility checks with minimized PII And historical records remain durable and readable throughout the approved retention lifecycle.
- [ ] T030 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-1Tests.cs and assert: Result response lost -> idempotent lookup returns receipt.
- [ ] T031 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-2Tests.cs and assert: Receipt render service error -> safe retry by reference.
- [ ] T032 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-3Tests.cs and assert: No registrations -> show the selected term and window state and, only when registration is open, a link to STU-02 subject discovery.
- [ ] T033 [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-4Tests.cs and assert: Historical term archived -> remains read-only and accessible.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T034 [FR-1] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Create the future failing FR-1 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationReceiptTests.cs. Test focus: atomic unique reference, accepted-only receipt, no-partial rejection and immutable historical snapshot. Prove the requirement against its linked AC/EC fixtures: Every accepted SPEC-014 submission MUST atomically persist one globally unique human-safe Reference and immutable ReceiptSnapshot on the canonical RegistrationSubmission; SPEC-015 MUST project that same durable record and MUST NOT create a second receipt row.
- [ ] T035 [FR-1] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Deliver FR-1 through the bounded Durable registration receipt workstream at src/StudentRegistration.Registration/Application/RegistrationReceiptService.cs only after T034 fails for the expected reason (depends on T034): Every accepted SPEC-014 submission MUST atomically persist one globally unique human-safe Reference and immutable ReceiptSnapshot on the canonical RegistrationSubmission; SPEC-015 MUST project that same durable record and MUST NOT create a second receipt row.
- [ ] T036 [FR-2] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Create the future failing FR-2 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationReceiptTests.cs. Test focus: atomic unique reference, accepted-only receipt, no-partial rejection and immutable historical snapshot. Prove the requirement against its linked AC/EC fixtures: Receipt MUST include term, course/group, credits, Lecturer/TA, room, day/time, policy version, and submission time.
- [ ] T037 [FR-2] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Deliver FR-2 through the bounded Durable registration receipt workstream at src/StudentRegistration.Registration/Application/RegistrationReceiptService.cs only after T036 fails for the expected reason (depends on T036): Receipt MUST include term, course/group, credits, Lecturer/TA, room, day/time, policy version, and submission time.
- [ ] T038 [FR-3] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationReceiptTests.cs. Test focus: atomic unique reference, accepted-only receipt, no-partial rejection and immutable historical snapshot. Prove the requirement against its linked AC/EC fixtures: A rejected atomic submission MUST state the reason and that no partial enrollment was created.
- [ ] T039 [FR-3] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Deliver FR-3 through the bounded Durable registration receipt workstream at src/StudentRegistration.Registration/Application/RegistrationReceiptService.cs only after T038 fails for the expected reason (depends on T038): A rejected atomic submission MUST state the reason and that no partial enrollment was created.
- [ ] T040 [FR-4] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Create the future failing FR-4 checks in tests/StudentRegistration.AuthorizationTests/RegistrationRecordScopeTests.cs. Test focus: student self and explicit Admin inspection scope, current/history, calendar/list/print equivalence and archived terms. Prove the requirement against its linked AC/EC fixtures: Students MUST view bounded, stable-sorted pages across current and historical registrations, with an optional TermId filter, plus the current timetable.
- [ ] T041 [FR-4] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Deliver FR-4 through the bounded Scoped current and historical records workstream at src/StudentRegistration.Registration/Application/RegistrationRecordQueries.cs only after T040 fails for the expected reason (depends on T040): Students MUST view bounded, stable-sorted pages across current and historical registrations, with an optional TermId filter, plus the current timetable.
- [ ] T042 [FR-5] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Create the future failing FR-5 checks in tests/StudentRegistration.AuthorizationTests/RegistrationRecordScopeTests.cs. Test focus: student self and explicit Admin inspection scope, current/history, calendar/list/print equivalence and archived terms. Prove the requirement against its linked AC/EC fixtures: An Admin with RegistrationRecords.Read MUST inspect a student's term-scoped list/detail only through the explicit Admin endpoints; ordinary staff have no general registration-record endpoint, and Lecturer/TA access remains limited to assigned-group roster projections in SPEC-016.
- [ ] T043 [FR-5] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Deliver FR-5 through the bounded Scoped current and historical records workstream at src/StudentRegistration.Registration/Application/RegistrationRecordQueries.cs only after T042 fails for the expected reason (depends on T042): An Admin with RegistrationRecords.Read MUST inspect a student's term-scoped list/detail only through the explicit Admin endpoints; ordinary staff have no general registration-record endpoint, and Lecturer/TA access remains limited to assigned-group roster projections in SPEC-016.
- [ ] T044 [FR-6] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Create the future failing FR-6 checks in tests/StudentRegistration.AuthorizationTests/RegistrationRecordScopeTests.cs. Test focus: student self and explicit Admin inspection scope, current/history, calendar/list/print equivalence and archived terms. Prove the requirement against its linked AC/EC fixtures: Calendar and printable table/list MUST present equivalent schedule data.
- [ ] T045 [FR-6] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Deliver FR-6 through the bounded Scoped current and historical records workstream at src/StudentRegistration.Registration/Application/RegistrationRecordQueries.cs only after T044 fails for the expected reason (depends on T044): Calendar and printable table/list MUST present equivalent schedule data.
- [ ] T046 [FR-7] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Create the future failing FR-7 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationReceiptTests.cs. Test focus: atomic unique reference, accepted-only receipt, no-partial rejection and immutable historical snapshot. Prove the requirement against its linked AC/EC fixtures: The immutable ReceiptSnapshot and DecisionSnapshot committed by SPEC-014 MUST retain original term, course/group, credits, Lecturer/TA, room, meeting, policy-version, and server-time meaning after later edits.
- [ ] T047 [FR-7] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Deliver FR-7 through the bounded Durable registration receipt workstream at src/StudentRegistration.Registration/Application/RegistrationReceiptService.cs only after T046 fails for the expected reason (depends on T046): The immutable ReceiptSnapshot and DecisionSnapshot committed by SPEC-014 MUST retain original term, course/group, credits, Lecturer/TA, room, meeting, policy-version, and server-time meaning after later edits.
- [ ] T048 [FR-8] [WORKSTREAM-NO-UNAPPROVED-RECORD-ACTIONS] Create the future failing FR-8 checks in tests/StudentRegistration.AuthorizationTests/RegistrationRecordActionPolicyTests.cs. Test focus: drop, withdrawal and correction actions and endpoints remain absent. Prove the requirement against its linked AC/EC fixtures: Drop/correction actions MUST be absent until approved policy/workflow is specified.
- [ ] T049 [FR-8] [WORKSTREAM-NO-UNAPPROVED-RECORD-ACTIONS] Deliver FR-8 through the bounded No unapproved record actions workstream at src/StudentRegistration.Registration/Application/RegistrationRecordActionPolicy.cs only after T048 fails for the expected reason (depends on T048): Drop/correction actions MUST be absent until approved policy/workflow is specified.


## Phase 5 - Frontend Route Tests and Integration

- [ ] T050 [STU-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-7] [AC-1] [AC-2] [AC-4] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-06 in tests/StudentRegistration.E2ETests/Specs/Spec015/RegistrationResultPageFeatureTests.cs.
- [ ] T051 [STU-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-7] [AC-1] [AC-2] [AC-4] Deliver the sole canonical Blazor implementation for STU-06 at src/StudentRegistration.Client/Pages/RegistrationResultPage.razor after T050 and the SPEC-003 contract/component checks fail for expected reasons (depends on T050).
- [ ] T052 [STU-07] [UI-CONTRACT-SPEC-003] [FR-4] [FR-6] [FR-7] [FR-8] [AC-4] [AC-5] [AC-6] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-07 in tests/StudentRegistration.E2ETests/Specs/Spec015/RegistrationHistoryPageFeatureTests.cs.
- [ ] T053 [STU-07] [UI-CONTRACT-SPEC-003] [FR-4] [FR-6] [FR-7] [FR-8] [AC-4] [AC-5] [AC-6] Deliver the sole canonical Blazor implementation for STU-07 at src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor after T052 and the SPEC-003 contract/component checks fail for expected reasons (depends on T052).

- [ ] T054 [API-Endpoint01] [API-Endpoint02] [API-Endpoint03] [API-Endpoint04] [API-Endpoint05] Deliver the canonical read-only handlers for GET /api/student/registrations, GET /api/student/registrations/{submissionId}, GET /api/student/registrations/current/timetable, GET /api/admin/students/{studentId}/terms/{termId}/registrations, and GET /api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId} at src/StudentRegistration.Registration/Endpoints/Spec015Endpoints.cs only after all contract, ownership, snapshot, acceptance, accessibility, action-exclusion, and E2E tests T015-T053 fail for expected reasons.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T055 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec015/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-015-NFR-1.md: Receipt retrieval SHOULD respond within 300 ms p95.
- [ ] T056 [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec015/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-015-NFR-2.md: Record access MUST have ownership/role-scope tests.
- [ ] T057 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec015/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-015-NFR-3.md: Printed/exported views MUST be accessible and minimize PII.
- [ ] T058 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec015/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-015-NFR-4.md: Historical records MUST be durable under the approved retention plan.

## Phase 7 - Scope and Release Evidence

- [ ] T059 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-015-scope-review.md that OS-1 remains excluded: Drop/withdraw/correction workflow until AASTMT approves SPEC changes.
- [ ] T060 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-015-scope-review.md that OS-2 remains excluded: Email/SMS receipt.
- [ ] T061 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-015-scope-review.md that OS-3 remains excluded: Public/shareable receipt link.
- [ ] T062 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-015-scope-review.md that OS-4 remains excluded: Transcript replacement.
- [ ] T063 [TRACE] Generate the completed FR/NFR/SC/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-015-traceability.md and reject release if any row lacks passing evidence.
- [ ] T064 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-015 in docs/release-evidence/SPEC-015-release-approval.md.

No task is complete and no implementation file has been created.
