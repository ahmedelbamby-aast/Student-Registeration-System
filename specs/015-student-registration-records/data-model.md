# Data Model: Student Registration Records

## Owned Read Models

- **RegistrationReceipt**: Immutable read value/projection of one canonical accepted submission; not a second EF entity/table.
- **RegistrationHistoryRowDto**: Bounded current/history list row.
- **RegistrationTimetableDto**: Calendar/list-equivalent term timetable.

## Consumed Canonical Data

- **RegistrationSubmission**, **Enrollment**, **DecisionSnapshot**,
  **Reference**, and **ReceiptSnapshot** are owned and atomically persisted by
  SPEC-014. SPEC-015 MUST NOT add a second receipt entity/table or rewrite a
  historical snapshot.

## Detailed Model

| Model/field | Type | Constraints |
|---|---|---|
| RegistrationSubmission.Reference | consumed string | globally unique; accepted result only; generated once |
| RegistrationSubmission.ReceiptSnapshot | consumed immutable value | exact original display details required by FR-2/FR-7 |
| DecisionSnapshot.PolicyVersion | consumed string | required historical decision version |
| RegistrationReceiptDto | read projection | reference, submission, term, result, timestamp, policy, group snapshot, total credits |
| RegistrationHistoryRowDto | read projection | explicit term, accepted-only nullable reference, status, submitted time, course/group count, total credits |
| RegistrationDetailDto | discriminated read union | accepted receipt projection or rejected result with reason and noPartialRegistration=true |
| Page<T> | contract | page >= 1; default 20; max 100; totalCount; stable SubmittedAtUtc-desc then SubmissionId order |

## Integrity and Access Rules

- Receipt/reference/snapshots commit in the SPEC-014 transaction with accepted
  enrollments; a replay reads the same record.
- Rejected submissions have no Reference or ReceiptSnapshot and return the
  rejected detail branch; they can never be rendered as an accepted receipt.
- Student queries always filter authenticated StudentId before identifiers.
- Admin queries require `RegistrationRecords.Read`, explicit StudentId and
  TermId scope, and emit an audit event without logging full record content.
- Records are read-only; drop, withdrawal, and correction are absent.
- Retention follows SPEC-005 and preserves snapshot readability.
