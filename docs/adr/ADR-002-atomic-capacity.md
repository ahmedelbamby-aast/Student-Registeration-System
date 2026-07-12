# ADR-002: Atomic Seat Allocation

- Status: Proposed
- Date: 2026-07-12
- Decision owners: Data Lead, Backend Lead, QA, Security

## Context

Many students may submit for the final seat simultaneously. Reading a count
then inserting can oversubscribe. A schedule must commit entirely or not at
all, and retries must not duplicate enrollment.

## Decision

Within one short SQL Server transaction:

- Revalidate term, policy, conflicts, ownership, and duplicates.
- Allocate each sorted group with a conditional UPDATE where EnrolledCount is
  below Capacity.
- Roll back every allocation if one update affects zero rows.
- Upsert enrollment and store an idempotent submission result.
- Enforce unique/check constraints and reconcile counters operationally.

Use rowversion for ordinary editing, not as the only seat guard.

## Consequences

Positive:

- Exactly one winner for a final seat.
- No partial timetable.
- Immediate, explainable 409 response for a stale/full group.

Accepted costs:

- Popular groups briefly serialize on one database row.
- EnrolledCount must be maintained and reconciled with enrollment rows.

## Rejected alternatives

- Application lock or distributed lock: duplicates database serialization.
- Read then insert: race unsafe.
- Queue/reservation service: more states and operations without measured need.

## Verification

A real-SQL test launches 100 simultaneous submissions at a 30-seat group and
must finish with exactly 30 successful active enrollments, no overbooking,
no duplicate active course enrollment, and no partial multi-group submission.
