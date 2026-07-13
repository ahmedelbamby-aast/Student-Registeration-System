# Tasks: Schedule Builder and Conflicts

**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby on 2026-07-13. Execute remaining readiness and test-first tasks in dependency order.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Approval is the final planning gate. Failing tests precede every model, service, endpoint, and page; `[P]` never targets one shared file.

## Phase 1 - Planning Readiness and Recorded Gate A Approval

- [ ] T001 [DEP-SPEC-003] Baseline STU-04/STU-05 conflict, calendar/list, accessibility, and functional-test contracts from specs/003-ux-storyboard-accessibility/ in specs/012-schedule-builder-conflicts/dependency-baseline.md.
- [ ] T002 [DEP-SPEC-010] Baseline canonical group/activity/meeting/capacity/state/version data from specs/010-offerings-groups-resources/ in specs/012-schedule-builder-conflicts/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-011] Baseline eligible offering/group and explanation contracts from specs/011-eligibility-subject-discovery/ in specs/012-schedule-builder-conflicts/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-018] Baseline performance, security, accessibility, two-editor, and operations gates from specs/018-quality-security-scalability-operations/ in specs/012-schedule-builder-conflicts/dependency-baseline.md.
- [ ] T005 [GATE] Analyze the approved demo travel-buffer-disabled state, ownership, plan routes/versions, conflict details/actions, dependency snapshots, review blocking, frontend links, and task traces and freeze the result in specs/012-schedule-builder-conflicts/checklists/implementation-readiness.md.
- [x] T006 [GATE] Record Ahmed ELbamby's 2026-07-13 Gate A demo approval in specs/012-schedule-builder-conflicts/checklists/approval.md; T007 and later remain blocked until T001-T005 pass.

## Phase 2 - Failing Model and Contract Tests

- [ ] T007 [P] [ENTITY-RegistrationPlan] [OWNER-SPEC-012] Create failing unique student-term, owner, total, review-state, and rowversion checks in tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelTests.cs.
- [ ] T008 [P] [ENTITY-RegistrationPlanItem] [OWNER-SPEC-012] Create failing unique plan-offering, group ownership, captured-version, and parent-controlled update checks in tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanItemModelTests.cs.
- [ ] T009 [P] [ENTITY-ScheduleConflict] [OWNER-SPEC-012] Create failing both-group/course/interval/overlap/code/message/action checks in tests/StudentRegistration.IntegrationTests/Specs/Spec012/ScheduleConflictModelTests.cs.
- [ ] T010 [P] [ENTITY-ValidationSnapshot] [OWNER-SPEC-012] Create failing timestamped academic/policy/catalogue/offering/group version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec012/ValidationSnapshotModelTests.cs.
- [ ] T011 [API-Endpoint01] [OWNER-SPEC-012] Finalize GET /api/student/terms/{termId}/registration-plan in specs/012-schedule-builder-conflicts/contracts/api.md.
- [ ] T012 [P] [API-Endpoint01] Create failing owner/term, empty/current plan, no-seat-reservation, and direct-object privacy checks in tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint01ContractTests.cs for GET /api/student/terms/{termId}/registration-plan.
- [ ] T013 [API-Endpoint02] [OWNER-SPEC-012] Finalize PUT /api/student/terms/{termId}/registration-plan in specs/012-schedule-builder-conflicts/contracts/api.md.
- [ ] T014 [P] [API-Endpoint02] Create failing complete-replacement, expected-rowversion, duplicate offering, stale-current-plan, and atomic response checks in tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint02ContractTests.cs for PUT /api/student/terms/{termId}/registration-plan.
- [ ] T015 [API-Endpoint03] [OWNER-SPEC-012] Finalize POST /api/student/terms/{termId}/registration-plan/validate in specs/012-schedule-builder-conflicts/contracts/api.md.
- [ ] T016 [P] [API-Endpoint03] Create failing non-mutating complete conflict/action/snapshot/stale-group/no-seat-reservation checks in tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint03ContractTests.cs for POST /api/student/terms/{termId}/registration-plan/validate.

## Phase 3 - Acceptance, Edge, and Success-Criterion Tests

