# API Contract: Student Registration Records

## Feature Contract

```typescript
interface RegistrationReceiptDto {
  submissionId: string;
  reference: string;
  term: TermSummaryDto;
  submittedAtUtc: string;
  resultCode: string;
  policyVersion: string;
  groups: RegistrationGroupSnapshotDto[];
  totalCredits: number;
}
interface RegistrationHistoryRowDto {
  submissionId: string;
  reference?: string;
  term: TermSummaryDto;
  status: "accepted" | "rejected";
  submittedAtUtc: string;
  groupCount: number;
  totalCredits: number;
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
```

## Student Endpoints

- `GET /api/student/registrations?page=1&pageSize=20&termId={optionalTermId}`
- `GET /api/student/registrations/{submissionId}`
- `GET /api/student/registrations/current/timetable`

The server derives StudentId from authentication before query composition.
Another student's identifier returns 404. Lists default to 20, reject page
sizes outside 1..100, and sort SubmittedAtUtc descending then SubmissionId.
The canonical SPEC-006 `Page.sort` echoes that applied order.
The detail endpoint returns the `RegistrationDetailDto` union: accepted rows
carry the immutable receipt projection; rejected rows carry no Reference or
ReceiptSnapshot and explicitly assert `noPartialRegistration`.

## Admin Inspection Endpoints

- `GET /api/admin/students/{studentId}/terms/{termId}/registrations?page=1&pageSize=20`
- `GET /api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId}`

Both require Admin plus `RegistrationRecords.Read`, validate that submission,
student and term match, apply the same bounded projection/PII minimization as
the student view, and audit actor, target student, term, endpoint, outcome and
correlation ID. Lecturer/TA do not receive these endpoints.

## Persistence Contract

All DTOs project the Reference, ReceiptSnapshot, DecisionSnapshot, and result
already stored atomically by SPEC-014. GET operations never create or update a
receipt and never expose drop, withdrawal, correction, or seat-decrement
actions.

## Responses

Success is 200. Invalid paging is 400 `PAGE_SIZE_INVALID`. Authentication is
401; insufficient Admin permission is 403; out-of-scope/mismatched identifiers
are privacy-safe 404. Dates are ISO-8601 server timestamps.
