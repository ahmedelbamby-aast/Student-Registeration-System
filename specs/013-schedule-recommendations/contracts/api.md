# API Contract: Schedule Recommendations

## Feature Contract

```typescript
interface ScheduleOptionDto {
  optionId: string;
  rank: number;
  groups: GroupDto[];
  score: number;
  scoreExplanation: Array<{ factor: string; value: number; message: string }>;
}
interface OptimizationResultDto {
  requestCorrelationId: string;
  planRowVersion: string;
  catalogueVersion: string;
  policyVersion: string;
  optimizerConfigurationVersion: string;
  status: "complete" | "no-solution" | "time-budget";
  options: ScheduleOptionDto[];
  conflicts: ScheduleConflictDto[];
  evaluatedAtUtc: string;
}
interface RecommendScheduleRequest {
  expectedPlanRowVersion: string;
  requestCorrelationId: string;
  preferences: SchedulePreferencesDto;
}
interface ApplyScheduleOptionRequest {
  optionId: string;
  expectedPlanRowVersion: string;
  requestCorrelationId: string;
  catalogueVersion: string;
  policyVersion: string;
  optimizerConfigurationVersion: string;
}
```

Endpoints: POST /api/student/registration-plans/{id}/recommendations and PUT
/api/student/registration-plans/{id}/recommended-option. Applying an option is
an atomic versioned plan update; stale input returns 409 PLAN_CHANGED or
STALE_INPUT.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
