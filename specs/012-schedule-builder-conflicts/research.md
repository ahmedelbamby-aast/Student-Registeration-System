# Research: Schedule Builder and Conflicts

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

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
