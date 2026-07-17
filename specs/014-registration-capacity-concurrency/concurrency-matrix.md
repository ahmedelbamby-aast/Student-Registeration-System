# Concurrency and Linearization Matrix

Every row is a mandatory design and real-SQL test obligation. Locks are local
SQL Server transaction boundaries and work across stateless app replicas.

| Race | Shared boundary / order | Allowed winner | Required loser/result | Invariant |
|---|---|---|---|---|
| Two students, final seat | SectionGroup conditional update | First committed valid allocation | 409 GROUP_FULL | EnrolledCount <= Capacity |
| One student, two different plans | SPEC-008 StudentTermAcademicState via ExecuteRegistrationBoundaryAsync before groups | First valid plan | Re-read and 409 policy/conflict | Combined enrollment remains valid |
| Same idempotency key, same payload | Atomic uncommitted claim plus 500-ms bounded wait | First claimant | Visible final replay or non-durable 202 with no submissionId | One execution/final result |
| Same scoped key, different payload | Unique (StudentId, TermId, ClientRequestId) claim plus payload comparison | Original scoped payload | 409 IDEMPOTENCY_KEY_REUSED | New payload never executes; same UUID in another term is independent |
| Multi-group allocation | Allocation savepoint after claim/validation | Valid whole plan | Roll back seat mutations to savepoint; commit stable rejected result | No partial enrollment/receipt; rejection replayable |
| Hold/profile edit vs submit | SPEC-008 StudentTermAcademicState row/version through ExecuteRegistrationBoundaryAsync | First serialized transaction | Re-read and reject or valid later commit | Final profile governs |
| Scheduled close vs request | Server ReceivedAtUtc | Request received inside window | WINDOW_CLOSED after cutoff | Browser time ignored |
| Emergency close vs submit | Registration-context version boundary | Emergency closure if committed first | WINDOW_CHANGED | No post-emergency uncommitted commit |
| Policy/catalogue publish vs submit | Policy scope/version then submission re-read | One valid serial order | POLICY_CHANGED or commit under final version | One policy version per decision |
| Group cancel/meeting/room/staff edit vs submit | Owning SectionGroup row/version advanced by every child mutation | One valid serial order | GROUP_CHANGED or valid enrollment | No enrollment in invalid group state |
| Capacity reduction vs submit | Same SectionGroup row/version | Either serial order | Invalid reduction or GROUP_FULL | 0 <= count <= capacity |
| Deadlock/transient error | Stable lock order plus complete execution-strategy retry | One complete transaction | Safe idempotent retry | Never retry a fragment |
| Process death after idempotency claim, before commit | Claim inside the registration SQL transaction | Retried complete request | Rolled-back claim can be reclaimed | No orphan Processing record |
| Process/network loss after commit | Stored submission/result/reference/receipt snapshot in same transaction | Committed result | Term-scoped result lookup/replay | No duplicate allocation, reference, receipt, or audit-success event |
| Reconciliation mismatch | SectionGroup lock/pause plus GroupId, observed rowversion and evidence-hash repair scope | One authorized operations-service repair from active Enrollment evidence | Duplicate replica replays; unauthorized/Admin invocation denied | Counter equals active enrollment before audited resume |

## Required proof

- Deterministic barrier-based SQL Server integration tests for every row.
- The mandatory target workload and 200 submissions/s spike run across at
  least two independently addressable application replicas.
- Fault injection before commit, during contested writes, and after commit.
- Target and spike load invariant queries. Double-target, 5x, and soak
  profiles are optional diagnostics and cannot replace or relax the mandatory
  target/spike proof.
- Metrics for lock wait, deadlock, idempotent replay, reason codes, and mismatch.
