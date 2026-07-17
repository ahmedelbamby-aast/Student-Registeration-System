# SPEC-016: Lecturer and Teaching Assistant Workspace

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** Approved for demo implementation by Ahmed ELbamby on 2026-07-13<br>
**Owner:** Product Owner<br>
**Reviewers:** Lecturer/TA representatives, Security, UX, QA<br>
**Target:** Sprint 7<br>
**Dependencies:** SPEC-003, SPEC-007, SPEC-010, SPEC-015, SPEC-018<br>

## Context

Lecturers and TAs share staff components but have different assignment scopes.
They need their timetable, partner staff, authorized group rosters, and
availability without access to policy, user, or unrelated student data. Staff
own availability edits; Admin use is limited to a bounded read-only view and
selecting a staff availability aggregate ID and rowversion as a read-only
offering-planning dependency.

## Functional Requirements

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
- FR-7: Lecturer and TA MUST NOT manage policy, users, capacity, terms, or
  unrelated rosters. Staff own availability edits in the POC. Admin MAY
  consume the bounded read-only availability view and select a staff
  availability aggregate ID and rowversion as an immutable offering-planning
  dependency, but no Admin availability
  mutation/correction/override command, permission, editable control,
  notification workflow, or correction-audit flow exists.
- FR-8: If an accepted availability change conflicts with a published
  assignment, the same transaction MUST create or update a durable
  ScheduleImpactAlert containing term, staff, affected group, availability and
  group versions, detected time, reason, and revalidation state. Admin MUST be
  able to discover it through SPEC-017 as read-only impact/revalidation state;
  it is not an availability-correction notification and no class moves
  automatically.
- FR-9: SPEC-016 MUST consume SPEC-010's versioned staff-plus-term
  StaffTermAvailability aggregate. Edits MUST validate the complete range set
  and replace it atomically through the Scheduling port; independent child
  inserts/updates/deletes are prohibited.
- FR-10: The Scheduling port MUST revalidate deadline, current aggregate
  version, current published assignments, and impact-alert state using server
  time inside one local SQL transaction. Availability update and required
  ScheduleImpactAlert write MUST commit or roll back together.

## Non-Functional Requirements

- NFR-1: Staff dashboard/assignment reads SHOULD respond within 300 ms p95.
- NFR-2: Every direct-object access MUST have assignment-scope authorization
  tests.
- NFR-3: Roster output MUST minimize PII and be safely audited.
- NFR-4: Timetable/availability MUST have keyboard and list/table operation.

## Acceptance Criteria

### AC-1: Scoped roster (FR-2, FR-5)
Given a TA is assigned to Group A but not Group B<br>
When the TA requests Group A and Group B rosters<br>
Then Group A returns one bounded page containing only UniversityId,
DisplayName, and EnrollmentState<br>
And Group B is denied with no data.

### AC-2: Shared page, different scope (FR-1, FR-3)
Given Lecturer and TA users open the same assignments route<br>
When server responses are rendered<br>
Then each sees only the server-authorized lecture/tutorial/lab assignments
defined by current GroupStaffAssignment records<br>
And the role context is stated near the page heading.

### AC-3: Availability deadline (FR-6)
Given the availability deadline has passed<br>
When staff attempts an update<br>
Then the command is rejected with deadline/server time<br>
And existing availability remains unchanged.

### AC-4: Staff detail and privilege boundary (FR-4, FR-7)
Given staff opens an assigned group and Admin reads availability and selects
its aggregate ID and rowversion for offering planning<br>
When staff attempts an Admin capacity route and Admin attempts an availability
correction or override<br>
Then assigned group details and read-only availability inputs are returned<br>
And both mutation attempts are denied or have no mapped route, permission,
editable control, notification workflow, or correction-audit flow.

### AC-5: Post-publication availability warning (FR-8)
Given an approved availability change conflicts with a published assignment<br>
When the change is saved through the allowed process<br>
Then one durable open ScheduleImpactAlert records the affected group and
captured versions for Admin discovery and revalidation<br>
And no class, room or staff assignment moves automatically.

### AC-6: Concurrent availability insert (FR-6, FR-9, FR-10)
Given two clients load the same staff-term availability version<br>
When they concurrently add overlapping ranges<br>
Then exactly one complete aggregate update succeeds<br>
And the loser receives 409 STALE_VERSION with the current range set.

### AC-7: Availability races group publication (FR-8, FR-10)
Given an availability update conflicts with a group being published for that
staff member<br>
When both transactions execute concurrently<br>
Then one valid serial order is recorded<br>
And an affected published group is never silently left without a warning and
revalidation state.

### AC-8: Staff workspace quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
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

## API Contracts

```typescript
interface StaffAssignmentDto {
  subjectCode: string;
  subjectTitle: string;
  group: GroupDto; // canonical SPEC-010 scheduling DTO
  staffRole: "Lecturer" | "TeachingAssistant";
  rosterCount: number;
}
interface StaffTimetableDto {
  roleContext: string;
  assignments: StaffAssignmentDto[];
}
interface RosterRowDto {
  universityId: string;
  displayName: string;
  enrollmentState: "active";
}
type RosterPageDto = Page<RosterRowDto>; // canonical Page<T> from SPEC-006
interface AvailabilityRangeDto {
  id: string;
  dayOfWeek: number;
  startLocal: string;
  endLocal: string;
  kind: "available" | "unavailable";
}
interface StaffTermAvailabilityDto {
  id: string;
  staffId: string;
  termId: string;
  deadlineUtc: string;
  rowVersion: string;
  ranges: AvailabilityRangeDto[];
}
interface ReplaceAvailabilityRequest {
  expectedStaffTermRowVersion: string;
  ranges: AvailabilityRangeDto[];
}
interface AvailabilityUpdateResult {
  availability: StaffTermAvailabilityDto;
  impactAlertIds: string[];
}
```

Endpoints: GET /api/staff/assignments, GET /api/staff/timetable, GET
/api/staff/groups/{groupId}/roster, GET /api/staff/availability, and PUT
/api/staff/availability. PUT replaces the complete staff-term range set and
returns 409 STALE_VERSION or AVAILABILITY_DEADLINE_PASSED when revalidation
fails.

Admin availability consumption uses SPEC-010's bounded read-only Admin view.
Import means selecting the aggregate ID and rowversion as an immutable
offering-planning dependency; it neither copies nor mutates the range set. No
Admin availability correction or
override endpoint, request contract, permission, editable control,
notification workflow, or correction-audit flow belongs to this POC.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| GroupStaffAssignment | bridge | authorized staff + group + role |
| StaffTermAvailability | aggregate root | unique staff + term; deadline; rowversion; owns complete range set |
| StaffAvailability | child range | parent aggregate ID; day/start/end/type; no independent concurrency version |
| StaffAssignmentDto | projection | current assigned group details only |
| RosterRow | projection | minimal approved student fields |

Every create/update/delete of a StaffAvailability child MUST lock and advance
the owning StaffTermAvailability rowversion. Children MUST NOT be updated
through an endpoint or transaction that bypasses the aggregate root.

## Out of Scope

- OS-1: Grade entry, attendance entry, or messaging.
- OS-2: Staff capacity/policy/term administration.
- OS-3: Access to unrelated groups/students.
- OS-4: Staff-driven automatic room/time changes and any Admin availability
  mutation/correction/override, permission, editable control, notification, or
  correction-audit flow.
