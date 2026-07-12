# API Contract: Schedule Recommendations

## Feature Contract

```typescript
interface ScheduleOptionDto {
  rank: number;
  groups: GroupDto[];
  score: number;
  scoreExplanation: Array<{ factor: string; value: number; message: string }>;
}
interface OptimizationResultDto {
  status: "complete" | "no-solution" | "time-budget";
  options: ScheduleOptionDto[];
  conflicts: ScheduleConflictDto[];
  evaluatedAtUtc: string;
}
```

Endpoint: POST /api/student/registration-plans/{id}/recommendations.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
