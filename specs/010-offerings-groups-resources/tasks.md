# Tasks: Offerings, Groups, and Resources

**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby on 2026-07-13. Execute remaining readiness and test-first tasks in dependency order.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Approval is the final planning gate. Tests precede every aggregate, application service, endpoint handler, and page; `[P]` tasks always target different files.

## Phase 1 - Planning Readiness and Recorded Gate A Approval

- [x] T001 [DEP-SPEC-003] Baseline STU-03, ADM-06, ADM-07, accessibility, and route-state contracts from specs/003-ux-storyboard-accessibility/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [x] T002 [DEP-SPEC-005] Baseline scheduling persistence, rowversion, constraint, transaction, and alert mappings from specs/005-erd-data-lifecycle/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [x] T003 [DEP-SPEC-006] Baseline DTO/error, bounded page, preview, expected-version, and idempotency conventions from specs/006-domain-class-api-contracts/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [x] T004 [DEP-SPEC-009] Baseline canonical course/catalogue/policy versions from specs/009-catalog-prerequisites-policy-admin/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [x] T005 [DEP-SPEC-018] Baseline performance, security, two-replica, and operations gates from specs/018-quality-security-scalability-operations/ in specs/010-offerings-groups-resources/dependency-baseline.md.
- [x] T006 [GATE] Verify accepted DEC-11 activity-bundle staffing, DEC-12 staff-only availability mutation/read-only Admin use, canonical availability ownership, resource versions/locks, APIs, routes, alerts, races, and task traces and freeze the result in specs/010-offerings-groups-resources/checklists/implementation-readiness.md.
- [x] T007 [GATE] Record Ahmed ELbamby's 2026-07-13 Gate A demo approval in specs/010-offerings-groups-resources/checklists/approval.md; T008 and later remain blocked until T001-T006 pass.

## Phase 2 - Failing Model and Contract Tests

