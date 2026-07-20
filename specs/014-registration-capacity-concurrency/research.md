# Research: Registration Capacity and Concurrency

**Owner-approved amendment:** `registration-roadmap-line-approval/1.0`, Ahmed
ELbamby, 2026-07-20.

## Decisions

### SQL Server is the linearization authority

**Decision**: Consume SPEC-008's database-backed `StudentTermAcademicState`
boundary through `ExecuteRegistrationBoundaryAsync`, then use stable lock
order, conditional SectionGroup updates, and unique/check constraints. A
duplicate Registration-owned student-term guard and process-local or
distributed application locks are prohibited.

**Rationale**: SQL Server is already required and coordinates every stateless
API replica. Reusing the Academics-owned boundary serializes registration with
profile/hold mutations without a second student-term row. Check-then-write
application logic cannot protect the final seat.

### Scoped idempotency record

**Decision**: RegistrationSubmission is the sole claim/final-result record,
unique by (StudentId, TermId, ClientRequestId). The same UUID in another term
is independent. Student identity is authenticated, term is route-scoped, and
the body contains neither.

**Rationale**: One record avoids competing idempotency models. Explicit scope
makes lookup, privacy, and retry behavior deterministic.

### Claim, savepoint, and final result

**Decision**: Create the claim inside the registration transaction. After final
validation, create an allocation savepoint. Deterministic allocation rejection
rolls seat/enrollment changes back to the savepoint and commits the stable
rejected result. Infrastructure failure rolls back the whole transaction and
claim.

**Rationale**: This preserves replayable business rejection without an orphan
Processing row or partial schedule.

### Durable receipt/reference

**Decision**: An accepted transaction generates one unique human-safe Reference
and immutable ReceiptSnapshot on RegistrationSubmission in the same commit as
seat counters, enrollments, DecisionSnapshot, audit, and final result.

**Rationale**: A lost response/retry returns the same receipt and cannot create
another reference, snapshot, audit-success event, or allocation. SPEC-015
projects this record rather than introducing a second table.

### Bounded duplicate observation

**Decision**: A same-scope concurrent retry waits at most 500 ms. If no final
row becomes visible, return a non-durable 202 with only ClientRequestId,
retryAfterSeconds, and the term-scoped result URL. Lookup by another student is
privacy-safe 404.

### Complete-transaction retry

**Decision**: EF/SQL transient execution retries restart the complete
idempotent transaction. No transaction fragment or remote call is retried
inside the transaction.

### Durable held-seat approval phase

**Decision**: From program term two onward, create one pending line and active
seat hold for every selected group. One Admin decision or one decision by a
Lecturer/TeachingAssistant currently assigned to that group finalizes each
line. The plan accepts only after every line approves; one rejection or window
close releases all holds.

**Rationale**: Per-line decisions match teaching assignment scope while the
submission aggregate preserves all-or-nothing schedule correctness. A hold is
capacity occupancy, not an Enrollment, waitlist position, or policy waiver.

### Occupied-seat counter

**Decision**: Add SPEC-010-owned HeldSeatCount and use
`EnrolledCount + HeldSeatCount` in every capacity predicate, constraint,
capacity edit, reconciliation, and projection. Hold creation, conversion, and
release acquire the existing StudentTerm and sorted SectionGroup boundaries.

**Rationale**: A database counter makes final-seat races linearizable across
replicas and keeps availability reads bounded. Evidence rows remain the repair
source of truth for both counters.

### Window-bound hold lifetime

**Decision**: Active holds persist until all line decisions complete, one line
rejects, or the authoritative registration window closes. Approval-vs-expiry
uses the same submission/line/group versions and one serial winner.

**Rationale**: This exactly implements the approved policy without inventing
an unrelated TTL. Window close is server-controlled and prevents indefinite
capacity occupation.

### First-program-term automatic batch

**Decision**: A durable SQL-backed batch selects applicable required
`CurriculumCourse` rows at RecommendedTerm one whose prerequisite position is
a root. Each student item uses the existing registration coordinator and
either enrolls a deterministic feasible complete schedule or records a safe
failure with no partial enrollment. No line approval or hold is created.

**Rationale**: A batch is operable and retryable without making browser login
or activation a registration side effect. Per-student short transactions avoid
one unbounded cohort transaction while idempotency prevents duplicates.

### Credit-load policy

**Decision**: Keep the stricter approved probation maximum. For other students,
<=18 is normal, 19-21 requires current authoritative CGPA >=3.00, and >21 is
rejected. Every self-service line still requires approval; approval cannot
waive another rule.

**Rationale**: Load eligibility and line approval are independent governed
decisions and remain explainable in the immutable DecisionSnapshot.

### Unified frontend components

**Decision**: Student, Lecturer, TeachingAssistant, and Admin routes consume
the same roadmap, CapacitySummary, approval status/timeline, decision, and
route-state components from SPEC-003.

**Rationale**: Shared components keep capacity and lifecycle language
consistent while authorization continues to be server-derived per role.

## Endpoint Contract

- `POST /api/student/terms/{termId}/registrations`
- `GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}`

New final result is 201, replay 200, bounded in-progress guidance 202,
validation 400, authentication/authorization 401/403, business/version/
same-scope payload conflict 409, and nonexistent/rolled-back/private lookup
404.

## Open Research

Institutional policy values remain governed by their source specs and fail
closed until approved. The 2026-07-20 values above are approved for the demo.
No additional concurrency technology is required; SQL Server remains the
linearization and durable-work authority.
