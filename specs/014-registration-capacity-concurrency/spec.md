# Feature Specification: Registration Capacity and Concurrency

**Feature Branch**: 014-registration-capacity-concurrency
**Created**: 2026-07-12
**Status**: Approved for demo implementation by Ahmed ELbamby on 2026-07-13; reconciled baseline reaffirmed 2026-07-17; registration-roadmap and line-approval amendment approved 2026-07-20
**Owner**: Data/Backend Lead
**Normative detail**: [requirements.md](requirements.md)

**Owner-approved amendment (2026-07-20):** `CurriculumCourse` is the
authoritative program roadmap. Required prerequisite-root subjects in a
student's first program term are enrolled automatically without prerequisite
or human approval. From program term two onward, a student submits a complete
plan and every selected subject receives a capacity-consuming seat hold until
one authorized line decision is recorded. Admin may decide any line;
Lecturers and Teaching Assistants may decide only lines whose selected group
they are currently assigned to. The plan becomes accepted atomically only
after every line is approved; one rejection or registration-window close
releases every hold. Normal load remains at most 18 credits, 19-21 credits
requires CGPA at least 3.00, more than 21 is rejected, and the stricter
probation rule remains authoritative.

## Context

Peak registration creates races for the final seats, but seat capacity is only
one contested invariant. Two different submissions by one student can each be
valid alone and invalid together; admin changes can race validation; duplicate
requests can arrive before, during, or after commit; and a scheduled window can
close while a request waits.

Every registration command therefore has a database-backed linearization
boundary. A self-service plan is held in full, then accepted in full after all
line approvals or rejected/expired with every hold released; a first-term
automatic plan is accepted in full or fails without partial enrollment.
Preview, recommendation, and browser state are stale by definition. ADR-002
and concurrency-matrix.md define the required ordering and winner/loser
outcomes.

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
And neither Group A nor Group B count/enrollment changes for the student<br>
And the deterministic rejection is committed and replayable by the same key
and payload.
### User Story 3 - Idempotent retry after lost response (FR-2, FR-7, FR-16) (P2)

As a Student, I need the Idempotent retry after lost response (FR-2, FR-7, FR-16) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a submission committed but its response was lost<br>
When the same idempotency key and payload are retried<br>
Then the original receipt/result is returned<br>
And no additional seat, enrollment, audit-success event, or receipt is created.
### User Story 4 - Collision load (FR-8, NFR-1, NFR-3) (P2)

As a Student, I need the Collision load (FR-8, NFR-1, NFR-3) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given a group capacity of 30 and no enrollments<br>
When 100 eligible students submit simultaneously through two replicas<br>
Then exactly 30 active enrollments and successful results exist<br>
And EnrolledCount is exactly 30.
### User Story 5 - Stale policy or plan (FR-3, FR-10, FR-14) (P3)

As a Student, I need the Stale policy or plan (FR-3, FR-10, FR-14) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given plan or governing policy version changed after review<br>
When submit occurs<br>
Then 409 PLAN_CHANGED or POLICY_CHANGED is returned with the current version<br>
And no capacity, enrollment, submission-success, or receipt mutation occurs.
### User Story 6 - Reconciliation mismatch containment (FR-11, NFR-6) (P3)

As a Student, I need the Reconciliation mismatch containment (FR-11, NFR-6) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given a controlled fault creates a counter/enrollment mismatch in a test
fixture<br>
When reconciliation runs<br>
Then the affected group is alerted and paused for new registration<br>
And controlled repair restores the count from active Enrollment evidence<br>
And the repair is audited.
### User Story 7 - Same student submits different plans concurrently (FR-3, FR-5, FR-12, NFR-1) (P3)

As a Student, I need the Same student submits different plans concurrently (FR-3, FR-5, FR-12, NFR-1) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

Given two plans for one student/term are individually valid but jointly exceed
credit load or overlap<br>
When different idempotency keys submit them concurrently<br>
Then only one plan may commit<br>
And the loser revalidates after acquiring the shared StudentTermAcademicState
boundary and receives
409 POLICY_CHANGED or SCHEDULE_CONFLICT<br>
And the combined enrollment remains valid.
### User Story 8 - Atomic idempotency claim and payload mismatch (FR-7, FR-13) (P3)

