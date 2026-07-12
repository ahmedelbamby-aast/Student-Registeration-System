# Research: AASTMT Policy Rulebook

## Decisions

### Modular boundary
**Decision**: Own this capability in the Academics module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface PolicyDecisionDto {
  eligible: boolean;
  policyVersion: string;
  evaluatedAtUtc: string;
  results: Array<{
    reasonCode: string;
    passed: boolean;
    explanation: string;
    sourceUrl: string;
    overridePossible: boolean;
  }>;
}
```

Endpoints: POST /api/admin/policies/{id}/simulate and GET
/api/student/offerings/{id}/eligibility.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
