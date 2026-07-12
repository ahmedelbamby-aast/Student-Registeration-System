# API Contract: Lecturer and Teaching Assistant Workspace

## Feature Contract

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

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