As a Student, I need the Atomic idempotency claim and payload mismatch (FR-7, FR-13) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-8 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-8)**

Given two requests simultaneously first-use the same idempotency key and
canonical payload<br>
When the claim is attempted<br>
Then one executes and the other either receives the stored final result or,
after at most 500 ms, a 202 RegistrationInProgressResponse containing no
submissionId<br>
And when that key is reused with a different payload it receives
409 IDEMPOTENCY_KEY_REUSED<br>
And the different payload is never executed.
### User Story 9 - Admin mutation races registration (FR-5, FR-14, FR-17) (P3)

As a Student, I need the Admin mutation races registration (FR-5, FR-14, FR-17) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-9 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-9)**

Given a hold, emergency window closure, policy publication, group cancellation,
meeting change, or capacity reduction races an otherwise valid submission<br>
When both transactions contend on their shared database boundary<br>
Then the outcome corresponds to one valid serial order<br>
And a losing registration re-reads the winner and rejects without partial
state<br>
And every committed enrollment satisfies the final governing versions.
### User Story 10 - Scheduled cutoff and emergency closure (FR-15) (P3)

As a Student, I need the Scheduled cutoff and emergency closure (FR-15) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-10 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-10)**

Given request A is received one millisecond before scheduled close and request
B one millisecond after<br>
When both are processed without an emergency closure<br>
Then A is eligible for transactional validation and B receives WINDOW_CLOSED<br>
And given an emergency closure commits before A commits<br>
Then A also receives WINDOW_CHANGED and creates no enrollment.
### User Story 11 - No unapproved drop workflow (FR-9) (P3)

As a Student, I need the No unapproved drop workflow (FR-9) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-11 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-11)**

Given an authenticated student or ordinary Admin<br>
When a drop, withdrawal, correction, or seat-decrement route is requested<br>
Then no such MVP endpoint or action exists<br>
And the response cannot mutate Enrollment or EnrolledCount.
### User Story 12 - Authenticated short-transaction quality (FR-1, NFR-2, NFR-4, NFR-5) (P3)

As a Student, I need the Authenticated short-transaction quality (FR-1, NFR-2, NFR-4, NFR-5) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-12 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-12)**

Given student-identity substitution attempts and the approved target-load
fixture with remote dependencies fault-injected<br>
When registration quality tests execute<br>
Then the server always resolves the authenticated student<br>
And submission is at most 2 seconds p95 at 75 per second for 10 minutes<br>
And SQL transaction traces contain no remote call<br>
And expected conflict codes are excluded from the unexpected-failure rate,
which remains below 0.1%.
### User Story 13 - Process failure after claim cannot orphan processing (FR-13, FR-16, FR-18) (P3)

As a Student, I need the Process failure after claim cannot orphan processing (FR-13, FR-16, FR-18) behavior so that Registration Capacity and Concurrency produces a verifiable outcome.

**Independent Test**: Execute AC-13 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-13)**

Given a fault terminates the application immediately after the idempotency
claim statement but before any enrollment write<br>
When the enclosing SQL transaction is inspected and the request is retried<br>
Then no committed orphan Processing record exists<br>
And retry can claim and execute normally<br>
And a fault after commit replays the stored final result.

### User Story 14 - First-term roadmap auto-enrollment (FR-19, FR-26) (P1)

As a first-program-term student, I need required roadmap roots registered
automatically so that I begin with a valid foundation schedule without a
self-service or approval step.

**Acceptance Scenario (AC-14)**

Given the student's authoritative program term is one and the published
roadmap has applicable required prerequisite roots<br>
When the idempotent auto-enrollment batch runs<br>
Then all applicable roots are enrolled in deterministic valid groups or none
are enrolled for that student<br>
And no prerequisite or line approval is requested.

### User Story 15 - Held self-service lines await scoped approval (FR-20-FR-25) (P1)

