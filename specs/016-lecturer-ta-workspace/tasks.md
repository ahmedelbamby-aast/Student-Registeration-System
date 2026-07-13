# Tasks: Lecturer and Teaching Assistant Workspace

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Planning Analysis, Dependency Baseline, and Final Human Approval

- [ ] T001 [GATE] Run and record the constitution-compliance review for SPEC-016 in specs/016-lecturer-ta-workspace/checklists/approval.md; this is planning analysis and does not authorize implementation.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-007] Validate the consumed upstream requirements, plan, data model, and API contract at specs/007-identity-account-lifecycle/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-010] Validate the consumed upstream requirements, plan, data model, and API contract at specs/010-offerings-groups-resources/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-015] Validate the consumed upstream requirements, plan, data model, and API contract at specs/015-student-registration-records/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T007 [GATE] Complete dependency validation, cross-spec consistency analysis, model/API/policy/task trace review, and freeze SPEC-016 in specs/016-lecturer-ta-workspace/checklists/implementation-readiness.md; record pass/fail and keep the package In Review. This readiness task does not authorize implementation.
- [ ] T008 [GATE] Only after T001-T007 pass, record Ahmed ELbamby's human approval in specs/016-lecturer-ta-workspace/checklists/approval.md and update the package status to Approved; no later model, test, source, migration, page, or deployment task may begin before this final planning gate is complete.

## Phase 2 - Models and API Contracts