- [ ] T017 [P] [AC-1] [FR-2] [FR-3] [FR-4] Create exact overlap/red-X/text/both-subject/action coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-1Tests.cs.
- [ ] T018 [P] [AC-2] [FR-2] Create adjacent half-open interval and approved disabled-demo-travel coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-2Tests.cs.
- [ ] T019 [P] [AC-3] [FR-5] Create blocked Review and accessible resolution-link coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-3Tests.cs.
- [ ] T020 [P] [AC-4] [FR-1] [FR-6] [FR-7] [FR-8] Create versioned add/change/remove/stale/advisory capacity coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-4Tests.cs.
- [ ] T021 [P] [AC-5] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create performance/determinism/view-equivalence/two-editor quality coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-5Tests.cs.
- [ ] T022 [P] [EC-1] Create one-slot-of-many conflict coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-1Tests.cs.
- [ ] T023 [P] [EC-2] Create stale update/current plan coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-2Tests.cs.
- [ ] T024 [P] [EC-3] Create full/unpublished/closed/cancelled group action coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-3Tests.cs.
- [ ] T025 [P] [EC-4] Create duplicate slot defensive de-duplication coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-4Tests.cs.
- [ ] T026 [P] [SC-1] Create every-overlap-before-review outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-1OutcomeTests.cs.
- [ ] T027 [P] [SC-2] Create hard-conflict blocks with accessible guidance outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-2OutcomeTests.cs.
- [ ] T028 [P] [SC-3] Create calendar/list equivalent-content outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-3OutcomeTests.cs.

## Phase 4 - Consolidated Behavior Tests and Delivery

- [ ] T029 [FR-1] [FR-6] [FR-7] [FR-8] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Create the failing consolidated owner/term/full-replacement/rowversion/actions/snapshot/advisory-capacity suite in tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs.
- [ ] T030 [FR-2] [FR-3] [WORKSTREAM-INTERVAL-CONFLICT-DETECTOR] Create the failing consolidated half-open/adjacent/multi-slot/de-duplication/details/actions suite in tests/StudentRegistration.DomainTests/Registration/ScheduleConflictDetectorTests.cs.
- [ ] T031 [FR-4] [FR-5] [WORKSTREAM-ACCESSIBLE-BLOCKING-CONFLICT-STATE] Create the failing red-X/text/icon/review-block/change-remove/calendar-list state suite in tests/StudentRegistration.Client.UnitTests/Scheduling/ConflictStateMapperTests.cs.
- [ ] T032 [ENTITY-RegistrationPlan] [OWNER-SPEC-012] Deliver the canonical RegistrationPlan at src/StudentRegistration.Registration/Domain/RegistrationPlan.cs after T007 fails.
- [ ] T033 [ENTITY-RegistrationPlanItem] [OWNER-SPEC-012] Deliver the canonical RegistrationPlanItem at src/StudentRegistration.Registration/Domain/RegistrationPlanItem.cs after T008 fails.
- [ ] T034 [ENTITY-ScheduleConflict] [OWNER-SPEC-012] Deliver the canonical ScheduleConflict at src/StudentRegistration.Registration/Domain/ScheduleConflict.cs after T009 fails.
- [ ] T035 [ENTITY-ValidationSnapshot] [OWNER-SPEC-012] Deliver the canonical ValidationSnapshot at src/StudentRegistration.Registration/Domain/ValidationSnapshot.cs after T010 fails.
- [ ] T036 [FR-1] [FR-6] [FR-7] [FR-8] [WORKSTREAM-VERSIONED-REGISTRATION-PLAN] Deliver versioned plan behavior at src/StudentRegistration.Registration/Application/RegistrationPlanService.cs after T029 fails.
- [ ] T037 [FR-2] [FR-3] [WORKSTREAM-INTERVAL-CONFLICT-DETECTOR] Deliver the interval conflict detector at src/StudentRegistration.Registration/Domain/ScheduleConflictDetector.cs after T030 fails.
- [ ] T038 [FR-4] [FR-5] [WORKSTREAM-ACCESSIBLE-BLOCKING-CONFLICT-STATE] Deliver accessible blocking conflict mapping at src/StudentRegistration.Client/Features/Scheduling/ConflictStateMapper.cs after T031 fails.

