# API Contract: Registration Capacity and Concurrency

## Feature Contract

```typescript
interface RegistrationMeetingStaffSnapshotDto {
  role: "Lecturer" | "TeachingAssistant";
  displayName: string;
}
interface RegistrationMeetingSnapshotDto {
  meetingId: string;
  activityType: "Lecture" | "Tutorial" | "Laboratory";
  dayOfWeek: number;
  startLocal: string;
  endLocal: string;
  roomCode: string;
  location: string;
  staff: RegistrationMeetingStaffSnapshotDto[];
}
interface RegistrationGroupSnapshotDto {
  offeringId: string;
  courseCode: string;
  subjectTitle: string;
  groupId: string;
  groupCode: string;
  credits: number;
  meetings: RegistrationMeetingSnapshotDto[];
}
interface RegistrationReceiptSnapshotDto {
  term: TermSummaryDto;
  groups: RegistrationGroupSnapshotDto[];
  totalCredits: number;
  policySetId: string;
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
  policySetId: string;
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

`RegistrationMeetingSnapshotDto.dayOfWeek` uses the shared .NET
`DayOfWeek` serialization `0..6` in the configured term timezone. Staff remain
nested under the meeting they teach, and `meetingId` plus `activityType`
preserve the canonical SPEC-010/SPEC-011 meeting-bound association. Array order
MUST NOT be used to infer a staff-to-meeting relationship.

## Endpoints and Scope

### POST /api/student/terms/{termId}/registrations

This endpoint requires an authenticated `Student` with the exact
`Registration.SubmitOwn` permission and a valid same-origin antiforgery token.
Missing or invalid antiforgery evidence returns `400 ANTIFORGERY_INVALID`
before handler execution. Student identity comes only from authentication;
`termId` comes from the route and is checked against the student's
server-resolved context. The body does not contain studentId or termId.

- 201: newly committed final result.
- 200: same-scope, same-payload final replay.
- 202: first same-scope claim is still uncommitted after at most 500 ms;
  contains no submissionId and names the term-scoped lookup URL.
- 400: malformed input.
- 401/403: authentication/authorization.
- 409: business/version conflict or same-scope key reused with different
  canonical payload.

### GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}

This endpoint also requires an authenticated `Student` with the exact
`Registration.SubmitOwn` permission. Authorization occurs before result,
owner, term, submission, or version disclosure. It returns only the
authenticated student's result in that route term: 200 final, the same bounded
non-durable 202 while the first transaction owns the scoped key, or 404
`REQUEST_NOT_FOUND` after rollback/nonexistence. A nonexistent key, rolled-back
claim, another student's key, and a key outside the authorized term are
indistinguishable and disclose no current version or submission identifier.

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

## Advisory Inputs and Final Policy Authority

SPEC-012 plan credit fields and SPEC-013 recommendation values are advisory.
They never authorize registration and never replace the effective SPEC-009
policy. After acquiring the database boundaries, the final commit re-reads the
current academic state, catalogue, PolicySet ID/version, plan, offering, and
group state and recomputes every rule. The approved demo policy applies an
18-credit normal maximum and a 12-credit maximum when GPA is below 2.0. A
static policy failure retains its stable policy reason; `POLICY_CHANGED` is
reserved for a changed governing policy version.

## Shared Serialization Boundary

The registration transaction uses the existing upstream database boundaries
in this order: SPEC-008 `StudentTermAcademicState` registration boundary;
registration-context/window records; the normalized SPEC-009
`CatalogueVersion.ScopeCode` serializable key range; the normalized SPEC-009
`PolicySet.ScopeCode` serializable key range; then SPEC-010 `SectionGroup` rows
sorted by group ID. Catalogue/policy publication and registration MUST acquire
the same SQL Server serializable update/key-range locks for the applicable
normalized scope and re-read the effective catalogue and PolicySet ID/version
after locking. Locking only the currently selected immutable version row is
insufficient because a concurrent publication may insert a new effective
version. Every group mutation and final seat allocation serialize on the same
owning `SectionGroup` row/version.

## Shared Rules

All protected operations use the authenticated `Student` role and exact
`Registration.SubmitOwn` permission, privacy-safe errors, ISO-8601 server time,
stable correlation IDs, and no remote call inside the registration
transaction. POST requires same-origin antiforgery; GET is read-only and
retains the privacy-safe lookup behavior above.