- [x] T008 [P] [ENTITY-CourseOffering] [OWNER-SPEC-010] Create failing term/course uniqueness, lifecycle, and rowversion checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/CourseOfferingModelTests.cs.
- [x] T009 [P] [ENTITY-SectionGroup] [OWNER-SPEC-010] Create failing code/state/capacity/enrolled-count/root-version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/SectionGroupModelTests.cs.
- [x] T010 [P] [ENTITY-MeetingSlot] [OWNER-SPEC-010] Create failing canonical Lecture/Tutorial/Laboratory activity, day/time/room, and parent-version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/MeetingSlotModelTests.cs.
- [x] T011 [P] [ENTITY-Room] [OWNER-SPEC-010] Create failing normalized code/location/capacity/availability/version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/RoomModelTests.cs.
- [x] T012 [P] [ENTITY-GroupStaffAssignment] [OWNER-SPEC-010] Create failing meeting/activity/staff/role uniqueness, Lecturer-for-Lecture, TA-for-Tutorial-or-Laboratory, and parent-version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/GroupStaffAssignmentModelTests.cs.
- [x] T013 [P] [ENTITY-StaffTermAvailability] [OWNER-SPEC-010] Create failing unique staff-term/deadline/complete-range/version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/StaffTermAvailabilityModelTests.cs.
- [x] T014 [P] [ENTITY-StaffAvailability] [OWNER-SPEC-010] Create failing child-range/no-independent-version checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/StaffAvailabilityModelTests.cs.
- [x] T015 [P] [ENTITY-ScheduleImpactAlert] [OWNER-SPEC-010] Create failing affected-resource/version/revalidation-state checks in tests/StudentRegistration.IntegrationTests/Specs/Spec010/ScheduleImpactAlertModelTests.cs.
- [x] T016 [API-Endpoint01] [OWNER-SPEC-010] Finalize GET /api/offerings/{offeringId} in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T017 [P] [API-Endpoint01] Create failing offering detail/state/version checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint01ContractTests.cs for GET /api/offerings/{offeringId}.
- [x] T018 [API-Endpoint02] [OWNER-SPEC-010] Finalize GET /api/groups/{groupId} in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T019 [P] [API-Endpoint02] Create failing group capacity/state/version and per-activity type/staff/room/location/day/time checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint02ContractTests.cs for GET /api/groups/{groupId}.
- [x] T020 [API-Endpoint03] [OWNER-SPEC-010] Finalize GET /api/admin/offerings in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T021 [P] [API-Endpoint03] Create failing bounded Admin offering list/filter/authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint03ContractTests.cs for GET /api/admin/offerings.
- [x] T022 [API-Endpoint04] [OWNER-SPEC-010] Finalize POST /api/admin/offerings in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T023 [P] [API-Endpoint04] Create failing idempotent draft offering create/validation checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint04ContractTests.cs for POST /api/admin/offerings.
- [x] T024 [API-Endpoint05] [OWNER-SPEC-010] Finalize PUT /api/admin/groups/{groupId} in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T025 [P] [API-Endpoint05] Create failing parent/dependency version and capacity invariant checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint05ContractTests.cs for PUT /api/admin/groups/{groupId}.
- [x] T026 [API-Endpoint06] [OWNER-SPEC-010] Finalize POST /api/admin/offerings/{offeringId}/validate in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T027 [P] [API-Endpoint06] Create failing accepted DEC-11 complete-bundle staffing, canonical Tutorial/display Section, overlap, capacity, resource-version, and preview-binding checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint06ContractTests.cs for POST /api/admin/offerings/{offeringId}/validate.
- [x] T028 [API-Endpoint07] [OWNER-SPEC-010] Finalize POST /api/admin/offerings/{offeringId}/publish in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T029 [P] [API-Endpoint07] Create failing stable-lock, in-transaction revalidation, stale dependency, audit, and replay checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint07ContractTests.cs for POST /api/admin/offerings/{offeringId}/publish.
- [x] T030 [API-Endpoint08] [OWNER-SPEC-010] Finalize GET /api/admin/rooms in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T031 [P] [API-Endpoint08] Create failing bounded room list/filter/authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint08ContractTests.cs for GET /api/admin/rooms.
- [x] T032 [API-Endpoint09] [OWNER-SPEC-010] Finalize POST /api/admin/rooms in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T033 [P] [API-Endpoint09] Create failing idempotent normalized room create checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint09ContractTests.cs for POST /api/admin/rooms.
- [x] T034 [API-Endpoint10] [OWNER-SPEC-010] Finalize PUT /api/admin/rooms/{roomId} in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T035 [P] [API-Endpoint10] Create failing room expected-version and published-impact checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint10ContractTests.cs for PUT /api/admin/rooms/{roomId}.
- [x] T036 [API-Endpoint11] [OWNER-SPEC-010] Finalize GET /api/admin/staff-availability in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T037 [P] [API-Endpoint11] Create failing bounded staff-term availability view and privacy checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint11ContractTests.cs for GET /api/admin/staff-availability.
- [x] T038 [OWNER-SPEC-010] Finalize the bounded read-only Admin availability view/import contract and explicit Admin mutation-route absence in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T039 [P] [FR-6] [AC-9] Create failing Admin availability route-absence, authorization, and aggregate-scope checks in tests/StudentRegistration.ContractTests/Specs/Spec010/AdminAvailabilityMutationAbsenceContractTests.cs, proving PUT /api/admin/staff/{staffId}/terms/{termId}/availability is not mapped and cannot mutate StaffTermAvailability.

## Phase 3 - Acceptance, Edge, and Success-Criterion Tests

