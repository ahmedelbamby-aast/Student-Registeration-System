# API Contract: Student Registration Records

## Feature Contract

```typescript
interface RegistrationReceiptDto {
  submissionId: string;
  reference: string;
  term: RegistrationTermSnapshotDto;
  submittedAtUtc: string;
  resultCode: string;
  policyVersion: string;
  groups: RegistrationRecordGroupDto[];
  totalCredits: number;
}
interface RegistrationTermSnapshotDto {
  id: string;
  code: string;
  displayName: string;
  timeZoneId: string;
}
interface RegistrationHistoryRowDto {
  submissionId: string;
  reference?: string;
  term: RegistrationTermSnapshotDto;
  status: "accepted" | "rejected";
  submittedAtUtc: string;
  groupCount: number;
  totalCredits: number;
  termState: "draft" | "registrationOpen" | "registrationClosed" | "teaching" | "completed" | "archived";
}
interface RegistrationRejectedResultDto {
  submissionId: string;
  status: "rejected";
  resultCode: string;
  safeMessage: string;
  submittedAtUtc: string;
  completedAtUtc: string;
  noPartialRegistration: true;
}
type RegistrationDetailDto =
  | { status: "accepted"; receipt: RegistrationReceiptDto }
  | { status: "rejected"; rejection: RegistrationRejectedResultDto };
type RegistrationHistoryPageDto = Page<RegistrationHistoryRowDto>; // canonical SPEC-006 Page<T>
interface RegistrationTimetableDto {
  term?: RegistrationTermSnapshotDto;
  termState: "none" | "registrationOpen" | "teaching";
  registrationWindowState: "none" | "scheduled" | "open" | "closed";
  groups: RegistrationRecordGroupDto[];
  subjectDiscoveryPath?: "/student/subjects";
}
```

`RegistrationTermSnapshotDto` is the immutable term identity stored by
SPEC-014 with the accepted receipt. It intentionally does not use the mutable
`TermSummaryDto`: current lifecycle state and row version were not part of the
historical snapshot and MUST NOT be fabricated or joined into an old receipt.
`RegistrationHistoryRowDto.termState` is deliberately separate: it is the
current lifecycle label used to identify archived rows and is never embedded
in, or substituted for, the immutable receipt term snapshot.

## Student Endpoints

- `GET /api/student/registrations?page=1&pageSize=20&termId={optionalTermId}`
- `GET /api/student/registrations/{submissionId}`
- `GET /api/student/registrations/current/timetable`

All three endpoints require an authenticated `Student` with the exact
`RegistrationRecords.ReadOwn` permission. The server derives StudentId from
authentication before query composition. Another student's identifier returns
privacy-safe 404. Lists default to page 1 and page size 20, reject page sizes
outside 1..100, and sort SubmittedAtUtc descending then SubmissionId. The
canonical SPEC-006 `Page.sort` echoes that applied order. The detail endpoint
returns the `RegistrationDetailDto` union: accepted rows carry the immutable
receipt projection; rejected rows carry no Reference or ReceiptSnapshot and
explicitly assert `noPartialRegistration`.

For these Student endpoints, missing authentication is 401, an authenticated
caller without `RegistrationRecords.ReadOwn` is 403, and an owned-resource miss
or cross-owner identifier is privacy-safe 404. Authorization executes before
resource, owner, term, snapshot, or version disclosure.

The current-timetable endpoint resolves exactly one server-current teaching
term, falling back to exactly one registration-open term when no teaching term
exists. More than one candidate in either state fails closed with 409
`CURRENT_TERM_AMBIGUOUS`. With no candidate it returns 200 with `termState`
`none`, no groups, and no discovery path. An empty registration-open term
includes `/student/subjects` only while the applicable published window is
open at the authoritative server instant. Window selection is open first,
otherwise earliest upcoming by `(OpensAtUtc, Id)`, otherwise latest closed by
`(ClosesAtUtc DESC, Id)`. Archived terms never become current; they remain
available through history/detail as immutable read-only records.
Calendar, chronological list, and print render from the same `groups` meeting
collection and do not issue separate schedule queries.

## Admin Inspection Endpoints

- `GET /api/admin/students/{studentId}/terms/{termId}/registrations?page=1&pageSize=20`
- `GET /api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId}`

Both require Admin plus `RegistrationRecords.Read`, validate that submission,
student and term match, apply the same bounded projection/PII minimization as
the student view, and audit actor, target student, term, endpoint, outcome and
correlation ID. Lecturer/TA do not receive these endpoints.

Admin inspection audit metadata is limited to actor identifier, target
StudentId, TermId, endpoint kind (`list` or `detail`), outcome, correlation ID,
and server timestamp. It MUST NOT contain University ID, student name,
receipt JSON, decision JSON, course/group details, credentials, or a full
record body.

## Persistence Contract

All DTOs project the Reference, ReceiptSnapshot, DecisionSnapshot, and result
already stored atomically by SPEC-014. GET operations never create or update a
receipt and never expose drop, withdrawal, correction, or seat-decrement
actions.

## Responses

Success is 200. Invalid paging is 400 `PAGE_SIZE_INVALID`. Missing
authentication is 401. Missing required permission is 403. Owned-resource
misses and out-of-scope or mismatched identifiers are privacy-safe 404. Dates
are ISO-8601 server timestamps. Unexpected failures use the canonical
privacy-safe `ApiError` and correlation reference; no response exposes SQL,
topology, credentials, or another student's data.

## Endpoint Outcome Matrices

| Endpoint | 200 | 400 | 401 | 403 | 404 | 409 | 429 | 500/503 |
|---|---|---|---|---|---|---|---|---|
| Student list | bounded `Page<RegistrationHistoryRowDto>` | `PAGE_SIZE_INVALID` or `TERM_ID_INVALID` | unauthenticated | wrong role/missing `RegistrationRecords.ReadOwn` | not used | not used | canonical rate limit | canonical safe error |
| Student detail | accepted/rejected `RegistrationDetailDto` | invalid GUID is route miss | unauthenticated | wrong role/missing `RegistrationRecords.ReadOwn` | missing or cross-owner submission | not used | canonical rate limit | canonical safe error |
| Current timetable | `RegistrationTimetableDto`, including empty | not used | unauthenticated | wrong role/missing `RegistrationRecords.ReadOwn` | student profile not found | `CURRENT_TERM_AMBIGUOUS` | canonical rate limit | canonical safe error |
| Admin list | bounded audited page | `PAGE_SIZE_INVALID` | unauthenticated | wrong role/missing `RegistrationRecords.Read` | target student/term not found | not used | canonical rate limit | canonical safe error |
| Admin detail | audited detail | invalid GUID is route miss | unauthenticated | wrong role/missing `RegistrationRecords.Read` | any student/term/submission mismatch | not used | canonical rate limit | canonical safe error |

Accepted detail contains the stored reference, term/group/staff/location/
meeting snapshot, credits, policy version, result code, and server submission
time. Rejected detail contains no reference or receipt, maps the stable result
code to a privacy-safe recovery message, and always returns
`noPartialRegistration: true`.
