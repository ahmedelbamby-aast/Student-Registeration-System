# SPEC-015: Student Registration Records

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Registrar/Policy SME, UX, Data, QA<br>
**Target:** Sprint 6<br>

## Context

After an atomic result, students and authorized staff need a reliable receipt,
current timetable, historical terms, and the exact policy decision used.
Rejected submissions must clearly state that no partial registration occurred.

## Functional Requirements

- FR-1: Successful submission MUST produce a unique receipt/reference.
- FR-2: Receipt MUST include term, course/group, credits, Lecturer/TA, room,
  day/time, policy version, and submission time.
- FR-3: A rejected atomic submission MUST state the reason and that no partial
  enrollment was created.
- FR-4: Students MUST view their current registrations/timetable and historical
  terms.
- FR-5: Authorized staff/admin MAY inspect records within server-enforced scope.
- FR-6: Calendar and printable table/list MUST present equivalent schedule data.
- FR-7: Decision snapshots and historical group details MUST retain their
  original meaning after later edits.
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
Then the API returns 403/404 according to security policy<br>
And no Student B data is returned.

### AC-4: Historical snapshot (FR-7)
Given a room/group display name changes after registration<br>
When the original receipt is inspected<br>
Then the stored historical snapshot remains available with original details.

### AC-5: Current/history views without unapproved actions (FR-4, FR-6, FR-8)
Given a student has current and historical registrations<br>
When both timetable views are opened<br>
Then calendar and accessible list/table show equivalent authorized records<br>
And no drop/correction action appears before its workflow is approved.

## Edge Cases

- EC-1: Result response lost -> idempotent lookup returns receipt.
- EC-2: Receipt render service error -> safe retry by reference.
- EC-3: No registrations -> meaningful empty state with eligible next action.
- EC-4: Historical term archived -> remains read-only and accessible.

## API Contracts

```typescript
interface RegistrationReceiptDto {
  submissionId: string;
  reference: string;
  term: TermSummaryDto;
  submittedAtUtc: string;
  policyVersion: string;
  groups: GroupDto[];
  totalCredits: number;
}
```

Endpoints: GET /api/student/registrations, GET
/api/student/registrations/{submissionId}, GET
/api/student/registrations/current/timetable.

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
