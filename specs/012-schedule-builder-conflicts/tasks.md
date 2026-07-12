# Tasks: Schedule Builder and Conflicts

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-012 in specs/012-schedule-builder-conflicts/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/012-schedule-builder-conflicts/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-010] Validate the consumed upstream requirements, plan, data model, and API contract at specs/010-offerings-groups-resources/ and record the accepted versions in specs/012-schedule-builder-conflicts/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-011] Validate the consumed upstream requirements, plan, data model, and API contract at specs/011-eligibility-subject-discovery/ and record the accepted versions in specs/012-schedule-builder-conflicts/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/012-schedule-builder-conflicts/dependency-baseline.md.
- [ ] T006 [GATE] Freeze SPEC-012 requirements, API, data-model, policy approvals, and dependency versions in specs/012-schedule-builder-conflicts/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T007 [P] [ENTITY-RegistrationPlan] [OWNER-SPEC-012] Create the future failing invariant/schema/serialization checks for canonical RegistrationPlan ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelTests.cs.
- [ ] T008 [ENTITY-RegistrationPlan] [OWNER-SPEC-012] Deliver the canonical RegistrationPlan model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/RegistrationPlan.cs after T007 fails for the expected reason (depends on T007).
- [ ] T009 [P] [ENTITY-RegistrationPlanItem] [OWNER-SPEC-012] Create the future failing invariant/schema/serialization checks for canonical RegistrationPlanItem ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanItemModelTests.cs.
- [ ] T010 [ENTITY-RegistrationPlanItem] [OWNER-SPEC-012] Deliver the canonical RegistrationPlanItem model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/RegistrationPlanItem.cs after T009 fails for the expected reason (depends on T009).
- [ ] T011 [P] [ENTITY-ScheduleConflict] [OWNER-SPEC-012] Create the future failing invariant/schema/serialization checks for canonical ScheduleConflict ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec012/ScheduleConflictModelTests.cs.
- [ ] T012 [ENTITY-ScheduleConflict] [OWNER-SPEC-012] Deliver the canonical ScheduleConflict model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/ScheduleConflict.cs after T011 fails for the expected reason (depends on T011).
- [ ] T013 [P] [ENTITY-ValidationSnapshot] [OWNER-SPEC-012] Create the future failing invariant/schema/serialization checks for canonical ValidationSnapshot ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec012/ValidationSnapshotModelTests.cs.
- [ ] T014 [ENTITY-ValidationSnapshot] [OWNER-SPEC-012] Deliver the canonical ValidationSnapshot model or governed artifact at src/StudentRegistration.Domain/Modules/Registration/ValidationSnapshot.cs after T013 fails for the expected reason (depends on T013).
- [ ] T015 [API-Endpoint01] [OWNER-SPEC-012] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for PUT /api/student/registration-plans/{id} in specs/012-schedule-builder-conflicts/contracts/api.md.
- [ ] T016 [P] [API-Endpoint01] Verify every documented response and authorization outcome for PUT /api/student/registration-plans/{id} in tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint01ContractTests.cs.
- [ ] T017 [API-Endpoint01] [OWNER-SPEC-012] Deliver the sole canonical PUT /api/student/registration-plans/{id} handler at src/StudentRegistration.Server/Modules/Registration/Endpoints/Spec012Endpoints.cs after T016 fails for the expected reason (depends on T016).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Overlap detection (FR-2, FR-3, FR-4) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Schedule Builder and Conflicts.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T017.
- [ ] T018 [P] [AC-1] [FR-2] [FR-3] [FR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-1Tests.cs for AC-1: Overlap detection (FR-2, FR-3, FR-4): Given Group A meets Monday 10:00-11:30 and Group B 11:00-12:00 When both are selected Then a conflict is returned and displayed with red X, Conflict text, both subjects, and 11:00-11:30 overlap.
### US2 - Adjacent meetings (FR-2) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Schedule Builder and Conflicts.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T017.
- [ ] T019 [P] [AC-2] [FR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-2Tests.cs for AC-2: Adjacent meetings (FR-2): Given Group A ends Monday 11:00 and Group B starts Monday 11:00 When both are selected Then no overlap exists unless an approved travel-buffer rule applies.
### US3 - Submission blocked (FR-5) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Schedule Builder and Conflicts.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T017.
- [ ] T020 [P] [AC-3] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-3Tests.cs for AC-3: Submission blocked (FR-5): Given an unresolved hard conflict When the student opens Review Then submission is disabled with a visible reason and resolution links.
### US4 - Versioned plan editing (FR-1, FR-6, FR-7, FR-8) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Schedule Builder and Conflicts.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T017.
- [ ] T021 [P] [AC-4] [FR-1] [FR-6] [FR-7] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-4Tests.cs for AC-4: Versioned plan editing (FR-1, FR-6, FR-7, FR-8): Given a current plan and advisory group capacity When the student selects a second group for one offering or saves with a stale rowversion Then the invalid/stale update is rejected And the student can load the current plan and change/remove a group.
### US5 - Plan quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Schedule Builder and Conflicts.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T017.
- [ ] T022 [P] [AC-5] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-5Tests.cs for AC-5: Plan quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given an eight-course/ten-groups-per-course plan, fixed meeting data, equivalent calendar/list fixtures, and two concurrent editors When schedule performance, determinism, equivalence, and rowversion tests run Then recalculation is at most 200 ms p95 And conflict output is deterministic And calendar/list content is identical And one stale editor receives 409 without a lost update.
- [ ] T023 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-1Tests.cs and assert: Multi-slot group conflicts on only one day -> group is still hard conflict.
- [ ] T024 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-2Tests.cs and assert: Stale plan version -> reject update and return current plan.
- [ ] T025 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-3Tests.cs and assert: Selected group unpublished/full -> mark stale/unavailable and require change; no silent replacement.
- [ ] T026 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-4Tests.cs and assert: Same meeting slot duplicate data -> publication validation prevents it; builder de-duplicates defensively.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T027 [P] [FR-1] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Create the future failing FR-1 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs. Test focus: one group per offering, add/change/remove, rowversion lost-update rejection and advisory capacity. Prove the requirement against its linked AC/EC fixtures: The student MUST add at most one group per course offering to a plan.
- [ ] T028 [FR-1] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Deliver FR-1 through the bounded Versioned registration plan workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationPlanService.cs only after T027 fails for the expected reason (depends on T027): The student MUST add at most one group per course offering to a plan.
- [ ] T029 [P] [FR-2] [WORKSTREAM-INTERVAL-CONFLICT-DETECTOR] Create the future failing FR-2 checks in tests/StudentRegistration.DomainTests/Registration/ScheduleConflictDetectorTests.cs. Test focus: strict overlap, adjacency, multi-slot conflicts and complete reason intervals. Prove the requirement against its linked AC/EC fixtures: The server MUST detect overlap for every meeting slot using strict interval logic.
- [ ] T030 [FR-2] [WORKSTREAM-INTERVAL-CONFLICT-DETECTOR] Deliver FR-2 through the bounded Interval conflict detector workstream at src/StudentRegistration.Domain/Modules/Registration/ScheduleConflictDetector.cs only after T029 fails for the expected reason (depends on T029): The server MUST detect overlap for every meeting slot using strict interval logic.
- [ ] T031 [P] [FR-3] [WORKSTREAM-INTERVAL-CONFLICT-DETECTOR] Create the future failing FR-3 checks in tests/StudentRegistration.DomainTests/Registration/ScheduleConflictDetectorTests.cs. Test focus: strict overlap, adjacency, multi-slot conflicts and complete reason intervals. Prove the requirement against its linked AC/EC fixtures: Each conflict MUST identify both groups, subjects, day, times, and resolution links.
- [ ] T032 [FR-3] [WORKSTREAM-INTERVAL-CONFLICT-DETECTOR] Deliver FR-3 through the bounded Interval conflict detector workstream at src/StudentRegistration.Domain/Modules/Registration/ScheduleConflictDetector.cs only after T031 fails for the expected reason (depends on T031): Each conflict MUST identify both groups, subjects, day, times, and resolution links.
- [ ] T033 [P] [FR-4] [WORKSTREAM-ACCESSIBLE-BLOCKING-CONFLICT-STATE] Create the future failing FR-4 checks in tests/StudentRegistration.Client.UnitTests/Scheduling/ConflictStateMapperTests.cs. Test focus: red-X plus text, blocked review/submit and accessible reasons/actions. Prove the requirement against its linked AC/EC fixtures: The UI MUST render a red X plus text/icon-accessible conflict state.
- [ ] T034 [FR-4] [WORKSTREAM-ACCESSIBLE-BLOCKING-CONFLICT-STATE] Deliver FR-4 through the bounded Accessible blocking conflict state workstream at src/StudentRegistration.Client/Features/Scheduling/ConflictStateMapper.cs only after T033 fails for the expected reason (depends on T033): The UI MUST render a red X plus text/icon-accessible conflict state.
- [ ] T035 [P] [FR-5] [WORKSTREAM-ACCESSIBLE-BLOCKING-CONFLICT-STATE] Create the future failing FR-5 checks in tests/StudentRegistration.Client.UnitTests/Scheduling/ConflictStateMapperTests.cs. Test focus: red-X plus text, blocked review/submit and accessible reasons/actions. Prove the requirement against its linked AC/EC fixtures: Review/submission MUST be blocked while any hard conflict exists.
- [ ] T036 [FR-5] [WORKSTREAM-ACCESSIBLE-BLOCKING-CONFLICT-STATE] Deliver FR-5 through the bounded Accessible blocking conflict state workstream at src/StudentRegistration.Client/Features/Scheduling/ConflictStateMapper.cs only after T035 fails for the expected reason (depends on T035): Review/submission MUST be blocked while any hard conflict exists.
- [ ] T037 [P] [FR-6] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Create the future failing FR-6 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs. Test focus: one group per offering, add/change/remove, rowversion lost-update rejection and advisory capacity. Prove the requirement against its linked AC/EC fixtures: Students MUST be able to change/remove groups and see recalculated credits/conflicts.
- [ ] T038 [FR-6] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Deliver FR-6 through the bounded Versioned registration plan workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationPlanService.cs only after T037 fails for the expected reason (depends on T037): Students MUST be able to change/remove groups and see recalculated credits/conflicts.
- [ ] T039 [P] [FR-7] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Create the future failing FR-7 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs. Test focus: one group per offering, add/change/remove, rowversion lost-update rejection and advisory capacity. Prove the requirement against its linked AC/EC fixtures: Plans MUST persist server-side and use rowversion.
- [ ] T040 [FR-7] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Deliver FR-7 through the bounded Versioned registration plan workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationPlanService.cs only after T039 fails for the expected reason (depends on T039): Plans MUST persist server-side and use rowversion.
- [ ] T041 [P] [FR-8] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs. Test focus: one group per offering, add/change/remove, rowversion lost-update rejection and advisory capacity. Prove the requirement against its linked AC/EC fixtures: The client MUST treat capacity displayed in a plan as advisory until final submission revalidates it.
- [ ] T042 [FR-8] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Deliver FR-8 through the bounded Versioned registration plan workstream at src/StudentRegistration.Server/Modules/Registration/RegistrationPlanService.cs only after T041 fails for the expected reason (depends on T041): The client MUST treat capacity displayed in a plan as advisory until final submission revalidates it.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T043 [P] [STU-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-6] [FR-7] [FR-8] [AC-1] [AC-2] [AC-4] [AC-5] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STU-04 in tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs.
- [ ] T044 [STU-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-6] [FR-7] [FR-8] [AC-1] [AC-2] [AC-4] [AC-5] Deliver the sole canonical Blazor implementation for STU-04 at src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor after T043 and the SPEC-003 contract/component checks fail for expected reasons (depends on T043).
- [ ] T045 [STU-05] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [AC-3] [AC-4] [AC-5] Finalize SPEC-012 data, actions, stable reasons, authorization, and stale/concurrent contribution for STU-05 at specs/012-schedule-builder-conflicts/contracts/routes/STU-05.md without editing the canonical Razor page.
- [ ] T046 [P] [STU-05] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [AC-3] [AC-4] [AC-5] Verify the SPEC-012 contribution consumed by STU-05 in tests/StudentRegistration.E2ETests/Specs/Spec012/RegistrationReviewPageContributorTests.cs.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T047 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-012-NFR-1.md: Conflict recalculation SHOULD complete within 200 ms p95 for 8 courses with 10 meeting slots each.
- [ ] T048 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-012-NFR-2.md: Conflict results MUST be deterministic.
- [ ] T049 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-012-NFR-3.md: Calendar and chronological list MUST contain equivalent content.
- [ ] T050 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-012-NFR-4.md: Plan editing MUST reject lost updates with 409.

## Phase 7 - Scope and Release Evidence

- [ ] T051 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-012-scope-review.md that OS-1 remains excluded: Creating/moving official staff or room schedules.
- [ ] T052 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-012-scope-review.md that OS-2 remains excluded: Automatic academic conflict override.
- [ ] T053 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-012-scope-review.md that OS-3 remains excluded: Final seat reservation while editing.
- [ ] T054 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-012-scope-review.md that OS-4 remains excluded: Optimized alternatives, covered by SPEC-013.
- [ ] T055 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-012-traceability.md and reject release if any row lacks passing evidence.
- [ ] T056 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-012 in docs/release-evidence/SPEC-012-release-approval.md.

No task is complete and no implementation file has been created.
