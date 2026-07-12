# Research: Registration Capacity and Concurrency

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
interface SubmitRegistrationRequest {
  planId: string;
  planRowVersion: string;
  termId: string;
  clientRequestId: string;
}
interface RegistrationCommitResult {
  submissionId: string;
  status: "accepted" | "rejected";
  resultCode: string;
  registeredGroups: GroupDto[];
  submittedAtUtc: string;
  policyVersion: string;
}
```

Endpoint: POST /api/student/registrations. Success 201 (or 200 for idempotent
replay); business conflicts 409; validation 400; auth 401/403.

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
