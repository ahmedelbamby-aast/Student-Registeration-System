# API Contract: Schedule Builder and Conflicts

## Feature Contract

```typescript
interface ScheduleConflictDto {
  code: "MEETING_OVERLAP" | "TRAVEL_BUFFER";
  firstGroupId: string;
  secondGroupId: string;
  dayOfWeek: number;
  overlapStartLocal: string;
  overlapEndLocal: string;
  message: string;
}
interface RegistrationPlanDto {
  id: string;
  termId: string;
  rowVersion: string;
  selectedGroups: GroupDto[];
  totalCredits: number;
  conflicts: ScheduleConflictDto[];
}
```

Endpoints: GET/PUT /api/student/registration-plans/{id}; POST validate.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
