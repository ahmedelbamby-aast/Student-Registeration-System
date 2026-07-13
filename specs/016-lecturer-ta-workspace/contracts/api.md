# API Contract: Lecturer and Teaching Assistant Workspace

## Feature Contract

```typescript
interface StaffAssignmentDto {
  group: GroupDto;
  staffRole: "Lecturer" | "TeachingAssistant";
  rosterCount: number;
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

Every route uses the authenticated staff ID and current GroupStaffAssignment
scope. Direct object authorization occurs before query execution.

The roster response is the SPEC-006 `Page<RosterRowDto>`; page defaults to 1,
pageSize to 20, max 100, and `sort` echoes the stable DisplayName then
UniversityId order. Only
UniversityId, DisplayName, and EnrollmentState are permitted. A successful
roster access writes privacy-safe audit metadata (actor, group, purpose,
outcome, row count, correlation), never the returned rows.

PUT replaces the complete SPEC-010 StaffTermAvailability range set through the
Scheduling application port. It returns 200 with the new aggregate rowversion
and any durable impact-alert IDs; 409 `STALE_VERSION` includes the authorized
current range set, and 409 `AVAILABILITY_DEADLINE_PASSED` includes server time
and deadline. If a published assignment conflicts, availability and a unique
Open ScheduleImpactAlert commit atomically; no class moves automatically.

SPEC-017 supplies the Admin alert discovery/revalidation surface. SPEC-016
does not expose an Admin mutation endpoint.

## Shared Rules

Validation uses stable safe codes; dates are ISO-8601 server values; list
payloads are bounded; no client role claim or client-authoritative scope is
accepted.
