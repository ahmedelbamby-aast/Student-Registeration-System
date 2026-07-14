# Feature Specification: Academic Term and Student Profile

**Feature Branch**: 008-academic-term-student-profile
**Created**: 2026-07-12
**Status**: APPROVED
**Owner**: Backend Lead
**Normative detail**: [requirements.md](requirements.md)
**Standing approval**: Ahmed ELbamby approved the clarified non-production demo
contract on 2026-07-14; production and later release gates remain separate.

## Context

Eligibility needs authoritative server time, the permitted registration term,
and a trusted academic profile. Device time and stale client profile data must
never open a window or change an academic decision.

## User Scenarios and Testing

### User Story 1 - Device-clock independence (FR-1, FR-6) (P1)

As a Student or authorized registrar, I need the Device-clock independence (FR-1, FR-6) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given a device clock is one day ahead<br>
When registration-window state is requested<br>
Then server time and configured term/window determine the state<br>
And a window is open exactly when `OpensAtUtc <= serverNowUtc < ClosesAtUtc`.
### User Story 2 - No active term (FR-2, FR-4) (P1)

As a Student or authorized registrar, I need the No active term (FR-2, FR-4) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given no registration term matches the student and server instant<br>
When the dashboard loads<br>
Then registration is read-only/unavailable<br>
And a clear no-active-window message is shown.
### User Story 3 - Hold changes before submit (FR-5, FR-6) (P2)

As a Student or authorized registrar, I need the Hold changes before submit (FR-5, FR-6) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a plan was eligible when created<br>
And a blocking hold is added before a registration test consumer attempts its
commit callback<br>
When the consumer locks and re-resolves the SPEC-008 student-term state<br>
Then the consumer is rejected with HOLD_BLOCKED<br>
And its commit callback is not invoked. Actual seat and enrollment conformance
is verified later by dependent SPEC-014.
### User Story 4 - Governed term/profile edit (FR-3, FR-7) (P2)

As a Student or authorized registrar, I need the Governed term/profile edit (FR-3, FR-7) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given Admin has permission, reason, source and current rowversion<br>
When a valid term/window or academic-profile correction is submitted<br>
Then explicit dates/state/provenance are saved and audited<br>
And a stale rowversion would be rejected. Creating a term also requires a
payload-bound client request ID, while publication and profile correction rely
on their required expected rowversions and atomic transaction.
### User Story 5 - Hold mutation races submission (FR-6, FR-8) (P3)

As a Student or authorized registrar, I need the Hold mutation races submission (FR-6, FR-8) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given a registration test consumer races an authorized Admin adding a blocking
hold for the same student and term<br>
When both operations use the SPEC-008 student-term boundary concurrently<br>
Then the operations have one valid serial order<br>
And a consumer that loses the student-term serialization boundary revalidates,
returns HOLD_BLOCKED, and does not invoke its commit callback. SPEC-014 remains
responsible for proving the same protocol around real seat/enrollment writes.
### User Story 6 - Concurrent window publication (FR-3, FR-4, FR-9) (P3)

As a Student or authorized registrar, I need the Concurrent window publication (FR-3, FR-4, FR-9) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given two draft windows overlap anywhere in the same term<br>
When two admins publish them concurrently<br>
Then exactly one publication may commit<br>
And the loser receives 409 STALE_VERSION or WINDOW_OVERLAP<br>
And no student matches two permitted registration contexts.
### User Story 7 - Term and profile quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Student or authorized registrar, I need the Term and profile quality gate (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

Given fake-clock boundary fixtures, two rebuilds of the same synthetic seed
version, and a 25,000-account shared-SQL fixture serving a 10-minute
two-replica load of 300 authenticated context reads per second,
persistence inspection, and
student/staff authorization matrix<br>
When the feature quality gate executes<br>
Then time behavior passes opening/closing boundary tests<br>
And both rebuilds contain the same logical students and complete existing
academic profile values<br>
And dashboard context is at most 300 ms p95 with fewer than 0.1% unexpected
failures<br>
And instants use UTC datetime2 while the term uses an IANA timezone identifier<br>
And academic data is visible only to self or approved staff scope.

### User Story 8 - Composed authenticated context (FR-1, FR-2, FR-4, FR-10) (P2)

As an authenticated user, I need one coherent session and academic context so
that the header, dashboard, and registration state agree on the server time,
role, term, and window.

**Independent Test**: Execute AC-8 with a SPEC-007 session double and
SPEC-008 academic fixtures.

**Acceptance Scenario (AC-8)**

Given an authenticated student and one matching published window<br>
When GET /api/context is requested<br>
Then identity and academic portions are composed with the same authoritative
server context and include one canonical nested registration-window summary
with ID, state, open/close instants, and version<br>
And no ambiguous term/window configuration is returned as a partial success.

