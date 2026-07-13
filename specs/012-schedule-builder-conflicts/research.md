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
  first: { groupId: string; groupCode: string; courseCode: string; subjectTitle: string; startLocal: string; endLocal: string };
  second: { groupId: string; groupCode: string; courseCode: string; subjectTitle: string; startLocal: string; endLocal: string };
  dayOfWeek: number;
  overlapStartLocal: string;
  overlapEndLocal: string;
  message: string;
  actions: Array<{ action: "change-group" | "remove-group"; targetGroupId: string; label: string; route: string }>;
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

Endpoints are owner/term scoped GET/PUT plus a non-mutating validate operation.

### Plan route and update shape
**Decision**: Use one active plan per authenticated student/term, a complete-
replacement PUT with expected rowversion, and a separate non-mutating validate
endpoint.
**Rationale**: This avoids arbitrary plan-ID authorization mistakes, item-level
lost updates, and ambiguous partial patch behavior.
**Alternatives rejected**: Client-owned plan IDs and independent item writes.

### Conflict and travel semantics
**Decision**: Use half-open interval overlap, return both meeting details plus
exact overlap and change/remove actions, and keep TRAVEL_BUFFER disabled for
the simple demo POC without inventing a matrix or duration.
**Rationale**: Deterministic adjacency and explicit recovery actions meet the
student UX requirement without inventing institutional policy.
**Alternatives rejected**: Closed intervals, guessed travel minutes, and a red
X with no resolution contract. A future approved specification may add travel
semantics without changing the meeting-overlap rule.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
