# Feature Specification: Lecturer and Teaching Assistant Workspace

**Feature Branch**: 016-lecturer-ta-workspace
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Product Owner
**Normative detail**: [requirements.md](requirements.md)

## Context

Lecturers and TAs share staff components but have different assignment scopes.
They need their timetable, partner staff, authorized group rosters, and
availability without access to policy, user, or unrelated student data.

## User Scenarios and Testing

### User Story 1 - Scoped roster (FR-2, FR-5) (P1)

As a Lecturer or Teaching Assistant, I need the Scoped roster (FR-2, FR-5) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given a TA is assigned to Group A but not Group B<br>
When the TA requests Group A and Group B rosters<br>
Then Group A returns one bounded page containing only UniversityId,
DisplayName, and EnrollmentState<br>
And Group B is denied with no data.
### User Story 2 - Shared page, different scope (FR-1, FR-3) (P1)

As a Lecturer or Teaching Assistant, I need the Shared page, different scope (FR-1, FR-3) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given Lecturer and TA users open the same assignments route<br>
When server responses are rendered<br>
Then each sees only the server-authorized lecture/tutorial/lab assignments
defined by current GroupStaffAssignment records<br>
And the role context is stated near the page heading.
### User Story 3 - Availability deadline (FR-6) (P2)

As a Lecturer or Teaching Assistant, I need the Availability deadline (FR-6) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given the availability deadline has passed<br>
When staff attempts an update<br>
Then the command is rejected with deadline/server time<br>
And existing availability remains unchanged.
### User Story 4 - Staff detail and privilege boundary (FR-4, FR-7) (P2)

As a Lecturer or Teaching Assistant, I need the Staff detail and privilege boundary (FR-4, FR-7) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given staff opens an assigned group and attempts an Admin capacity route<br>
When both requests are authorized<br>
Then assigned subject/staff/room/time/capacity details are returned<br>
And the Admin operation is denied.
### User Story 5 - Post-publication availability warning (FR-8) (P3)

As a Lecturer or Teaching Assistant, I need the Post-publication availability warning (FR-8) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given an approved availability change conflicts with a published assignment<br>
When the change is saved through the allowed process<br>
Then one durable open ScheduleImpactAlert records the affected group and
captured versions for Admin revalidation<br>
And no class, room or staff assignment moves automatically.
### User Story 6 - Concurrent availability insert (FR-6, FR-9, FR-10) (P3)

As a Lecturer or Teaching Assistant, I need the Concurrent availability insert (FR-6, FR-9, FR-10) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given two clients load the same staff-term availability version<br>
When they concurrently add overlapping ranges<br>
Then exactly one complete aggregate update succeeds<br>
And the loser receives 409 STALE_VERSION with the current range set.
### User Story 7 - Availability races group publication (FR-8, FR-10) (P3)

As a Lecturer or Teaching Assistant, I need the Availability races group publication (FR-8, FR-10) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

Given an availability update conflicts with a group being published for that
staff member<br>
When both transactions execute concurrently<br>
Then one valid serial order is recorded<br>
And an affected published group is never silently left without a warning and
revalidation state.
### User Story 8 - Staff workspace quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Lecturer or Teaching Assistant, I need the Staff workspace quality gate (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-8 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-8)**

Given approved read load, direct-object authorization matrix, privacy/audit
inspection, and keyboard/calendar-list fixtures<br>
When workspace quality tests execute<br>
Then reads are at most 300 ms p95<br>
And every unassigned object is denied without data<br>
And rosters contain only approved fields with safe audit metadata<br>
And timetable/availability is fully keyboard operable with a list/table
alternative.

## Edge Cases

- EC-1: Staff has both Lecturer and TA assignments -> display authorized
  contexts without duplicate group entries.
- EC-2: Assignment removed while page open -> stale refresh denies roster.
- EC-3: Concurrent availability edit -> stale version gets 409.
- EC-4: No assignments -> clear empty state, no broad search access.
- EC-5: Deadline passes after the page loads -> in-transaction server-time
  validation rejects the update with AVAILABILITY_DEADLINE_PASSED.

## Requirements

### Functional Requirements

- FR-1: Lecturer and TA MUST use the shared staff login and shared workspace
  templates.
- FR-2: The API MUST scope assignments/timetable/rosters to the authenticated
  staff user's current assignments.
- FR-3: Lecturer MUST see assigned lecture groups; TA MUST see assigned
  tutorial/lab groups according to server data.
- FR-4: Staff MUST view group code, subject, role partners, room, meeting
  slots, capacity and roster count.
- FR-5: Staff MAY view only a bounded page of UniversityId, DisplayName, and
  EnrollmentState for an assigned group. GPA, standing, holds, contact data,
  grades, transcript, and unrelated identifiers MUST NOT be returned. The list
  defaults to 20, caps at 100, and is stable-sorted by DisplayName then
  UniversityId.
- FR-6: Staff MUST create/edit their own availability before the server-time
  deadline through the Scheduling application port owned by SPEC-010, using
  the expected StaffTermAvailability rowversion and complete-range
  replacement; SPEC-016 MUST NOT redefine or bypass that aggregate.
- FR-7: Staff MUST NOT manage policy, users, capacity, terms, or unrelated
  rosters.
- FR-8: If an accepted availability change conflicts with a published
  assignment, the same transaction MUST create or update a durable
  ScheduleImpactAlert containing term, staff, affected group, availability and
  group versions, detected time, reason, and revalidation state. Admin MUST be
  able to discover it through SPEC-017; no class moves automatically.
- FR-9: SPEC-016 MUST consume SPEC-010's versioned staff-plus-term
  StaffTermAvailability aggregate. Edits MUST validate the complete range set
  and replace it atomically through the Scheduling port; independent child
  inserts/updates/deletes are prohibited.
- FR-10: The Scheduling port MUST revalidate deadline, current aggregate
  version, current published assignments, and impact-alert state using server
  time inside one local SQL transaction. Availability update and required
  ScheduleImpactAlert write MUST commit or roll back together.

### Non-Functional Requirements

- NFR-1: Staff dashboard/assignment reads SHOULD respond within 300 ms p95.
- NFR-2: Every direct-object access MUST have assignment-scope authorization
  tests.
- NFR-3: Roster output MUST minimize PII and be safely audited.
- NFR-4: Timetable/availability MUST have keyboard and list/table operation.

### Key Entities

- **StaffAssignmentDto**: SPEC-016-owned projection over SPEC-010 GroupStaffAssignment.
- **StaffTermAvailability**: Consumed Scheduling aggregate owned by SPEC-010.
- **StaffAvailability**: Consumed child value owned by SPEC-010.
- **RosterRowDto**: SPEC-016-owned minimal read projection.
- **ScheduleImpactAlert**: Consumed durable revalidation state owned by SPEC-010/Scheduling.

## Success Criteria

- **SC-1**: Staff can view only assignments and roster data within their current scope.
- **SC-2**: Lecturer and TA contexts use shared journeys without merging their permissions.
- **SC-3**: Availability changes never silently move a published class.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-015](../015-student-registration-records/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| STF-01 | /staff | StaffDashboardPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-016 |
| STF-02 | /staff/timetable | StaffTimetablePage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-016 |
| STF-03 | /staff/groups/{groupId}/roster | StaffRosterPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-016 |
| STF-04 | /staff/availability | StaffAvailabilityPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-016 |

## Out of Scope

- OS-1: Grade entry, attendance entry, or messaging.
- OS-2: Staff capacity/policy/term administration.
- OS-3: Access to unrelated groups/students.
- OS-4: Staff-driven automatic room/time changes.
