# Implementation Plan: Registration Capacity and Concurrency

**Branch**: 014-registration-capacity-concurrency | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Approved for non-production demo implementation by Ahmed ELbamby on 2026-07-13; reconciled baseline reaffirmed 2026-07-17 (Gate A).

## Summary

Implement the atomic registration command in
`StudentRegistration.Registration`, with SQL Server as the cross-replica
linearization authority. One short transaction owns the student-term
idempotency claim, final validation, sorted group allocation, enrollments,
decision/receipt snapshot, human-safe reference, audit record, and final
result. Deterministic rejection is replayable; infrastructure failure leaves
no claim or partial state.

## Technical Context

- **Runtime**: C#/.NET 10, ASP.NET Core, EF Core, LINQ, SQL Server.
- **Module**: `src/StudentRegistration.Registration/`.
- **SQL adapter**:
  `src/StudentRegistration.Infrastructure.SqlServer/Registration/`.
- **EF mapping contribution**:
  `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs`;
  SPEC-004 remains the sole DbContext writer.
- **Consumed modules**: IdentityAccess, Academics, and Scheduling public ports.
- **API scope**: authenticated student plus route term; no client student ID.
- **Scale**: two or more stateless API replicas; no process-local lock.
- **Targets**: 75 submissions/s for 10 minutes; 200/s for 60 seconds; p95 <= 2
  seconds while preserving every invariant.

## Workstreams and Order

1. Baseline all declared upstream specifications and shared serialization
   boundaries; complete consistency/concurrency analysis.
2. Freeze the student+term idempotency scope, receipt snapshot, SQL constraints,
   transaction/savepoint sequence, error map, and both endpoints against Ahmed
   ELbamby's recorded 2026-07-13 Gate A approval.
3. Write failing schema, contract, acceptance, every-row concurrency-matrix,
   fault-injection, and load tests against real SQL Server.
4. Implement the authenticated command, consume SPEC-008's
   `StudentTermAcademicState` boundary through
   `ExecuteRegistrationBoundaryAsync`, and add the canonical payload hash and
   in-transaction idempotency claim.
5. Implement sorted conditional group updates, savepoint rollback, enrollment,
   receipt/reference snapshot, audit, and final-result commit.
6. Implement bounded same-key observation, deterministic replay,
   reconciliation/pause, complete-transaction retry, and endpoint handlers only
   after behavior tests fail.
7. Run two-replica collision/load suites and invariant queries.

## Design Decisions

### Linearization Order

SPEC-008 `StudentTermAcademicState` via `ExecuteRegistrationBoundaryAsync` ->
remaining registration-context/version rows -> published policy/version
boundary -> `SectionGroup` rows sorted by ID.
Mutable inputs are re-read after these boundaries. There are no HTTP calls,
messages, email, or other remote work inside the transaction.

## Data and Idempotency Decisions

- `RegistrationSubmission` is the idempotency claim and final-result record;
  there is no competing `IdempotencyRecord`.
- Its unique key is `(StudentId, TermId, ClientRequestId)`. The same opaque
  key may be used independently in another term. A request for another
  student's key returns the privacy-safe not-found outcome.
- The accepted transaction creates a unique human-safe `Reference` and
  immutable `ReceiptSnapshot` on the submission. Replays return these same
  values and never create another receipt or seat.
- `SectionGroup` is owned by SPEC-010 and only consumed here.

## Constitution and Approval Gate

Gate A authorizes non-production demo implementation while every dependency
baseline remains Approved and the planned SQL/race and consistency gates pass.
Gate B-D evidence and production/release approval remain separate and
mandatory for their respective milestones.

## Artifacts

- [Requirements](requirements.md)
- [Concurrency matrix](concurrency-matrix.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Tasks](tasks.md)

## Complexity Tracking

The solution uses one database transaction, constraints, row versions, and
conditional updates. It introduces no queue, distributed lock, seat
reservation, or partial-acceptance workflow.
