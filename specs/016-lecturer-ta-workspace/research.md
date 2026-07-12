# Research: Lecturer and Teaching Assistant Workspace

## Decisions

### Modular boundary
**Decision**: Own this capability in the StaffAdministration module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
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



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
