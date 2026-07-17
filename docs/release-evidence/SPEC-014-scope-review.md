# SPEC-014 out-of-scope review

**Review date:** 2026-07-17  
**Scope:** SPEC-014 source, SQL adapter, migrations, endpoint/page manifests,
API and route contracts, client route binding, and automated tests  
**Evidence state:** PASS for T117-T121 scope exclusions only

## Verdict

| Task | Item | Excluded behavior | Verdict | Decisive evidence |
|---|---|---|---|---|
| T117 | OS-1 | Waitlist, temporary seat reservation, or queue | **PASS** | No production type, migration table, route, worker, channel, or queue surface was found. The bounded 202 observation path stores no reservation and allocates no seat. |
| T118 | OS-2 | Distributed/application-instance locks | **PASS** | No process/distributed-lock API or application lock was found. Concurrency uses the shared SQL transaction, row/key-range locks, constraints, and savepoint only. |
| T119 | OS-3 | Partial schedule acceptance | **PASS** | Multi-group allocation rolls every earlier counter mutation back to one savepoint and returns an empty allocated-group list on rejection. Rejected persisted results cannot contain a receipt. |
| T120 | OS-4 | Capacity override above approved group capacity | **PASS** | Allocation is conditional on `EnrolledCount < Capacity`; the database enforces `EnrolledCount <= Capacity`; capacity reduction is rejected if below current enrollment. No override/bypass route or command exists. |
| T121 | OS-5 | Student drop, withdrawal, correction, or seat-decrement workflow | **PASS** | SPEC-014 owns only submit and request-result lookup routes. Enrollment has only `Active` state, and no endpoint/domain command exposes a removal or decrement. Internal reconciliation is service-only invariant repair, not an enrollment action. |

These verdicts prove that the five capabilities remain absent from the reviewed
repository surface. They do not complete T122 traceability, T123 approval, or
authorize a release.

## Inspected surface

The review used repository files only and excluded generated `bin` and `obj`
content. It inspected:

- `specs/014-registration-capacity-concurrency/spec.md`, `requirements.md`,
  `plan.md`, `research.md`, `data-model.md`, `concurrency-matrix.md`, and every
  file under `contracts/`;
- `src/StudentRegistration.Registration`, including its current endpoint,
  application, domain, and port files;
- `src/StudentRegistration.Infrastructure.SqlServer/Registration`;
- the S6 migration `20260713060000_Registration.cs`, its designer/snapshot,
  and the SPEC-010 `SectionGroup` capacity constraint in
  `20260713020000_CatalogueScheduling.cs`;
- `.specify/endpoint-manifest.json`, the current registration endpoint files,
  `RegistrationApiClient`, and `RegistrationReviewPage.razor`;
- SPEC-014 acceptance, application, contract, integration/concurrency/edge,
  migration, model, and E2E tests.

The S6 migration creates only `RegistrationSubmissions` and `Enrollments`.
Its `Down` method's EF `DropTable` calls are migration rollback mechanics, not
a student drop route or enrollment workflow.

## Reproducible forbidden-surface scans

Run these commands from the repository root in PowerShell. An `rg` exit code of
1 means the targeted forbidden implementation surface had no matches.

```powershell
$production = @(
  'src/StudentRegistration.Registration',
  'src/StudentRegistration.Infrastructure.SqlServer/Registration',
  'src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713060000_Registration.cs'
)

rg -n -i 'waitlist|seat.?reservation|temporary.?reservation|priority.?queue|message.?queue|queue.?consumer|background.?queue|IHostedService|BackgroundService|Channel<' $production 'specs/014-registration-capacity-concurrency/contracts'
# observed: no matches

rg -n 'SemaphoreSlim|Mutex|Monitor\.|lock\s*\(|IDistributedLock|DistributedLock|RedLock|sp_getapplock|ConcurrentDictionary' $production
# observed: no matches

rg -n -i 'partial.?accept|partially.?accept|accepted.?groups|rejected.?groups|ispartially|partial.?result' $production 'specs/014-registration-capacity-concurrency/contracts'
# observed: no executable partial-acceptance surface; contract mentions are prohibitions

rg -n -i 'capacity.?override|override.?capacity|force.?enroll|force.?allocat|ignore.?capacity|bypass.?capacity|overbook.?allow' $production '.specify/endpoint-manifest.json'
# observed: no matches

rg -n -i 'Map(Get|Post|Put|Patch|Delete).*?(drop|withdraw|correction|seat.?decrement)|"/(drop|withdraw|correction|seat.?decrement)|DecrementSeat|DropEnrollment|WithdrawEnrollment|CorrectEnrollment|CancelEnrollment' src -g '*Endpoints.cs' $production '.specify/endpoint-manifest.json'
# observed: no matches
```