As a term-two-or-later student, I need each selected subject to hold a seat
while authorized staff decide it so that capacity and approval state are
truthful.

**Acceptance Scenario (AC-15)**

Given an eligible self-service plan whose occupied-seat capacity is available<br>
When the student submits<br>
Then one active hold and pending line are created for every selected subject
atomically<br>
And an assigned Lecturer/TA can decide only their group lines while Admin can
decide any line.

### User Story 16 - All lines finalize atomically (FR-21, FR-23, FR-24) (P1)

As a student with a pending plan, I need approval completion or rejection to
resolve every hold consistently so that I never receive a partial schedule.

**Acceptance Scenario (AC-16)**

Given every line but one is approved<br>
When the final line is approved<br>
Then the server revalidates and converts all holds to enrollments atomically<br>
And when any line is rejected or the window closes, every hold is released and
no enrollment from that submission exists.

### User Story 17 - Governed normal, overload, and capacity visibility (FR-20, FR-25, FR-27) (P2)

As a student or authorized staff member, I need clear load and capacity state
so that the same server decision is understandable in every role workspace.

**Acceptance Scenario (AC-17)**

Given normal, probation, overload, and over-maximum fixtures<br>
When policy and UI verification runs<br>
Then the probation maximum remains stricter, <=18 follows the normal rule,
19-21 requires CGPA >=3.00, and >21 is rejected<br>
And every role sees capacity/enrolled/held/available counts without holder PII
through the unified components.

## Edge Cases

- EC-1: Deadlock/transient SQL error -> retry the complete idempotent
  transaction only after the earlier attempt is known not to have committed.
- EC-2: Deterministic unique-constraint race after a conditional update ->
  roll back to the allocation savepoint, verify no seat mutation remains, and
  commit the stable rejected result; if SQL has aborted the transaction, roll
  back the whole transaction/claim and retry safely.
- EC-3: Request cancellation or network loss after commit -> recover the
  committed stored result by idempotency key; never compensate a valid commit.
- EC-4: Capacity reduction races enrollment or a pending hold -> shared
  SectionGroup boundary preserves
  `0 <= EnrolledCount + HeldSeatCount <= Capacity`.
- EC-5: Counter reconciliation mismatch -> alert, pause the affected group,
  and use an authorized, audited, idempotent controlled repair.
- EC-6: Two different plans for the same student/term -> serialize through one
  guard even when routed to different application replicas.
- EC-7: Same key and payload arrive while the first transaction is uncommitted
  -> wait at most 500 ms, then replay a visible final result or return the
  non-durable 202 response and poll resultUrl; GET returns 404
  REQUEST_NOT_FOUND if the first transaction rolled back, after which the
  original POST may be retried. Never start another allocation concurrently.
- EC-8: Same key with different payload/owner/term -> return
  IDEMPOTENCY_KEY_REUSED without revealing another user's result.
- EC-9: Application process fails between SQL commit and HTTP response -> the
  atomic submission/idempotency record remains sufficient to replay the result.
- EC-10: Application process fails after in-transaction idempotency claim but
  before commit -> SQL rollback removes the claim and every partial mutation.
- EC-11: Approval and window-close expiry race -> one submission/version
  transition wins; the outcome is either fully accepted before close or fully
  expired with all holds released.
- EC-12: An assigned staff member loses the group assignment after loading a
  line -> in-transaction authorization denies the decision without disclosing
  a newer version or changing the hold.
- EC-13: Capacity reduction races an active hold -> one SectionGroup serial
  order wins and Capacity never falls below EnrolledCount + HeldSeatCount.
- EC-14: First-term batch lacks a complete feasible schedule -> that student
  receives no partial enrollment and an Admin-visible safe failure is stored.

## Requirements

### Functional Requirements

- FR-1: Submission MUST resolve the student from authenticated server identity
  and MUST NOT accept a client-supplied student identifier.
- FR-2: The route MUST identify TermId and the request MUST contain PlanId,
  expected plan rowversion, and a client-generated ClientRequestId. The server
  MUST reject a route term outside the authenticated student's resolved
  registration context; TermId and student ID MUST NOT be duplicated as
  client-authoritative body fields.
