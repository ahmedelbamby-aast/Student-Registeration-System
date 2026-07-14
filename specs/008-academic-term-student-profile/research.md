# Research: Academic Term and Student Profile

**Standing approval**: Ahmed ELbamby approved these clarified non-production
demo decisions on 2026-07-14. Production and later release gates remain
separate.

## Decisions

### Modular boundary
**Decision**: Own this capability in the Academics module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface StudentAcademicContextDto {
  universityId: string;
  programCode: string;
  cohort: string;
  currentGpa: number;
  earnedCredits: number;
  standing: string;
  transcriptSummary: { attemptedCredits: number; earnedCredits: number; attemptCount: number };
  transcriptAttempts: Page<{ attemptId: string; supersedesAttemptId?: string; courseCode: string; termCode: string; credits: number; grade?: string; status: string; provenance: string }>;
  activeHolds: Array<{ termId: string; code: string; message: string; blocksRegistration: boolean; effectiveFromUtc: string; effectiveToUtc?: string; source: string }>;
  dataVersion: string;
  dataAsOfUtc: string;
  provenance: Page<{ source: string; reference: string; importedAtUtc: string }>;
}
type PublicAcademicContextDto = PublicContextDto; // canonical SPEC-006 public shape
interface RegistrationWindowSummaryDto { // canonical shared SPEC-006 type
  id: string;
  state: "upcoming" | "open" | "closed";
  opensAtUtc: string;
  closesAtUtc: string;
  rowVersion: string;
}
```

The shared `AppContextDto` contains this nullable nested summary and the
canonical `available | maintenance | unavailable` service state. SPEC-008
supplies values and does not publish a second AppContext DTO.

Endpoints: GET /api/public/context, GET /api/context, and GET
/api/students/me/academic-context plus SPEC-008-owned term/window and student
Admin endpoints. SPEC-017 consumes audit/report contracts instead of owning
Academics mutations.

### Academic AppContext composition
**Decision**: SPEC-008 owns `/api/context` composition and the academic
portion; SPEC-007 owns session fields and SPEC-006 owns shared DTO/error
conventions.
**Rationale**: One endpoint gives every replica and page a coherent server
instant, role, term, window, and version without duplicate ownership.
**Alternatives rejected**: Browser clock/term inference and two independently
loaded contexts with no shared version.

### Deterministic term and window resolution
**Decision**: A service-ready context admits exactly one RegistrationOpen term
and at most one Teaching term. Zero RegistrationOpen terms is an authoritative
absence; ambiguity returns `CONTEXT_UNAVAILABLE`. Windows use
`OpensAtUtc <= now < ClosesAtUtc` and resolve open first, otherwise earliest
upcoming by open-time/ID, otherwise latest closed by close-time-descending/ID.
**Rationale**: Singular DTOs require one server-owned deterministic result at
opening, closing, no-open-window, and corrupted-configuration boundaries.
**Alternatives rejected**: Browser-selected terms, unordered `First()`, inclusive
closing instants, and returning an arbitrary term after conflicting Admin writes.

### Bounded profile projection
**Decision**: Page transcript attempts and provenance independently using the
canonical default 20/maximum 100 contract. Return every active hold together,
but cap active holds at 100 and fail the profile closed above that cap.
**Rationale**: Eligibility cannot safely use a partial hold set, while long
transcript/provenance histories must remain bounded for predictable load.
**Alternatives rejected**: Unbounded arrays, silently truncated holds, and an
invented second pagination protocol.

### Bounded commands and errors
**Decision**: Limit a term to 20 windows/version entries and a correction to 20
typed operations. Require 10..500-character reasons, nonblank source/reference
values of at most 200 characters, and 3..50-character searches. Feature field
errors allow at most 20 keys, 5 messages per key, and 256 characters per message.
**Rationale**: These small POC limits make SQL work, audit, validation, and HTTP
payloads predictable without adding a generic bulk framework.
**Alternatives rejected**: Unbounded maps/arrays, silent truncation, and a
separate batch-processing subsystem.

### Student-term serialization
**Decision**: Own `StudentTermAcademicState` upstream in Academics. Hold/profile
mutations and registration submissions lock it before reading decision inputs.
**Rationale**: Registration already depends on Academics, avoiding a forward
dependency while preventing hold-versus-submit write skew.
**Alternatives rejected**: A downstream-owned guard consumed by SPEC-008 and
application-instance locks.

The SPEC-008 acceptance proof uses a registration test consumer that acquires
the public student-term boundary and exposes a commit callback. Failed
revalidation must return `HOLD_BLOCKED` without invoking that callback. Actual
seat/enrollment conformance remains SPEC-014-owned.

### Conservative window publication
**Decision**: Forbid any overlap between Published windows in the same term,
regardless of scope. Publication locks the term and all candidate/existing
windows in stable ID order before rechecking the invariant.
**Rationale**: This POC rule is deterministic, prevents ambiguous student
matching, and avoids an unnecessary scope-overlap algebra.
**Alternatives rejected**: Scope-specific overlap resolution and relying on an
application-instance pre-check.

### Append-only transcript correction
**Decision**: Every correction identifies `termId`. Correcting a transcript
attempt appends an immutable replacement linked through
`supersedesAttemptId`; the prior attempt remains unchanged.
**Rationale**: This preserves sourced academic history and gives the
student-term concurrency boundary an unambiguous identity.
**Alternatives rejected**: In-place transcript edits and implicit current-term
selection from the browser.

The supplied supersession identifier is normalized and must name the current
leaf for the same student/course/term. A prior row has one successor, the chain
is acyclic, and summaries count only current leaves. This prevents correction
forks and double-counted credits without introducing event sourcing.

### Term creation replay and versioned retry safety
**Decision**: Only POST term creation requires payload-bound idempotency because
it has no expected aggregate version. Store globally unique
`CreationClientRequestId` and `CreationPayloadHash` on AcademicTerm. Publication
and profile correction use their required expected rowversions and atomic SQL
transaction; they create no generic idempotency record.
**Rationale**: Creation can replay the original aggregate after response loss,
while a committed versioned mutation makes the submitted expected version stale.
**Alternatives rejected**: A seventh command-receipt entity, process-memory
deduplication, and blindly retrying a mutation after response loss.

### Explicit lifecycle commands
**Decision**: Term/window command DTOs carry their lifecycle. Draft-to-Published
uses only the publish endpoint; Published interval/scope is immutable, and PUT
supports governed versioned emergency-close/supersede transitions.
**Rationale**: Every persisted state has an explicit owner command, and EC-5 can
be exercised without generic property patching.
**Alternatives rejected**: Date-inferred lifecycle, direct publish through a
generic update, and in-place edits to a Published interval.

### Canonical stale-response protocol
**Decision**: Use canonical `ApiError.currentVersion` for the directly
contested aggregate and bounded `fieldErrors` for validation. For a
multi-version term aggregate or `WINDOW_OVERLAP`, the client refetches the
bounded aggregate before retrying.
**Rationale**: This preserves one shared error schema without adding a second
version-map or conflict-detail response.
**Alternatives rejected**: Feature-specific ApiError fields and trusting cached
versions after a conflict.

### Academic-profile authorization scope
**Decision**: Student self access requires `AcademicProfile.ReadOwn`. The Admin
locator requires `AcademicProfiles.Manage`, TermId, and a nonblank bounded
query, returns only minimal locator fields, and detail/correction binds the
named StudentId and TermId. Lecturer and TeachingAssistant are denied.
**Rationale**: A usable Admin journey does not require an unrestricted student
dump or leakage of correction identifiers to Student/Lecturer/TA views.
**Alternatives rejected**: Admin-as-superuser, empty-query enumeration, and one
overbroad profile DTO for every role.

### Time representation boundary
**Decision**: SPEC-008 persists instants as UTC datetime2 and validates the
term's IANA timezone. SPEC-010 owns recurring meeting day/time persistence.
**Rationale**: The term/profile slice should not create a dependency on a
downstream scheduling model.
**Alternatives rejected**: Browser-local instants and duplicating SPEC-010
meeting fields in Academics.

### Complete synthetic academic profiles
**Decision**: The Academics seed contributor populates only the existing
profile contract: University ID link, ProgramCode, cohort, GPA, earned and
attempted credits, standing, transcript attempts, active blocking/non-blocking
holds, StudentTermAcademicState, synthetic provenance, version, and as-of time.
Logical values derive from the seed-profile version and stable fixture ordinal.
**Rationale**: Development and tests exercise realistic normal and boundary
states without importing real student data or quietly expanding the personal
data model.
**Alternatives rejected**: Partial student rows, random unrepeatable academic
values, copied production records, and invented email/phone/address/birth-date
fields.

### Class-diagram synchronization
**Decision**: Implementation must update the governed class diagram so the API
composition boundary owns session+academic AppContext composition and the six
SPEC-008 domain entities/current shared window value are represented.
**Rationale**: The existing diagram's AcademicContextResolver-to-session-reader
edge contradicts the approved module boundary and would invite an
Academics-to-Identity dependency.
**Alternatives rejected**: Leaving known documentation drift or coupling the
Academics application service directly to IdentityAccess.

### Feature load evidence
**Decision**: Measure 300 authenticated `GET /api/context` reads/second for ten
continuous minutes across two independently addressable stateless replicas
sharing SQL and the approved 25,000-account fixture. Require p95 <= 300 ms and
unexpected failures < 0.1% and record achieved rate, replica distribution, SQL
configuration, latency, and failures.
**Rationale**: This is the exact bounded SPEC-008 read target and proves that
context resolution does not depend on process-local state.
**Alternatives rejected**: Anonymous/public reads, an empty fixture, one
replica, a shorter extrapolated run, or claiming this replaces SPEC-018's later
mixed load and failover gates.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
