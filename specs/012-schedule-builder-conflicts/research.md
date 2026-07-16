# Research: Schedule Builder and Conflicts

**Approved amendment:** `spec012-credit-load/1.0`, approved by Ahmed ELbamby
on 2026-07-16, keeps plan-response credit guidance fixed and intentionally
small for the demo.

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
interface ScheduleConflictParticipantDto {
  groupId: string;
  groupCode: string;
  courseCode: string;
  subjectTitle: string;
  startLocal: string;
  endLocal: string;
}
interface ScheduleConflictDto {
  code: "MEETING_OVERLAP" | "TRAVEL_BUFFER";
  first: ScheduleConflictParticipantDto;
  second: ScheduleConflictParticipantDto;
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
  defaultTargetCredits: 18;
  maximumAllowedCredits: 18;
  loadReasons: Array<{
    code: string;
    blocking: boolean;
    message: string;
    policySetId: string;
    policyVersion: string;
    sourceReference: string;
  }>;
  conflicts: ScheduleConflictDto[];
}
```

Endpoints are owner/term scoped GET/PUT plus a non-mutating validate operation.
All responses carry the server-composed 18-credit default target, fixed
18-credit maximum, and safe policy/source load reasons so direct schedule
navigation never depends on client state or a browser GPA calculation.

### Simple fixed credit-load response
**Decision**: Every RegistrationPlan response reports
`defaultTargetCredits=18`, `maximumAllowedCredits=18`, and server-authored
`loadReasons` with policy/source provenance.
**Rationale**: The demo has one approved normal maximum. Reasons remain
server-authored and explainable without adding a second maximum or overload
workflow.
**Alternatives rejected**: The earlier draft 12/18 GPA branch and an overload
flow. The browser never derives the maximum or invents a reason.

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