- [x] T040 [P] [AC-1] [FR-1] [FR-2] [FR-3] Create valid Lecturer-staffed Lecture plus TA-staffed Tutorial/Laboratory publication and student-detail coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-1Tests.cs.
- [x] T041 [P] [AC-2] [FR-3] Create room overlap interval/reason coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-2Tests.cs.
- [x] T042 [P] [AC-3] [FR-4] Create full/nonselectable group coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-3Tests.cs.
- [x] T043 [P] [AC-4] [FR-5] [FR-6] [FR-7] Create audited transactional publish/capacity coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-4Tests.cs.
- [x] T044 [P] [AC-5] [FR-3] [FR-7] [FR-8] Create two-admin room publication one-winner coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-5Tests.cs.
- [x] T045 [P] [AC-6] [FR-5] [FR-9] Create capacity-reduction versus allocation serial-order coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-6Tests.cs.
- [x] T046 [P] [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create offering/read/presentation quality-gate coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-7Tests.cs.
- [x] T047 [P] [AC-8] [FR-8] [FR-10] Create child-mutation/group-version and availability-publication serial-order coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-8Tests.cs.
- [x] T048 [P] [AC-9] [FR-6] [FR-8] [FR-10] Create staff-owned availability, read-only Admin view/import, absent Admin mutation route, and staff-change impact-alert coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-9Tests.cs.
- [x] T049 [P] [EC-1] Create invalid multi-slot group coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-1Tests.cs.
- [x] T050 [P] [EC-2] Create capacity-versus-enrollment race coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-2Tests.cs.
- [x] T051 [P] [EC-3] Create post-publication staff-unavailability alert coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-3Tests.cs.
- [x] T052 [P] [EC-4] Create overnight-slot MVP rejection coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-4Tests.cs.
- [x] T053 [P] [EC-5] Create multi-resource stable-lock/deadlock-retry coverage in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-5Tests.cs.
- [x] T054 [P] [SC-1] Create complete/conflict-free publication outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/SC-1OutcomeTests.cs.
- [x] T055 [P] [SC-2] Create visible group detail outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/SC-2OutcomeTests.cs.
- [x] T056 [P] [SC-3] Create capacity-never-below-enrollment outcome evidence in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/SC-3OutcomeTests.cs.

## Phase 4 - Consolidated Behavior Tests and Delivery

- [x] T057 [FR-1] [FR-2] [WORKSTREAM-OFFERING-AND-GROUP-LIFECYCLE] Create the failing consolidated complete activity-bundle, activity-specific staffing, canonical Tutorial/display Section, and student-detail suite in tests/StudentRegistration.IntegrationTests/Scheduling/OfferingLifecycleTests.cs.
- [x] T058 [FR-3] [FR-8] [FR-10] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Create the failing consolidated room/staff/version/stable-lock/parent-propagation suite in tests/StudentRegistration.IntegrationTests/Scheduling/ResourcePublicationRaceTests.cs.
- [x] T059 [FR-4] [FR-5] [FR-9] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Create the failing consolidated selectable/capacity/registration-race suite in tests/StudentRegistration.IntegrationTests/Scheduling/GroupCapacityRaceTests.cs.
- [x] T060 [FR-6] [WORKSTREAM-RESOURCE-AND-AVAILABILITY-OWNERSHIP] Create the failing consolidated staff-owned availability, read-only Admin planning input, Admin mutation-route absence, room, and durable-alert suite in tests/StudentRegistration.IntegrationTests/Scheduling/ResourceAvailabilityTests.cs.
- [x] T061 [FR-7] [WORKSTREAM-TRANSACTIONAL-AUDITED-PUBLICATION] Create the failing rollback/audit transaction suite in tests/StudentRegistration.IntegrationTests/Scheduling/OfferingPublicationTransactionTests.cs.
- [x] T062 [ENTITY-CourseOffering] [OWNER-SPEC-010] Deliver the canonical CourseOffering at src/StudentRegistration.Scheduling/Domain/CourseOffering.cs after T008 fails.
- [x] T063 [ENTITY-SectionGroup] [OWNER-SPEC-010] Deliver the canonical SectionGroup at src/StudentRegistration.Scheduling/Domain/SectionGroup.cs after T009 fails.
- [x] T064 [ENTITY-MeetingSlot] [OWNER-SPEC-010] Deliver the canonical Lecture/Tutorial/Laboratory MeetingSlot at src/StudentRegistration.Scheduling/Domain/MeetingSlot.cs after T010 fails.
- [x] T065 [ENTITY-Room] [OWNER-SPEC-010] Deliver the canonical Room at src/StudentRegistration.Scheduling/Domain/Room.cs after T011 fails.
- [x] T066 [ENTITY-GroupStaffAssignment] [OWNER-SPEC-010] Deliver the canonical per-meeting/activity GroupStaffAssignment at src/StudentRegistration.Scheduling/Domain/GroupStaffAssignment.cs after T012 fails.
- [x] T067 [ENTITY-StaffTermAvailability] [OWNER-SPEC-010] Deliver the canonical StaffTermAvailability at src/StudentRegistration.Scheduling/Domain/StaffTermAvailability.cs after T013 fails.
- [x] T068 [ENTITY-StaffAvailability] [OWNER-SPEC-010] Deliver the canonical StaffAvailability at src/StudentRegistration.Scheduling/Domain/StaffAvailability.cs after T014 fails.
- [x] T069 [ENTITY-ScheduleImpactAlert] [OWNER-SPEC-010] Deliver the canonical ScheduleImpactAlert at src/StudentRegistration.Scheduling/Domain/ScheduleImpactAlert.cs after T015 fails.
- [x] T070 [FR-1] [FR-2] [WORKSTREAM-OFFERING-AND-GROUP-LIFECYCLE] Deliver offering/group lifecycle and complete activity-bundle enforcement at src/StudentRegistration.Scheduling/Application/OfferingService.cs after T057 fails.
- [x] T071 [FR-3] [FR-8] [FR-10] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Deliver publication conflict/version validation at src/StudentRegistration.Scheduling/Application/OfferingPublicationValidator.cs after T058 fails.
- [x] T072 [FR-4] [FR-5] [FR-9] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Deliver group capacity boundary at src/StudentRegistration.Scheduling/Application/SectionGroupCapacityService.cs after T059 fails.
- [x] T073 [FR-6] [WORKSTREAM-RESOURCE-AND-AVAILABILITY-OWNERSHIP] Deliver room, staff-owned availability, read-only Admin projection, and durable-alert behavior at src/StudentRegistration.Scheduling/Application/ResourceAvailabilityService.cs after T060 fails.
- [x] T074 [FR-7] [WORKSTREAM-TRANSACTIONAL-AUDITED-PUBLICATION] Deliver transactional audited publication at src/StudentRegistration.Scheduling/Application/OfferingPublicationService.cs after T061 fails.

## Phase 5 - Endpoint Handlers After Behavior Tests

- [x] T075 [API-Endpoint01] Deliver GET /api/offerings/{offeringId} at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T017 and T057 fail.
- [x] T076 [API-Endpoint02] Deliver GET /api/groups/{groupId} at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T019 and T057 fail.
- [x] T077 [API-Endpoint03] Deliver GET /api/admin/offerings at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T021 and T057 fail.
- [x] T078 [API-Endpoint04] Deliver POST /api/admin/offerings at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T023 and T057 fail.
- [x] T079 [API-Endpoint05] Deliver PUT /api/admin/groups/{groupId} at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T025, T057, and T059 fail.
- [x] T080 [API-Endpoint06] Deliver POST /api/admin/offerings/{offeringId}/validate at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T027 and T058 fail.
- [x] T081 [API-Endpoint07] Deliver POST /api/admin/offerings/{offeringId}/publish at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T029, T058, and T061 fail.
- [x] T082 [API-Endpoint08] Deliver GET /api/admin/rooms at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T031 and T060 fail.
- [x] T083 [API-Endpoint09] Deliver POST /api/admin/rooms at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T033 and T060 fail.
- [x] T084 [API-Endpoint10] Deliver PUT /api/admin/rooms/{roomId} at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T035 and T060 fail.
- [x] T085 [API-Endpoint11] Deliver GET /api/admin/staff-availability at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T037 and T060 fail.
- [x] T086 [FR-6] [AC-9] Deliver the endpoint-registration exclusion at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T039 and T060 fail, mapping only the Admin availability GET and no Admin availability mutation route.

## Phase 6 - Schedule-Impact Alert API

- [x] T087 [API-Endpoint12] [OWNER-SPEC-010] Finalize GET /api/admin/schedule-impact-alerts in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T088 [P] [API-Endpoint12] Create failing bounded alert list/filter/authorization checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint12ContractTests.cs for GET /api/admin/schedule-impact-alerts.
- [x] T089 [API-Endpoint12] Deliver GET /api/admin/schedule-impact-alerts at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T088 and T060 fail.
- [x] T090 [API-Endpoint13] [OWNER-SPEC-010] Finalize POST /api/admin/schedule-impact-alerts/{alertId}/revalidate in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T091 [P] [API-Endpoint13] Create failing expected alert/group/room/staff versions and durable revalidation-result checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint13ContractTests.cs for POST /api/admin/schedule-impact-alerts/{alertId}/revalidate.
- [x] T092 [API-Endpoint13] Deliver POST /api/admin/schedule-impact-alerts/{alertId}/revalidate at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T091, T058, and T060 fail.
- [x] T093 [API-Endpoint14] [OWNER-SPEC-010] Finalize POST /api/admin/schedule-impact-alerts/{alertId}/resolve in specs/010-offerings-groups-resources/contracts/api.md.
- [x] T094 [P] [API-Endpoint14] Create failing expected-version/reason/passing-revalidation and no-silent-resolution checks in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint14ContractTests.cs for POST /api/admin/schedule-impact-alerts/{alertId}/resolve.
- [x] T095 [API-Endpoint14] Deliver POST /api/admin/schedule-impact-alerts/{alertId}/resolve at src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs after T094 and T060 fail.

## Phase 7 - Frontend Functional Tests and Pages

- [x] T096 [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-9] [AC-3] [AC-6] [AC-7] Finalize the SPEC-010 subject-detail contribution with per-activity type, staff, room/location, day/time, and Tutorial display-label data in specs/010-offerings-groups-resources/contracts/routes/STU-03.md without editing its Razor page.
- [x] T097 [P] [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-9] [AC-3] [AC-6] [AC-7] Verify the contribution in tests/StudentRegistration.E2ETests/Specs/Spec010/SubjectDetailsPageContributorTests.cs.
- [x] T098 [P] [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-4] [AC-5] [AC-6] [AC-8] Create failing offering edit/validate/publish journeys in tests/StudentRegistration.E2ETests/Specs/Spec010/OfferingAdministrationPageFeatureTests.cs.
- [x] T099 [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-4] [AC-5] [AC-6] [AC-8] Deliver OfferingAdministrationPage at src/StudentRegistration.Client/Pages/OfferingAdministrationPage.razor after T098 and SPEC-017 contributor tests fail.
- [x] T100 [P] [ADM-07] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-6] [FR-8] [FR-10] [AC-2] [AC-5] [AC-8] [AC-9] Create failing room, read-only availability view/import, absent edit/override action, impact-list, revalidate, and resolve journeys in tests/StudentRegistration.E2ETests/Specs/Spec010/ResourceAdministrationPageFeatureTests.cs.
- [x] T101 [ADM-07] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-6] [FR-8] [FR-10] [AC-2] [AC-5] [AC-8] [AC-9] Deliver ResourceAdministrationPage at src/StudentRegistration.Client/Pages/ResourceAdministrationPage.razor after T100 and SPEC-017 contributor tests fail.