Positive boundary evidence was then located with:

```powershell
rg -n 'UPDLOCK|HOLDLOCK|ROWLOCK|IsolationLevel\.Serializable|BeginTransaction|CreateSavepoint|RollbackToSavepoint|LOCK_TIMEOUT' src/StudentRegistration.Registration src/StudentRegistration.Infrastructure.SqlServer/Registration tests/StudentRegistration.IntegrationTests/Registration tests/StudentRegistration.IntegrationTests/Specs/Spec014

rg -n 'EnrolledCount.*Capacity|Capacity.*EnrolledCount|GROUP_FULL|ReduceCapacityAsync' src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlSeatAllocator.cs src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713020000_CatalogueScheduling.cs tests/StudentRegistration.IntegrationTests/Registration/SqlSeatAllocatorConcurrencyTests.cs tests/StudentRegistration.IntegrationTests/Specs/Spec014

rg -n 'EnrollmentState|CK_Enrollments_State|Registration.Reconcile|IsServiceIdentity|REGISTRATION_RECONCILE_FORBIDDEN|RepairAsync|SetRegistrationPaused' src/StudentRegistration.Registration/Domain/Enrollment.cs src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713060000_Registration.cs src/StudentRegistration.Infrastructure.SqlServer/Registration/EnrollmentCounterReconciler.cs

rg -n '"owner": "014"' .specify/endpoint-manifest.json
```

The last command returns exactly the intended SPEC-014 surface:

```text
POST /api/student/terms/{termId}/registrations
GET  /api/student/terms/{termId}/registrations/by-request/{clientRequestId}
```

No current or declared SPEC-014 route admits any OS-1 through OS-5 action.

## T117 / OS-1 — no waitlist, reservation, or queue

**Result: PASS.**

- The S6 migration contains only the idempotency/final-result
  `RegistrationSubmissions` table and the active `Enrollments` table. There is
  no waitlist, reservation, queue item, lease, priority, or expiry table.
- `SqlSeatAllocator` changes `SectionGroup.EnrolledCount` only inside the
  caller's SQL transaction. It has no hold/release/reserve method and creates
  no temporary seat record.
- `RegistrationSubmissionStore.WaitForFinalResultAsync` performs at most 500 ms
  of caller-request polling with `Task.Delay`. This observes the same
  idempotency key; it is not a background worker, queue, reservation, or proof
  that a seat is held. A 202 remains explicitly non-durable guidance.
- The API/route contracts expose submit and private result lookup only. Neither
  contract offers join-waitlist, reserve-seat, priority, enqueue, or dequeue.

The terms `processing` and `REGISTRATION_IN_PROGRESS` describe an uncertain
same-key transaction outcome. They do not represent a queued registration or a
temporarily reserved seat.

## T118 / OS-2 — no distributed or process lock

**Result: PASS.**

The prohibited-lock scan found no `lock (...)`, `SemaphoreSlim`, process
`Mutex`, `Monitor`, `IDistributedLock`, RedLock, `sp_getapplock`, or equivalent
application-instance coordination in the reviewed production surface.

The implementation deliberately does contain database serialization:

