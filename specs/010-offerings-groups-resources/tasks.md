# Tasks: Offerings, Groups, and Resources

**Status**: Planned only. Do not execute until DEC-11/DEC-12 handling, consistency analysis, and Ahmed ELbamby's approval pass.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Approval is the final planning gate. Tests precede every aggregate, application service, endpoint handler, and page; `[P]` tasks always target different files.

## Phase 1 - Planning Readiness and Final Approval

- [ ] T001 [DEP-SPEC-003] Baseline STU-03, ADM-06, ADM-07, accessibility, and route-state contracts from specs/003-ux-storyboard-accessibility/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T002 [DEP-SPEC-005] Baseline scheduling persistence, rowversion, constraint, transaction, and alert mappings from specs/005-erd-data-lifecycle/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-006] Baseline DTO/error, bounded page, preview, expected-version, and idempotency conventions from specs/006-domain-class-api-contracts/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-009] Baseline canonical course/catalogue/policy versions from specs/009-catalog-prerequisites-policy-admin/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-018] Baseline performance, security, two-replica, and operations gates from specs/018-quality-security-scalability-operations/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T006 [GATE] Analyze DEC-11/DEC-12 status, canonical availability ownership, resource versions/locks, APIs, routes, alerts, races, and task traces and freeze the result in specs/010-offerings-groups-resources/checklists/implementation-readiness.md.
- [ ] T007 [GATE] As the final planning action, record Ahmed ELbamby's human approval in specs/010-offerings-groups-resources/checklists/approval.md; T008 and later are forbidden before T001-T007 pass.

## Phase 2 - Failing Model and Contract Tests

