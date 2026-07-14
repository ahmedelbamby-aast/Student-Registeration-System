# Immutable History and Supersession

**Contract version:** `immutable-history/1.0`<br>
**Requirement:** FR-5<br>
**Bounded delivery:** design-time contract only<br>
**Runtime source dependency:** None<br>
**Fail-closed boundary:** unapproved production behavior remains blocked

Historical correction uses append or supersession; in-place rewriting is
prohibited. This contract preserves meaning without inventing an unapproved
retention or deletion schedule.

## Historical records

- No transcript attempt is overwritten.
- No transcript attempt is updated or deleted by a correction. An initial
  attempt is appended once; a correction appends a new sourced
  `TranscriptAttempt` whose nullable `SupersedesAttemptId` identifies the
  prior row. The target must be the current leaf and the new row must retain
  the same `StudentId`, `TermId`, and `CourseCode`. A filtered unique successor
  key prevents branching. Because the self-reference targets an already
  existing immutable row and updates/deletes are forbidden, the chain is
  acyclic. The superseded row remains unchanged and queryable, and the current
  projection follows the unique leaf in that valid chain.
- Enrollments retain successful registration history. Unapproved drop,
  withdrawal, and correction commands do not rewrite it.
- Published policy sets are immutable and superseded by new effective-dated
  versions.
- Published catalogue versions are immutable. `CatalogueDraft` is mutable only
  before publication; publishing creates a new immutable version rather than
  editing an earlier publication.
- Decision snapshots retain the exact rule version and input summary used.
- Audit events are append-only for sensitive administrative actions.
- RegistrationSubmission final result and snapshots remain replayable for the
  payload-bound idempotency key.
- RegistrationReceipt is a projection, not a second table or write path.

## Lifecycle boundary

Testing disposal, Development guarded reset, and seven-day local artifact
cleanup do not imply production retention. Those controls cover wholly
synthetic demo databases or Git-ignored local artifacts only.

Hard deletion and retention for real or production data remain unapproved and
fail closed. Canonical owner specifications and future SQL evidence must prove
append/supersession behavior before any release claim is made.