- FR-3: The server MUST revalidate window, student profile/holds, policy and
  catalogue versions, eligibility, credit load, duplicate courses, group
  state/meeting versions, and timetable inside the commit transaction.
- FR-4: Every selected self-service group seat MUST be held by a conditional
  atomic SQL update that succeeds only when the published group is selectable
  and `EnrolledCount + HeldSeatCount < Capacity`. First-term automatic
  enrollment uses the same occupied-seat predicate and allocates directly to
  Enrollment. A hold consumes capacity but is not an Enrollment or waitlist.
- FR-5: Registration MUST acquire database serialization boundaries in this
  stable order: the SPEC-008 `StudentTermAcademicState` boundary through
  `ExecuteRegistrationBoundaryAsync`, the remaining registration-context/
  version records, then SectionGroup rows sorted by group ID.
- FR-6: After the idempotency claim and final validation but before the first
  seat mutation, the transaction MUST create an allocation savepoint. A
  deterministic allocation/invariant rejection MUST roll back to that
  savepoint, verify that no seat/enrollment mutation remains, store the
  rejected result and rejection audit, and commit the claim/result. A
  transient or transaction-aborting infrastructure failure MUST roll back the
  entire transaction and its claim. No failure path MAY create an active
  partial enrollment, counter, receipt, or success audit state.
- FR-7: A concurrent request using the same authenticated-student/route-term/
  ClientRequestId scope MUST NOT execute allocation again. If the final
  claim/result is committed, the same canonical payload MUST replay it. While
  the first claim remains uncommitted, the server MUST wait at most 500 ms for
  that scoped key; if no final row becomes visible, it MUST return the
  non-durable bounded 202 RegistrationInProgressResponse containing
  clientRequestId, retryAfterSeconds, and the term-scoped resultUrl only. The
  202 MUST NOT expose or imply a visible submissionId or committed Processing
  row; payload mismatch is checked once the winning claim becomes visible.
  Reusing the same opaque ClientRequestId in another term is independent and
  MUST NOT conflict; another student cannot discover the first student's key.
- FR-8: Unique, foreign-key, and check constraints MUST be final guards for
  student/offering duplicates, idempotency ownership, group ownership, one
  active hold per submission line, and
  `0 <= EnrolledCount + HeldSeatCount <= Capacity`.
- FR-9: Student drop, withdrawal, enrollment correction, and seat decrement
  workflows MUST NOT be exposed in MVP; each requires a separate approved
  policy/workflow specification.
- FR-10: Expected business conflicts MUST return 409 with a stable reason code,
  current version where relevant, and no mutation.
- FR-11: A scheduled worker MUST reconcile both EnrolledCount against active
  Enrollment and HeldSeatCount against active RegistrationSeatHold evidence
  under the SectionGroup lock, pause and alert on mismatch, and permit repair
  only to the Registration.Reconcile operations identity using a
  GroupId/rowversion/evidence-hash idempotency scope. Repair MUST recompute
  both counters, audit atomically, and verify the occupied-seat invariant
  before resume; no Admin/public repair endpoint exists in MVP.
- FR-12: Every registration mutation for one student and term MUST consume the
  database-backed SPEC-008 `StudentTermAcademicState` boundary through
  `ExecuteRegistrationBoundaryAsync`; the successful transaction advances its
  version, and in-memory or duplicate Registration-owned guards are prohibited
  because multiple application replicas are supported.
- FR-13: RegistrationSubmission is the sole submission idempotency claim and
  durable lifecycle record. Its database uniqueness scope MUST be (StudentId,
  TermId, ClientRequestId), and it MUST atomically store that owner/scope,
  canonical payload hash, origin, requested credits, internal processing or
  durable PendingApproval/Accepted/Rejected/Expired state, deterministic
  result, timestamps, and—when accepted—a unique human-safe Reference plus
  immutable ReceiptSnapshot. Reuse in the same scope with a different payload
  MUST return 409 IDEMPOTENCY_KEY_REUSED; reuse in another term is independent.