### User Story 9 - Complete Admin term and profile journeys (FR-3, FR-7, FR-9, FR-11) (P2)

As an authorized Admin, I need searchable term/student data and governed owner
commands so that ADM-02 and ADM-04 are functional rather than presentation-only pages.

**Independent Test**: Execute AC-9 using only SPEC-008 APIs plus audit and
authorization test doubles.

**Acceptance Scenario (AC-9)**

Given current rowversions, reason, source, and permission<br>
When an Admin pages data or submits a valid term/window/profile command<br>
Then bounded data or one audited mutation is returned and stale/invalid writes
change nothing. Student location requires a term ID plus a 3-to-50-character
query before a named detail or correction may be requested.

## Edge Cases

- EC-1: Any overlapping Published windows in the same term -> publication
  fails, regardless of student scope.
- EC-2: Missing GPA/provenance in an imported record, or any missing required
  existing academic-profile field in a synthetic seed row -> the affected
  policy decision fails closed and the seed profile is not marked ready. More
  than 100 simultaneously active holds also leaves the profile not ready.
- EC-3: Daylight/timezone rule changes -> UTC window remains unambiguous and
  display uses configured timezone library.
- EC-4: Stale admin edit -> 409 with current rowversion.
- EC-5: A scheduled window closes while a request is in flight -> the
  server-received timestamp governs the scheduled cutoff, while an emergency
  administrative closure/version change blocks every uncommitted request.
- EC-6: More than one RegistrationOpen or Teaching term is detected -> no
  arbitrary winner is selected; return CONTEXT_UNAVAILABLE and no partial
  context.
- EC-7: No window is open -> choose the earliest upcoming window, otherwise
  the latest closed window, using UTC time plus ID as a deterministic tie-break;
  no candidate produces the `none` state.

## Requirements

### Functional Requirements

- FR-1: The server MUST expose current UTC time and configured institutional
  timezone, initially Africa/Cairo.
- FR-2: The system MUST distinguish teaching term from registration term.
- FR-3: Terms/windows MUST have explicit dates, states, scope, and rowversion.
  Term commands carry Draft/RegistrationOpen/RegistrationClosed/Teaching/
  Completed/Archived state. Window commands carry Draft/Published/
  EmergencyClosed/Superseded lifecycle; Draft-to-Published is owned only by
  the publish command, while Published interval/scope values are immutable.
  Term creation persists one globally unique payload-bound
  `CreationClientRequestId` and `CreationPayloadHash` for replay safety.
- FR-4: A term MUST NOT contain overlapping Published registration windows,
  regardless of scope. A service-ready context has exactly one
  RegistrationOpen term and at most one Teaching term; zero RegistrationOpen
  terms is an authoritative no-registration-term result and any ambiguity
  fails as CONTEXT_UNAVAILABLE. Window containment is half-open. For a resolved
  term/scope, select the open window, otherwise earliest upcoming by
  OpensAtUtc/ID, otherwise latest closed by ClosesAtUtc-descending/ID.
- FR-5: Student profile MUST include University ID, program/cohort, GPA,
  earned credits, standing, transcript summary/attempts, all active holds with
  blocking flags, provenance, data version, and as-of time. Each Development
  and Testing synthetic student MUST populate this same complete existing
  field set, including linked `Student`, `TranscriptAttempt`, `StudentHold`,
  and `StudentTermAcademicState` rows where applicable. Fixture values and
  stable University-ID/ApplicationUser links are determined by seed-profile
  version plus fixture ordinal; no production student data or new demographic
  fields are inferred. Transcript attempts and provenance MUST use the
  canonical default-20/maximum-100 pagination contract. All active holds MUST
  be returned together with a hard maximum of 100; a profile above that cap
  MUST fail closed as not ready. Transcript corrections append a new attempt
  that supersedes the current leaf for the same student/course/term and MUST
  NOT overwrite history. A prior attempt has at most one successor, chains are
  acyclic, and transcript summaries count only current leaves. Student responses
  omit hold IDs; a named Admin detail includes them for governed correction.
- FR-6: Registration commands MUST re-resolve time, term, window, student
  state, and holds.
- FR-7: Admin profile corrections MUST require authorization, reason, source,
  optimistic concurrency, and audit. A correction contains 1..20 typed
  operations; reason is 10..500 characters and source/source-reference values
  are nonblank and at most 200 characters.
- FR-8: A hold/profile mutation and a registration submission for the same
  student/term MUST participate in the SPEC-008-owned
  `StudentTermAcademicState` boundary and advance its version. SPEC-008 MUST
  prove that public lock/version protocol with a registration test consumer
  whose commit callback is never invoked after failed revalidation; SPEC-014
  proves real seat/enrollment conformance later.
