# API Contract: Registration Capacity and Concurrency

## Feature Contract

```typescript
interface RegistrationGroupSnapshotDto {
  offeringId: string;
  courseCode: string;
  subjectTitle: string;
  groupId: string;
  groupCode: string;
  credits: number;
  staff: Array<{ role: "Lecturer" | "TeachingAssistant"; displayName: string }>;
  meetings: Array<{ dayOfWeek: number; startLocal: string; endLocal: string; roomCode: string; location: string }>;
}
interface RegistrationReceiptSnapshotDto {
  term: TermSummaryDto;
  groups: RegistrationGroupSnapshotDto[];
  totalCredits: number;
  policyVersion: string;
  submittedAtUtc: string;
}
interface SubmitRegistrationRequest {
  planId: string;
  expectedPlanRowVersion: string;
  clientRequestId: string;
}
interface RegistrationFinalResult {
  submissionId: string;
  status: "accepted" | "rejected";
  resultCode: string;
  registeredGroups: RegistrationGroupSnapshotDto[];
  receivedAtUtc: string;
  completedAtUtc: string;
  policyVersion: string;
  planRowVersion: string;
  reference?: string;
  receiptSnapshot?: RegistrationReceiptSnapshotDto;
}
interface RegistrationInProgressResponse {
  clientRequestId: string;
  status: "processing";
  retryAfterSeconds: number;
  resultUrl: string;
}
```

## Endpoints and Scope

### POST /api/student/terms/{termId}/registrations

Student identity comes only from authentication; `termId` comes from the
route and is checked against the student's server-resolved context. The body
does not contain studentId or termId.

- 201: newly committed final result.
- 200: same-scope, same-payload final replay.
- 202: first same-scope claim is still uncommitted after at most 500 ms;
  contains no submissionId and names the term-scoped lookup URL.
- 400: malformed input.
- 401/403: authentication/authorization.
- 409: business/version conflict or same-scope key reused with different
  canonical payload.

### GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}

Returns only the authenticated student's result in that route term: 200 final,
the same bounded non-durable 202 while the first transaction owns the scoped
key, or 404 `REQUEST_NOT_FOUND` after rollback/nonexistence. A key owned by
another student is indistinguishable from not found.

## Idempotency Semantics

The scope is exactly `(authenticated StudentId, route TermId,
ClientRequestId)`. The same UUID in a different term is allowed and
independent. `RegistrationSubmission` is both claim and stored result; no
second IdempotencyRecord exists. A 202 is retry guidance, not proof of a
committed Processing row.

An accepted first execution creates one globally unique human-safe Reference
and one immutable ReceiptSnapshot in the same SQL transaction as seats,
enrollments, decision snapshot, audit, and final result. Every replay returns
the same reference/snapshot.

## Shared Rules

All protected operations use server authorization, privacy-safe errors,
ISO-8601 server time, stable correlation IDs, and no remote call inside the
registration transaction.
