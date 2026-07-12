# SPEC-016: Lecturer and Teaching Assistant Workspace

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Lecturer/TA representatives, Security, UX, QA<br>
**Target:** Sprint 7<br>

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
Then each sees only role-appropriate current assignments<br>
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

## Edge Cases

- EC-1: Staff has both Lecturer and TA assignments -> display authorized
  contexts without duplicate group entries.
- EC-2: Assignment removed while page open -> stale refresh denies roster.
- EC-3: Concurrent availability edit -> stale version gets 409.
- EC-4: No assignments -> clear empty state, no broad search access.

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
  rowVersion: string;
}
```

Endpoints: GET /api/staff/assignments, GET /api/staff/timetable, GET
/api/staff/groups/{id}/roster, GET/PUT /api/staff/availability.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| GroupStaffAssignment | bridge | authorized staff + group + role |
| StaffAvailability | range | own staff ID; deadline; rowversion |
| StaffAssignmentDto | projection | current assigned group details only |
| RosterRow | projection | minimal approved student fields |

## Out of Scope

- OS-1: Grade entry, attendance entry, or messaging.
- OS-2: Staff capacity/policy/term administration.
- OS-3: Access to unrelated groups/students.
- OS-4: Staff-driven automatic room/time changes.
