# Tasks: Offerings, Groups, and Resources

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-010 in specs/010-offerings-groups-resources/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-006] Validate the consumed upstream requirements, plan, data model, and API contract at specs/006-domain-class-api-contracts/ and record the accepted versions in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-009] Validate the consumed upstream requirements, plan, data model, and API contract at specs/009-catalog-prerequisites-policy-admin/ and record the accepted versions in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/010-offerings-groups-resources/dependency-baseline.md.
- [ ] T007 [GATE] Freeze SPEC-010 requirements, API, data-model, policy approvals, and dependency versions in specs/010-offerings-groups-resources/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T008 [P] [ENTITY-CourseOffering] [OWNER-SPEC-010] Create the future failing invariant/schema/serialization checks for canonical CourseOffering ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec010/CourseOfferingModelTests.cs.
- [ ] T009 [ENTITY-CourseOffering] [OWNER-SPEC-010] Deliver the canonical CourseOffering model or governed artifact at src/StudentRegistration.Domain/Modules/Scheduling/CourseOffering.cs after T008 fails for the expected reason (depends on T008).
- [ ] T010 [P] [ENTITY-SectionGroup] [OWNER-SPEC-010] Create the future failing invariant/schema/serialization checks for canonical SectionGroup ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec010/SectionGroupModelTests.cs.
- [ ] T011 [ENTITY-SectionGroup] [OWNER-SPEC-010] Deliver the canonical SectionGroup model or governed artifact at src/StudentRegistration.Domain/Modules/Scheduling/SectionGroup.cs after T010 fails for the expected reason (depends on T010).
- [ ] T012 [P] [ENTITY-MeetingSlot] [OWNER-SPEC-010] Create the future failing invariant/schema/serialization checks for canonical MeetingSlot ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec010/MeetingSlotModelTests.cs.
- [ ] T013 [ENTITY-MeetingSlot] [OWNER-SPEC-010] Deliver the canonical MeetingSlot model or governed artifact at src/StudentRegistration.Domain/Modules/Scheduling/MeetingSlot.cs after T012 fails for the expected reason (depends on T012).
- [ ] T014 [P] [ENTITY-Room] [OWNER-SPEC-010] Create the future failing invariant/schema/serialization checks for canonical Room ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec010/RoomModelTests.cs.
- [ ] T015 [ENTITY-Room] [OWNER-SPEC-010] Deliver the canonical Room model or governed artifact at src/StudentRegistration.Domain/Modules/Scheduling/Room.cs after T014 fails for the expected reason (depends on T014).
- [ ] T016 [P] [ENTITY-GroupStaffAssignment] [OWNER-SPEC-010] Create the future failing invariant/schema/serialization checks for canonical GroupStaffAssignment ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec010/GroupStaffAssignmentModelTests.cs.
- [ ] T017 [ENTITY-GroupStaffAssignment] [OWNER-SPEC-010] Deliver the canonical GroupStaffAssignment model or governed artifact at src/StudentRegistration.Domain/Modules/Scheduling/GroupStaffAssignment.cs after T016 fails for the expected reason (depends on T016).
- [ ] T018 [P] [ENTITY-StaffAvailability] [CONSUMER-SPEC-016] Verify SPEC-010 consumes the canonical StaffAvailability at src/StudentRegistration.Domain/Modules/StaffAdministration/StaffAvailability.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec010/StaffAvailabilityModelTests.cs.
- [ ] T019 [API-Endpoint01] [OWNER-SPEC-010] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/offerings/{id} in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T020 [P] [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/offerings/{id} in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint01ContractTests.cs.
- [ ] T021 [API-Endpoint01] [OWNER-SPEC-010] Deliver the sole canonical GET /api/offerings/{id} handler at src/StudentRegistration.Server/Modules/Scheduling/Endpoints/Spec010Endpoints.cs after T020 fails for the expected reason (depends on T020).
- [ ] T022 [API-Endpoint02] [OWNER-SPEC-010] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/groups/{id} in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T023 [P] [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/groups/{id} in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint02ContractTests.cs.
- [ ] T024 [API-Endpoint02] [OWNER-SPEC-010] Deliver the sole canonical GET /api/groups/{id} handler at src/StudentRegistration.Server/Modules/Scheduling/Endpoints/Spec010Endpoints.cs after T023 fails for the expected reason (depends on T023).
- [ ] T025 [API-Endpoint03] [OWNER-SPEC-010] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/admin/offerings in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T026 [P] [API-Endpoint03] Verify every documented response and authorization outcome for POST /api/admin/offerings in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint03ContractTests.cs.
- [ ] T027 [API-Endpoint03] [OWNER-SPEC-010] Deliver the sole canonical POST /api/admin/offerings handler at src/StudentRegistration.Server/Modules/Scheduling/Endpoints/Spec010Endpoints.cs after T026 fails for the expected reason (depends on T026).
- [ ] T028 [API-Endpoint04] [OWNER-SPEC-010] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for PUT /api/admin/groups/{id} in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T029 [P] [API-Endpoint04] Verify every documented response and authorization outcome for PUT /api/admin/groups/{id} in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint04ContractTests.cs.
- [ ] T030 [API-Endpoint04] [OWNER-SPEC-010] Deliver the sole canonical PUT /api/admin/groups/{id} handler at src/StudentRegistration.Server/Modules/Scheduling/Endpoints/Spec010Endpoints.cs after T029 fails for the expected reason (depends on T029).
- [ ] T031 [API-Endpoint05] [OWNER-SPEC-010] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/admin/offerings/{id}/validate in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T032 [P] [API-Endpoint05] Verify every documented response and authorization outcome for POST /api/admin/offerings/{id}/validate in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint05ContractTests.cs.
- [ ] T033 [API-Endpoint05] [OWNER-SPEC-010] Deliver the sole canonical POST /api/admin/offerings/{id}/validate handler at src/StudentRegistration.Server/Modules/Scheduling/Endpoints/Spec010Endpoints.cs after T032 fails for the expected reason (depends on T032).
- [ ] T034 [API-Endpoint06] [OWNER-SPEC-010] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/admin/offerings/{id}/publish in specs/010-offerings-groups-resources/contracts/api.md.
- [ ] T035 [P] [API-Endpoint06] Verify every documented response and authorization outcome for POST /api/admin/offerings/{id}/publish in tests/StudentRegistration.ContractTests/Specs/Spec010/Endpoint06ContractTests.cs.
- [ ] T036 [API-Endpoint06] [OWNER-SPEC-010] Deliver the sole canonical POST /api/admin/offerings/{id}/publish handler at src/StudentRegistration.Server/Modules/Scheduling/Endpoints/Spec010Endpoints.cs after T035 fails for the expected reason (depends on T035).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Valid group publish (FR-1, FR-2, FR-3) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Offerings, Groups, and Resources.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T036.
- [ ] T037 [P] [AC-1] [FR-1] [FR-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-1Tests.cs for AC-1: Valid group publish (FR-1, FR-2, FR-3): Given a group has capacity 30, room capacity 35, valid times, and available Lecturer/TA When Admin validates and publishes Then the group becomes visible to eligible students with all details.
### US2 - Room overlap (FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Offerings, Groups, and Resources.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T036.
- [ ] T038 [P] [AC-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-2Tests.cs for AC-2: Room overlap (FR-3): Given two groups use the same room at overlapping times When publication is attempted Then publication is blocked with both groups and overlap interval.
### US3 - Full group (FR-4) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Offerings, Groups, and Resources.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T036.
- [ ] T039 [P] [AC-3] [FR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-3Tests.cs for AC-3: Full group (FR-4): Given EnrolledCount equals Capacity When a student opens group details Then group is marked Full and cannot be selected.
### US4 - Audited transactional publish/capacity edit (FR-5, FR-6, FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Offerings, Groups, and Resources.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T036.
- [ ] T040 [P] [AC-4] [FR-5] [FR-6] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-4Tests.cs for AC-4: Audited transactional publish/capacity edit (FR-5, FR-6, FR-7): Given an authorized Admin has current versions and a capacity not below active enrollment When offering publication or capacity change is committed Then the complete change is transactional and audited And stale/invalid capacity is rejected without partial publication.
### US5 - Concurrent room publication (FR-3, FR-7, FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Offerings, Groups, and Resources.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T036.
- [ ] T041 [P] [AC-5] [FR-3] [FR-7] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-5Tests.cs for AC-5: Concurrent room publication (FR-3, FR-7, FR-8): Given two draft groups request the same room and overlapping meeting interval When separate admins publish them concurrently Then exactly one group may publish And the loser receives 409 RESOURCE_CONFLICT after in-transaction revalidation.
### US6 - Capacity reduction races allocation (FR-5, FR-9) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Offerings, Groups, and Resources.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T036.
- [ ] T042 [P] [AC-6] [FR-5] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-6Tests.cs for AC-6: Capacity reduction races allocation (FR-5, FR-9): Given one seat remains and an admin attempts to reduce capacity while one eligible student submits When both operations contend on the SectionGroup boundary Then either serialized outcome may win But Capacity is never below EnrolledCount and EnrolledCount never exceeds Capacity.
### US7 - Offering read and presentation quality (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Offerings, Groups, and Resources.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T036.
- [ ] T043 [P] [AC-7] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-7Tests.cs for AC-7: Offering read and presentation quality (NFR-1, NFR-2, NFR-3, NFR-4): Given the approved read-load dataset, invalid publication fixtures, two configured timezones, and a large admin list When performance, reason-code, time-display, and pagination tests execute Then offering reads are at most 300 ms p95 And validation returns stable actionable codes And meeting display uses the term timezone unambiguously And admin lists remain bounded, paged, and filtered.
### US8 - Child schedule mutation advances group version (FR-8, FR-10) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Offerings, Groups, and Resources.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T036.
- [ ] T044 [P] [AC-8] [FR-8] [FR-10] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec010/AC-8Tests.cs for AC-8: Child schedule mutation advances group version (FR-8, FR-10): Given a registration has captured SectionGroup rowversion 8 When an admin changes one meeting slot, room, staff assignment, or group state and commits rowversion 9 Then the registration re-read detects GROUP_CHANGED and cannot enroll against rowversion 8 And concurrent availability/publication uses one valid staff-term serial order.
- [ ] T045 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-1Tests.cs and assert: Multi-slot group has one invalid slot -> entire group cannot publish.
- [ ] T046 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-2Tests.cs and assert: Capacity change races with enrollment -> transaction/rowversion preserves Capacity >= EnrolledCount.
- [ ] T047 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-3Tests.cs and assert: Staff becomes unavailable after publish -> flag affected group for admin resolution; do not silently move the class.
- [ ] T048 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-4Tests.cs and assert: Overnight meeting slot -> reject in MVP unless separately specified.
- [ ] T049 [P] [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec010/EdgeCases/EC-5Tests.cs and assert: A transaction touches multiple rooms/staff/groups -> acquire every resource lock in stable type-and-ID order; a deadlock retry reruns the whole idempotent transaction, never a partial publication.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T050 [P] [FR-1] [WORKSTREAM-OFFERING-AND-GROUP-LIFECYCLE] Create the future failing FR-1 checks in tests/StudentRegistration.IntegrationTests/Scheduling/OfferingLifecycleTests.cs. Test focus: required group code/capacity/state/meetings/room/Lecturer/TA data. Prove the requirement against its linked AC/EC fixtures: Admin MUST create term course offerings and one or more section groups.
- [ ] T051 [FR-1] [WORKSTREAM-OFFERING-AND-GROUP-LIFECYCLE] Deliver FR-1 through the bounded Offering and group lifecycle workstream at src/StudentRegistration.Server/Modules/Scheduling/OfferingService.cs only after T050 fails for the expected reason (depends on T050): Admin MUST create term course offerings and one or more section groups.
- [ ] T052 [P] [FR-2] [WORKSTREAM-OFFERING-AND-GROUP-LIFECYCLE] Create the future failing FR-2 checks in tests/StudentRegistration.IntegrationTests/Scheduling/OfferingLifecycleTests.cs. Test focus: required group code/capacity/state/meetings/room/Lecturer/TA data. Prove the requirement against its linked AC/EC fixtures: Each group MUST have code, capacity, state, meeting slots, room(s), and required Lecturer/TA assignments before publish.
- [ ] T053 [FR-2] [WORKSTREAM-OFFERING-AND-GROUP-LIFECYCLE] Deliver FR-2 through the bounded Offering and group lifecycle workstream at src/StudentRegistration.Server/Modules/Scheduling/OfferingService.cs only after T052 fails for the expected reason (depends on T052): Each group MUST have code, capacity, state, meeting slots, room(s), and required Lecturer/TA assignments before publish.
- [ ] T054 [P] [FR-3] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Scheduling/ResourcePublicationRaceTests.cs. Test focus: room/staff/availability/capacity overlaps, stable lock order and parent group version propagation. Prove the requirement against its linked AC/EC fixtures: Publish validation MUST reject staff overlap/unavailability, room overlap/unavailability, room capacity below group capacity, invalid slots, missing roles, and duplicate offering/group codes.
- [ ] T055 [FR-3] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Deliver FR-3 through the bounded Publication conflict validation workstream at src/StudentRegistration.Server/Modules/Scheduling/OfferingPublicationValidator.cs only after T054 fails for the expected reason (depends on T054): Publish validation MUST reject staff overlap/unavailability, room overlap/unavailability, room capacity below group capacity, invalid slots, missing roles, and duplicate offering/group codes.
- [ ] T056 [P] [FR-4] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Create the future failing FR-4 checks in tests/StudentRegistration.IntegrationTests/Scheduling/GroupCapacityRaceTests.cs. Test focus: published/selectable state and shared SectionGroup serialization with registration. Prove the requirement against its linked AC/EC fixtures: Students MUST NOT select full, unpublished, cancelled, or closed groups.
- [ ] T057 [FR-4] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Deliver FR-4 through the bounded Group selection and capacity boundary workstream at src/StudentRegistration.Server/Modules/Scheduling/SectionGroupCapacityService.cs only after T056 fails for the expected reason (depends on T056): Students MUST NOT select full, unpublished, cancelled, or closed groups.
- [ ] T058 [P] [FR-5] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Create the future failing FR-5 checks in tests/StudentRegistration.IntegrationTests/Scheduling/GroupCapacityRaceTests.cs. Test focus: published/selectable state and shared SectionGroup serialization with registration. Prove the requirement against its linked AC/EC fixtures: Capacity MUST NOT be set below active EnrolledCount.
- [ ] T059 [FR-5] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Deliver FR-5 through the bounded Group selection and capacity boundary workstream at src/StudentRegistration.Server/Modules/Scheduling/SectionGroupCapacityService.cs only after T058 fails for the expected reason (depends on T058): Capacity MUST NOT be set below active EnrolledCount.
- [ ] T060 [P] [FR-6] [WORKSTREAM-TRANSACTIONAL-AUDITED-PUBLICATION] Create the future failing FR-6 checks in tests/StudentRegistration.IntegrationTests/Scheduling/OfferingPublicationTransactionTests.cs. Test focus: rowversion, audit and complete rollback for multi-resource publication. Prove the requirement against its linked AC/EC fixtures: Staff assignments/availability and resource changes MUST be optimistic-concurrency protected and audited.
- [ ] T061 [FR-6] [WORKSTREAM-TRANSACTIONAL-AUDITED-PUBLICATION] Deliver FR-6 through the bounded Transactional audited publication workstream at src/StudentRegistration.Server/Modules/Scheduling/OfferingPublicationService.cs only after T060 fails for the expected reason (depends on T060): Staff assignments/availability and resource changes MUST be optimistic-concurrency protected and audited.
- [ ] T062 [P] [FR-7] [WORKSTREAM-TRANSACTIONAL-AUDITED-PUBLICATION] Create the future failing FR-7 checks in tests/StudentRegistration.IntegrationTests/Scheduling/OfferingPublicationTransactionTests.cs. Test focus: rowversion, audit and complete rollback for multi-resource publication. Prove the requirement against its linked AC/EC fixtures: Publication MUST be transactional.
- [ ] T063 [FR-7] [WORKSTREAM-TRANSACTIONAL-AUDITED-PUBLICATION] Deliver FR-7 through the bounded Transactional audited publication workstream at src/StudentRegistration.Server/Modules/Scheduling/OfferingPublicationService.cs only after T062 fails for the expected reason (depends on T062): Publication MUST be transactional.
- [ ] T064 [P] [FR-8] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Scheduling/ResourcePublicationRaceTests.cs. Test focus: room/staff/availability/capacity overlaps, stable lock order and parent group version propagation. Prove the requirement against its linked AC/EC fixtures: Publication MUST lock every touched offering, room, and staff resource in stable identifier order and revalidate overlaps/availability inside the same transaction to prevent concurrent write skew.
- [ ] T065 [FR-8] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Deliver FR-8 through the bounded Publication conflict validation workstream at src/StudentRegistration.Server/Modules/Scheduling/OfferingPublicationValidator.cs only after T064 fails for the expected reason (depends on T064): Publication MUST lock every touched offering, room, and staff resource in stable identifier order and revalidate overlaps/availability inside the same transaction to prevent concurrent write skew.
- [ ] T066 [P] [FR-9] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Create the future failing FR-9 checks in tests/StudentRegistration.IntegrationTests/Scheduling/GroupCapacityRaceTests.cs. Test focus: published/selectable state and shared SectionGroup serialization with registration. Prove the requirement against its linked AC/EC fixtures: Capacity edits and registration seat allocation MUST serialize on the same SectionGroup database row/version and preserve 0 <= EnrolledCount <= Capacity for every outcome.
- [ ] T067 [FR-9] [WORKSTREAM-GROUP-SELECTION-AND-CAPACITY-BOUNDARY] Deliver FR-9 through the bounded Group selection and capacity boundary workstream at src/StudentRegistration.Server/Modules/Scheduling/SectionGroupCapacityService.cs only after T066 fails for the expected reason (depends on T066): Capacity edits and registration seat allocation MUST serialize on the same SectionGroup database row/version and preserve 0 <= EnrolledCount <= Capacity for every outcome.
- [ ] T068 [P] [FR-10] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Create the future failing FR-10 checks in tests/StudentRegistration.IntegrationTests/Scheduling/ResourcePublicationRaceTests.cs. Test focus: room/staff/availability/capacity overlaps, stable lock order and parent group version propagation. Prove the requirement against its linked AC/EC fixtures: Every group-state, meeting-slot, room-assignment, and staff-assignment mutation MUST lock and advance the owning SectionGroup rowversion. Staff availability mutation and group publication MUST also share the versioned staff-term availability boundary defined by SPEC-016.
- [ ] T069 [FR-10] [WORKSTREAM-PUBLICATION-CONFLICT-VALIDATION] Deliver FR-10 through the bounded Publication conflict validation workstream at src/StudentRegistration.Server/Modules/Scheduling/OfferingPublicationValidator.cs only after T068 fails for the expected reason (depends on T068): Every group-state, meeting-slot, room-assignment, and staff-assignment mutation MUST lock and advance the owning SectionGroup rowversion. Staff availability mutation and group publication MUST also share the versioned staff-term availability boundary defined by SPEC-016.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T070 [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-9] [AC-3] [AC-6] [AC-7] Finalize SPEC-010 data, actions, stable reasons, authorization, and stale/concurrent contribution for STU-03 at specs/010-offerings-groups-resources/contracts/routes/STU-03.md without editing the canonical Razor page.
- [ ] T071 [P] [STU-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-9] [AC-3] [AC-6] [AC-7] Verify the SPEC-010 contribution consumed by STU-03 in tests/StudentRegistration.E2ETests/Specs/Spec010/SubjectDetailsPageContributorTests.cs.
- [ ] T072 [P] [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-4] [AC-5] [AC-6] [AC-8] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-06 in tests/StudentRegistration.E2ETests/Specs/Spec010/OfferingAdministrationPageFeatureTests.cs.
- [ ] T073 [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-5] [FR-6] [FR-7] [FR-8] [FR-9] [FR-10] [AC-1] [AC-2] [AC-4] [AC-5] [AC-6] [AC-8] Deliver the sole canonical Blazor implementation for ADM-06 at src/StudentRegistration.Client/Pages/OfferingAdministrationPage.razor after T072 and the SPEC-003 contract/component checks fail for expected reasons (depends on T072).
- [ ] T074 [P] [ADM-07] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-6] [FR-8] [FR-10] [AC-2] [AC-5] [AC-8] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-07 in tests/StudentRegistration.E2ETests/Specs/Spec010/ResourceAdministrationPageFeatureTests.cs.
- [ ] T075 [ADM-07] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-6] [FR-8] [FR-10] [AC-2] [AC-5] [AC-8] Deliver the sole canonical Blazor implementation for ADM-07 at src/StudentRegistration.Client/Pages/ResourceAdministrationPage.razor after T074 and the SPEC-003 contract/component checks fail for expected reasons (depends on T074).

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T076 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-1.md: Offering/group reads SHOULD complete within 300 ms p95 at the SPEC-018 300-read-requests-per-second target.
- [ ] T077 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-2.md: Publication validation MUST produce stable actionable reason codes.
- [ ] T078 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-3.md: Meeting display MUST use term timezone and unambiguous day/time.
- [ ] T079 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec010/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-010-NFR-4.md: Large admin lists MUST be paged/filtered.

## Phase 7 - Scope and Release Evidence

- [ ] T080 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-010-scope-review.md that OS-1 remains excluded: Institution-wide timetable generation.
- [ ] T081 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-010-scope-review.md that OS-2 remains excluded: Automatic reassignment of Lecturer, TA, or room for one student.
- [ ] T082 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-010-scope-review.md that OS-3 remains excluded: Waitlist and seat reservation.
- [ ] T083 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-010-scope-review.md that OS-4 remains excluded: Normal admin force-over-capacity action.
- [ ] T084 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-010-traceability.md and reject release if any row lacks passing evidence.
- [ ] T085 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-010 in docs/release-evidence/SPEC-010-release-approval.md.

No task is complete and no implementation file has been created.