## Phase 5 - Endpoint Handlers After Behavior Tests

- [ ] T039 [API-Endpoint01] Deliver GET /api/student/terms/{termId}/registration-plan at src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs after T012 and T029 fail.
- [ ] T040 [API-Endpoint02] Deliver PUT /api/student/terms/{termId}/registration-plan at src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs after T014 and T029 fail.
- [ ] T041 [API-Endpoint03] Deliver POST /api/student/terms/{termId}/registration-plan/validate at src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs after T016, T029, and T030 fail.

## Phase 6 - Frontend Functional Tests and Page

- [ ] T042 [P] [STU-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-6] [FR-7] [FR-8] [AC-1] [AC-2] [AC-4] [AC-5] Create failing add/change/remove/overlap/stale/view-equivalence journeys in tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs.
- [ ] T043 [STU-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [FR-6] [FR-7] [FR-8] [AC-1] [AC-2] [AC-4] [AC-5] Deliver ScheduleBuilderPage at src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor after T042 fails.
- [ ] T044 [STU-05] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [AC-3] [AC-4] [AC-5] Finalize the blocked-review/conflict/action/snapshot contribution in specs/012-schedule-builder-conflicts/contracts/routes/STU-05.md without editing its Razor page.
- [ ] T045 [P] [STU-05] [UI-CONTRACT-SPEC-003] [FR-5] [FR-7] [FR-8] [AC-3] [AC-4] [AC-5] Verify the Review contribution in tests/StudentRegistration.E2ETests/Specs/Spec012/RegistrationReviewPageContributorTests.cs.

## Phase 7 - Quality, Scope, and Release Evidence

- [ ] T046 [ENTITY-RegistrationPlan] [ENTITY-RegistrationPlanItem] [ENTITY-ScheduleConflict] [ENTITY-ValidationSnapshot] [PERSISTENCE-MAPPING] [MIGRATION-S4DiscoveryPlanning] Create the failing real-SQL plan/read-model mapping, unique active student-term, parent/item FK, offering uniqueness, conflict/snapshot storage, and query-index suite in tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelConfigurationTests.cs plus incremental-migration/update/rollback/snapshot parity tests in tests/StudentRegistration.IntegrationTests/Persistence/S4DiscoveryPlanningMigrationTests.cs.
- [ ] T047 [ENTITY-RegistrationPlan] [ENTITY-RegistrationPlanItem] [ENTITY-ScheduleConflict] [ENTITY-ValidationSnapshot] [PERSISTENCE-MAPPING] [MIGRATION-S4DiscoveryPlanning] Deliver the RegistrationPlan EF Core mapping contribution at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationPlanModelConfiguration.cs, generate src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713040000_DiscoveryPlanning.cs, and update src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs after T046 and SPEC-011 mapping pass; SPEC-004 remains the sole DbContext writer.
- [ ] T048 [P] [NFR-1] Produce eight-course/ten-slot recalculation evidence in tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-012-NFR-1.md.
- [ ] T049 [P] [NFR-2] Produce fixed-input deterministic conflict evidence in tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-012-NFR-2.md.
- [ ] T050 [P] [NFR-3] Produce calendar/list semantic equivalence evidence in tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-012-NFR-3.md.
- [ ] T051 [P] [NFR-4] Produce two-editor no-lost-update evidence in tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-012-NFR-4.md.
- [ ] T052 [OS-1] [OS-2] [OS-3] [OS-4] Record verified schedule-builder scope exclusions including the disabled demo travel-buffer rule in docs/release-evidence/SPEC-012-scope-review.md.
- [ ] T053 [TRACE] [SC-1] [SC-2] [SC-3] Generate the complete FR/NFR/AC/EC/SC/route/entity/endpoint trace matrix in docs/release-evidence/SPEC-012-traceability.md.
- [ ] T054 [GATE] Record product, UX, Policy SME, backend, QA, security, accessibility, and operations release approvals in docs/release-evidence/SPEC-012-release-approval.md.

No task is complete and no implementation file has been created.