- [ ] T008 [P] [ENTITY-CourseOffering] [OWNER-SPEC-010] Create failing term/course uniqueness, lifecycle, and rowversion checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/CourseOfferingModelTests.cs.
- [ ] T009 [P] [ENTITY-SectionGroup] [OWNER-SPEC-010] Create failing code/state/capacity/enrolled-count/root-version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/SectionGroupModelTests.cs.
- [ ] T010 [P] [ENTITY-MeetingSlot] [OWNER-SPEC-010] Create failing activity/day/time/room and parent-version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/MeetingSlotModelTests.cs.
- [ ] T011 [P] [ENTITY-Room] [OWNER-SPEC-010] Create failing normalized code/location/capacity/availability/version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/RoomModelTests.cs.
- [ ] T012 [P] [ENTITY-GroupStaffAssignment] [OWNER-SPEC-010] Create failing group/staff/role uniqueness and parent-version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/GroupStaffAssignmentModelTests.cs.
- [ ] T013 [P] [ENTITY-StaffTermAvailability] [OWNER-SPEC-010] Create failing unique staff-term/deadline/complete-range/version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/StaffTermAvailabilityModelTests.cs.
- [ ] T014 [P] [ENTITY-StaffAvailability] [OWNER-SPEC-010] Create failing child-range/no-independent-version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/StaffAvailabilityModelTests.cs.
- [ ] T015 [P] [ENTITY-ScheduleImpactAlert] [OWNER-SPEC-010] Create failing affected-resource/version/revalidation-state checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/ScheduleImpactAlertModelTests.cs.
- [ ] T016 [API-Endpoint01] [OWNER-SPEC-010] Finalize GET /api/offerings/{offeringId} in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T017 [P] [API-Endpoint01] Create failing offering detail/state/version checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint01ContractTests.cs for GET /api/offerings/{offeringId}.
- [ ] T018 [API-Endpoint02] [OWNER-SPEC-010] Finalize GET /api/groups/{groupId} in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T019 [P] [API-Endpoint02] Create failing group capacity/staff/meeting/state/version checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint02ContractTests.cs for GET /api/groups/{groupId}.
- [ ] T020 [API-Endpoint03] [OWNER-SPEC-010] Finalize GET /api/admin/offerings in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T021 [P] [API-Endpoint03] Create failing bounded Admin offering list/filter/authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint03ContractTests.cs for GET /api/admin/offerings.
- [ ] T022 [API-Endpoint04] [OWNER-SPEC-010] Finalize POST /api/admin/offerings in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T023 [P] [API-Endpoint04] Create failing idempotent draft offering create/validation checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint04ContractTests.cs for POST /api/admin/offerings.
- [ ] T024 [API-Endpoint05] [OWNER-SPEC-010] Finalize PUT /api/admin/groups/{groupId} in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T025 [P] [API-Endpoint05] Create failing parent/dependency version and capacity invariant checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint05ContractTests.cs for PUT /api/admin/groups/{groupId}.
- [ ] T026 [API-Endpoint06] [OWNER-SPEC-010] Finalize POST /api/admin/offerings/{offeringId}/validate in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T027 [P] [API-Endpoint06] Create failing DEC-11, overlap, capacity, resource-version, and preview-binding checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint06ContractTests.cs for POST /api/admin/offerings/{offeringId}/validate.
- [ ] T028 [API-Endpoint07] [OWNER-SPEC-010] Finalize POST /api/admin/offerings/{offeringId}/publish in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T029 [P] [API-Endpoint07] Create failing stable-lock, in-transaction revalidation, stale dependency, audit, and replay checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint07ContractTests.cs for POST /api/admin/offerings/{offeringId}/publish.
- [ ] T030 [API-Endpoint08] [OWNER-SPEC-010] Finalize GET /api/admin/rooms in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T031 [P] [API-Endpoint08] Create failing bounded room list/filter/authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint08ContractTests.cs for GET /api/admin/rooms.
- [ ] T032 [API-Endpoint09] [OWNER-SPEC-010] Finalize POST /api/admin/rooms in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T033 [P] [API-Endpoint09] Create failing idempotent normalized room create checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint09ContractTests.cs for POST /api/admin/rooms.
- [ ] T034 [API-Endpoint10] [OWNER-SPEC-010] Finalize PUT /api/admin/rooms/{roomId} in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T035 [P] [API-Endpoint10] Create failing room expected-version and published-impact checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint10ContractTests.cs for PUT /api/admin/rooms/{roomId}.
- [ ] T036 [API-Endpoint11] [OWNER-SPEC-010] Finalize GET /api/admin/staff-availability in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T037 [P] [API-Endpoint11] Create failing bounded staff-term availability view and privacy checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint11ContractTests.cs for GET /api/admin/staff-availability.
- [ ] T038 [API-Endpoint12] [OWNER-SPEC-010] Finalize PUT /api/admin/staff/{staffId}/terms/{termId}/availability in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T039 [P] [API-Endpoint12] Create failing DEC-12 permission/reason/preview/version/audit/notification/alert checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint12ContractTests.cs for PUT /api/admin/staff/{staffId}/terms/{termId}/availability.

## Phase 3 - Acceptance, Edge, and Success-Criterion Tests