- FR-9: Registration-window publication MUST lock the affected term and every
  candidate/existing window in stable ID order, recheck that no Published
  windows overlap anywhere in the term inside the transaction, and reject a
  stale expected version. Each term and expected-version map is limited to 20
  windows. Cancellation before commit leaves no publication or audit effect;
  a committed response loss is resolved through the expected-version/refetch
  protocol.
- FR-10: The authenticated AppContext MUST compose the SPEC-007 session
  portion with SPEC-008 server time, timezone, terms, one nullable canonical
  `RegistrationWindowSummaryDto` containing window ID/state/open/close/version,
  the canonical available/maintenance/unavailable service state, and
  `supportReferencePath` without trusting browser time or term input. SPEC-008
  MUST consume the shared SPEC-006 DTOs and MUST NOT define a duplicate
  academic AppContext DTO. Missing or ambiguous contributors return 503
  CONTEXT_UNAVAILABLE, never a guessed term/window or partial DTO.
- FR-11: ADM-02 and ADM-04 MUST have bounded owner APIs for reads and governed,
  versioned, reasoned, sourced, audited term/window/profile mutations. Term
  commands require `AcademicTerms.Manage`; profile commands require the
  separately governed `AcademicProfiles.Manage` permission. The Admin student
  locator requires TermId and a trimmed 3..50-character query and returns only
  minimal locator fields. Named detail/correction additionally binds StudentId
  and TermId. Student self and a named permitted Admin are allowed; Lecturer
  and TeachingAssistant are denied. The ten endpoint contracts MUST declare
  their request/response, success, validation, authorization, conflict,
  unavailable, and unexpected-error outcomes. Feature field errors are bounded
  to 20 keys, 5 messages per key, and 256 characters per message.

### Non-Functional Requirements

- NFR-1: Time-dependent behavior MUST use TimeProvider and boundary tests.
- NFR-2: Authenticated dashboard context MUST remain at or below 300 ms p95
  during a continuous 10-minute run of 300 GET /api/context reads per second
  across two independently addressable stateless replicas sharing SQL and the
  approved 25,000-account fixture, with fewer than 0.1% unexpected failures.
- NFR-3: SPEC-008 instants MUST be stored in UTC datetime2 and every term MUST
  use a valid IANA timezone identifier. Recurring meeting persistence remains
  owned and verified by downstream SPEC-010.
- NFR-4: Student academic data MUST be restricted to self and approved staff
  scopes. Non-production fixtures MUST be wholly synthetic, and logs, traces,
  snapshots, and test reports MUST NOT contain a full student profile.

### Key Entities

- **AcademicTerm**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **AcademicTerm creation replay metadata**: The term stores globally unique
  CreationClientRequestId and CreationPayloadHash; this is not a seventh entity
  or a generic idempotency table.
- **RegistrationWindow**: Feature-owned concept with a conservative no-overlap
  Published invariant; attributes and relationships are refined in
  requirements.md and the shared ERD.
- **Student**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **TranscriptAttempt**: Feature-owned immutable historical concept; a
  correction appends a superseding attempt rather than overwriting one.
- **StudentHold**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StudentTermAcademicState**: SPEC-008-owned per-student/per-term version and serialization boundary consumed by registration features.

## Success Criteria

- **SC-1**: Registration availability is determined only by authoritative institutional time and approved windows.
- **SC-2**: Every eligibility decision uses a complete, sourced academic profile, and the same synthetic seed version reproduces the same logical profile values without production student data.
- **SC-3**: No two Published registration windows overlap anywhere in the same
  term, so no overlapping active registration context can apply to a student.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.
- The shared SPEC-006 AppContext contract includes the one canonical nullable
  `RegistrationWindowSummaryDto`; dependency baselining must confirm that
  canonical owner contract before implementation.
- The SPEC-006 class diagram must be reconciled during implementation to keep
  session composition at the API boundary and include the six SPEC-008 domain
  entities; the planning package records that required documentation delivery
  but does not edit the upstream diagram itself.

## Dependencies

- [SPEC-002](../002-aastmt-policy-rulebook/spec.md)
- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| AUTH-01 | / | RoleGatewayPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-008 |
| STU-01 | /student | StudentDashboardPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-008 |
| ADM-02 | /admin/terms | TermAdministrationPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-008 |
| ADM-04 | /admin/students | StudentAdministrationPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-008 |

## Out of Scope

- OS-1: Computing official grades from assessment events.
- OS-2: Inferring a term from month/date alone.
- OS-3: Browser clock as an authority.
- OS-4: SIS synchronization mechanism until integration is specified.

SPEC-008 verifies only the upstream student-term lock/version protocol through
a test consumer. Actual seat/enrollment writes and their conformance belong to
SPEC-014. Recurring class-meeting persistence belongs to SPEC-010.
