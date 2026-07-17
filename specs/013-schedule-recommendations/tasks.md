# Tasks: Schedule Recommendations

**Status**: Completed for the approved non-production demo scope; all 66 tasks have passing named evidence.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: A task is checked only after its named evidence exists and passes verification.

## Phase 1 - Planning Baseline and Recorded Gate A Verification

- [x] T001 [GATE] Run and record the constitution-compliance review for SPEC-013 in specs/013-schedule-recommendations/checklists/approval.md; this is planning analysis and does not authorize implementation.
- [x] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/013-schedule-recommendations/dependency-baseline.md.
- [x] T003 [DEP-SPEC-010] [DEP-SPEC-011] [DEP-SPEC-012] Validate the consumed Scheduling availability/group contract at specs/010-offerings-groups-resources/, eligibility/discovery contract at specs/011-eligibility-subject-discovery/, and versioned plan/conflict contract at specs/012-schedule-builder-conflicts/; record all accepted versions in specs/013-schedule-recommendations/dependency-baseline.md.
- [x] T004 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/013-schedule-recommendations/dependency-baseline.md.
- [x] T005 [GATE] Complete dependency validation, cross-spec consistency analysis, model/API/policy/task trace review, and verify the approved SPEC-013 baseline in specs/013-schedule-recommendations/checklists/implementation-readiness.md; record pass/fail and return the package to In Review if this gate fails.
- [x] T006 [GATE] Before any later model, test, source, migration, page, or deployment task, verify Ahmed ELbamby's 2026-07-13 Gate A demo approval recorded in specs/013-schedule-recommendations/clarifications.md remains current; a superseding baseline change returns the package to In Review.

## Phase 2 - Models and API Contracts