- FR-14: After acquiring the required database boundaries, the server MUST
  re-read and validate every mutable input. Initial self-service submission
  MUST atomically create every line and hold, increment held counters, store
  PendingApproval, and audit. Final approval MUST atomically consume every
  hold, decrement held counters, increment enrolled counters, create every
  Enrollment, accepted result, reference, immutable receipt/decision
  snapshots, and audit event. Rejection or expiry MUST atomically release
  every active hold and store its final audited result. No transaction may
  contain a remote call.
- FR-15: The server MUST capture ReceivedAtUtc once at authenticated command
  ingress. Scheduled opening/closing boundaries use that instant; an emergency
  administrative closure or registration-context version change before commit
  MUST reject the uncommitted request. A committed PendingApproval submission
  that reaches scheduled/emergency close MUST expire through FR-23 and release
  all holds.
- FR-16: Accepted and deterministic business-rejected outcomes MUST be stored
  as final results and replayable. A transient infrastructure failure before
  commit MAY be retried; an uncertain/lost response after commit MUST be
  recovered by idempotency key.
- FR-17: The registration transaction MUST use the shared serialization
  boundaries defined by SPEC-008 for student/term/window mutations, SPEC-009
  for policy publication, and SPEC-010 for group/capacity publication.
- FR-18: The idempotency claim and internal Processing state MUST be created
  inside the same SQL transaction as PendingApproval or a final result. The
  allocation savepoint MUST be created after that claim so a deterministic
  rejection can retain and finalize it; no separately committed orphaned
  Processing claim is permitted.
- FR-19: `CurriculumCourse` roadmap membership, `RecommendedTerm`, required
  flag, cohort scope, and prerequisite-root status MUST be resolved on the
  server. For program term one, every applicable required root is enrolled by
  an idempotent FirstTermAutoEnrollmentBatch without prerequisite or approval;
  student self-service submission is unavailable. From program term two
  onward, registration is student-initiated and all normal prerequisites and
  roadmap gates apply.
- FR-20: The load evaluator MUST keep the approved probation maximum, allow a
  non-probation normal load of at most 18 credits, allow 19 through 21 credits
  only when current authoritative CGPA is at least 3.00, and reject more than
  21 credits. Approval MUST NOT waive prerequisites, holds, standing,
  conflicts, publication state, or any other failing rule.
- FR-21: A self-service submission MUST create one RegistrationSubmissionLine
  and one active RegistrationSeatHold for every selected group in one
  all-or-nothing transaction. Every line begins PendingApproval and requires
  exactly one final Approve or Reject decision. Holds remain active until a
  decision finalizes the plan or the registration window closes.
- FR-22: Admin with `RegistrationApproval.DecideAll` MAY decide any pending
  line. Lecturer or Teaching Assistant with
  `RegistrationApproval.DecideAssigned` MAY decide only a line whose selected
  group has their current effective GroupStaffAssignment. Authorization MUST
  precede line existence/version disclosure and MUST be revalidated inside the
  decision transaction.
- FR-23: When every line has an approved decision, the server MUST revalidate
  the entire plan and convert all holds to active enrollments atomically. One
  rejected line, expired window, stale governing input, or failed final
  revalidation rejects or expires the entire submission and releases every
  active hold; partial plan acceptance is prohibited.
- FR-24: Line decisions MUST require expected submission/line versions and an
  actor-scoped ClientRequestId. Same-payload retry returns the stored decision;
  different-payload reuse returns IDEMPOTENCY_KEY_REUSED. Approval, rejection,
  expiry, and duplicate decisions serialize so exactly one terminal line or
  plan transition wins and every decision records actor, role, reason,
  assignment scope where applicable, server time, and correlation.
- FR-25: Capacity changes and immediate or automatic allocation MUST serialize
  on the same SectionGroup row and use occupied seats. Capacity MUST NOT be
  reduced below EnrolledCount + HeldSeatCount. General capacity projections
  for Student, Admin, Lecturer, and Teaching Assistant MUST expose capacity,
  enrolled count, held count, and available count without holder identity.
