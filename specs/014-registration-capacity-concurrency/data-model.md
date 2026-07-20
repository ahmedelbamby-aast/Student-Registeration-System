# Data Model: Registration Capacity and Concurrency

**Owner-approved amendment:** `registration-roadmap-line-approval/1.0`, Ahmed
ELbamby, 2026-07-20.

## Canonically Owned Models

- **RegistrationSubmission**: Registration aggregate row and sole idempotency
  claim/lifecycle/result record.
- **RegistrationSubmissionLine**: One selected offering/group subject and its
  line-approval state.
- **RegistrationSeatHold**: Capacity-consuming active hold for one pending
  self-service line.
- **RegistrationApprovalDecision**: Immutable decision and actor/scope audit
  evidence for one line.
- **FirstTermAutoEnrollmentBatch** and **FirstTermAutoEnrollmentItem**:
  durable, retryable first-program-term roadmap orchestration and per-student
  result.
- **Enrollment**: Active student-to-offering/group enrollment.
- **DecisionSnapshot**: Immutable exact decision/version evidence.

## Consumed Model

- **StudentTermAcademicState**: Owned by SPEC-008/Academics. Registration
  consumes its unique student+term rowversion and
  `ExecuteRegistrationBoundaryAsync` transaction callback, advances the
  version on success, and never creates a duplicate guard or mapping.
- **SectionGroup**: Owned by SPEC-010/Scheduling. Registration consumes its
  published capacity/version contract and never redefines ownership.

## Detailed Model

| Field/entity | Type | Constraints |
|---|---|---|
| StudentTermAcademicState | consumed SPEC-008 aggregate row | unique StudentId + TermId; shared database serialization/version boundary; invoked through ExecuteRegistrationBoundaryAsync and advanced on successful commit |
| RegistrationSubmission.ClientRequestId | UUID | unique with StudentId + TermId; same UUID in another term is independent |
| RegistrationSubmission.PayloadHash | fixed hash | canonical server-computed payload; immutable |
| RegistrationSubmission.Origin | enum | StudentSelfService or FirstTermAutomatic |
| RegistrationSubmission.State | enum | internal Processing; durable PendingApproval, Accepted, Rejected, or Expired |
| RegistrationSubmission.RequestedCredits | decimal | server-computed from selected published roadmap courses |
| RegistrationSubmissionLine | child aggregate member | unique SubmissionId + OfferingId; selected GroupId; PendingApproval/Approved/Rejected state; rowversion |
| RegistrationSeatHold | entity | unique SubmissionLineId; StudentId/TermId/OfferingId/GroupId; Active/Consumed/Released/Expired; held/released timestamps; rowversion |
| RegistrationApprovalDecision | immutable entity | unique line terminal decision; actor ID/role, decision, reason, assignment-scope GroupId when staff, policy/version, client request ID, correlation, decided time |
| FirstTermAutoEnrollmentBatch | aggregate | TermId + CatalogueVersionId + cohort scope + purpose uniqueness; Pending/Running/Complete/CompletedWithFailures/Failed; lease/version/timestamps |
| FirstTermAutoEnrollmentItem | child/entity | unique batch + StudentId; Pending/Running/Accepted/Failed; idempotency key, safe failure code, resulting SubmissionId |
| RegistrationSubmission.Reference | human-safe string | non-null and globally unique for Accepted; generated once |
| RegistrationSubmission.ReceiptSnapshot | immutable JSON/value | accepted term/course/group/credits/staff/room/meeting/policy/submission display facts |
| RegistrationSubmission.Result | immutable value | deterministic status/code/versions/timestamps; replay source |
| Enrollment | entity | unique active StudentId + OfferingId; GroupId must belong to OfferingId |
| SectionGroup.EnrolledCount | integer | active enrollment counter |
| SectionGroup.HeldSeatCount | integer | active pending-line hold counter |
| SectionGroup.Version | rowversion | consumed capacity/publication concurrency token |
| DecisionSnapshot | immutable JSON/value | exact policy/profile/plan/group input and result versions |

## Integrity Rules

- Unique constraint: `(StudentId, TermId, ClientRequestId)`.
- Unique constraint: `RegistrationSubmissionLine(SubmissionId, OfferingId)`.
- Unique constraint: one `RegistrationSeatHold` per SubmissionLineId and one
  active hold per StudentId + TermId + OfferingId.
- Unique constraint: one final `RegistrationApprovalDecision` per line; its
  actor-scoped ClientRequestId is payload-bound and idempotent.
- Check constraint:
  `Capacity >= 0 AND EnrolledCount >= 0 AND HeldSeatCount >= 0 AND
  EnrolledCount + HeldSeatCount <= Capacity`.
- Registration, profile, and hold mutations for one student and term serialize
  through the one SPEC-008 `StudentTermAcademicState` row; SPEC-014 adds no
  second student-term guard.
- Accepted submissions require a unique Reference and ReceiptSnapshot;
  rejected/expired submissions have neither active enrollments nor an accepted
  receipt. PendingApproval submissions have one active hold for every line and
  no Enrollment.
- Every self-service line requires exactly one final decision. Admin may decide
  any line; Lecturer/TeachingAssistant decisions require a current effective
  GroupStaffAssignment to that line's selected group.
- The last required approval revalidates the complete plan, changes every
  active hold to Consumed, decrements HeldSeatCount, increments EnrolledCount,
  and creates every Enrollment in one transaction. One rejected line or
  window-close expiry releases every hold in one transaction.
- A held-to-enrolled conversion leaves occupied seats unchanged. Capacity
  changes use occupied seats and cannot reduce below EnrolledCount +
  HeldSeatCount.
- FirstTermAutoEnrollmentBatch selects applicable required
  CurriculumCourse rows whose RecommendedTerm is one and whose prerequisite
  graph position is a root. Each student item enrolls all selected roots or
  none, uses deterministic valid SectionGroups, and is idempotent by student,
  term, catalogue version, and batch purpose.
- General capacity projections expose Capacity, EnrolledCount, HeldSeatCount,
  and derived AvailableSeatCount without any hold-owner identity.
- Reference/snapshot, enrollment, counter, decision snapshot, audit, and final
  submission state commit atomically in the registration SQL transaction.
- Foreign keys and unique/check constraints are final guards.
- Server timestamps and explicit TermId are mandatory; lifecycle/retention
  follows SPEC-005.
