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
  expectedPlanRowVersion: string;
  termId: string;
  clientRequestId: string;
}
interface RegistrationFinalResult {
  submissionId: string;
  status: "accepted" | "rejected";
  resultCode: string;
  registeredGroups: GroupDto[];
  receivedAtUtc: string;
  completedAtUtc?: string;
  policyVersion: string;
  planRowVersion: string;
}
interface RegistrationInProgressResponse {
  clientRequestId: string;
  status: "processing";
  retryAfterSeconds: number;
  resultUrl: string;
}
```

Endpoint: POST /api/student/registrations. New final result is 201; idempotent
final replay is 200; bounded lock-wait expiry is 202 with
RegistrationInProgressResponse and no submissionId; business/version/
idempotency conflicts are 409; validation is 400; authentication/authorization
are 401/403. GET /api/student/registrations/by-request/{clientRequestId}
returns the authenticated student's committed final result, the same bounded
202 while the first transaction still holds the key, or 404 REQUEST_NOT_FOUND
after a rolled-back/nonexistent claim. A 202 is transport-level retry guidance,
not evidence of a separately committed Processing row.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