## Phase 8 - Quality, Scope, and Release Evidence

- [x] T102 [ENTITY-CourseOffering] [ENTITY-SectionGroup] [ENTITY-MeetingSlot] [ENTITY-Room] [ENTITY-GroupStaffAssignment] [ENTITY-StaffTermAvailability] [ENTITY-StaffAvailability] [ENTITY-ScheduleImpactAlert] [PERSISTENCE-MAPPING] [MIGRATION-S2CatalogueScheduling] Create the failing real-SQL scheduling mapping, parent-version, capacity, unique resource, alert state, range, rowversion, and collision-index suite in tests/StudentRegistration.IntegrationTests/Specs/Spec010/SchedulingModelConfigurationTests.cs plus incremental-migration/update/rollback/snapshot parity tests in tests/StudentRegistration.IntegrationTests/Persistence/S2CatalogueSchedulingMigrationTests.cs.
- [x] T103 [ENTITY-CourseOffering] [ENTITY-SectionGroup] [ENTITY-MeetingSlot] [ENTITY-Room] [ENTITY-GroupStaffAssignment] [ENTITY-StaffTermAvailability] [ENTITY-StaffAvailability] [ENTITY-ScheduleImpactAlert] [PERSISTENCE-MAPPING] [MIGRATION-S2CatalogueScheduling] Deliver the complete Scheduling EF Core mapping contribution at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/SchedulingModelConfiguration.cs, generate src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713020000_CatalogueScheduling.cs, and update src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs after T102 and SPEC-009 mapping pass; SPEC-004 remains the sole DbContext writer.
- [x] T104 [P] [NFR-1] Produce offering/group read-load evidence in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-1.md.
- [x] T105 [P] [NFR-2] Produce stable actionable publication-code evidence in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-2.md.
- [x] T106 [P] [NFR-3] Produce term-timezone display evidence in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-3.md.
- [x] T107 [P] [NFR-4] Produce bounded Admin list/filter evidence in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-4.md.
- [x] T108 [OS-1] [OS-2] [OS-3] [OS-4] Record verified scheduling scope exclusions in docs/release-evidence/SPEC-010-scope-review.md.
- [x] T109 [TRACE] [SC-1] [SC-2] [SC-3] Generate the complete FR/NFR/AC/EC/SC/route/entity/endpoint trace matrix in docs/release-evidence/SPEC-010-traceability.md.
- [x] T110 [GATE] Record Admin, staff representative, data, QA, security, accessibility, and operations release approvals in docs/release-evidence/SPEC-010-release-approval.md.

