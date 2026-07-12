# SPEC-016: Lecturer and Teaching Assistant Workspace

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Lecturer/TA representatives, Security, UX, QA<br>
**Target:** Sprint 7<br>
**Dependencies:** SPEC-003, SPEC-007, SPEC-010, SPEC-015, SPEC-018<br>

## Context

Lecturers and TAs share staff components but have different assignment scopes.
They need their timetable, partner staff, authorized group rosters, and
availability without access to policy, user, or unrelated student data.

## Functional Requirements

- FR-1: Lecturer and TA MUST use the shared staff login and shared workspace
  templates.
- FR-2: The API MUST scope assignments/timetable/rosters to the authenticated
  staff user's current assignments.
- FR-3: Lecturer MUST see assigned lecture groups; TA MUST see assigned
  tutorial/lab groups according to server data.
- FR-4: Staff MUST view group code, subject, role partners, room, meeting
  slots, capacity and roster count.
- FR-5: Staff MAY view the minimum authorized roster fields for assigned
  groups.
- FR-6: Staff MUST create/edit own availability before deadline using
  concurrency protection.
- FR-7: Staff MUST NOT manage policy, users, capacity, terms, or unrelated
  rosters.
- FR-8: Availability changes after schedule publication MUST trigger an admin
  warning and MUST NOT silently move a class.
- FR-9: Availability MUST be a versioned staff-plus-term aggregate; edits MUST
  validate the complete range set and replace/update it atomically rather than
  inserting independently validated ranges.
- FR-10: Availability deadline and published-schedule impact MUST be
  revalidated with server time inside the same transaction as the aggregate
  update.

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
Then Group A is returned with approved minimal fields<br>
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
Given staff opens an assigned group and attempts an Admin capacity route<br>
When both requests are authorized<br>
Then assigned subject/staff/room/time/capacity details are returned<br>
And the Admin operation is denied.

### AC-5: Post-publication availability warning (FR-8)
Given an approved availability change conflicts with a published assignment<br>
When the change is saved through the allowed process<br>
Then Admin receives an affected-group warning<br>
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
  group: GroupDto;
  staffRole: "Lecturer" | "TeachingAssistant";
  rosterCount: number;
}
interface AvailabilityRangeDto {
  id: string;
  dayOfWeek: number;
  startLocal: string;
  endLocal: string;
  type: "available" | "unavailable" | "preferred";
}
interface StaffTermAvailabilityDto {
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
```

Endpoints: GET /api/staff/assignments, GET /api/staff/timetable, GET
/api/staff/groups/{id}/roster, GET /api/staff/availability, and PUT
/api/staff/availability. PUT replaces the complete staff-term range set and
returns 409 STALE_VERSION or AVAILABILITY_DEADLINE_PASSED when revalidation
fails.

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
- OS-4: Staff-driven automatic room/time changes.
