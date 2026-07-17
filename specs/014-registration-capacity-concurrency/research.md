# Research: Registration Capacity and Concurrency

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

## Endpoint Contract

- `POST /api/student/terms/{termId}/registrations`
- `GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}`

New final result is 201, replay 200, bounded in-progress guidance 202,
validation 400, authentication/authorization 401/403, business/version/
same-scope payload conflict 409, and nonexistent/rolled-back/private lookup
404.

## Open Research

Institutional policy values remain governed by their source specs and fail
closed until approved. No additional concurrency technology is required.