- [x] T007 [ENTITY-SchedulePreferences] [OWNER-SPEC-013] Create the future failing invariant/schema/serialization checks for canonical SchedulePreferences ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec013/SchedulePreferencesModelTests.cs.
- [x] T008 [ENTITY-SchedulePreferences] [OWNER-SPEC-013] Deliver the canonical SchedulePreferences model or governed artifact at src/StudentRegistration.Registration/Domain/SchedulePreferences.cs after T007 fails for the expected reason (depends on T007).
- [x] T009 [ENTITY-ScheduleOption] [OWNER-SPEC-013] Create the future failing invariant/schema/serialization checks for canonical ScheduleOption ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec013/ScheduleOptionModelTests.cs.
- [x] T010 [ENTITY-ScheduleOption] [OWNER-SPEC-013] Deliver the canonical ScheduleOption model or governed artifact at src/StudentRegistration.Registration/Domain/ScheduleOption.cs after T009 fails for the expected reason (depends on T009).
- [x] T011 [ENTITY-ScoreComponent] [ENTITY-OptimizerConfiguration] [OWNER-SPEC-013] Create the future failing invariant/schema/serialization checks for canonical ScoreComponent and immutable lexicographic OptimizerConfiguration ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec013/ScoreComponentModelTests.cs and tests/StudentRegistration.IntegrationTests/Specs/Spec013/OptimizerConfigurationModelTests.cs.
- [x] T012 [ENTITY-ScoreComponent] [ENTITY-OptimizerConfiguration] [OWNER-SPEC-013] Deliver the canonical ScoreComponent at src/StudentRegistration.Registration/Domain/ScoreComponent.cs and canonical OptimizerConfiguration at src/StudentRegistration.Registration/Domain/OptimizerConfiguration.cs after T011 fails for the expected reason (depends on T011).
- [x] T013 [ENTITY-OptimizationDiagnostic] [OWNER-SPEC-013] Create the future failing invariant/schema/serialization checks for canonical OptimizationDiagnostic ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec013/OptimizationDiagnosticModelTests.cs.
- [x] T014 [ENTITY-OptimizationDiagnostic] [OWNER-SPEC-013] Deliver the canonical OptimizationDiagnostic model or governed artifact at src/StudentRegistration.Registration/Domain/OptimizationDiagnostic.cs after T013 fails for the expected reason (depends on T013).
- [x] T015 [API-Endpoint01] [OWNER-SPEC-013] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/student/terms/{termId}/registration-plan/recommendations in specs/013-schedule-recommendations/contracts/api.md.
- [x] T016 [API-Endpoint01] Verify every documented response and authorization outcome for POST /api/student/terms/{termId}/registration-plan/recommendations in tests/StudentRegistration.ContractTests/Specs/Spec013/Endpoint01ContractTests.cs.
- [x] T017 [API-Endpoint01] [FR-1] [FR-6] [FR-7] Create the future failing endpoint-orchestration, ownership, budget, and diagnostic behavior tests for POST /api/student/terms/{termId}/registration-plan/recommendations in tests/StudentRegistration.ApplicationTests/Specs/Spec013/Endpoint01BehaviorTests.cs; handler delivery is deferred until all linked AC/FR tests fail for expected reasons.
- [x] T018 [API-Endpoint02] [OWNER-SPEC-013] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for PUT /api/student/terms/{termId}/registration-plan/recommended-option in specs/013-schedule-recommendations/contracts/api.md.
- [x] T019 [API-Endpoint02] Verify every documented response and authorization outcome for PUT /api/student/terms/{termId}/registration-plan/recommended-option in tests/StudentRegistration.ContractTests/Specs/Spec013/Endpoint02ContractTests.cs.
- [x] T020 [API-Endpoint02] [FR-9] [FR-10] Create the future failing token tamper, owner, expiry, cross-replica, stale-version, and atomic-apply endpoint tests for PUT /api/student/terms/{termId}/registration-plan/recommended-option in tests/StudentRegistration.ApplicationTests/Specs/Spec013/Endpoint02BehaviorTests.cs; handler delivery is deferred until all linked AC/FR tests fail for expected reasons.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Alternative found (FR-1, FR-2, FR-4) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Schedule Recommendations.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [x] T021 [SC-1] [AC-1] [FR-1] [FR-2] [FR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-1Tests.cs for AC-1: Alternative found (FR-1, FR-2, FR-4): Given two selected groups overlap and a non-overlapping alternate group exists When recommendations are requested Then at least one complete conflict-free schedule is returned And no subject is omitted or duplicated.
### US2 - Best score explanation (FR-5) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Schedule Recommendations.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [x] T022 [SC-2] [AC-2] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-2Tests.cs for AC-2: Best score explanation (FR-5): Given two feasible schedules and one has fewer gaps under approved preferences When results are ranked Then that schedule ranks first And the gap and other score components are shown.
### US3 - No solution (FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Schedule Recommendations.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [x] T023 [SC-3] [AC-3] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-3Tests.cs for AC-3: No solution (FR-7): Given every combination has a hard overlap When search completes Then no fake solution is returned And an inclusion-minimal blocking set, stable hard reason codes, all involved intervals, and change/remove actions for every member are shown.
### US4 - Time budget (FR-6, NFR-3) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Schedule Recommendations.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [x] T024 [AC-4] [FR-6] [NFR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-4Tests.cs for AC-4: Time budget (FR-6, NFR-3): Given search exceeds its configured budget When cancellation occurs Then the response indicates budget expiry with any verified results And final submission remains blocked unless a complete valid plan is chosen.
### US5 - Recommendation revalidation (FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Schedule Recommendations.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [x] T025 [AC-5] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-5Tests.cs for AC-5: Recommendation revalidation (FR-8): Given a recommended schedule was valid when calculated And a group fills before submission When the student submits that option Then final registration revalidates and rejects the stale group And no recommendation is treated as a reservation.
### US6 - Plan changes during optimization (FR-9, FR-10) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Schedule Recommendations.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [x] T026 [AC-6] [FR-9] [FR-10] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-6Tests.cs for AC-6: Plan changes during optimization (FR-9, FR-10): Given optimization starts for plan rowversion 5 When the student edits the plan to rowversion 6 before the result is applied Then applying the old option returns 409 PLAN_CHANGED And rowversion 6 remains unchanged.
### US7 - Out-of-order responses (FR-9, FR-10, NFR-2) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Schedule Recommendations.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [x] T027 [AC-7] [FR-9] [FR-10] [NFR-2] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-7Tests.cs for AC-7: Out-of-order responses (FR-9, FR-10, NFR-2): Given request B for the current plan starts after request A When B completes first and A completes later Then the client renders only the response matching the current request correlation ID and plan version And the server rejects any stale apply attempt.
### US8 - Optimizer pruning, performance, and coverage (FR-3, NFR-1, NFR-4) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Schedule Recommendations.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T020.
- [x] T028 [AC-8] [FR-3] [NFR-1] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-8Tests.cs for AC-8: Optimizer pruning, performance, and coverage (FR-3, NFR-1, NFR-4): Given eight courses with ten groups each and fixtures exercising every constraint/pruning branch When the optimizer benchmark and branch-coverage suite executes Then constrained courses are processed first and invalid partial schedules are pruned And p95 completion is at most 500 ms And optimizer branch coverage is at least 90%.
- [x] T029 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec013/EdgeCases/EC-1Tests.cs and assert: Group becomes full during search -> may be excluded from fresh query; final commit remains authority.
- [x] T030 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec013/EdgeCases/EC-2Tests.cs and assert: One selected course has zero viable groups -> immediate no-solution.
- [x] T031 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec013/EdgeCases/EC-3Tests.cs and assert: Equal scores -> stable tie-break by course/group identifiers.
- [x] T032 [EC-4] Exercise EC-4 in tests/StudentRegistration.IntegrationTests/Specs/Spec013/EdgeCases/EC-4Tests.cs and assert that an invalid preference bound, factor order, or configuration version is rejected while the last Product Owner/Technical Lead-approved version remains active.
- [x] T033 [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec013/EdgeCases/EC-5Tests.cs and assert: Catalogue/group/policy changes during search -> the result uses one coherent captured version set or returns STALE_INPUT; mixed-version options MUST NOT be returned.

## Phase 4 - Requirement Tests and Bounded Delivery

- [x] T034 [FR-1] [WORKSTREAM-BOUNDED-DETERMINISTIC-OPTIMIZER] Create the future failing FR-1 checks in tests/StudentRegistration.ApplicationTests/Registration/ScheduleOptimizerTests.cs. Test focus: exactly one group, hard constraints, constrained-first pruning and up to three deterministic options. Prove the requirement against its linked AC/EC fixtures: The optimizer MUST choose exactly one published viable group per selected course.
- [x] T035 [FR-1] [WORKSTREAM-BOUNDED-DETERMINISTIC-OPTIMIZER] Deliver FR-1 through the bounded Bounded deterministic optimizer workstream at src/StudentRegistration.Registration/Domain/ScheduleOptimizer.cs only after T034 fails for the expected reason (depends on T034): The optimizer MUST choose exactly one published viable group per selected course.
- [x] T036 [FR-2] [WORKSTREAM-BOUNDED-DETERMINISTIC-OPTIMIZER] Create the future failing FR-2 checks in tests/StudentRegistration.ApplicationTests/Registration/ScheduleOptimizerTests.cs. Test focus: exactly one group, hard constraints, constrained-first pruning, up to three deterministic options, and no demo travel-buffer rule. Prove the requirement against its linked AC/EC fixtures: It MUST enforce all hard meeting, availability, completeness, eligibility, and credit constraints. The approved simple demo policy disables travel-buffer constraints and MUST NOT guess a duration or matrix.
- [x] T037 [FR-2] [WORKSTREAM-BOUNDED-DETERMINISTIC-OPTIMIZER] Deliver FR-2 through the bounded Bounded deterministic optimizer workstream at src/StudentRegistration.Registration/Domain/ScheduleOptimizer.cs only after T036 fails for the expected reason (depends on T036): It MUST enforce all hard meeting, availability, completeness, eligibility, and credit constraints. The approved simple demo policy disables travel-buffer constraints and MUST NOT guess a duration or matrix.
- [x] T038 [FR-3] [WORKSTREAM-BOUNDED-DETERMINISTIC-OPTIMIZER] Create the future failing FR-3 checks in tests/StudentRegistration.ApplicationTests/Registration/ScheduleOptimizerTests.cs. Test focus: exactly one group, hard constraints, constrained-first pruning and up to three deterministic options. Prove the requirement against its linked AC/EC fixtures: It MUST order constrained courses first and prune invalid partial schedules.
- [x] T039 [FR-3] [WORKSTREAM-BOUNDED-DETERMINISTIC-OPTIMIZER] Deliver FR-3 through the bounded Bounded deterministic optimizer workstream at src/StudentRegistration.Registration/Domain/ScheduleOptimizer.cs only after T038 fails for the expected reason (depends on T038): It MUST order constrained courses first and prune invalid partial schedules.
- [x] T040 [FR-4] [WORKSTREAM-BOUNDED-DETERMINISTIC-OPTIMIZER] Create the future failing FR-4 checks in tests/StudentRegistration.ApplicationTests/Registration/ScheduleOptimizerTests.cs. Test focus: exactly one group, hard constraints, constrained-first pruning and up to three deterministic options. Prove the requirement against its linked AC/EC fixtures: It SHOULD return up to three distinct feasible schedules.
- [x] T041 [FR-4] [WORKSTREAM-BOUNDED-DETERMINISTIC-OPTIMIZER] Deliver FR-4 through the bounded Bounded deterministic optimizer workstream at src/StudentRegistration.Registration/Domain/ScheduleOptimizer.cs only after T040 fails for the expected reason (depends on T040): It SHOULD return up to three distinct feasible schedules.
- [x] T042 [FR-5] [WORKSTREAM-PREFERENCE-SCORING] Create the future failing FR-5 checks in tests/StudentRegistration.ApplicationTests/Registration/ScheduleScorerTests.cs for validated preference bounds, versioned lexicographic preference-violation/idle-minute/teaching-day/stable-group factors, explained components, stable ties, and explicit absence of numeric weights.
- [x] T043 [FR-5] [WORKSTREAM-PREFERENCE-SCORING] Deliver the bounded Preference scoring workstream at src/StudentRegistration.Registration/Domain/ScheduleScorer.cs only after T042 fails (depends on T042), using the approved immutable factor order/version and rejecting invalid configuration without silently reweighting.
- [x] T044 [FR-6] [WORKSTREAM-CANCELLATION-AND-DIAGNOSTICS] Create the future failing FR-6 checks in tests/StudentRegistration.ApplicationTests/Registration/OptimizationBudgetTests.cs. Test focus: cancellation, time budget, complete verified results and inclusion-minimal blocking diagnostics with resolution actions. Prove the requirement against its linked AC/EC fixtures: It MUST support cancellation and a configured computation time budget.
- [x] T045 [FR-6] [WORKSTREAM-CANCELLATION-AND-DIAGNOSTICS] Deliver FR-6 through the bounded Cancellation and diagnostics workstream at src/StudentRegistration.Registration/Application/OptimizationCoordinator.cs only after T044 fails for the expected reason (depends on T044): It MUST support cancellation and a configured computation time budget.
- [x] T046 [FR-7] [WORKSTREAM-CANCELLATION-AND-DIAGNOSTICS] Create the future failing FR-7 checks in tests/StudentRegistration.ApplicationTests/Registration/OptimizationBudgetTests.cs. Test focus: cancellation, time budget, complete verified results and inclusion-minimal blocking diagnostics with resolution actions. Prove the requirement against its linked AC/EC fixtures: If no feasible result exists, it MUST return at least one **inclusion-minimal** blocking set: removing any member makes that reported hard conflict no longer hold. Every diagnostic MUST include the affected course/group IDs, stable hard-reason code, both involved meeting intervals where applicable, and a direct change-group or remove-course action for every member. Diagnostics MUST be ordered deterministically by set size and then stable course/group identifiers.
- [x] T047 [FR-7] [WORKSTREAM-CANCELLATION-AND-DIAGNOSTICS] Deliver FR-7 through the bounded Cancellation and diagnostics workstream at src/StudentRegistration.Registration/Application/OptimizationCoordinator.cs only after T046 fails for the expected reason (depends on T046): If no feasible result exists, it MUST return at least one **inclusion-minimal** blocking set: removing any member makes that reported hard conflict no longer hold. Every diagnostic MUST include the affected course/group IDs, stable hard-reason code, both involved meeting intervals where applicable, and a direct change-group or remove-course action for every member. Diagnostics MUST be ordered deterministically by set size and then stable course/group identifiers.
- [x] T048 [FR-8] [WORKSTREAM-VERSIONED-RECOMMENDATION-APPLICATION] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Registration/RecommendationVersionTests.cs. Test focus: signed owner/plan/version/expiry option token, stale/out-of-order responses, atomic plan apply and no capacity reservation. Prove the requirement against its linked AC/EC fixtures: Final submission MUST revalidate all results; a recommendation does not reserve seats.
- [x] T049 [FR-8] [WORKSTREAM-VERSIONED-RECOMMENDATION-APPLICATION] Deliver FR-8 through the bounded Versioned recommendation application workstream at src/StudentRegistration.Registration/Application/RecommendationApplicationService.cs only after T048 fails for the expected reason (depends on T048): Final submission MUST revalidate all results; a recommendation does not reserve seats.
- [x] T050 [FR-9] [WORKSTREAM-VERSIONED-RECOMMENDATION-APPLICATION] Create the future failing FR-9 checks in tests/StudentRegistration.IntegrationTests/Registration/RecommendationVersionTests.cs. Test focus: signed owner/plan/version/expiry option token, stale/out-of-order responses, atomic plan apply and no capacity reservation. Prove the requirement against its linked AC/EC fixtures: A recommendation request MUST include the expected plan rowversion and request correlation ID; each result MUST identify the captured plan, catalogue/group, policy, and optimizer-configuration versions. Every option MUST carry an authenticated, encrypted, expiring option token binding the authenticated student, plan, complete group selection, captured versions, correlation ID, issued time, and expiry so any stateless replica can validate it without server memory or a durable option table.
- [x] T051 [FR-9] [WORKSTREAM-VERSIONED-RECOMMENDATION-APPLICATION] Deliver FR-9 through the bounded Versioned recommendation application workstream at src/StudentRegistration.Registration/Application/RecommendationApplicationService.cs only after T050 fails for the expected reason (depends on T050): A recommendation request MUST include the expected plan rowversion and request correlation ID; each result MUST identify the captured plan, catalogue/group, policy, and optimizer-configuration versions. Every option MUST carry an authenticated, encrypted, expiring option token binding the authenticated student, plan, complete group selection, captured versions, correlation ID, issued time, and expiry so any stateless replica can validate it without server memory or a durable option table.
- [x] T052 [FR-10] [WORKSTREAM-VERSIONED-RECOMMENDATION-APPLICATION] Create the future failing FR-10 checks in tests/StudentRegistration.IntegrationTests/Registration/RecommendationVersionTests.cs. Test focus: signed owner/plan/version/expiry option token, stale/out-of-order responses, atomic plan apply and no capacity reservation. Prove the requirement against its linked AC/EC fixtures: Applying an option MUST submit that option token and the current expected plan rowversion, validate signature/expiry/owner/plan/payload and dependency versions, and perform one atomic versioned plan mutation. A tampered, cross-owner, expired, stale-plan, or stale-dependency token MUST fail without mutation using a stable safe code.
- [x] T053 [FR-10] [WORKSTREAM-VERSIONED-RECOMMENDATION-APPLICATION] Deliver FR-10 through the bounded Versioned recommendation application workstream at src/StudentRegistration.Registration/Application/RecommendationApplicationService.cs only after T052 fails for the expected reason (depends on T052): Applying an option MUST submit that option token and the current expected plan rowversion, validate signature/expiry/owner/plan/payload and dependency versions, and perform one atomic versioned plan mutation. A tampered, cross-owner, expired, stale-plan, or stale-dependency token MUST fail without mutation using a stable safe code.


## Phase 5 - Frontend Route Tests and Integration

- [x] T054 [STU-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-4] [FR-5] [FR-6] [FR-7] [FR-9] [FR-10] [AC-1] [AC-2] [AC-3] [AC-4] [AC-6] [AC-7] Finalize SPEC-013 data, actions, stable reasons, authorization, time-budget, invalid/expired-token, and stale/out-of-order contribution for STU-04 at specs/013-schedule-recommendations/contracts/routes/STU-04.md; SPEC-012 retains canonical page ownership and only the bounded contributor integration declared by this contract may edit the page.
- [x] T055 [STU-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-4] [FR-5] [FR-6] [FR-7] [FR-9] [FR-10] [AC-1] [AC-2] [AC-3] [AC-4] [AC-6] [AC-7] Verify the SPEC-013 contribution consumed by STU-04 in tests/StudentRegistration.E2ETests/Specs/Spec013/ScheduleBuilderPageContributorTests.cs.

- [x] T056 [API-Endpoint01] [API-Endpoint02] Deliver the canonical handlers and bounded STU-04 contributor integration only after all contract, acceptance, edge, token, optimizer, diagnostic, stale-version, atomic-apply, and contributor E2E tests T015-T055 fail for expected reasons: src/StudentRegistration.Registration/Endpoints/Spec013Endpoints.cs, src/StudentRegistration.Client/Features/Registration/RegistrationApiClient.cs, src/StudentRegistration.Client/Features/Registration/ScheduleRecommendationsPanel.razor, src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor, src/StudentRegistration.Api/Operations/ObservabilityExtensions.cs, and the existing API composition/mapping files. The telemetry change is additive and limited to SPEC-013 safe codes/routes.

## Phase 6 - Measurable Non-Functional Evidence

- [x] T057 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-013-NFR-1.md: p95 optimization MUST be <= 500 ms for 8 courses with up to 10 groups each under approved hardware/load.
- [x] T058 [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-013-NFR-2.md: Same inputs/configuration MUST produce the same ordering.
- [x] T059 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-013-NFR-3.md: Time-budget expiry MUST return a safe status, not an unbounded task.
- [x] T060 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-013-NFR-4.md: Unit coverage for optimizer branches MUST be at least 90%.

## Phase 7 - Scope and Release Evidence

- [x] T061 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-013-scope-review.md that OS-1 remains excluded: Machine learning/AI ranking.
- [x] T062 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-013-scope-review.md that OS-2 remains excluded: Institution-wide timetable generation or changing published resources.
- [x] T063 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-013-scope-review.md that OS-3 remains excluded: Guaranteed seat/reservation.
- [x] T064 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-013-scope-review.md that OS-4 remains excluded: OR-Tools dependency until benchmark evidence requires it.
- [x] T065 [TRACE] Generate the completed FR/NFR/SC/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-013-traceability.md and reject release if any row lacks passing evidence.
- [x] T066 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-013 in docs/release-evidence/SPEC-013-release-approval.md.

Completion remains governed by the per-task evidence rule above.
