# API Contract: Lecturer and Teaching Assistant Workspace

## Feature Contract

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

## Endpoints

- `GET /api/staff/assignments`
- `GET /api/staff/timetable`
- `GET /api/staff/groups/{groupId}/roster?page=1&pageSize=20`
- `GET /api/staff/availability`
- `PUT /api/staff/availability`

Every route requires an authenticated active Lecturer or TeachingAssistant
context with `Context.Read`. The server derives StaffId and effective roles
from the authenticated session. No route accepts a staff ID, role, permission,
or assignment scope from the client. Direct object authorization occurs before
query execution.

### GET /api/staff/assignments

Returns 200 with a bounded array of `StaffAssignmentDto`, ordered by subject
code, group code, and group ID. A dual-role staff member receives only the
currently selected server-issued Lecturer or TeachingAssistant context; roles
are never unioned, and duplicate rows for one group collapse to one group card.
Each group includes only current assigned details, role partners,
room/meeting data, capacity, and active roster count. No broad group search is
performed. No assignments returns 200 with an empty array.

Responses are 200 with the authorized array, 401 `UNAUTHORIZED`, 403
`FORBIDDEN`, 429 `RATE_LIMITED`, or the canonical privacy-safe 500
`UNEXPECTED_ERROR` / 503 `SERVICE_UNAVAILABLE`. No validation or conflict
response is defined for this bodyless read.

### GET /api/staff/timetable

Returns 200 `StaffTimetableDto` projected from the same authorized assignment
snapshot as the dashboard. Calendar and chronological list/table presentations
use the same `assignments[].group.meetings` data. A removed assignment is absent
on refresh; the endpoint does not retain stale client scope.

Responses are 200 `StaffTimetableDto`, 401 `UNAUTHORIZED`, 403 `FORBIDDEN`,
429 `RATE_LIMITED`, or the canonical privacy-safe 500 `UNEXPECTED_ERROR` / 503
`SERVICE_UNAVAILABLE`. Calendar and chronological-list views consume the same
ordered assignment collection.

### GET /api/staff/groups/{groupId}/roster

Accepts optional `page` and `pageSize`; page defaults to `1`, pageSize has
default `20` and maximum `100`. Current-assignment authorization occurs before
roster rows are queried. Missing, removed, unrelated, and unknown group IDs
share privacy-safe 404 `STAFF_GROUP_NOT_FOUND` with no group or student data.
Success is the canonical
SPEC-006 `Page<RosterRowDto>` sorted by `displayName:asc,universityId:asc`.
Only UniversityId, DisplayName, and the literal active EnrollmentState are
serialized. Successful and denied attempts write actor, group, purpose,
outcome, row count, correlation ID, and server time; audit data never contains
roster rows, University IDs, or display names; audit metadata contains no roster
row value.

Responses are 200 `Page<RosterRowDto>`, 400 `PAGE_INVALID` or
`PAGE_SIZE_INVALID`, 401 `UNAUTHORIZED`, 403 `FORBIDDEN`, 404
`STAFF_GROUP_NOT_FOUND`, 429 `RATE_LIMITED`, or the canonical privacy-safe 500
`UNEXPECTED_ERROR` / 503 `SERVICE_UNAVAILABLE`.

### GET /api/staff/availability

Returns 200 `StaffTermAvailabilityDto` for the authenticated staff member and
server-current applicable term. It exposes the canonical two-value range
`kind`, server-controlled deadline, and encoded aggregate rowversion. Missing
configured term/aggregate fails closed with 404; it never returns another
staff member's availability.

### PUT /api/staff/availability

Accepts `ReplaceAvailabilityRequest` and replaces the complete range set for
the authenticated staff member's server-current term. The request must contain
the expected aggregate rowversion and a non-empty, internally valid set of
canonical `available | unavailable` ranges. The Scheduling port re-reads
server time, deadline, aggregate version, current published assignments, and
impact-alert state inside one local SQL transaction. Success is 200
`AvailabilityUpdateResult`. A conflicting published assignment creates or
updates one durable Open alert and returns its ID; no group, meeting, room, or
staff assignment moves automatically.

The roster response is the SPEC-006 `Page<RosterRowDto>`; page defaults to 1,
pageSize to 20, max 100, and `sort` echoes the stable DisplayName then
UniversityId order. Only
UniversityId, DisplayName, and EnrollmentState are permitted. A successful
roster access writes privacy-safe audit metadata (actor, group, purpose,
outcome, row count, correlation), never the returned rows.

PUT replaces the complete SPEC-010 StaffTermAvailability range set through the
Scheduling application port. It returns 200 with the new aggregate rowversion
and any durable impact-alert IDs; 409 `STALE_VERSION` includes the authorized
current range set in an `AvailabilityConflictDto`, and 409
`AVAILABILITY_DEADLINE_PASSED` includes server time and deadline in an
`AvailabilityDeadlineConflictDto`. If a published assignment conflicts,
availability and a unique Open ScheduleImpactAlert commit atomically; no class
moves automatically. Conflict payloads retain the canonical `ApiError` code,
safe message, correlation ID, and currentVersion.

SPEC-010 supplies the bounded read-only Admin availability view; import means
selecting a staff availability aggregate ID and rowversion as an immutable
offering-planning dependency. It does not copy or mutate the range set.
SPEC-017 supplies the Admin alert discovery/revalidation surface. SPEC-016
exposes no Admin availability mutation/correction/override endpoint, request,
permission, editable control, notification workflow, or correction-audit flow.

## Shared Rules

Validation uses stable safe codes; dates are ISO-8601 server values; list
payloads are bounded; no client role claim or client-authoritative scope is
accepted.

## Endpoint Outcome Matrix

| Endpoint | 200 | 400 | 401 | 403 | 404 | 409 | 429 | 500/503 |
|---|---|---|---|---|---|---|---|---|
| Assignments | bounded authorized array, possibly empty | not used | unauthenticated | inactive/wrong staff context or missing `Context.Read` | not used | not used | canonical rate limit | canonical privacy-safe error |
| Timetable | `StaffTimetableDto`, possibly empty | not used | unauthenticated | inactive/wrong staff context or missing `Context.Read` | not used | not used | canonical rate limit | canonical privacy-safe error |
| Roster | bounded exact-field page | `PAGE_INVALID` or `PAGE_SIZE_INVALID` | unauthenticated | inactive/wrong staff context or missing `Context.Read` | `STAFF_GROUP_NOT_FOUND` for missing/removed/unassigned group without data | not used | canonical rate limit | canonical privacy-safe error |
| Availability GET | own aggregate | not used | unauthenticated | inactive/wrong staff context or missing `Context.Read` | no applicable term/aggregate | not used | canonical rate limit | canonical privacy-safe error |
| Availability PUT | updated aggregate and alert IDs | `AVAILABILITY_RANGE_INVALID` | unauthenticated | inactive/wrong staff context or missing `Context.Read` | no applicable term/aggregate | `STALE_VERSION` or `AVAILABILITY_DEADLINE_PASSED` | canonical rate limit | atomic rollback and canonical privacy-safe error |

Invalid GUID route values are route misses. Expected business denials never
include SQL, topology, credentials, unrelated identifiers, group detail, or
student data. Unexpected failures use the canonical `ApiError` and correlation
reference.
