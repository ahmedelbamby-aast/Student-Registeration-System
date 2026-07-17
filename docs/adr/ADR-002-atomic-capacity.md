# ADR-002: Atomic Seat Allocation

- Status: Proposed
- Date: 2026-07-12
- Decision owners: Data Lead, Backend Lead, QA, Security

## Context

Many students may submit for the final seat simultaneously. The same student
may also submit two different plans that are valid alone but invalid together.
Reading a count or validating outside the serialization boundary can therefore
violate capacity, credit, timetable, term, hold, or policy invariants. A
schedule must commit entirely or not at all, and retries must not duplicate it.

## Decision

Within one short SQL Server transaction:

- After beginning the transaction, atomically claim the
  student/term/idempotency key and compare a server-canonical payload hash.
- Enter the SPEC-008 `StudentTermAcademicState` transaction boundary through
  `ExecuteRegistrationBoundaryAsync`, then lock the remaining registration
  context/version records and sorted SectionGroup rows in that stable order.
- Revalidate term, policy, profile/holds, conflicts, ownership, group versions,
  credit load, and duplicates after acquiring those boundaries.
- Create an allocation savepoint after the claim and revalidation, before the
  first seat mutation.
- Allocate each sorted group with a conditional UPDATE where EnrolledCount is
  below Capacity.
- If a deterministic allocation/invariant failure occurs, roll back every seat
  mutation to the savepoint, verify no partial state remains, store the stable
  rejected result/rejection audit, and commit that replayable claim/result.
- If a transient or transaction-aborting failure occurs, roll back the whole
  transaction and claim; retry only after non-commit is known.
- Store successful enrollments, snapshot, audit event, and accepted
  idempotent result atomically.
- Enforce unique/check constraints and reconcile counters operationally.

A second request blocked on the same uncommitted idempotency key waits at most
500 ms. If no committed final row becomes visible, it receives a non-durable
202 containing clientRequestId, retryAfterSeconds, and resultUrl, but no
submissionId. The 202 does not expose an uncommitted Processing row. After the
first transaction commits, the final result is replayed; after rollback, the
lookup returns REQUEST_NOT_FOUND and the original POST may be retried.

Scheduled cutoff uses one authoritative server ReceivedAtUtc. Emergency closure
or context-version change before commit blocks the request. Use rowversion for
ordinary editing, not as the seat allocator. Student-term serialization uses
the unique StudentTermAcademicState row and its transaction protocol.

## Consequences

Positive:

- Exactly one winner for a final seat.
- One valid serial order for different plans submitted by the same student.
- No partial timetable.
- Explainable, replayable 409 response for a deterministic stale/full group.

Accepted costs:

- Popular groups briefly serialize on one database row.
- Mutations for one student/term briefly serialize on the shared academic-state
  row.
- EnrolledCount must be maintained and reconciled with enrollment rows.

## Rejected alternatives

- Application lock or distributed lock: duplicates database serialization.
- Read then insert: race unsafe.
- Queue/reservation service: more states and operations without measured need.
- Per-application student lock: fails when requests use different replicas.

## Verification

Real-SQL barrier tests cover every row in
specs/014-registration-capacity-concurrency/concurrency-matrix.md, including
100 simultaneous submissions at a 30-seat group, two different plans for one
student, idempotency payload mismatch, admin-versus-submit races, deadlock
retry, and process/network loss after commit. The two-replica suite must
preserve every invariant.
