# Tasks: Lecturer and Teaching Assistant Workspace

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-016 in specs/016-lecturer-ta-workspace/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-007] Validate the consumed upstream requirements, plan, data model, and API contract at specs/007-identity-account-lifecycle/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-010] Validate the consumed upstream requirements, plan, data model, and API contract at specs/010-offerings-groups-resources/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-015] Validate the consumed upstream requirements, plan, data model, and API contract at specs/015-student-registration-records/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/016-lecturer-ta-workspace/dependency-baseline.md.
- [ ] T007 [GATE] Freeze SPEC-016 requirements, API, data-model, policy approvals, and dependency versions in specs/016-lecturer-ta-workspace/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T008 [P] [ENTITY-StaffAssignment] [OWNER-SPEC-016] Create the future failing invariant/schema/serialization checks for canonical StaffAssignment ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec016/StaffAssignmentModelTests.cs.
- [ ] T009 [ENTITY-StaffAssignment] [OWNER-SPEC-016] Deliver the canonical StaffAssignment model or governed artifact at src/StudentRegistration.Domain/Modules/StaffAdministration/StaffAssignment.cs after T008 fails for the expected reason (depends on T008).
- [ ] T010 [P] [ENTITY-StaffTermAvailability] [OWNER-SPEC-016] Create the future failing invariant/schema/serialization checks for canonical StaffTermAvailability ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec016/StaffTermAvailabilityModelTests.cs.
- [ ] T011 [ENTITY-StaffTermAvailability] [OWNER-SPEC-016] Deliver the canonical StaffTermAvailability model or governed artifact at src/StudentRegistration.Domain/Modules/StaffAdministration/StaffTermAvailability.cs after T010 fails for the expected reason (depends on T010).
- [ ] T012 [P] [ENTITY-StaffAvailability] [OWNER-SPEC-016] Create the future failing invariant/schema/serialization checks for canonical StaffAvailability ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec016/StaffAvailabilityModelTests.cs.
- [ ] T013 [ENTITY-StaffAvailability] [OWNER-SPEC-016] Deliver the canonical StaffAvailability model or governed artifact at src/StudentRegistration.Domain/Modules/StaffAdministration/StaffAvailability.cs after T012 fails for the expected reason (depends on T012).
- [ ] T014 [P] [ENTITY-RosterRow] [OWNER-SPEC-016] Create the future failing invariant/schema/serialization checks for canonical RosterRow ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec016/RosterRowModelTests.cs.
- [ ] T015 [ENTITY-RosterRow] [OWNER-SPEC-016] Deliver the canonical RosterRow model or governed artifact at src/StudentRegistration.Domain/Modules/StaffAdministration/RosterRow.cs after T014 fails for the expected reason (depends on T014).
- [ ] T016 [P] [ENTITY-GroupSummary] [CONSUMER-SPEC-011] Verify SPEC-016 consumes the canonical GroupSummary at src/StudentRegistration.Domain/Modules/Registration/GroupSummary.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec016/GroupSummaryModelTests.cs.
- [ ] T017 [API-Endpoint01] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/staff/assignments in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T018 [P] [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/staff/assignments in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint01ContractTests.cs.
- [ ] T019 [API-Endpoint01] [OWNER-SPEC-016] Deliver the sole canonical GET /api/staff/assignments handler at src/StudentRegistration.Server/Modules/StaffAdministration/Endpoints/Spec016Endpoints.cs after T018 fails for the expected reason (depends on T018).
- [ ] T020 [API-Endpoint02] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/staff/timetable in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T021 [P] [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/staff/timetable in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint02ContractTests.cs.
- [ ] T022 [API-Endpoint02] [OWNER-SPEC-016] Deliver the sole canonical GET /api/staff/timetable handler at src/StudentRegistration.Server/Modules/StaffAdministration/Endpoints/Spec016Endpoints.cs after T021 fails for the expected reason (depends on T021).
- [ ] T023 [API-Endpoint03] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/staff/groups/{id}/roster in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T024 [P] [API-Endpoint03] Verify every documented response and authorization outcome for GET /api/staff/groups/{id}/roster in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint03ContractTests.cs.
- [ ] T025 [API-Endpoint03] [OWNER-SPEC-016] Deliver the sole canonical GET /api/staff/groups/{id}/roster handler at src/StudentRegistration.Server/Modules/StaffAdministration/Endpoints/Spec016Endpoints.cs after T024 fails for the expected reason (depends on T024).
- [ ] T026 [API-Endpoint04] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/staff/availability in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T027 [P] [API-Endpoint04] Verify every documented response and authorization outcome for GET /api/staff/availability in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint04ContractTests.cs.
- [ ] T028 [API-Endpoint04] [OWNER-SPEC-016] Deliver the sole canonical GET /api/staff/availability handler at src/StudentRegistration.Server/Modules/StaffAdministration/Endpoints/Spec016Endpoints.cs after T027 fails for the expected reason (depends on T027).
- [ ] T029 [API-Endpoint05] [OWNER-SPEC-016] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for PUT /api/staff/availability in specs/016-lecturer-ta-workspace/contracts/api.md.
- [ ] T030 [P] [API-Endpoint05] Verify every documented response and authorization outcome for PUT /api/staff/availability in tests/StudentRegistration.ContractTests/Specs/Spec016/Endpoint05ContractTests.cs.
- [ ] T031 [API-Endpoint05] [OWNER-SPEC-016] Deliver the sole canonical PUT /api/staff/availability handler at src/StudentRegistration.Server/Modules/StaffAdministration/Endpoints/Spec016Endpoints.cs after T030 fails for the expected reason (depends on T030).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Scoped roster (FR-2, FR-5) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T032 [P] [AC-1] [FR-2] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-1Tests.cs for AC-1: Scoped roster (FR-2, FR-5): Given a TA is assigned to Group A but not Group B When the TA requests Group A and Group B rosters Then Group A is returned with approved minimal fields And Group B is denied with no data.
### US2 - Shared page, different scope (FR-1, FR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T033 [P] [AC-2] [FR-1] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-2Tests.cs for AC-2: Shared page, different scope (FR-1, FR-3): Given Lecturer and TA users open the same assignments route When server responses are rendered Then each sees only the server-authorized lecture/tutorial/lab assignments defined by current GroupStaffAssignment records And the role context is stated near the page heading.
### US3 - Availability deadline (FR-6) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T034 [P] [AC-3] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-3Tests.cs for AC-3: Availability deadline (FR-6): Given the availability deadline has passed When staff attempts an update Then the command is rejected with deadline/server time And existing availability remains unchanged.
### US4 - Staff detail and privilege boundary (FR-4, FR-7) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T035 [P] [AC-4] [FR-4] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-4Tests.cs for AC-4: Staff detail and privilege boundary (FR-4, FR-7): Given staff opens an assigned group and attempts an Admin capacity route When both requests are authorized Then assigned subject/staff/room/time/capacity details are returned And the Admin operation is denied.
### US5 - Post-publication availability warning (FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T036 [P] [AC-5] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-5Tests.cs for AC-5: Post-publication availability warning (FR-8): Given an approved availability change conflicts with a published assignment When the change is saved through the allowed process Then Admin receives an affected-group warning And no class, room or staff assignment moves automatically.
### US6 - Concurrent availability insert (FR-6, FR-9, FR-10) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T037 [P] [AC-6] [FR-6] [FR-9] [FR-10] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-6Tests.cs for AC-6: Concurrent availability insert (FR-6, FR-9, FR-10): Given two clients load the same staff-term availability version When they concurrently add overlapping ranges Then exactly one complete aggregate update succeeds And the loser receives 409 STALE_VERSION with the current range set.
### US7 - Availability races group publication (FR-8, FR-10) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T038 [P] [AC-7] [FR-8] [FR-10] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-7Tests.cs for AC-7: Availability races group publication (FR-8, FR-10): Given an availability update conflicts with a group being published for that staff member When both transactions execute concurrently Then one valid serial order is recorded And an affected published group is never silently left without a warning and revalidation state.
### US8 - Staff workspace quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Lecturer and Teaching Assistant Workspace.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T039 [P] [AC-8] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec016/AC-8Tests.cs for AC-8: Staff workspace quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given approved read load, direct-object authorization matrix, privacy/audit inspection, and keyboard/calendar-list fixtures When workspace quality tests execute Then reads are at most 300 ms p95 And every unassigned object is denied without data And rosters contain only approved fields with safe audit metadata And timetable/availability is fully keyboard operable with a list/table alternative.
- [ ] T040 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-1Tests.cs and assert: Staff has both Lecturer and TA assignments -> display authorized contexts without duplicate group entries.
- [ ] T041 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-2Tests.cs and assert: Assignment removed while page open -> stale refresh denies roster.
- [ ] T042 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-3Tests.cs and assert: Concurrent availability edit -> stale version gets 409.
- [ ] T043 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-4Tests.cs and assert: No assignments -> clear empty state, no broad search access.
- [ ] T044 [P] [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec016/EdgeCases/EC-5Tests.cs and assert: Deadline passes after the page loads -> in-transaction server-time validation rejects the update with AVAILABILITY_DEADLINE_PASSED.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T045 [P] [FR-1] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-1 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, minimal roster, detail fields, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Lecturer and TA MUST use the shared staff login and shared workspace templates.
- [ ] T046 [FR-1] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-1 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffWorkspaceQueries.cs only after T045 fails for the expected reason (depends on T045): Lecturer and TA MUST use the shared staff login and shared workspace templates.
- [ ] T047 [P] [FR-2] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-2 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, minimal roster, detail fields, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: The API MUST scope assignments/timetable/rosters to the authenticated staff user's current assignments.
- [ ] T048 [FR-2] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-2 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffWorkspaceQueries.cs only after T047 fails for the expected reason (depends on T047): The API MUST scope assignments/timetable/rosters to the authenticated staff user's current assignments.
- [ ] T049 [P] [FR-3] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-3 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, minimal roster, detail fields, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Lecturer MUST see assigned lecture groups; TA MUST see assigned tutorial/lab groups according to server data.
- [ ] T050 [FR-3] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-3 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffWorkspaceQueries.cs only after T049 fails for the expected reason (depends on T049): Lecturer MUST see assigned lecture groups; TA MUST see assigned tutorial/lab groups according to server data.
- [ ] T051 [P] [FR-4] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-4 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, minimal roster, detail fields, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Staff MUST view group code, subject, role partners, room, meeting slots, capacity and roster count.
- [ ] T052 [FR-4] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-4 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffWorkspaceQueries.cs only after T051 fails for the expected reason (depends on T051): Staff MUST view group code, subject, role partners, room, meeting slots, capacity and roster count.
- [ ] T053 [P] [FR-5] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-5 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, minimal roster, detail fields, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Staff MAY view the minimum authorized roster fields for assigned groups.
- [ ] T054 [FR-5] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-5 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffWorkspaceQueries.cs only after T053 fails for the expected reason (depends on T053): Staff MAY view the minimum authorized roster fields for assigned groups.
- [ ] T055 [P] [FR-6] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Create the future failing FR-6 checks in tests/StudentRegistration.IntegrationTests/Staff/StaffAvailabilityConcurrencyTests.cs. Test focus: complete range replacement, staff-term rowversion, overlap, deadline and concurrent one-winner behavior. Prove the requirement against its linked AC/EC fixtures: Staff MUST create/edit own availability before deadline using concurrency protection.
- [ ] T056 [FR-6] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Deliver FR-6 through the bounded Versioned availability aggregate workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffAvailabilityService.cs only after T055 fails for the expected reason (depends on T055): Staff MUST create/edit own availability before deadline using concurrency protection.
- [ ] T057 [P] [FR-7] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Create the future failing FR-7 checks in tests/StudentRegistration.AuthorizationTests/StaffWorkspaceScopeTests.cs. Test focus: Lecturer/TA assignment scope, minimal roster, detail fields, dual role and direct-object denial. Prove the requirement against its linked AC/EC fixtures: Staff MUST NOT manage policy, users, capacity, terms, or unrelated rosters.
- [ ] T058 [FR-7] [WORKSTREAM-ASSIGNMENT-SCOPED-WORKSPACE-QUERIES] Deliver FR-7 through the bounded Assignment-scoped workspace queries workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffWorkspaceQueries.cs only after T057 fails for the expected reason (depends on T057): Staff MUST NOT manage policy, users, capacity, terms, or unrelated rosters.
- [ ] T059 [P] [FR-8] [WORKSTREAM-PUBLISHED-SCHEDULE-IMPACT] Create the future failing FR-8 checks in tests/StudentRegistration.IntegrationTests/Staff/AvailabilityPublicationRaceTests.cs. Test focus: shared staff boundary, affected-group warning and no silent class move. Prove the requirement against its linked AC/EC fixtures: Availability changes after schedule publication MUST trigger an admin warning and MUST NOT silently move a class.
- [ ] T060 [FR-8] [WORKSTREAM-PUBLISHED-SCHEDULE-IMPACT] Deliver FR-8 through the bounded Published schedule impact workstream at src/StudentRegistration.Server/Modules/StaffAdministration/ScheduleImpactService.cs only after T059 fails for the expected reason (depends on T059): Availability changes after schedule publication MUST trigger an admin warning and MUST NOT silently move a class.
- [ ] T061 [P] [FR-9] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Create the future failing FR-9 checks in tests/StudentRegistration.IntegrationTests/Staff/StaffAvailabilityConcurrencyTests.cs. Test focus: complete range replacement, staff-term rowversion, overlap, deadline and concurrent one-winner behavior. Prove the requirement against its linked AC/EC fixtures: Availability MUST be a versioned staff-plus-term aggregate; edits MUST validate the complete range set and replace/update it atomically rather than inserting independently validated ranges.
- [ ] T062 [FR-9] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Deliver FR-9 through the bounded Versioned availability aggregate workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffAvailabilityService.cs only after T061 fails for the expected reason (depends on T061): Availability MUST be a versioned staff-plus-term aggregate; edits MUST validate the complete range set and replace/update it atomically rather than inserting independently validated ranges.
- [ ] T063 [P] [FR-10] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Create the future failing FR-10 checks in tests/StudentRegistration.IntegrationTests/Staff/StaffAvailabilityConcurrencyTests.cs. Test focus: complete range replacement, staff-term rowversion, overlap, deadline and concurrent one-winner behavior. Prove the requirement against its linked AC/EC fixtures: Availability deadline and published-schedule impact MUST be revalidated with server time inside the same transaction as the aggregate update.
- [ ] T064 [FR-10] [WORKSTREAM-VERSIONED-AVAILABILITY-AGGREGATE] Deliver FR-10 through the bounded Versioned availability aggregate workstream at src/StudentRegistration.Server/Modules/StaffAdministration/StaffAvailabilityService.cs only after T063 fails for the expected reason (depends on T063): Availability deadline and published-schedule impact MUST be revalidated with server time inside the same transaction as the aggregate update.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T065 [P] [STF-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [AC-2] [AC-4] [AC-8] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STF-01 in tests/StudentRegistration.E2ETests/Specs/Spec016/StaffDashboardPageFeatureTests.cs.
- [ ] T066 [STF-01] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-3] [FR-4] [AC-2] [AC-4] [AC-8] Deliver the sole canonical Blazor implementation for STF-01 at src/StudentRegistration.Client/Pages/StaffDashboardPage.razor after T065 and the SPEC-003 contract/component checks fail for expected reasons (depends on T065).
- [ ] T067 [P] [STF-02] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-4] [AC-2] [AC-4] [AC-8] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STF-02 in tests/StudentRegistration.E2ETests/Specs/Spec016/StaffTimetablePageFeatureTests.cs.
- [ ] T068 [STF-02] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-4] [AC-2] [AC-4] [AC-8] Deliver the sole canonical Blazor implementation for STF-02 at src/StudentRegistration.Client/Pages/StaffTimetablePage.razor after T067 and the SPEC-003 contract/component checks fail for expected reasons (depends on T067).
- [ ] T069 [P] [STF-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-5] [FR-7] [AC-1] [AC-4] [AC-8] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STF-03 in tests/StudentRegistration.E2ETests/Specs/Spec016/StaffRosterPageFeatureTests.cs.
- [ ] T070 [STF-03] [UI-CONTRACT-SPEC-003] [FR-2] [FR-4] [FR-5] [FR-7] [AC-1] [AC-4] [AC-8] Deliver the sole canonical Blazor implementation for STF-03 at src/StudentRegistration.Client/Pages/StaffRosterPage.razor after T069 and the SPEC-003 contract/component checks fail for expected reasons (depends on T069).
- [ ] T071 [P] [STF-04] [UI-CONTRACT-SPEC-003] [FR-6] [FR-8] [FR-9] [FR-10] [AC-3] [AC-5] [AC-6] [AC-7] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for STF-04 in tests/StudentRegistration.E2ETests/Specs/Spec016/StaffAvailabilityPageFeatureTests.cs.
- [ ] T072 [STF-04] [UI-CONTRACT-SPEC-003] [FR-6] [FR-8] [FR-9] [FR-10] [AC-3] [AC-5] [AC-6] [AC-7] Deliver the sole canonical Blazor implementation for STF-04 at src/StudentRegistration.Client/Pages/StaffAvailabilityPage.razor after T071 and the SPEC-003 contract/component checks fail for expected reasons (depends on T071).

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T073 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec016/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-016-NFR-1.md: Staff dashboard/assignment reads SHOULD respond within 300 ms p95.
- [ ] T074 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec016/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-016-NFR-2.md: Every direct-object access MUST have assignment-scope authorization tests.
- [ ] T075 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec016/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-016-NFR-3.md: Roster output MUST minimize PII and be safely audited.
- [ ] T076 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec016/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-016-NFR-4.md: Timetable/availability MUST have keyboard and list/table operation.

## Phase 7 - Scope and Release Evidence

- [ ] T077 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-016-scope-review.md that OS-1 remains excluded: Grade entry, attendance entry, or messaging.
- [ ] T078 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-016-scope-review.md that OS-2 remains excluded: Staff capacity/policy/term administration.
- [ ] T079 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-016-scope-review.md that OS-3 remains excluded: Access to unrelated groups/students.
- [ ] T080 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-016-scope-review.md that OS-4 remains excluded: Staff-driven automatic room/time changes.
- [ ] T081 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-016-traceability.md and reject release if any row lacks passing evidence.
- [ ] T082 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-016 in docs/release-evidence/SPEC-016-release-approval.md.

No task is complete and no implementation file has been created.