- [ ] T040 [P] [AC-1] [FR-1] [FR-2] [FR-3] Create valid activity-staffed group publication coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-1Tests.cs.
- [ ] T041 [P] [AC-2] [FR-3] Create room overlap interval/reason coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-2Tests.cs.
- [ ] T042 [P] [AC-3] [FR-4] Create full/nonselectable group coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-3Tests.cs.
- [ ] T043 [P] [AC-4] [FR-5] [FR-6] [FR-7] Create audited transactional publish/capacity coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-4Tests.cs.
- [ ] T044 [P] [AC-5] [FR-3] [FR-7] [FR-8] Create two-admin room publication one-winner coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-5Tests.cs.
- [ ] T045 [P] [AC-6] [FR-5] [FR-9] Create capacity-reduction versus allocation serial-order coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-6Tests.cs.
- [ ] T046 [P] [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create offering/read/presentation quality-gate coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-7Tests.cs.
- [ ] T047 [P] [AC-8] [FR-8] [FR-10] Create child-mutation/group-version and availability-publication serial-order coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-8Tests.cs.
- [ ] T048 [P] [AC-9] [FR-6] [FR-8] [FR-10] Create audited Admin availability correction and alert coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-9Tests.cs.
- [ ] T049 [P] [EC-1] Create invalid multi-slot group coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-1Tests.cs.
- [ ] T050 [P] [EC-2] Create capacity-versus-enrollment race coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-2Tests.cs.
- [ ] T051 [P] [EC-3] Create post-publication staff-unavailability alert coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-3Tests.cs.
- [ ] T052 [P] [EC-4] Create overnight-slot MVP rejection coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-4Tests.cs.
- [ ] T053 [P] [EC-5] Create multi-resource stable-lock/deadlock-retry coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-5Tests.cs.
- [ ] T054 [P] [SC-1] Create complete/conflict-free publication outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/SC-1OutcomeTests.cs.
- [ ] T055 [P] [SC-2] Create visible group detail outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/SC-2OutcomeTests.cs.
- [ ] T056 [P] [SC-3] Create capacity-never-below-enrollment outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/SC-3OutcomeTests.cs.

## Phase 4 - Consolidated Behavior Tests and Delivery

- [ ] T057 [FR-1] [FR-2] [WORKSTREAM-OFFERING-AND-GROUP-LIFECYCLE] Create the failing consolidated activity/group/staff/detail suite in tests/StudentRegistration.IntegrationTests/Scheduling/OfferingLifecycleTests.cs.
- [ ] T058 [FR-3] [FR-8] [FR-10] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Create the failing consolidated room/staff/version/stable-lock/parent-propagation suite in tests/StudentRegistration.IntegrationTests/Scheduling/ResourcePublicationRaceTests.cs.
- [ ] T059 [FR-4] [FR-5] [FR-9] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Create the failing consolidated selectable/capacity/registration-race suite in tests/StudentRegistration.IntegrationTests/Scheduling/GroupCapacityRaceTests.cs.
- [ ] T060 [FR-6] [WORKSTREAM-RESOURCE-AND-AVAILABILITY-OWNERSHIP] Create the failing consolidated staff-owned/Admin-correction/room/alert suite in tests/StudentRegistration.IntegrationTests/Scheduling/ResourceAvailabilityTests.cs.
- [ ] T061 [FR-7] [WORKSTREAM-TRANSACTIONAL-AUDITED-PUBLICATION] Create the failing rollback/audit transaction suite in tests/StudentRegistration.IntegrationTests/Scheduling/OfferingPublicationTransactionTests.cs.
- [ ] T062 [ENTITY-CourseOffering] [OWNER-SPEC-010] Deliver the canonical CourseOffering at src/StudentRegistration.Scheduling/Domain/CourseOffering.cs after T008 fails.
- [ ] T063 [ENTITY-SectionGroup] [OWNER-SPEC-010] Deliver the canonical SectionGroup at src/StudentRegistration.Scheduling/Domain/SectionGroup.cs after T009 fails.
- [ ] T064 [ENTITY-MeetingSlot] [OWNER-SPEC-010] Deliver the canonical MeetingSlot at src/StudentRegistration.Scheduling/Domain/MeetingSlot.cs after T010 fails.
- [ ] T065 [ENTITY-Room] [OWNER-SPEC-010] Deliver the canonical Room at src/StudentRegistration.Scheduling/Domain/Room.cs after T011 fails.
- [ ] T066 [ENTITY-GroupStaffAssignment] [OWNER-SPEC-010] Deliver the canonical GroupStaffAssignment at src/StudentRegistration.Scheduling/Domain/GroupStaffAssignment.cs after T012 fails.
- [ ] T067 [ENTITY-StaffTermAvailability] [OWNER-SPEC-010] Deliver the canonical StaffTermAvailability at src/StudentRegistration.Scheduling/Domain/StaffTermAvailability.cs after T013 fails.
- [ ] T068 [ENTITY-StaffAvailability] [OWNER-SPEC-010] Deliver the canonical StaffAvailability at src/StudentRegistration.Scheduling/Domain/StaffAvailability.cs after T014 fails.
- [ ] T069 [ENTITY-ScheduleImpactAlert] [OWNER-SPEC-010] Deliver the canonical ScheduleImpactAlert at src/StudentRegistration.Scheduling/Domain/ScheduleImpactAlert.cs after T015 fails.
- [ ] T070 [FR-1] [FR-2] [WORKSTREAM-OFFERING-AND-GROUP-LIFECYCLE] Deliver offering/group lifecycle at src/StudentRegistration.Scheduling/Application/OfferingService.cs after T057 fails.
- [ ] T071 [FR-3] [FR-8] [FR-10] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Deliver publication conflict/version validation at src/StudentRegistration.Scheduling/Application/OfferingPublicationValidator.cs after T058 fails.
- [ ] T072 [FR-4] [FR-5] [FR-9] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Deliver group capacity boundary at src/StudentRegistration.Scheduling/Application/SectionGroupCapacityService.cs after T059 fails.
- [ ] T073 [FR-6] [WORKSTREAM-RESOURCE-AND-AVAILABILITY-OWNERSHIP] Deliver room/availability/Admin-correction/alert behavior at src/StudentRegistration.Scheduling/Application/ResourceAvailabilityService.cs after T060 fails.
- [ ] T074 [FR-7] [WORKSTREAM-TRANSACTIONAL-AUDITED-PUBLICATION] Deliver transactional audited publication at src/StudentRegistration.Scheduling/Application/OfferingPublicationService.cs after T061 fails.

## Phase 5 - Endpoint Handlers After Behavior Tests

- [ ] T075 [API-Endpoint01] Deliver GET /api/offerings/{offeringId} at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T017 and T057 fail.
- [ ] T076 [API-Endpoint02] Deliver GET /api/groups/{groupId} at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T019 and T057 fail.
- [ ] T077 [API-Endpoint03] Deliver GET /api/admin/offerings at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T021 and T057 fail.
- [ ] T078 [API-Endpoint04] Deliver POST /api/admin/offerings at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T023 and T057 fail.
- [ ] T079 [API-Endpoint05] Deliver PUT /api/admin/groups/{groupId} at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T025, T057, and T059 fail.
- [ ] T080 [API-Endpoint06] Deliver POST /api/admin/offerings/{offeringId}/validate at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T027 and T058 fail.
- [ ] T081 [API-Endpoint07] Deliver POST /api/admin/offerings/{offeringId}/publish at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T029, T058, and T061 fail.
- [ ] T082 [API-Endpoint08] Deliver GET /api/admin/rooms at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T031 and T060 fail.
- [ ] T083 [API-Endpoint09] Deliver POST /api/admin/rooms at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T033 and T060 fail.
- [ ] T084 [API-Endpoint10] Deliver PUT /api/admin/rooms/{roomId} at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T035 and T060 fail.
- [ ] T085 [API-Endpoint11] Deliver GET /api/admin/staff-availability at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T037 and T060 fail.
- [ ] T086 [API-Endpoint12] Deliver PUT /api/admin/staff/{staffId}/terms/{termId}/availability at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T039, T058, and T060 fail.

## Phase 6 - Schedule-Impact Alert API

- [ ] T087 [API-Endpoint13] [OWNER-SPEC-010] Finalize GET /api/admin/schedule-impact-alerts in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T088 [P] [API-Endpoint13] Create failing bounded alert list/filter/authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint13ContractTests.cs for GET /api/admin/schedule-impact-alerts.
- [ ] T089 [API-Endpoint13] Deliver GET /api/admin/schedule-impact-alerts at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T088 and T060 fail.
- [ ] T090 [API-Endpoint14] [OWNER-SPEC-010] Finalize POST /api/admin/schedule-impact-alerts/{alertId}/revalidate in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T091 [P] [API-Endpoint14] Create failing expected alert/group/room/staff versions and durable revalidation-result checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint14ContractTests.cs for POST /api/admin/schedule-impact-alerts/{alertId}/revalidate.
- [ ] T092 [API-Endpoint14] Deliver POST /api/admin/schedule-impact-alerts/{alertId}/revalidate at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T091, T058, and T060 fail.
- [ ] T093 [API-Endpoint15] [OWNER-SPEC-010] Finalize POST /api/admin/schedule-impact-alerts/{alertId}/resolve in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T094 [P] [API-Endpoint15] Create failing expected-version/reason/passing-revalidation and no-silent-resolution checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint15ContractTests.cs for POST /api/admin/schedule-impact-alerts/{alertId}/resolve.
- [ ] T095 [API-Endpoint15] Deliver POST /api/admin/schedule-impact-alerts/{alertId}/resolve at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T094 and T060 fail.

## Phase 7 - Frontend Functional Tests and Pages

- [ ] T096 [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-9] [AC-3] [AC-6] [AC-7] Finalize the SPEC-010 subject-detail data/state contribution in specs/010-offerings-groups-resources/contracts/routes/STU-03.md without editing its Razor page.
- [ ] T097 [P] [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-9] [AC-3] [AC-6] [AC-7] Verify the contribution in tests/StudentRegistration.E2ETests/Specs/Spec010/SubjectDetailsPageContributorTests.cs.
- [ ] T098 [P] [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-4] [AC-5] [AC-6] [AC-8] Create failing offering edit/validate/publish journeys in tests/StudentRegistration.E2ETests/Specs/Spec010/OfferingAdministrationPageFeatureTests.cs.
- [ ] T099 [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-4] [AC-5] [AC-6] [AC-8] Deliver OfferingAdministrationPage at src/StudentRegistration.Client/Pages/OfferingAdministrationPage.razor after T098 and SPEC-017 contributor tests fail.
- [ ] T100 [P] [ADM-07] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-6] [FR-8] [FR-10] [AC-2] [AC-5] [AC-8] [AC-9] Create failing room/availability/correction/impact-list/revalidate/resolve journeys in tests/StudentRegistration.E2ETests/Specs/Spec010/ResourceAdministrationPageFeatureTests.cs.
- [ ] T101 [ADM-07] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-6] [FR-8] [FR-10] [AC-2] [AC-5] [AC-8] [AC-9] Deliver ResourceAdministrationPage at src/StudentRegistration.Client/Pages/ResourceAdministrationPage.razor after T100 and SPEC-017 contributor tests fail.

