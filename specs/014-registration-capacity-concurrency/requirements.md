# SPEC-014: Registration Capacity and Concurrency

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Data/Backend Lead<br>
**Reviewers:** Security, QA, DevOps, Registrar<br>
**Target:** Sprint 6<br>
**Dependencies:** SPEC-007, SPEC-010, SPEC-011, SPEC-012, SPEC-013, SPEC-018<br>

## Context

Peak registration creates races for the final seats. Every schedule must be
accepted in full or rejected in full. Preview data is stale by definition, so
term, policy, overlap, ownership, duplicates, and capacity require final
server/SQL validation. ADR-002 defines the proposed strategy.

## Functional Requirements

- FR-1: Submission MUST resolve the student from authenticated server identity.
- FR-2: Submission MUST accept PlanId, plan rowversion, term, and a
  client-generated idempotency key.
- FR-3: The server MUST revalidate window, student/holds, policy version,
  eligibility, credit load, duplicates, group state, and timetable.
- FR-4: Every selected group seat MUST be allocated by a conditional atomic SQL
  update within the enrollment transaction.
- FR-5: Group IDs MUST be allocated in stable sorted order to reduce deadlocks.
- FR-6: If any allocation or invariant fails, the whole transaction MUST roll
  back and create no active partial enrollment.
- FR-7: The same student/term/idempotency key MUST return the original result
  and MUST NOT allocate again.
- FR-8: Unique/check constraints MUST be final guards for duplicates/capacity.
- FR-9: Drops MUST transition enrollment and decrement capacity transactionally.
- FR-10: Expected business conflicts MUST return 409 with stable reason codes.
- FR-11: The system MUST reconcile EnrolledCount to active Enrollment and alert
  on mismatch.

## Non-Functional Requirements

- NFR-1: Zero group overbooking and zero duplicate active offering enrollment.
- NFR-2: Submission p95 MUST be <= 2 seconds at 75 submissions/s for 10 min.
- NFR-3: A 200 submissions/s, 60-second spike MUST preserve all invariants.
- NFR-4: Database transactions MUST be short and contain no remote calls.
- NFR-5: Expected conflicts MUST not count as server failures; unexpected
  failure rate MUST remain below 0.1% at target.

## Acceptance Criteria

### AC-1: Final seat race (FR-4, FR-6, NFR-1)
Given one seat remains in a published group<br>
And two eligible students submit concurrently<br>
When both transactions reach SQL Server<br>
Then exactly one succeeds<br>
And one receives 409 GROUP_FULL<br>
And EnrolledCount never exceeds Capacity.

### AC-2: Multi-group rollback (FR-4, FR-6)
Given a plan contains Group A with a seat and Group B that becomes full<br>
When final allocation is attempted in one transaction<br>
Then the submission is rejected<br>
And neither Group A nor Group B count/enrollment changes for the student.

### AC-3: Idempotent retry (FR-2, FR-7)
Given a submission committed but the response was lost<br>
When the same idempotency key is retried<br>
Then the original receipt/result is returned<br>
And no additional seat or enrollment is created.

### AC-4: Collision load (FR-8, NFR-1)
Given a group capacity of 30 and no enrollments<br>
When 100 eligible students submit simultaneously<br>
Then exactly 30 active enrollments exist<br>
And exactly 30 successful results are recorded<br>
And EnrolledCount is 30.

### AC-5: Stale policy/plan (FR-3, FR-10)
Given plan or policy version changed after review<br>
When submit occurs<br>
Then 409 PLAN_CHANGED or POLICY_CHANGED is returned<br>
And no capacity/enrollment mutation occurs.

### AC-6: Ordered drop and reconciliation (FR-5, FR-9, FR-11)
Given an active multi-group enrollment and matching counters<br>
When an approved drop transitions one group in the sorted transactional path<br>
Then its counter decrements exactly once<br>
And reconciliation still matches active Enrollment counts.

## Edge Cases

- EC-1: Deadlock/transient SQL error -> EF execution strategy retries the
  complete transaction only when safe with idempotency.
- EC-2: Unique constraint races conditional update -> roll back and map to
  stable duplicate conflict.
- EC-3: Cancellation after commit -> retry recovers stored result.
- EC-4: Capacity reduced concurrently -> only valid Capacity >= EnrolledCount
  command can commit.
- EC-5: Counter reconciliation mismatch -> alert and controlled repair;
  registration for affected group MAY be paused.

## API Contracts

```typescript
interface SubmitRegistrationRequest {
  planId: string;
  planRowVersion: string;
  termId: string;
  clientRequestId: string;
}
interface RegistrationCommitResult {
  submissionId: string;
  status: "accepted" | "rejected";
  resultCode: string;
  registeredGroups: GroupDto[];
  submittedAtUtc: string;
  policyVersion: string;
}
```

Endpoint: POST /api/student/registrations. Success 201 (or 200 for idempotent
replay); business conflicts 409; validation 400; auth 401/403.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| RegistrationSubmission.ClientRequestId | UUID | unique with student + term |
| Enrollment | entity | unique student + offering |
| SectionGroup.EnrolledCount | integer | 0 <= count <= capacity |
| SectionGroup.Version | rowversion | concurrency token |
| DecisionSnapshot | JSON/value | policy/input/result version retained |

## Out of Scope

- OS-1: Waitlist, reservation timeout, or queue.
- OS-2: Distributed lock.
- OS-3: Partial schedule acceptance.
- OS-4: Capacity override that exceeds approved group capacity.