- [ ] T009 [ENTITY-StaffAssignment] [OWNER-SPEC-016] Create future failing projection/schema tests for StaffAssignmentDto over SPEC-010 GroupStaffAssignment in tests/StudentRegistration.IntegrationTests/Specs/Spec016/StaffAssignmentModelTests.cs; prove no duplicate persistence entity is introduced.
- [ ] T010 [ENTITY-StaffAssignment] [OWNER-SPEC-016] Deliver the canonical StaffAssignment workspace read value at src/StudentRegistration.StaffAdministration/Domain/StaffAssignment.cs after T009 fails; it projects SPEC-010 assignment data without duplicate persistence (depends on T009).
- [ ] T011 [ENTITY-StaffTermAvailability] [CONSUMER-SPEC-010] Verify in tests/StudentRegistration.IntegrationTests/Specs/Spec016/StaffTermAvailabilityModelTests.cs that SPEC-016 consumes the canonical src/StudentRegistration.Scheduling/Domain/StaffTermAvailability.cs aggregate and edits it only through the Scheduling application port.
- [ ] T012 [ENTITY-StaffTermAvailability] [CONSUMER-SPEC-010] Create future failing port contract tests in tests/StudentRegistration.ApplicationTests/Staff/SchedulingAvailabilityPortTests.cs for complete-range replacement, expected aggregate rowversion, server deadline, and no StaffAdministration persistence mapping (depends on T011).
- [ ] T013 [ENTITY-StaffAvailability] [CONSUMER-SPEC-010] Verify in tests/StudentRegistration.IntegrationTests/Specs/Spec016/StaffAvailabilityModelTests.cs that StaffAvailability is the child value at src/StudentRegistration.Scheduling/Domain/StaffAvailability.cs and cannot be independently inserted, updated, or deleted.
- [ ] T014 [ENTITY-StaffAvailability] [CONSUMER-SPEC-010] Create future failing architecture tests in tests/StudentRegistration.ArchitectureTests/StaffAvailabilityOwnershipTests.cs that reject any duplicate StaffAdministration availability entity/repository or bypass of the Scheduling port (depends on T013).
- [ ] T015 [ENTITY-RosterRow] [OWNER-SPEC-016] Create future failing schema/serialization/privacy tests in tests/StudentRegistration.IntegrationTests/Specs/Spec016/RosterRowModelTests.cs proving RosterRowDto contains only UniversityId, DisplayName, and EnrollmentState and Page<T> enforces default 20/max 100/stable order.
- [ ] T016 [ENTITY-RosterRow] [OWNER-SPEC-016] Deliver the canonical minimal RosterRow read value at src/StudentRegistration.StaffAdministration/Domain/RosterRow.cs after T015 fails; API projection remains exactly UniversityId, DisplayName, and EnrollmentState (depends on T015).
- [ ] T017 [ENTITY-ScheduleImpactAlert] [CONSUMER-SPEC-010] Verify SPEC-016 consumes the canonical ScheduleImpactAlert at src/StudentRegistration.Scheduling/Domain/ScheduleImpactAlert.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec016/ScheduleImpactAlertModelTests.cs.
- [ ] T018 [API-Endpoint01] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/staff/assignments in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T019 [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/staff/assignments in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint01ContractTests.cs.
- [ ] T020 [API-Endpoint01] [FR-1] [FR-2] [FR-3] [FR-4] Create future failing assignments ownership/scope/dual-role endpoint behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec016/Endpoint01BehaviorTests.cs; handler delivery is deferred until linked AC/FR tests fail.
- [ ] T021 [API-Endpoint02] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/staff/timetable in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T022 [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/staff/timetable in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint02ContractTests.cs.
- [ ] T023 [API-Endpoint02] [FR-2] [FR-3] [FR-4] Create future failing timetable scope, equivalent list, assignment removal, and empty-state endpoint behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec016/Endpoint02BehaviorTests.cs; handler delivery is deferred.
- [ ] T024 [API-Endpoint03] [OWNER-SPEC-016] Finalize GET /api/staff/groups/{groupId}/roster in specs/016-lecturer-ta-workspace/contracts/api.md as Page<RosterRowDto>, with page/pageSize query parameters, only UniversityId/DisplayName/EnrollmentState, default 20/max 100, stable sort, assignment preauthorization, audit metadata, and privacy-safe errors.
- [ ] T025 [API-Endpoint03] Verify every documented response and authorization outcome for GET /api/staff/groups/{groupId}/roster in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint03ContractTests.cs.
- [ ] T026 [API-Endpoint03] [FR-2] [FR-5] [NFR-3] Create future failing roster paging, exact-field allow-list, pre-query assignment denial, stable sort, and safe audit behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec016/Endpoint03BehaviorTests.cs; handler delivery is deferred.
- [ ] T027 [API-Endpoint04] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/staff/availability in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T028 [API-Endpoint04] Verify every documented response and authorization outcome for GET /api/staff/availability in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint04ContractTests.cs.
- [ ] T029 [API-Endpoint04] [FR-6] [FR-9] Create future failing authenticated-own availability and consumed Scheduling aggregate endpoint behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec016/Endpoint04BehaviorTests.cs; handler delivery is deferred.
- [ ] T030 [API-Endpoint05] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for PUT /api/staff/availability in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T031 [API-Endpoint05] Verify every documented response and authorization outcome for PUT /api/staff/availability in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint05ContractTests.cs.
- [ ] T032 [API-Endpoint05] [FR-6] [FR-8] [FR-9] [FR-10] Create future failing Scheduling-port, complete replacement, deadline/stale version, atomic impact-alert, and no-automatic-move endpoint behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec016/Endpoint05BehaviorTests.cs; handler delivery is deferred.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Scoped roster (FR-2, FR-5) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T032.
- [ ] T033 [SC-1] [AC-1] [FR-2] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-1Tests.cs for AC-1: Scoped roster (FR-2, FR-5): Given a TA is assigned to Group A but not Group B When the TA requests Group A and Group B rosters Then Group A is returned with one bounded page with UniversityId, DisplayName, and EnrollmentState only And Group B is denied with no data.
### US2 - Shared page, different scope (FR-1, FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T032.
- [ ] T034 [SC-2] [AC-2] [FR-1] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-2Tests.cs for AC-2: Shared page, different scope (FR-1, FR-3): Given Lecturer and TA users open the same assignments route When server responses are rendered Then each sees only the server-authorized lecture/tutorial/lab assignments defined by current GroupStaffAssignment records And the role context is stated near the page heading.
### US3 - Availability deadline (FR-6) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T032.
- [ ] T035 [AC-3] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-3Tests.cs for AC-3: Availability deadline (FR-6): Given the availability deadline has passed When staff attempts an update Then the command is rejected with deadline/server time And existing availability remains unchanged.
### US4 - Staff detail and privilege boundary (FR-4, FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T032.
- [ ] T036 [AC-4] [FR-4] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-4Tests.cs for AC-4: Staff detail and privilege boundary (FR-4, FR-7): Given staff opens an assigned group and attempts an Admin capacity route When both requests are authorized Then assigned subject/staff/room/time/capacity details are returned And the Admin operation is denied.
### US5 - Post-publication availability warning (FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T032.
- [ ] T037 [SC-3] [AC-5] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-5Tests.cs for AC-5: Post-publication availability warning (FR-8): Given an approved availability change conflicts with a published assignment When the change is saved through the allowed process Then Admin receives an affected-group warning And no class, room or staff assignment moves automatically.
### US6 - Concurrent availability insert (FR-6, FR-9, FR-10) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T032.
- [ ] T038 [AC-6] [FR-6] [FR-9] [FR-10] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-6Tests.cs for AC-6: Concurrent availability insert (FR-6, FR-9, FR-10): Given two clients load the same staff-term availability version When they concurrently add overlapping ranges Then exactly one complete aggregate update succeeds And the loser receives 409 STALE_VERSION with the current range set.
### US7 - Availability races group publication (FR-8, FR-10) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T032.
- [ ] T039 [AC-7] [FR-8] [FR-10] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-7Tests.cs for AC-7: Availability races group publication (FR-8, FR-10): Given an availability update conflicts with a group being published for that staff member When both transactions execute concurrently Then one valid serial order is recorded And an affected published group is never silently left without a warning and revalidation state.
### US8 - Staff workspace quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T032.
- [ ] T040 [AC-8] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-8Tests.cs for AC-8: Staff workspace quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given approved read load, direct-object authorization matrix, privacy/audit inspection, and keyboard/calendar-list fixtures When workspace quality tests execute Then reads are at most 300 ms p95 And every unassigned object is denied without data And rosters contain only approved fields with safe audit metadata And timetable/availability is fully keyboard operable with a list/table alternative.
- [ ] T041 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-1Tests.cs and assert: Staff has both Lecturer and TA assignments -> display authorized contexts without duplicate group entries.
- [ ] T042 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-2Tests.cs and assert: Assignment removed while page open -> stale refresh denies roster.
- [ ] T043 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-3Tests.cs and assert: Concurrent availability edit -> stale version gets 409.
- [ ] T044 [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-4Tests.cs and assert: No assignments -> clear empty state, no broad search access.
- [ ] T045 [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-5Tests.cs and assert: Deadline passes after the page loads -> in-transaction server-time validation rejects the update with AVAILABILITY_DEADLINE_PASSED.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T046 [FR-1] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-1 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, exact roster DTO, bounded Page, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Lecturer and TA MUST use the shared staff login and shared workspace templates.
- [ ] T047 [FR-1] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-1 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs only after T046 fails for the expected reason (depends on T046): Lecturer and TA MUST use the shared staff login and shared workspace templates.
- [ ] T048 [FR-2] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-2 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, exact roster DTO, bounded Page, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: The API MUST scope assignments/timetable/rosters to the authenticated staff user's current assignments.
- [ ] T049 [FR-2] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-2 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs only after T048 fails for the expected reason (depends on T048): The API MUST scope assignments/timetable/rosters to the authenticated staff user's current assignments.
- [ ] T050 [FR-3] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-3 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, exact roster DTO, bounded Page, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Lecturer MUST see assigned lecture groups; TA MUST see assigned tutorial/lab groups according to server data.
- [ ] T051 [FR-3] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-3 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs only after T050 fails for the expected reason (depends on T050): Lecturer MUST see assigned lecture groups; TA MUST see assigned tutorial/lab groups according to server data.
- [ ] T052 [FR-4] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-4 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, exact roster DTO, bounded Page, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Staff MUST view group code, subject, role partners, room, meeting slots, capacity and roster count.
- [ ] T053 [FR-4] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-4 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs only after T052 fails for the expected reason (depends on T052): Staff MUST view group code, subject, role partners, room, meeting slots, capacity and roster count.
- [ ] T054 [FR-5] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-5 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, exact roster DTO, bounded Page, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Staff MAY view only a bounded page of UniversityId, DisplayName, and EnrollmentState for an assigned group. GPA, standing, holds, contact data, grades, transcript, and unrelated identifiers MUST NOT be returned. The list defaults to 20, caps at 100, and is stable-sorted by DisplayName then UniversityId.
- [ ] T055 [FR-5] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-5 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs only after T054 fails for the expected reason (depends on T054): Staff MAY view only a bounded page of UniversityId, DisplayName, and EnrollmentState for an assigned group. GPA, standing, holds, contact data, grades, transcript, and unrelated identifiers MUST NOT be returned. The list defaults to 20, caps at 100, and is stable-sorted by DisplayName then UniversityId.
- [ ] T056 [FR-6] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Create the future failing FR-6 checks in tests/StudentRegistration.IntegrationTests/Staff/StaffAvailabilityConcurrencyTests.cs. Test focus: consume SPEC-010 Scheduling aggregate for complete range replacement, rowversion, overlap, deadline and concurrent one-winner behavior. Prove the requirement against its linked AC/EC fixtures: Staff MUST create/edit their own availability before the server-time deadline through the Scheduling application port owned by SPEC-010, using the expected StaffTermAvailability rowversion and complete-range replacement; SPEC-016 MUST NOT redefine or bypass that aggregate.
- [ ] T057 [FR-6] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Deliver FR-6 through the bounded Versioned availability aggregate workstream at src/StudentRegistration.StaffAdministration/Application/StaffAvailabilityFacade.cs only after T056 fails for the expected reason (depends on T056): Staff MUST create/edit their own availability before the server-time deadline through the Scheduling application port owned by SPEC-010, using the expected StaffTermAvailability rowversion and complete-range replacement; SPEC-016 MUST NOT redefine or bypass that aggregate.
- [ ] T058 [FR-7] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-7 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, exact roster DTO, bounded Page, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Staff MUST NOT manage policy, users, capacity, terms, or unrelated rosters.
- [ ] T059 [FR-7] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-7 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.StaffAdministration/Application/StaffWorkspaceQueries.cs only after T058 fails for the expected reason (depends on T058): Staff MUST NOT manage policy, users, capacity, terms, or unrelated rosters.
- [ ] T060 [FR-8] [WORKSTREAM-PUBLISHED-SCHEDULE-IMPACT] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Staff/AvailabilityPublicationRaceTests.cs. Test focus: durable Scheduling-owned affected-group alert, revalidation state and no silent class move. Prove the requirement against its linked AC/EC fixtures: If an accepted availability change conflicts with a published assignment, the same transaction MUST create or update a durable ScheduleImpactAlert containing term, staff, affected group, availability and group versions, detected time, reason, and revalidation state. Admin MUST be able to discover it through SPEC-017; no class moves automatically.
- [ ] T061 [FR-8] [WORKSTREAM-PUBLISHED-SCHEDULE-IMPACT] Deliver FR-8 through the bounded Published schedule impact workstream at src/StudentRegistration.StaffAdministration/Application/ScheduleImpactQuery.cs only after T060 fails for the expected reason (depends on T060): If an accepted availability change conflicts with a published assignment, the same transaction MUST create or update a durable ScheduleImpactAlert containing term, staff, affected group, availability and group versions, detected time, reason, and revalidation state. Admin MUST be able to discover it through SPEC-017; no class moves automatically.
- [ ] T062 [FR-9] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Create the future failing FR-9 checks in tests/StudentRegistration.IntegrationTests/Staff/StaffAvailabilityConcurrencyTests.cs. Test focus: consume SPEC-010 Scheduling aggregate for complete range replacement, rowversion, overlap, deadline and concurrent one-winner behavior. Prove the requirement against its linked AC/EC fixtures: SPEC-016 MUST consume SPEC-010's versioned staff-plus-term StaffTermAvailability aggregate. Edits MUST validate the complete range set and replace it atomically through the Scheduling port; independent child inserts/updates/deletes are prohibited.
- [ ] T063 [FR-9] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Deliver FR-9 through the bounded Versioned availability aggregate workstream at src/StudentRegistration.StaffAdministration/Application/StaffAvailabilityFacade.cs only after T062 fails for the expected reason (depends on T062): SPEC-016 MUST consume SPEC-010's versioned staff-plus-term StaffTermAvailability aggregate. Edits MUST validate the complete range set and replace it atomically through the Scheduling port; independent child inserts/updates/deletes are prohibited.
- [ ] T064 [FR-10] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Create the future failing FR-10 checks in tests/StudentRegistration.IntegrationTests/Staff/StaffAvailabilityConcurrencyTests.cs. Test focus: consume SPEC-010 Scheduling aggregate for complete range replacement, rowversion, overlap, deadline and concurrent one-winner behavior. Prove the requirement against its linked AC/EC fixtures: The Scheduling port MUST revalidate deadline, current aggregate version, current published assignments, and impact-alert state using server time inside one local SQL transaction. Availability update and required ScheduleImpactAlert write MUST commit or roll back together.
- [ ] T065 [FR-10] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Deliver FR-10 through the bounded Versioned availability aggregate workstream at src/StudentRegistration.StaffAdministration/Application/StaffAvailabilityFacade.cs only after T064 fails for the expected reason (depends on T064): The Scheduling port MUST revalidate deadline, current aggregate version, current published assignments, and impact-alert state using server time inside one local SQL transaction. Availability update and required ScheduleImpactAlert write MUST commit or roll back together.


## Phase 5 - Frontend Route Tests and Integration

- [ ] T066 [STF-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [AC-2] [AC-4] [AC-8] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STF-01 in tests/StudentRegistration.E2ETests/Specs/Spec016/StaffDashboardPageFeatureTests.cs.
- [ ] T067 [STF-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [AC-2] [AC-4] [AC-8] Deliver the sole canonical Blazor implementation for STF-01 at src/StudentRegistration.Client/Pages/StaffDashboardPage.razor after T066 and the SPEC-003 contract/component checks fail for expected reasons (depends on T066).
- [ ] T068 [STF-02] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-4] [AC-2] [AC-4] [AC-8] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STF-02 in tests/StudentRegistration.E2ETests/Specs/Spec016/StaffTimetablePageFeatureTests.cs.
- [ ] T069 [STF-02] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-4] [AC-2] [AC-4] [AC-8] Deliver the sole canonical Blazor implementation for STF-02 at src/StudentRegistration.Client/Pages/StaffTimetablePage.razor after T068 and the SPEC-003 contract/component checks fail for expected reasons (depends on T068).
- [ ] T070 [STF-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-5] [FR-7] [AC-1] [AC-4] [AC-8] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STF-03 in tests/StudentRegistration.E2ETests/Specs/Spec016/StaffRosterPageFeatureTests.cs.
- [ ] T071 [STF-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-5] [FR-7] [AC-1] [AC-4] [AC-8] Deliver the sole canonical Blazor implementation for STF-03 at src/StudentRegistration.Client/Pages/StaffRosterPage.razor after T070 and the SPEC-003 contract/component checks fail for expected reasons (depends on T070).
- [ ] T072 [STF-04] [UI-CONTRACT-SPEC-003] [FR-6] [FR-8] [FR-9] [FR-10] [AC-3] [AC-5] [AC-6] [AC-7] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STF-04 in tests/StudentRegistration.E2ETests/Specs/Spec016/StaffAvailabilityPageFeatureTests.cs.
- [ ] T073 [STF-04] [UI-CONTRACT-SPEC-003] [FR-6] [FR-8] [FR-9] [FR-10] [AC-3] [AC-5] [AC-6] [AC-7] Deliver the sole canonical Blazor implementation for STF-04 at src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor after T072 and the SPEC-003 contract/component checks fail for expected reasons (depends on T072).

- [ ] T074 [API-Endpoint01] [API-Endpoint02] [API-Endpoint03] [API-Endpoint04] [API-Endpoint05] Deliver the canonical handlers for GET /api/staff/assignments, GET /api/staff/timetable, GET /api/staff/groups/{groupId}/roster, GET /api/staff/availability, and PUT /api/staff/availability at src/StudentRegistration.StaffAdministration/Endpoints/Spec016Endpoints.cs only after all contract, authorization, exact-roster, aggregate, publication-race, alert-query, deadline, and E2E tests T018-T073 fail for expected reasons.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T075 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec016/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-016-NFR-1.md: Staff dashboard/assignment reads SHOULD respond within 300 ms p95.
- [ ] T076 [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec016/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-016-NFR-2.md: Every direct-object access MUST have assignment-scope authorization tests.
- [ ] T077 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec016/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-016-NFR-3.md: Roster output MUST minimize PII and be safely audited.
- [ ] T078 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec016/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-016-NFR-4.md: Timetable/availability MUST have keyboard and list/table operation.

## Phase 7 - Scope and Release Evidence

- [ ] T079 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-016-scope-review.md that OS-1 remains excluded: Grade entry, attendance entry, or messaging.
- [ ] T080 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-016-scope-review.md that OS-2 remains excluded: Staff capacity/policy/term administration.
- [ ] T081 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-016-scope-review.md that OS-3 remains excluded: Access to unrelated groups/students.
- [ ] T082 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-016-scope-review.md that OS-4 remains excluded: Staff-driven automatic room/time changes.
- [ ] T083 [TRACE] Generate the completed FR/NFR/SC/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-016-traceability.md and reject release if any row lacks passing evidence.
- [ ] T084 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-016 in docs/release-evidence/SPEC-016-release-approval.md.

No task is complete and no implementation file has been created.