## Phase 8 - Quality, Scope, and Release Evidence

- [ ] T102 [ENTITY-CourseOffering] [ENTITY-SectionGroup] [ENTITY-MeetingSlot] [ENTITY-Room] [ENTITY-GroupStaffAssignment] [ENTITY-StaffTermAvailability] [ENTITY-StaffAvailability] [ENTITY-ScheduleImpactAlert] [PERSISTENCE-MAPPING] [MIGRATION-S2CatalogueScheduling] Create the failing real-SQL scheduling mapping, parent-version, capacity, unique resource, alert state, range, rowversion, and collision-index suite in tests/StudentRegistration.IntegrationTests/Specs/Spec010/SchedulingModelConfigurationTests.cs plus incremental-migration/update/rollback/snapshot parity tests in tests/StudentRegistration.IntegrationTests/Persistence/S2CatalogueSchedulingMigrationTests.cs.
- [ ] T103 [ENTITY-CourseOffering] [ENTITY-SectionGroup] [ENTITY-MeetingSlot] [ENTITY-Room] [ENTITY-GroupStaffAssignment] [ENTITY-StaffTermAvailability] [ENTITY-StaffAvailability] [ENTITY-ScheduleImpactAlert] [PERSISTENCE-MAPPING] [MIGRATION-S2CatalogueScheduling] Deliver the complete Scheduling EF Core mapping contribution at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/SchedulingModelConfiguration.cs, generate src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713020000_CatalogueScheduling.cs, and update src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs after T102 and SPEC-009 mapping pass; SPEC-004 remains the sole DbContext writer.
- [ ] T104 [P] [NFR-1] Produce offering/group read-load evidence in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-1.md.
- [ ] T105 [P] [NFR-2] Produce stable actionable publication-code evidence in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-2.md.
- [ ] T106 [P] [NFR-3] Produce term-timezone display evidence in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-3.md.
- [ ] T107 [P] [NFR-4] Produce bounded Admin list/filter evidence in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-4.md.
- [ ] T108 [OS-1] [OS-2] [OS-3] [OS-4] Record verified scheduling scope exclusions in docs/release-evidence/SPEC-010-scope-review.md.
- [ ] T109 [TRACE] [SC-1] [SC-2] [SC-3] Generate the complete FR/NFR/AC/EC/SC/route/entity/endpoint trace matrix in docs/release-evidence/SPEC-010-traceability.md.
- [ ] T110 [GATE] Record Admin, staff representative, data, QA, security, accessibility, and operations release approvals in docs/release-evidence/SPEC-010-release-approval.md.

No task is complete and no implementation file has been created.
