# Data Model: Registration Capacity and Concurrency

## Canonically Owned Models

- **RegistrationSubmission**: Registration aggregate row and sole idempotency
  claim/final-result record.
- **Enrollment**: Active student-to-offering/group enrollment.
- **DecisionSnapshot**: Immutable exact decision/version evidence.
- **StudentTermRegistrationGuard**: Student+term serialization boundary.

## Consumed Model

- **SectionGroup**: Owned by SPEC-010/Scheduling. Registration consumes its
  published capacity/version contract and never redefines ownership.

## Detailed Model

| Field/entity | Type | Constraints |
|---|---|---|
| StudentTermRegistrationGuard | aggregate row | unique StudentId + TermId; database serialization/version boundary |
| RegistrationSubmission.ClientRequestId | UUID | unique with StudentId + TermId; same UUID in another term is independent |
| RegistrationSubmission.PayloadHash | fixed hash | canonical server-computed payload; immutable |
| RegistrationSubmission.State | enum | internal Processing in transaction, Accepted, Rejected; no separately committed Processing row |
| RegistrationSubmission.Reference | human-safe string | non-null and globally unique for Accepted; generated once |
| RegistrationSubmission.ReceiptSnapshot | immutable JSON/value | accepted term/course/group/credits/staff/room/meeting/policy/submission display facts |
| RegistrationSubmission.Result | immutable value | deterministic status/code/versions/timestamps; replay source |
| Enrollment | entity | unique active StudentId + OfferingId; GroupId must belong to OfferingId |
| SectionGroup.EnrolledCount | integer | consumed invariant 0 <= count <= Capacity |
| SectionGroup.Version | rowversion | consumed capacity/publication concurrency token |
| DecisionSnapshot | immutable JSON/value | exact policy/profile/plan/group input and result versions |

## Integrity Rules

- Unique constraint: `(StudentId, TermId, ClientRequestId)`.
- Accepted submissions require a unique Reference and ReceiptSnapshot;
  rejected submissions have neither active enrollments nor an accepted receipt.
- Reference/snapshot, enrollment, counter, decision snapshot, audit, and final
  submission state commit atomically in the registration SQL transaction.
- Foreign keys and unique/check constraints are final guards.
- Server timestamps and explicit TermId are mandatory; lifecycle/retention
  follows SPEC-005.
