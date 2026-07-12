# Research: Schedule Recommendations

## Decisions

### Modular boundary
**Decision**: Own this capability in the Registration module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
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

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