No task is complete and no implementation file has been created.

## Owner-Approved 2026-07-20 Held-Capacity Amendment

These tasks append new work without changing any historical completion record.
Ahmed ELbamby's explicit 2026-07-20 instruction approves the bounded hold and
all-role capacity projection described in the amended specification.

- [ ] T111 [GATE] Record the 2026-07-20 owner-approved held-capacity amendment and rebaseline SPEC-009, SPEC-011, SPEC-014, SPEC-015, SPEC-016, and SPEC-017 dependencies in specs/010-offerings-groups-resources/dependency-baseline.md and checklists/approval.md.
- [ ] T112 [US10] [OWNER-SPEC-010] Amend SectionGroup/capacity contracts for Capacity, EnrolledCount, HeldCount, AvailableCount, privacy-safe role projections, and expected rowversions in specs/010-offerings-groups-resources/contracts/api.md and src/StudentRegistration.Contracts/Scheduling/SchedulingContracts.cs.
- [ ] T113 [P] [US10] [FR-11] [FR-12] Create failing domain and SQL collision tests for hold/enrollment final-seat races, capacity reduction below enrolled plus held, completed subject decisions retaining holds, and atomic plan-approve/plan-reject/window-close terminal races in tests/StudentRegistration.IntegrationTests/Specs/Spec010/HeldCapacityConcurrencyTests.cs.
- [ ] T114 [P] [US10] [DEFECT-LIVE-ENDPOINTS] Create failing real-SQL tests proving the SPEC-010 offering/group/room/alert endpoints return durable results instead of unconditional `SCHEDULING_UNAVAILABLE` in tests/StudentRegistration.IntegrationTests/Specs/Spec010/LiveSchedulingEndpointTests.cs.
- [ ] T115 [US10] [FR-11] Add HeldCount and derived AvailableCount to src/StudentRegistration.Scheduling/Domain/SectionGroup.cs and preserve the amended invariant in src/StudentRegistration.Scheduling/Application/SectionGroupCapacityService.cs after T113 fails.
- [ ] T116 [US10] [PERSISTENCE] Add the held-capacity check constraint, rowversion/index changes, and an incremental EF migration in src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/SchedulingModelConfiguration.cs and src/StudentRegistration.Infrastructure.SqlServer/Migrations after T113 fails.
- [ ] T117 [US10] [DEFECT-LIVE-ENDPOINTS] Implement and register the real SQL offering/resource stores, then wire src/StudentRegistration.Scheduling/Endpoints/Spec010Endpoints.cs to owner services so valid calls no longer fall through to unavailable responses after T114 and T116.
- [ ] T118 [US10] [DEFECT-DEMO-SEED] Seed published offerings, complete activity bundles, rooms, assigned Lecturer/TA staff, and varied capacity/held-count fixtures through src/StudentRegistration.Api/Development/DemoDatabaseInitializer.cs after the SPEC-009 roadmap seed exists.
- [ ] T119 [P] [US10] [ALL-ROLES] Add authorization and contract tests showing Student, Admin, assigned Lecturer, and assigned Teaching Assistant receive the same capacity counts while unassigned/direct-object access and holder PII remain denied in tests/StudentRegistration.AuthorizationTests/HeldCapacityVisibilityTests.cs.
- [ ] T120 [US10] [ADM-06] Replace raw term/course GUID entry with accessible selects/autocomplete and show total/enrolled/held/available values in src/StudentRegistration.Client/Pages/OfferingAdministrationPage.razor.
- [ ] T121 [P] [US10] [STU-02] [STU-03] [STF-01] Add total/enrolled/held/available presentation to student and staff pages/components in src/StudentRegistration.Client/Features/Registration/EligibilityPresentation.razor and src/StudentRegistration.Client/Pages/StaffDashboardPage.razor without holder PII.
- [ ] T122 [P] [US10] [E2E] Add live SQL/browser offering creation, capacity edit, held-seat visibility, and all-role consistency journeys in tests/StudentRegistration.E2ETests/Specs/Spec010/LiveHeldCapacityFeatureTests.cs.
- [ ] T123 [TRACE] [US10] Update held-capacity traceability, scope review, migration evidence, concurrency evidence, and release approval in docs/release-evidence/SPEC-010-traceability.md, docs/release-evidence/SPEC-010-scope-review.md, and docs/release-evidence/SPEC-010-release-approval.md after T111-T122 pass.
