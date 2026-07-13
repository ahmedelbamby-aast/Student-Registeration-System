# Registration Transaction Schema Contract

**Contract:** `registration-transaction-schema/1.0`<br>
**Requirement:** FR-9<br>
**Canonical owner:** SPEC-014<br>
**EF contribution:** `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs` (`writable-transactional`)<br>
**Bounded delivery:** design-time contract only<br>
**Runtime source dependency:** None<br>
**Fail-closed boundary:** unapproved production behavior remains blocked

SPEC-005 governs this persistence contract. SPEC-014 alone implements the
runtime entities, EF mapping, and registration transaction; this document does
not activate or require that downstream runtime source.

## Student-term serialization guard

- Guard fields: Id, StudentId, TermId, Version rowversion.
- `Unique StudentTermRegistrationGuard(StudentId, TermId)` permits one durable
  serialization boundary for a student in an academic term.
- The guard row and its `rowversion` participate in the same short SQL
  transaction as the submission outcome. An in-memory lock is not a substitute.

## RegistrationSubmission record

RegistrationSubmission is the sole idempotency claim/final-result record.
A separate IdempotencyRecord entity, mapping, or table is prohibited.

| Meaning | Field contract |
|---|---|
| Identity | `Id` |
| StudentId: owner | `StudentId` |
| TermId: scope | `TermId` |
| ClientRequestId: key | `ClientRequestId` |
| Payload binding | `PayloadHash` |
| Lifecycle | `ProcessingState`, `ResultCode` |
| Accepted result | `Reference`, `ReceiptSnapshotJson` |
| Historical decision | `DecisionSnapshotJson` |
| Time | `ReceivedAtUtc`, `UpdatedAtUtc`, nullable `CompletedAtUtc` |

- `Unique RegistrationSubmission(StudentId, TermId, ClientRequestId)` enforces
  the owner/scope/key boundary. Reusing the opaque key in another term is an
  independent scope.
- RegistrationSubmission.ReceivedAtUtc is its immutable creation instant;
  `UpdatedAtUtc` records progress, and CompletedAtUtc is nullable until a final
  outcome.
- `PayloadHash` is calculated from the canonical request. Same key and same
  payload replays the stored deterministic result. A different payload returns
  409 IDEMPOTENCY_KEY_REUSED and never executes another allocation.
- `Unique non-null RegistrationSubmission.Reference` applies to accepted
  results. Reference and ReceiptSnapshotJson exist only for an accepted final
  result; rejected results have neither. DecisionSnapshotJson preserves the
  exact decision, governing inputs, and policy version used at commit.
- The stored deterministic result survives application-process restart. A
  post-commit process failure replays the stored result by the same scoped key.

## Atomic lifecycle and failure outcomes

RegistrationSubmission idempotency claim, final result, decision snapshot,
audit event, and any successful counters/enrollments commit in one SQL
transaction.

| Outcome | Required persistence behavior |
|---|---|
| Accepted | Commit the final submission, unique reference, receipt and decision snapshots, audit event, counters, and enrollments atomically. |
| Deterministic rejection | A deterministic rejection rolls allocation changes back to the savepoint before the final Rejected result commits. The final payload-bound rejection and audit remain replayable. |
| Infrastructure failure before commit | An infrastructure failure rolls back the entire transaction and idempotency claim. A complete retry is allowed only after the earlier transaction is known not to have committed. |
| Response lost after commit | A post-commit process failure replays the stored result; it never allocates again. |

The internal claim is created before the allocation savepoint but remains
inside the final transaction. No standalone Processing row may commit. A
standalone row is forbidden because a Processing claim cannot be committed on
its own.

When the first transaction is still uncommitted, another same-payload request
waits for at most 500 ms for a visible final result. The bounded 500 ms HTTP 202
response is transport-only, contains no SubmissionId, and persists no result
row. It is retry guidance, not evidence of a committed Processing claim.

Any unknown state, mismatched payload, missing guard, or unapproved attempt to
bypass these constraints fails closed without seat, enrollment, receipt, or
idempotency mutation.
