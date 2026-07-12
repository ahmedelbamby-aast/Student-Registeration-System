# API Contract: Registration Capacity and Concurrency

## Feature Contract

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

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
