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
  policyVersion: string;
  groups: GroupDto[];
}
```

Endpoint: GET /api/student/terms/{termId}/offerings with q, eligibility,
credits, day, availability, page, and pageSize.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
