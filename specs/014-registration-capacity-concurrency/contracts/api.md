# API Contract: Registration Capacity and Concurrency

**Amendment:** `registration-roadmap-line-approval/1.0`, owner-approved by
Ahmed ELbamby on 2026-07-20. This amendment adds a durable
`pendingApproval` state and scoped line decisions. The existing non-durable
`processing` 202 remains a distinct idempotency-observation outcome.

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
  status: "pendingApproval" | "accepted" | "rejected" | "expired";
  resultCode: string;
  registeredGroups: RegistrationGroupSnapshotDto[];
  receivedAtUtc: string;
  completedAtUtc: string;
  policySetId: string;
  policyVersion: string;
  planRowVersion: string;
  reference?: string;
  receiptSnapshot?: RegistrationReceiptSnapshotDto;
  origin: "studentSelfService" | "firstTermAutomatic";
  requestedCredits: number;
  lines: RegistrationSubmissionLineDto[];
}
interface RegistrationInProgressResponse {
  clientRequestId: string;
  status: "processing";
  retryAfterSeconds: number;
  resultUrl: string;
}
interface CapacitySummaryDto {
  capacity: number;
  enrolledCount: number;
  heldSeatCount: number;
  availableSeatCount: number;
}
interface RegistrationSubmissionLineDto {
  lineId: string;
  offeringId: string;
  groupId: string;
  courseCode: string;
  subjectTitle: string;
  credits: number;
  state: "pendingApproval" | "approved" | "rejected" | "expired";
  capacity: CapacitySummaryDto;
  rowVersion: string;
  decision?: RegistrationApprovalDecisionDto;
}
interface RegistrationApprovalDecisionDto {
  decision: "approved" | "rejected";
  actorRole: "Admin" | "Lecturer" | "TeachingAssistant";
  reason: string;
  decidedAtUtc: string;
}
interface DecideRegistrationLineRequest {
  decision: "approve" | "reject";
  reason: string;
  expectedSubmissionRowVersion: string;
  expectedLineRowVersion: string;
  clientRequestId: string;
}
interface RegistrationApprovalQueueRowDto {
  submissionId: string;
  line: RegistrationSubmissionLineDto;
  studentUniversityId: string;
  studentDisplayName: string;
  requestedCredits: number;
  currentCgpa: number;
  overload: boolean;
  submittedAtUtc: string;
  windowClosesAtUtc: string;
}
interface FirstTermAutoEnrollmentBatchDto {
  batchId: string;
  termId: string;
  catalogueVersionId: string;
  cohortScope: string;
  state: "pending" | "running" | "complete" | "completedWithFailures" | "failed";
  totalStudents: number;
  acceptedStudents: number;
  failedStudents: number;
  createdAtUtc: string;
  completedAtUtc?: string;
  rowVersion: string;
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

- 201: newly committed durable `pendingApproval` self-service result or newly
  committed accepted first-term automatic result.
- 200: same-scope, same-payload durable lifecycle replay.
- 202: first same-scope claim is still uncommitted after at most 500 ms;
  contains no submissionId and names the term-scoped lookup URL.
- 400: malformed input.
- 401/403: authentication/authorization.
- 409: business/version conflict or same-scope key reused with different
  canonical payload.

For authoritative program term one this student self-service route returns
`409 FIRST_TERM_AUTOMATIC_REGISTRATION`; the student reads the automatic result
through registration-record endpoints. From term two onward, the server keeps
the stricter probation maximum, applies normal maximum 18, permits 19-21 only
with CGPA >=3.00, and rejects >21 before creating a hold. Every valid
self-service command creates all pending lines and active holds atomically.

### GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}

This endpoint also requires an authenticated `Student` with the exact
`Registration.SubmitOwn` permission. Authorization occurs before result,
owner, term, submission, or version disclosure. It returns only the
authenticated student's result in that route term: 200 durable pending or
final result, the same bounded
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

A PendingApproval execution stores the same scoped claim plus every line and
hold. Replay returns identical submission/line identifiers. Decision commands
use an independent actor + line + ClientRequestId scope and bind the canonical
decision payload; conflicting reuse returns 409 IDEMPOTENCY_KEY_REUSED.

## Advisory Inputs and Final Policy Authority

SPEC-012 plan credit fields and SPEC-013 recommendation values are advisory.
They never authorize registration and never replace the effective SPEC-009
policy. After acquiring the database boundaries, hold creation and final
approval both re-read the
current academic state, catalogue, PolicySet ID/version, plan, offering, and
group state and recompute every rule. The approved demo policy applies an
18-credit normal maximum, a 12-credit maximum when GPA is below 2.0, and
permits 19-21 credits only when CGPA is at least 3.00; >21 is rejected. A
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

## Approval Endpoints

### Staff assignment-scoped approval

- `GET /api/staff/registration-approvals?state=pending&page=1&pageSize=20`
- `GET /api/staff/registration-approvals/{submissionId}/lines/{lineId}`
- `POST /api/staff/registration-approvals/{submissionId}/lines/{lineId}/decision`

These routes require Lecturer or TeachingAssistant plus
`RegistrationApproval.DecideAssigned`. The server authorizes before lookup
and revalidates a current effective GroupStaffAssignment to the line's selected
group inside the decision transaction. Unassigned, ended, or changed scope is
privacy-safe 403/404 with no line/version disclosure. POST requires same-origin
antiforgery and `DecideRegistrationLineRequest`.

### Admin global approval

- `GET /api/admin/registration-approvals?state=pending&page=1&pageSize=20`
- `GET /api/admin/registration-approvals/{submissionId}/lines/{lineId}`
- `POST /api/admin/registration-approvals/{submissionId}/lines/{lineId}/decision`

These routes require Admin plus `RegistrationApproval.DecideAll`. Admin may
decide any pending line but cannot bypass eligibility, capacity, conflict, or
load rules. Lists use bounded stable pagination. General capacity output uses
`CapacitySummaryDto` and never contains holder identity.

Decision success returns 200 with the current RegistrationFinalResult. An
intermediate approval leaves the submission pending. Approval of the final
line runs full in-transaction revalidation and returns accepted only when all
holds convert atomically. One rejection returns rejected and releases all
holds. Window close returns expired and releases all holds. Expected
stale/terminal outcomes use 409 `STALE_VERSION`, `LINE_ALREADY_DECIDED`,
`REGISTRATION_WINDOW_CLOSED`, or the governed validation code without partial
mutation.

## First-Term Automatic Enrollment

Term publication creates a durable internal FirstTermAutoEnrollmentBatch for
students whose server-authoritative program-term ordinal is one. It selects
applicable required `CurriculumCourse` rows with RecommendedTerm one and root
prerequisite status, then processes each student idempotently through the same
registration boundaries. It creates no approval line or hold: each student's
complete feasible schedule enrolls atomically with origin
`firstTermAutomatic`.

Admin monitoring is read-only except for idempotent failed-item retry:

- `GET /api/admin/terms/{termId}/first-term-auto-enrollment-batches`
- `GET /api/admin/first-term-auto-enrollment-batches/{batchId}`
- `POST /api/admin/first-term-auto-enrollment-batches/{batchId}/retry-failed`

Retry requires Admin plus the owning academic/registration permission,
antiforgery, expected batch rowversion, reason, and ClientRequestId. It cannot
select different courses, waive a failure, or create a partial schedule.
