# Research: Eligibility and Subject Discovery

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
interface OfferingEligibilityDto {
  offeringId: string;
  courseCode: string;
  title: string;
  credits: number;
  eligible: boolean;
  reasons: Array<{ code: string; passed: boolean; message: string }>;
  groups: GroupSummaryDto[];
}
```

Endpoints provide a bounded discovery page and one complete eligibility detail.

### Explanation ownership
**Decision**: SPEC-011 owns evaluation projections only and references approved
SPEC-009 PolicySet versions plus upstream academic/offering versions.
**Rationale**: Decisions are explainable without copying policy or scheduling
aggregates into Registration.
**Alternatives rejected**: A SPEC-011 PolicyVersion entity and opaque boolean
eligibility.

### Pagination protocol
**Decision**: Default page 1/size 20, maximum 100, reject invalid/oversized
values, normalize/search at 100 characters, and append offering ID to every
sort.
**Rationale**: One explicit protocol is easy for UI/tests and yields bounded,
deterministic ordering.
**Alternatives rejected**: Silent clamping and unspecified sorting.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
