# SPEC-015: Student Registration Records

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Registrar/Policy SME, UX, Data, QA<br>
**Target:** Sprint 6<br>
**Dependencies:** SPEC-003, SPEC-008, SPEC-014, SPEC-018<br>

## Context

After an atomic result, students and authorized staff need a reliable receipt,
current timetable, historical terms, and the exact policy decision used.
Rejected submissions must clearly state that no partial registration occurred.

## Functional Requirements

- FR-1: Every accepted SPEC-014 submission MUST atomically persist one
  globally unique human-safe Reference and immutable ReceiptSnapshot on the
  canonical RegistrationSubmission; SPEC-015 MUST project that same durable
  record and MUST NOT create a second receipt row.
- FR-2: Receipt MUST include term, course/group, credits, Lecturer/TA, room,
  day/time, policy version, and submission time.
- FR-3: A rejected atomic submission MUST state the reason and that no partial
  enrollment was created.
- FR-4: Students MUST view bounded, stable-sorted pages across current and historical registrations, with an optional TermId filter, plus the current timetable.
- FR-5: An Admin with RegistrationRecords.Read MUST inspect a student's
  term-scoped list/detail only through the explicit Admin endpoints; ordinary
  staff have no general registration-record endpoint, and Lecturer/TA access
  remains limited to assigned-group roster projections in SPEC-016.
- FR-6: Calendar and printable table/list MUST present equivalent schedule data.
- FR-7: The immutable ReceiptSnapshot and DecisionSnapshot committed by
  SPEC-014 MUST retain original term, course/group, credits, Lecturer/TA, room,
  meeting, policy-version, and server-time meaning after later edits.
- FR-8: Drop/correction actions MUST be absent until approved policy/workflow
  is specified.

## Non-Functional Requirements

- NFR-1: Receipt retrieval SHOULD respond within 300 ms p95.
- NFR-2: Record access MUST have ownership/role-scope tests.
- NFR-3: Printed/exported views MUST be accessible and minimize PII.
- NFR-4: Historical records MUST be durable under the approved retention plan.

## Acceptance Criteria

### AC-1: Accepted receipt (FR-1, FR-2)
Given a registration commits successfully<br>
When the result page loads<br>
Then it shows a unique reference and every registered group with staff,
location, day/time, credits, policy version, and server timestamp.

### AC-2: Atomic rejection (FR-3)
Given one selected group became full and transaction rolled back<br>
When the rejection is shown<br>
Then GROUP_FULL and resolution action are displayed<br>
And the message explicitly says no subjects were partially registered.

### AC-3: Ownership (FR-5, NFR-2)
Given Student A knows Student B's submission identifier<br>
When Student A requests it<br>
Then the API returns 404 and no Student B data is returned<br>
And given an Admin with RegistrationRecords.Read requests Student B's matching
student/term-scoped endpoint<br>
Then the authorized record is returned and the inspection is audited.

### AC-4: Historical snapshot (FR-7)
Given a room/group display name changes after registration<br>
When the original receipt is inspected<br>
Then the stored historical snapshot remains available with original details.

### AC-5: Current/history views without unapproved actions (FR-4, FR-6, FR-8)
Given a student has current and historical registrations<br>
When both timetable views are opened<br>
Then calendar and accessible list/table show equivalent authorized records<br>
And no drop/correction action appears before its workflow is approved.

### AC-6: Registration-record quality gate (NFR-1, NFR-3, NFR-4)
Given the approved read-load dataset, accessible print/export checks, and
records spanning the full retention fixture<br>
When record quality tests execute<br>
Then receipt retrieval is at most 300 ms p95<br>
And printed/exported views meet accessibility checks with minimized PII<br>
And historical records remain durable and readable throughout the approved
retention lifecycle.

## Edge Cases

- EC-1: Result response lost -> idempotent lookup returns receipt.
- EC-2: Receipt render service error -> safe retry by reference.
- EC-3: No registrations -> show the selected term and window state and, only
  when registration is open, a link to STU-02 subject discovery.
- EC-4: Historical term archived -> remains read-only and accessible.

## API Contracts

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
type RegistrationHistoryPageDto = Page<RegistrationHistoryRowDto>;
```

Endpoints: GET /api/student/registrations, GET
/api/student/registrations/{submissionId}, GET
/api/student/registrations/current/timetable, GET
/api/admin/students/{studentId}/terms/{termId}/registrations, and GET
/api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId}.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| RegistrationSubmission.Reference | string | unique human-safe reference |
| Enrollment.State | enum | controlled lifecycle |
| ReceiptSnapshot | JSON/value | immutable original display details |
| DecisionSnapshot.PolicyVersion | string | required historical version |

## Out of Scope

- OS-1: Drop/withdraw/correction workflow until AASTMT approves SPEC changes.
- OS-2: Email/SMS receipt.
- OS-3: Public/shareable receipt link.
- OS-4: Transcript replacement.
