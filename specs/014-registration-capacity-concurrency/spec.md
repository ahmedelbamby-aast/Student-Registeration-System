# Feature Specification: Registration Capacity and Concurrency

**Feature Branch**: 014-registration-capacity-concurrency
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Data/Backend Lead
**Normative detail**: [requirements.md](requirements.md)

## Context

Peak registration creates races for the final seats. Every schedule must be
accepted in full or rejected in full. Preview data is stale by definition, so
term, policy, overlap, ownership, duplicates, and capacity require final
server/SQL validation. ADR-002 defines the proposed strategy.

## User Scenarios and Testing

### User Story 1 - Final seat race (FR-4, FR-6, NFR-1) (P1)

As a Student, I need the Final seat race (FR-4, FR-6, NFR-1) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given one seat remains in a published group<br>
And two eligible students submit concurrently<br>
When both transactions reach SQL Server<br>
Then exactly one succeeds<br>
And one receives 409 GROUP_FULL<br>
And EnrolledCount never exceeds Capacity.
### User Story 2 - Multi-group rollback (FR-4, FR-6) (P1)

As a Student, I need the Multi-group rollback (FR-4, FR-6) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given a plan contains Group A with a seat and Group B that becomes full<br>
When final allocation is attempted in one transaction<br>
Then the submission is rejected<br>
And neither Group A nor Group B count/enrollment changes for the student.
### User Story 3 - Idempotent retry (FR-2, FR-7) (P2)

As a Student, I need the Idempotent retry (FR-2, FR-7) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a submission committed but the response was lost<br>
When the same idempotency key is retried<br>
Then the original receipt/result is returned<br>
And no additional seat or enrollment is created.
### User Story 4 - Collision load (FR-8, NFR-1) (P2)

As a Student, I need the Collision load (FR-8, NFR-1) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given a group capacity of 30 and no enrollments<br>
When 100 eligible students submit simultaneously<br>
Then exactly 30 active enrollments exist<br>
And exactly 30 successful results are recorded<br>
And EnrolledCount is 30.
### User Story 5 - Stale policy/plan (FR-3, FR-10) (P3)

As a Student, I need the Stale policy/plan (FR-3, FR-10) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given plan or policy version changed after review<br>
When submit occurs<br>
Then 409 PLAN_CHANGED or POLICY_CHANGED is returned<br>
And no capacity/enrollment mutation occurs.
### User Story 6 - Ordered drop and reconciliation (FR-5, FR-9, FR-11) (P3)

As a Student, I need the Ordered drop and reconciliation (FR-5, FR-9, FR-11) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

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

## Requirements

### Functional Requirements

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

### Key Entities

- **RegistrationSubmission**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Enrollment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SectionGroup**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **DecisionSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Concurrent submissions never overbook a group or duplicate an active course enrollment.
- **SC-2**: A multi-group registration is either fully accepted or has no committed enrollment changes.
- **SC-3**: Retrying the same submission never allocates an additional seat.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-011](../011-eligibility-subject-discovery/spec.md)
- [SPEC-012](../012-schedule-builder-conflicts/spec.md)
- [SPEC-013](../013-schedule-recommendations/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Out of Scope

- OS-1: Waitlist, reservation timeout, or queue.
- OS-2: Distributed lock.
- OS-3: Partial schedule acceptance.
- OS-4: Capacity override that exceeds approved group capacity.
