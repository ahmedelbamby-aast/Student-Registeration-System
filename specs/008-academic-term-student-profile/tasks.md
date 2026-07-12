# Tasks: Academic Term and Student Profile

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-008 in specs/008-academic-term-student-profile/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-002] Validate the consumed upstream requirements, plan, data model, and API contract at specs/002-aastmt-policy-rulebook/ and record the accepted versions in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-007] Validate the consumed upstream requirements, plan, data model, and API contract at specs/007-identity-account-lifecycle/ and record the accepted versions in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/008-academic-term-student-profile/dependency-baseline.md.
- [ ] T007 [GATE] Freeze SPEC-008 requirements, API, data-model, policy approvals, and dependency versions in specs/008-academic-term-student-profile/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T008 [P] [ENTITY-AcademicTerm] [OWNER-SPEC-008] Create the future failing invariant/schema/serialization checks for canonical AcademicTerm ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec008/AcademicTermModelTests.cs.
- [ ] T009 [ENTITY-AcademicTerm] [OWNER-SPEC-008] Deliver the canonical AcademicTerm model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/AcademicTerm.cs after T008 fails for the expected reason (depends on T008).
- [ ] T010 [P] [ENTITY-RegistrationWindow] [OWNER-SPEC-008] Create the future failing invariant/schema/serialization checks for canonical RegistrationWindow ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec008/RegistrationWindowModelTests.cs.
- [ ] T011 [ENTITY-RegistrationWindow] [OWNER-SPEC-008] Deliver the canonical RegistrationWindow model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/RegistrationWindow.cs after T010 fails for the expected reason (depends on T010).
- [ ] T012 [P] [ENTITY-Student] [OWNER-SPEC-008] Create the future failing invariant/schema/serialization checks for canonical Student ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec008/StudentModelTests.cs.
- [ ] T013 [ENTITY-Student] [OWNER-SPEC-008] Deliver the canonical Student model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/Student.cs after T012 fails for the expected reason (depends on T012).
- [ ] T014 [P] [ENTITY-TranscriptAttempt] [OWNER-SPEC-008] Create the future failing invariant/schema/serialization checks for canonical TranscriptAttempt ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec008/TranscriptAttemptModelTests.cs.
- [ ] T015 [ENTITY-TranscriptAttempt] [OWNER-SPEC-008] Deliver the canonical TranscriptAttempt model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/TranscriptAttempt.cs after T014 fails for the expected reason (depends on T014).
- [ ] T016 [P] [ENTITY-StudentHold] [OWNER-SPEC-008] Create the future failing invariant/schema/serialization checks for canonical StudentHold ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec008/StudentHoldModelTests.cs.
- [ ] T017 [ENTITY-StudentHold] [OWNER-SPEC-008] Deliver the canonical StudentHold model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/StudentHold.cs after T016 fails for the expected reason (depends on T016).
- [ ] T018 [API-Endpoint01] [OWNER-SPEC-008] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/public/context in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T019 [P] [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/public/context in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint01ContractTests.cs.
- [ ] T020 [API-Endpoint01] [OWNER-SPEC-008] Deliver the sole canonical GET /api/public/context handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec008Endpoints.cs after T019 fails for the expected reason (depends on T019).
- [ ] T021 [API-Endpoint02] [OWNER-SPEC-008] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/context in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T022 [P] [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/context in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint02ContractTests.cs.
- [ ] T023 [API-Endpoint02] [OWNER-SPEC-008] Deliver the sole canonical GET /api/context handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec008Endpoints.cs after T022 fails for the expected reason (depends on T022).
- [ ] T024 [API-Endpoint03] [OWNER-SPEC-008] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/students/me/academic-context in specs/008-academic-term-student-profile/contracts/api.md.
- [ ] T025 [P] [API-Endpoint03] Verify every documented response and authorization outcome for GET /api/students/me/academic-context in tests/StudentRegistration.ContractTests/Specs/Spec008/Endpoint03ContractTests.cs.
- [ ] T026 [API-Endpoint03] [OWNER-SPEC-008] Deliver the sole canonical GET /api/students/me/academic-context handler at src/StudentRegistration.Server/Modules/Academics/Endpoints/Spec008Endpoints.cs after T025 fails for the expected reason (depends on T025).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Device-clock independence (FR-1, FR-6) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Academic Term and Student Profile.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T027 [P] [AC-1] [FR-1] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-1Tests.cs for AC-1: Device-clock independence (FR-1, FR-6): Given a device clock is one day ahead When registration-window state is requested Then server time and configured term/window determine the state.
### US2 - No active term (FR-2, FR-4) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Academic Term and Student Profile.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T028 [P] [AC-2] [FR-2] [FR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-2Tests.cs for AC-2: No active term (FR-2, FR-4): Given no registration term matches the student and server instant When the dashboard loads Then registration is read-only/unavailable And a clear no-active-window message is shown.
### US3 - Hold changes before submit (FR-5, FR-6) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Academic Term and Student Profile.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T029 [P] [AC-3] [FR-5] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-3Tests.cs for AC-3: Hold changes before submit (FR-5, FR-6): Given a plan was eligible when created And a blocking hold is added before submission When the student submits Then submission is rejected with the hold reason And no seat/enrollment changes occur.
### US4 - Governed term/profile edit (FR-3, FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Academic Term and Student Profile.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T030 [P] [AC-4] [FR-3] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-4Tests.cs for AC-4: Governed term/profile edit (FR-3, FR-7): Given Admin has permission, reason, source and current rowversion When a valid term/window or academic-profile correction is submitted Then explicit dates/state/provenance are saved and audited And a stale rowversion would be rejected.
### US5 - Hold mutation races submission (FR-6, FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Academic Term and Student Profile.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T031 [P] [AC-5] [FR-6] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-5Tests.cs for AC-5: Hold mutation races submission (FR-6, FR-8): Given an eligible student submits while an authorized admin adds a blocking hold for the same term When both transactions execute concurrently Then the operations have one valid serial order And a submission that loses the student-term serialization boundary revalidates and returns HOLD_BLOCKED without enrollment changes.
### US6 - Concurrent window publication (FR-3, FR-4, FR-9) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Academic Term and Student Profile.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T032 [P] [AC-6] [FR-3] [FR-4] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-6Tests.cs for AC-6: Concurrent window publication (FR-3, FR-4, FR-9): Given two draft windows overlap for the same term and student scope When two admins publish them concurrently Then exactly one publication may commit And the loser receives 409 STALE_VERSION or WINDOW_OVERLAP And no student matches two permitted registration contexts.
### US7 - Term and profile quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Academic Term and Student Profile.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T033 [P] [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec008/AC-7Tests.cs for AC-7: Term and profile quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given fake-clock boundary fixtures, approved dashboard read load, persistence inspection, and student/staff authorization matrix When the feature quality gate executes Then time behavior passes opening/closing boundary tests And dashboard context is at most 300 ms p95 And instants use UTC datetime2 while recurring meetings use local day/time plus IANA timezone And academic data is visible only to self or approved staff scope.
- [ ] T034 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-1Tests.cs and assert: Overlapping active windows for same scope -> publication fails.
- [ ] T035 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-2Tests.cs and assert: Missing GPA/provenance -> affected policy decision fails closed.
- [ ] T036 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-3Tests.cs and assert: Daylight/timezone rule changes -> UTC window remains unambiguous and display uses configured timezone library.
- [ ] T037 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-4Tests.cs and assert: Stale admin edit -> 409 with current rowversion.
- [ ] T038 [P] [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec008/EdgeCases/EC-5Tests.cs and assert: A scheduled window closes while a request is in flight -> the server-received timestamp governs the scheduled cutoff, while an emergency administrative closure/version change blocks every uncommitted request.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T039 [P] [FR-1] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Create the future failing FR-1 checks in tests/StudentRegistration.ApplicationTests/Academics/AcademicContextBoundaryTests.cs. Test focus: server time, teaching versus registration term, one context and submit-time re-resolution. Prove the requirement against its linked AC/EC fixtures: The server MUST expose current UTC time and configured institutional timezone, initially Africa/Cairo.
- [ ] T040 [FR-1] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Deliver FR-1 through the bounded Academic context resolution workstream at src/StudentRegistration.Server/Modules/Academics/AcademicContextResolver.cs only after T039 fails for the expected reason (depends on T039): The server MUST expose current UTC time and configured institutional timezone, initially Africa/Cairo.
- [ ] T041 [P] [FR-2] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Create the future failing FR-2 checks in tests/StudentRegistration.ApplicationTests/Academics/AcademicContextBoundaryTests.cs. Test focus: server time, teaching versus registration term, one context and submit-time re-resolution. Prove the requirement against its linked AC/EC fixtures: The system MUST distinguish teaching term from registration term.
- [ ] T042 [FR-2] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Deliver FR-2 through the bounded Academic context resolution workstream at src/StudentRegistration.Server/Modules/Academics/AcademicContextResolver.cs only after T041 fails for the expected reason (depends on T041): The system MUST distinguish teaching term from registration term.
- [ ] T043 [P] [FR-3] [WORKSTREAM-TERM-AND-WINDOW-PUBLICATION] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Academics/RegistrationWindowConcurrencyTests.cs. Test focus: versioned dates/scope, overlap rejection and concurrent publication one-winner behavior. Prove the requirement against its linked AC/EC fixtures: Terms/windows MUST have explicit dates, states, scope, and rowversion.
- [ ] T044 [FR-3] [WORKSTREAM-TERM-AND-WINDOW-PUBLICATION] Deliver FR-3 through the bounded Term and window publication workstream at src/StudentRegistration.Server/Modules/Academics/RegistrationWindowService.cs only after T043 fails for the expected reason (depends on T043): Terms/windows MUST have explicit dates, states, scope, and rowversion.
- [ ] T045 [P] [FR-4] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Create the future failing FR-4 checks in tests/StudentRegistration.ApplicationTests/Academics/AcademicContextBoundaryTests.cs. Test focus: server time, teaching versus registration term, one context and submit-time re-resolution. Prove the requirement against its linked AC/EC fixtures: At most one permitted registration context MAY match a student at an instant.
- [ ] T046 [FR-4] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Deliver FR-4 through the bounded Academic context resolution workstream at src/StudentRegistration.Server/Modules/Academics/AcademicContextResolver.cs only after T045 fails for the expected reason (depends on T045): At most one permitted registration context MAY match a student at an instant.
- [ ] T047 [P] [FR-5] [WORKSTREAM-STUDENT-ACADEMIC-PROFILE] Create the future failing FR-5 checks in tests/StudentRegistration.IntegrationTests/Academics/ProfileHoldConcurrencyTests.cs. Test focus: sourced GPA/credits/standing/transcript/holds, governed correction and submission serialization. Prove the requirement against its linked AC/EC fixtures: Student profile MUST include University ID, program/cohort, GPA, earned credits, standing, transcript summary, active holds, and provenance.
- [ ] T048 [FR-5] [WORKSTREAM-STUDENT-ACADEMIC-PROFILE] Deliver FR-5 through the bounded Student academic profile workstream at src/StudentRegistration.Server/Modules/Academics/StudentAcademicProfileService.cs only after T047 fails for the expected reason (depends on T047): Student profile MUST include University ID, program/cohort, GPA, earned credits, standing, transcript summary, active holds, and provenance.
- [ ] T049 [P] [FR-6] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Create the future failing FR-6 checks in tests/StudentRegistration.ApplicationTests/Academics/AcademicContextBoundaryTests.cs. Test focus: server time, teaching versus registration term, one context and submit-time re-resolution. Prove the requirement against its linked AC/EC fixtures: Registration commands MUST re-resolve time, term, window, student state, and holds.
- [ ] T050 [FR-6] [WORKSTREAM-ACADEMIC-CONTEXT-RESOLUTION] Deliver FR-6 through the bounded Academic context resolution workstream at src/StudentRegistration.Server/Modules/Academics/AcademicContextResolver.cs only after T049 fails for the expected reason (depends on T049): Registration commands MUST re-resolve time, term, window, student state, and holds.
- [ ] T051 [P] [FR-7] [WORKSTREAM-STUDENT-ACADEMIC-PROFILE] Create the future failing FR-7 checks in tests/StudentRegistration.IntegrationTests/Academics/ProfileHoldConcurrencyTests.cs. Test focus: sourced GPA/credits/standing/transcript/holds, governed correction and submission serialization. Prove the requirement against its linked AC/EC fixtures: Admin profile corrections MUST require authorization, reason, source, optimistic concurrency, and audit.
- [ ] T052 [FR-7] [WORKSTREAM-STUDENT-ACADEMIC-PROFILE] Deliver FR-7 through the bounded Student academic profile workstream at src/StudentRegistration.Server/Modules/Academics/StudentAcademicProfileService.cs only after T051 fails for the expected reason (depends on T051): Admin profile corrections MUST require authorization, reason, source, optimistic concurrency, and audit.
- [ ] T053 [P] [FR-8] [WORKSTREAM-STUDENT-ACADEMIC-PROFILE] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Academics/ProfileHoldConcurrencyTests.cs. Test focus: sourced GPA/credits/standing/transcript/holds, governed correction and submission serialization. Prove the requirement against its linked AC/EC fixtures: A hold/profile mutation and a registration submission for the same student/term MUST participate in one database-backed student-term serialization boundary and advance its aggregate version.
- [ ] T054 [FR-8] [WORKSTREAM-STUDENT-ACADEMIC-PROFILE] Deliver FR-8 through the bounded Student academic profile workstream at src/StudentRegistration.Server/Modules/Academics/StudentAcademicProfileService.cs only after T053 fails for the expected reason (depends on T053): A hold/profile mutation and a registration submission for the same student/term MUST participate in one database-backed student-term serialization boundary and advance its aggregate version.
- [ ] T055 [P] [FR-9] [WORKSTREAM-TERM-AND-WINDOW-PUBLICATION] Create the future failing FR-9 checks in tests/StudentRegistration.IntegrationTests/Academics/RegistrationWindowConcurrencyTests.cs. Test focus: versioned dates/scope, overlap rejection and concurrent publication one-winner behavior. Prove the requirement against its linked AC/EC fixtures: Registration-window publication MUST lock the affected term/scope in a stable order, recheck overlap inside the transaction, and reject a stale expected version.
- [ ] T056 [FR-9] [WORKSTREAM-TERM-AND-WINDOW-PUBLICATION] Deliver FR-9 through the bounded Term and window publication workstream at src/StudentRegistration.Server/Modules/Academics/RegistrationWindowService.cs only after T055 fails for the expected reason (depends on T055): Registration-window publication MUST lock the affected term/scope in a stable order, recheck overlap inside the transaction, and reject a stale expected version.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T057 [P] [AUTH-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-4] [AC-1] [AC-2] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for AUTH-01 in tests/StudentRegistration.E2ETests/Specs/Spec008/RoleGatewayPageFeatureTests.cs.
- [ ] T058 [AUTH-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-4] [AC-1] [AC-2] Deliver the sole canonical Blazor implementation for AUTH-01 at src/StudentRegistration.Client/Pages/RoleGatewayPage.razor after T057 and the SPEC-003 contract/component checks fail for expected reasons (depends on T057).
- [ ] T059 [P] [STU-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-4] [FR-5] [AC-1] [AC-2] [AC-7] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-01 in tests/StudentRegistration.E2ETests/Specs/Spec008/StudentDashboardPageFeatureTests.cs.
- [ ] T060 [STU-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-4] [FR-5] [AC-1] [AC-2] [AC-7] Deliver the sole canonical Blazor implementation for STU-01 at src/StudentRegistration.Client/Pages/StudentDashboardPage.razor after T059 and the SPEC-003 contract/component checks fail for expected reasons (depends on T059).
- [ ] T061 [P] [ADM-02] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-7] [FR-9] [AC-4] [AC-6] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-02 in tests/StudentRegistration.E2ETests/Specs/Spec008/TermAdministrationPageFeatureTests.cs.
- [ ] T062 [ADM-02] [UI-CONTRACT-SPEC-003] [FR-3] [FR-4] [FR-7] [FR-9] [AC-4] [AC-6] Deliver the sole canonical Blazor implementation for ADM-02 at src/StudentRegistration.Client/Pages/TermAdministrationPage.razor after T061 and the SPEC-003 contract/component checks fail for expected reasons (depends on T061).
- [ ] T063 [P] [ADM-04] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [AC-4] [AC-5] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-04 in tests/StudentRegistration.E2ETests/Specs/Spec008/StudentAdministrationPageFeatureTests.cs.
- [ ] T064 [ADM-04] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [AC-4] [AC-5] Deliver the sole canonical Blazor implementation for ADM-04 at src/StudentRegistration.Client/Pages/StudentAdministrationPage.razor after T063 and the SPEC-003 contract/component checks fail for expected reasons (depends on T063).

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T065 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-008-NFR-1.md: Time-dependent behavior MUST use TimeProvider and boundary tests.
- [ ] T066 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-008-NFR-2.md: Dashboard context SHOULD load within 300 ms p95 at the SPEC-018 300-read-requests-per-second target.
- [ ] T067 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-008-NFR-3.md: Instants MUST be stored in UTC datetime2; recurring class times use DayOfWeek/TimeOnly and term timezone.
- [ ] T068 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-008-NFR-4.md: Student academic data MUST be restricted to self and approved staff scopes.

## Phase 7 - Scope and Release Evidence

- [ ] T069 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-008-scope-review.md that OS-1 remains excluded: Computing official grades from assessment events.
- [ ] T070 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-008-scope-review.md that OS-2 remains excluded: Inferring a term from month/date alone.
- [ ] T071 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-008-scope-review.md that OS-3 remains excluded: Browser clock as an authority.
- [ ] T072 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-008-scope-review.md that OS-4 remains excluded: SIS synchronization mechanism until integration is specified.
- [ ] T073 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-008-traceability.md and reject release if any row lacks passing evidence.
- [ ] T074 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-008 in docs/release-evidence/SPEC-008-release-approval.md.

No task is complete and no implementation file has been created.
