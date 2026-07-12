# Tasks: Student Registration Records

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-015 in specs/015-student-registration-records/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/015-student-registration-records/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-008] Validate the consumed upstream requirements, plan, data model, and API contract at specs/008-academic-term-student-profile/ and record the accepted versions in specs/015-student-registration-records/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-014] Validate the consumed upstream requirements, plan, data model, and API contract at specs/014-registration-capacity-concurrency/ and record the accepted versions in specs/015-student-registration-records/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/015-student-registration-records/dependency-baseline.md.
- [ ] T006 [GATE] Freeze SPEC-015 requirements, API, data-model, policy approvals, and dependency versions in specs/015-student-registration-records/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T007 [P] [ENTITY-RegistrationReceipt] [OWNER-SPEC-015] Create the future failing invariant/schema/serialization checks for canonical RegistrationReceipt ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec015/RegistrationReceiptModelTests.cs.
- [ ] T008 [ENTITY-RegistrationReceipt] [OWNER-SPEC-015] Deliver the canonical RegistrationReceipt model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/RegistrationReceipt.cs after T007 fails for the expected reason (depends on T007).
- [ ] T009 [P] [ENTITY-RegistrationSubmission] [CONSUMER-SPEC-014] Verify SPEC-015 consumes the canonical RegistrationSubmission at src/StudentRegistration.Domain/Modules/Registration/RegistrationSubmission.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec015/RegistrationSubmissionModelTests.cs.
- [ ] T010 [P] [ENTITY-Enrollment] [CONSUMER-SPEC-014] Verify SPEC-015 consumes the canonical Enrollment at src/StudentRegistration.Domain/Modules/Registration/Enrollment.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EnrollmentModelTests.cs.
- [ ] T011 [P] [ENTITY-DecisionSnapshot] [CONSUMER-SPEC-014] Verify SPEC-015 consumes the canonical DecisionSnapshot at src/StudentRegistration.Domain/Modules/Registration/DecisionSnapshot.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec015/DecisionSnapshotModelTests.cs.
- [ ] T012 [API-Endpoint01] [OWNER-SPEC-015] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/student/registrations in specs/015-student-registration-records/contracts/api.md.
- [ ] T013 [P] [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/student/registrations in tests/StudentRegistration.ContractTests/Specs/Spec015/Endpoint01ContractTests.cs.
- [ ] T014 [API-Endpoint01] [OWNER-SPEC-015] Deliver the sole canonical GET /api/student/registrations handler at src/StudentRegistration.Server/Modules/Registration/Endpoints/Spec015Endpoints.cs after T013 fails for the expected reason (depends on T013).
- [ ] T015 [API-Endpoint02] [OWNER-SPEC-015] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/student/registrations/{submissionId} in specs/015-student-registration-records/contracts/api.md.
- [ ] T016 [P] [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/student/registrations/{submissionId} in tests/StudentRegistration.ContractTests/Specs/Spec015/Endpoint02ContractTests.cs.
- [ ] T017 [API-Endpoint02] [OWNER-SPEC-015] Deliver the sole canonical GET /api/student/registrations/{submissionId} handler at src/StudentRegistration.Server/Modules/Registration/Endpoints/Spec015Endpoints.cs after T016 fails for the expected reason (depends on T016).
- [ ] T018 [API-Endpoint03] [OWNER-SPEC-015] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/student/registrations/current/timetable in specs/015-student-registration-records/contracts/api.md.
- [ ] T019 [P] [API-Endpoint03] Verify every documented response and authorization outcome for GET /api/student/registrations/current/timetable in tests/StudentRegistration.ContractTests/Specs/Spec015/Endpoint03ContractTests.cs.
- [ ] T020 [API-Endpoint03] [OWNER-SPEC-015] Deliver the sole canonical GET /api/student/registrations/current/timetable handler at src/StudentRegistration.Server/Modules/Registration/Endpoints/Spec015Endpoints.cs after T019 fails for the expected reason (depends on T019).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Accepted receipt (FR-1, FR-2) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [ ] T021 [P] [AC-1] [FR-1] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-1Tests.cs for AC-1: Accepted receipt (FR-1, FR-2): Given a registration commits successfully When the result page loads Then it shows a unique reference and every registered group with staff, location, day/time, credits, policy version, and server timestamp.
### US2 - Atomic rejection (FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [ ] T022 [P] [AC-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-2Tests.cs for AC-2: Atomic rejection (FR-3): Given one selected group became full and transaction rolled back When the rejection is shown Then GROUP_FULL and resolution action are displayed And the message explicitly says no subjects were partially registered.
### US3 - Ownership (FR-5, NFR-2) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [ ] T023 [P] [AC-3] [FR-5] [NFR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-3Tests.cs for AC-3: Ownership (FR-5, NFR-2): Given Student A knows Student B's submission identifier When Student A requests it Then the API returns 403/404 according to security policy And no Student B data is returned.
### US4 - Historical snapshot (FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [ ] T024 [P] [AC-4] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-4Tests.cs for AC-4: Historical snapshot (FR-7): Given a room/group display name changes after registration When the original receipt is inspected Then the stored historical snapshot remains available with original details.
### US5 - Current/history views without unapproved actions (FR-4, FR-6, FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [ ] T025 [P] [AC-5] [FR-4] [FR-6] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-5Tests.cs for AC-5: Current/history views without unapproved actions (FR-4, FR-6, FR-8): Given a student has current and historical registrations When both timetable views are opened Then calendar and accessible list/table show equivalent authorized records And no drop/correction action appears before its workflow is approved.
### US6 - Registration-record quality gate (NFR-1, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Student Registration Records.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [ ] T026 [P] [AC-6] [NFR-1] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec015/AC-6Tests.cs for AC-6: Registration-record quality gate (NFR-1, NFR-3, NFR-4): Given the approved read-load dataset, accessible print/export checks, and records spanning the full retention fixture When record quality tests execute Then receipt retrieval is at most 300 ms p95 And printed/exported views meet accessibility checks with minimized PII And historical records remain durable and readable throughout the approved retention lifecycle.
- [ ] T027 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-1Tests.cs and assert: Result response lost -> idempotent lookup returns receipt.
- [ ] T028 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-2Tests.cs and assert: Receipt render service error -> safe retry by reference.
- [ ] T029 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-3Tests.cs and assert: No registrations -> show the selected term and window state and, only when registration is open, a link to STU-02 subject discovery.
- [ ] T030 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec015/EdgeCases/EC-4Tests.cs and assert: Historical term archived -> remains read-only and accessible.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T031 [P] [FR-1] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Create the future failing FR-1 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationReceiptTests.cs. Test focus: unique reference, complete accepted details, no-partial rejection and immutable historical snapshot. Prove the requirement against its linked AC/EC fixtures: Successful submission MUST produce a unique receipt/reference.
- [ ] T032 [FR-1] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Deliver FR-1 through the bounded Durable registration receipt workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationReceiptService.cs only after T031 fails for the expected reason (depends on T031): Successful submission MUST produce a unique receipt/reference.
- [ ] T033 [P] [FR-2] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Create the future failing FR-2 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationReceiptTests.cs. Test focus: unique reference, complete accepted details, no-partial rejection and immutable historical snapshot. Prove the requirement against its linked AC/EC fixtures: Receipt MUST include term, course/group, credits, Lecturer/TA, room, day/time, policy version, and submission time.
- [ ] T034 [FR-2] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Deliver FR-2 through the bounded Durable registration receipt workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationReceiptService.cs only after T033 fails for the expected reason (depends on T033): Receipt MUST include term, course/group, credits, Lecturer/TA, room, day/time, policy version, and submission time.
- [ ] T035 [P] [FR-3] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationReceiptTests.cs. Test focus: unique reference, complete accepted details, no-partial rejection and immutable historical snapshot. Prove the requirement against its linked AC/EC fixtures: A rejected atomic submission MUST state the reason and that no partial enrollment was created.
- [ ] T036 [FR-3] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Deliver FR-3 through the bounded Durable registration receipt workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationReceiptService.cs only after T035 fails for the expected reason (depends on T035): A rejected atomic submission MUST state the reason and that no partial enrollment was created.
- [ ] T037 [P] [FR-4] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Create the future failing FR-4 checks in tests/StudentRegistration.AuthorizationTests/RegistrationRecordScopeTests.cs. Test focus: self/staff scope, current/history, calendar/list/print equivalence and archived terms. Prove the requirement against its linked AC/EC fixtures: Students MUST view their current registrations/timetable and historical terms.
- [ ] T038 [FR-4] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Deliver FR-4 through the bounded Scoped current and historical records workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationRecordQueries.cs only after T037 fails for the expected reason (depends on T037): Students MUST view their current registrations/timetable and historical terms.
- [ ] T039 [P] [FR-5] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Create the future failing FR-5 checks in tests/StudentRegistration.AuthorizationTests/RegistrationRecordScopeTests.cs. Test focus: self/staff scope, current/history, calendar/list/print equivalence and archived terms. Prove the requirement against its linked AC/EC fixtures: Authorized staff/admin MAY inspect records within server-enforced scope.
- [ ] T040 [FR-5] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Deliver FR-5 through the bounded Scoped current and historical records workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationRecordQueries.cs only after T039 fails for the expected reason (depends on T039): Authorized staff/admin MAY inspect records within server-enforced scope.
- [ ] T041 [P] [FR-6] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Create the future failing FR-6 checks in tests/StudentRegistration.AuthorizationTests/RegistrationRecordScopeTests.cs. Test focus: self/staff scope, current/history, calendar/list/print equivalence and archived terms. Prove the requirement against its linked AC/EC fixtures: Calendar and printable table/list MUST present equivalent schedule data.
- [ ] T042 [FR-6] [WORKSTREAM-SCOPED-CURRENT-AND-HISTORICAL-RECORDS] Deliver FR-6 through the bounded Scoped current and historical records workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationRecordQueries.cs only after T041 fails for the expected reason (depends on T041): Calendar and printable table/list MUST present equivalent schedule data.
- [ ] T043 [P] [FR-7] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Create the future failing FR-7 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationReceiptTests.cs. Test focus: unique reference, complete accepted details, no-partial rejection and immutable historical snapshot. Prove the requirement against its linked AC/EC fixtures: Decision snapshots and historical group details MUST retain their original meaning after later edits.
- [ ] T044 [FR-7] [WORKSTREAM-DURABLE-REGISTRATION-RECEIPT] Deliver FR-7 through the bounded Durable registration receipt workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationReceiptService.cs only after T043 fails for the expected reason (depends on T043): Decision snapshots and historical group details MUST retain their original meaning after later edits.
- [ ] T045 [P] [FR-8] [WORKSTREAM-NO-UNAPPROVED-RECORD-ACTIONS] Create the future failing FR-8 checks in tests/StudentRegistration.AuthorizationTests/RegistrationRecordActionPolicyTests.cs. Test focus: drop/correction actions and endpoints remain absent. Prove the requirement against its linked AC/EC fixtures: Drop/correction actions MUST be absent until approved policy/workflow is specified.
- [ ] T046 [FR-8] [WORKSTREAM-NO-UNAPPROVED-RECORD-ACTIONS] Deliver FR-8 through the bounded No unapproved record actions workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationRecordActionPolicy.cs only after T045 fails for the expected reason (depends on T045): Drop/correction actions MUST be absent until approved policy/workflow is specified.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T047 [P] [STU-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-7] [AC-1] [AC-2] [AC-4] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-06 in tests/StudentRegistration.E2ETests/Specs/Spec015/RegistrationResultPageFeatureTests.cs.
- [ ] T048 [STU-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-7] [AC-1] [AC-2] [AC-4] Deliver the sole canonical Blazor implementation for STU-06 at src/StudentRegistration.Client/Pages/RegistrationResultPage.razor after T047 and the SPEC-003 contract/component checks fail for expected reasons (depends on T047).
- [ ] T049 [P] [STU-07] [UI-CONTRACT-SPEC-003] [FR-4] [FR-6] [FR-7] [FR-8] [AC-4] [AC-5] [AC-6] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-07 in tests/StudentRegistration.E2ETests/Specs/Spec015/RegistrationHistoryPageFeatureTests.cs.
- [ ] T050 [STU-07] [UI-CONTRACT-SPEC-003] [FR-4] [FR-6] [FR-7] [FR-8] [AC-4] [AC-5] [AC-6] Deliver the sole canonical Blazor implementation for STU-07 at src/StudentRegistration.Client/Pages/RegistrationHistoryPage.razor after T049 and the SPEC-003 contract/component checks fail for expected reasons (depends on T049).

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T051 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec015/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-015-NFR-1.md: Receipt retrieval SHOULD respond within 300 ms p95.
- [ ] T052 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec015/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-015-NFR-2.md: Record access MUST have ownership/role-scope tests.
- [ ] T053 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec015/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-015-NFR-3.md: Printed/exported views MUST be accessible and minimize PII.
- [ ] T054 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec015/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-015-NFR-4.md: Historical records MUST be durable under the approved retention plan.

## Phase 7 - Scope and Release Evidence

- [ ] T055 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-015-scope-review.md that OS-1 remains excluded: Drop/withdraw/correction workflow until AASTMT approves SPEC changes.
- [ ] T056 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-015-scope-review.md that OS-2 remains excluded: Email/SMS receipt.
- [ ] T057 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-015-scope-review.md that OS-3 remains excluded: Public/shareable receipt link.
- [ ] T058 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-015-scope-review.md that OS-4 remains excluded: Transcript replacement.
- [ ] T059 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-015-traceability.md and reject release if any row lacks passing evidence.
- [ ] T060 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-015 in docs/release-evidence/SPEC-015-release-approval.md.

No task is complete and no implementation file has been created.