- FR-26: FirstTermAutoEnrollmentBatch MUST be durable, bounded, retryable, and
  idempotent per student, term, catalogue version, and batch purpose. Each
  student is enrolled all-or-nothing into the applicable required term-one
  roots using deterministic valid groups. Missing offerings, schedule
  infeasibility, or capacity shortage produces an Admin-visible safe failure
  and no partial student schedule.
- FR-27: Student review/results, staff approval queues, and Admin registration
  monitoring MUST compose the unified SPEC-003 design-system primitives for
  roadmap, capacity, approval status/timeline, decision actions, and loading,
  empty, success, validation, denied, stale, offline, and service-error states.

### Non-Functional Requirements

- NFR-1: There MUST be zero group overbooking, duplicate active offering
  enrollment, partial schedule commit, or combined same-student policy/timetable
  violation in every target and spike concurrency test.
- NFR-2: Submission p95 MUST be at most 2 seconds at 75 submissions per second
  for 10 minutes using the production-like dataset.
- NFR-3: A 200-submission-per-second, 60-second spike MUST preserve every NFR-1
  invariant across at least two application replicas.
- NFR-4: Database transactions MUST be short, cancellation-aware before commit,
  and contain no HTTP, message-broker, email, or other remote call.
- NFR-5: Expected conflicts MUST not count as server failures; unexpected
  failure rate MUST remain below 0.1% at target load.
- NFR-6: Deadlock count, lock-wait p95, idempotent replay count, conflict-code
  count, and reconciliation mismatch count MUST be observable without logging
  student credentials or full academic records.

### Key Entities

- **RegistrationSubmission**: SPEC-014/Registration-owned aggregate and sole scoped idempotency/final-result record.
- **RegistrationSubmissionLine**: Selected subject/group line and approval lifecycle owned by SPEC-014.
- **RegistrationSeatHold**: Capacity-consuming pending line hold owned by SPEC-014.
- **RegistrationApprovalDecision**: Immutable actor/scoped decision evidence owned by SPEC-014.
- **FirstTermAutoEnrollmentBatch**: Durable idempotent first-program-term orchestration owned by SPEC-014.
- **Enrollment**: SPEC-014/Registration-owned active enrollment entity.
- **SectionGroup**: Consumed Scheduling aggregate owned by SPEC-010; SPEC-014 may only allocate through its published concurrency contract.
- **DecisionSnapshot**: SPEC-014/Registration-owned immutable decision evidence.
- **StudentTermAcademicState**: SPEC-008/Academics-owned database serialization
  row consumed through `ExecuteRegistrationBoundaryAsync`; SPEC-014 does not
  redefine or remap it.

## Success Criteria

- **SC-1**: Concurrent submissions never overbook a group or duplicate an active course enrollment.
- **SC-2**: A multi-group registration is either fully accepted or has no committed enrollment changes.
- **SC-3**: Retrying the same submission never allocates an additional seat.
- **SC-4**: Every self-service plan either converts all held lines to active
  enrollments after complete approval or releases all holds without partial
  enrollment.
- **SC-5**: First-program-term students receive exactly the applicable required
  roadmap roots automatically, while term-two-and-later students use the
  approval-backed self-service flow.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-011](../011-eligibility-subject-discovery/spec.md)
- [SPEC-012](../012-schedule-builder-conflicts/spec.md)
- [SPEC-013](../013-schedule-recommendations/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| STU-05 | /student/review | RegistrationReviewPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-014 |
| STU-06 | /student/registration/result/{id} | RegistrationResultPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-015 |
| ADM-08 | /admin/registrations | RegistrationAdministrationPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-017 |

## Out of Scope

- OS-1: Waitlist, queue, and any reservation other than the bounded
  owner-approved RegistrationSeatHold attached to a pending self-service line.
- OS-2: Distributed/application-instance locks.
- OS-3: Partial schedule acceptance.
- OS-4: Capacity override above approved group capacity.
- OS-5: Student drop, withdrawal, correction, or seat-decrement workflow.
