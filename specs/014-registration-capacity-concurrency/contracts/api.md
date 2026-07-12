# API Contract: Registration Capacity and Concurrency

## Feature Contract

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

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