- `UPDLOCK`, `HOLDLOCK`, and `ROWLOCK` on the owning SQL rows/key ranges;
- `IsolationLevel.Serializable` for the bounded reconciliation/registration
  database boundary;
- conditional SQL updates and database unique/check constraints;
- one allocation savepoint inside the caller's local SQL transaction; and
- `SET LOCK_TIMEOUT 500` for bounded idempotency-claim contention.

These are SQL Server transaction semantics shared by all stateless replicas.
They are released by transaction completion and are not distributed locks,
process-local locks, or application-instance ownership. `LOCK_TIMEOUT` limits
waiting for a database lock; it does not create an application lock.

## T119 / OS-3 — no partial schedule acceptance

**Result: PASS.**

- `SqlSeatAllocator.AllocateWithSavepointAsync` sorts the complete group set,
  creates `registration_allocation`, and rolls back to it on the first failed
  conditional update.
- The rejection result returns `IsAccepted=false` and
  `Array.Empty<Guid>()`; `SqlSeatAllocatorConcurrencyTests` asserts the
  allocated-group list is empty after an injected later-group failure.
- `CK_RegistrationSubmissions_ResultShape` permits a receipt/reference only for
  `accepted`; `rejected` requires both to be null and retains only the complete
  rejection/decision evidence.
- Race R05 requires “No partial enrollment/receipt; rejection replayable,” and
  the STU-06/ADM-08 contributor contracts present one complete atomic outcome.

`SeatAllocationBatchResult.AllocatedGroupIds` is diagnostic output for a fully
accepted batch. Earlier successful loop iterations are never returned after a
later failure because the savepoint rollback clears the batch result.

## T120 / OS-4 — no capacity override

**Result: PASS.**

- The allocation statement succeeds only when the group is published, not
  registration-paused, and `EnrolledCount < Capacity`; otherwise it returns
  `GROUP_FULL`.
- `CK_SectionGroups_Capacity` independently enforces non-negative values and
  `EnrolledCount <= Capacity` in SQL Server.
- `ReduceCapacityAsync` uses the same row boundary and updates capacity only
  when `EnrolledCount <= newCapacity`. It cannot reduce capacity beneath
  already committed enrollment and is not an override.
- Race R01 and AC-1 assert one winner for the final seat and
  `EnrolledCount <= Capacity`; Race R11 covers a concurrent valid/invalid
  capacity reduction without bypassing the constraint.
- No force-enroll, ignore-capacity, bypass, overbook, or capacity-override route
  or command was found.

## T121 / OS-5 — no drop, withdrawal, correction, or seat decrement

**Result: PASS.**

- The endpoint manifest gives SPEC-014 only atomic submit and private
  by-request lookup. The all-endpoint scan finds no `/drop`, `/withdraw`,
  `/correction`, seat-decrement, delete-enrollment, or equivalent command.
- `EnrollmentState` contains only `Active`; `Enrollment` exposes no state-change
  or removal method. The S6 check constraint permits only `active`.
- `RegistrationConflictTests` scans all endpoint source for forbidden actions,
  and AC-11 specifies zero Enrollment and EnrolledCount mutations for Student
  and ordinary Admin attempts.

`EnrollmentCounterReconciler.RepairAsync` is intentionally not classified as a
drop/correction workflow. It has no endpoint, cannot change an Enrollment row,
requires a service identity plus exact `Registration.Reconcile`, uses the
observed group rowversion and hash of the current active-enrollment IDs, and
sets the stored counter to that already-authoritative count with an atomic
audit event. Student and Admin roles cannot invoke it. Its possible numerical
counter decrease repairs drift; it does not remove a seat-owning enrollment or
admit an OS-5 user workflow.

## Drift and release rule

Any new waitlist/reservation/queue store or worker, process/distributed lock,
partial-result DTO, capacity-bypass path, or enrollment removal/decrement route
invalidates this review. Admitting one requires a separately approved
specification, updated manifests/contracts/migrations, negative and positive
authorization/concurrency tests, and a new scope review.

This document must not be used as T122 traceability evidence or T123 human
release approval.
