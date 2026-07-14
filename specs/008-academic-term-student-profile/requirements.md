# SPEC-008: Academic Term and Student Profile

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** APPROVED<br>
**Owner:** Backend Lead<br>
**Reviewers:** Registrar/Policy SME, Data, QA<br>
**Target:** Sprint 1<br>
**Dependencies:** SPEC-002, SPEC-003, SPEC-005, SPEC-006, SPEC-007, SPEC-018<br>
**Standing approval:** Ahmed ELbamby approved this clarified non-production
demo contract on 2026-07-14; production and later release gates remain
separate.<br>

## Context

Eligibility needs authoritative server time, the permitted registration term,
and a trusted academic profile. Device time and stale client profile data must
never open a window or change an academic decision.

## Functional Requirements

- FR-1: The server MUST expose current UTC time and configured institutional
  timezone, initially Africa/Cairo.
- FR-2: The system MUST distinguish teaching term from registration term.
- FR-3: Terms/windows MUST have explicit dates, states, scope, and rowversion.
  `AcademicTerm` lifecycle uses the canonical SPEC-006 TermSummaryDto states:
  Draft, RegistrationOpen, RegistrationClosed, Teaching, Completed, or Archived;
  `RegistrationWindow` lifecycle is Draft, Published, EmergencyClosed, or
  Superseded. Upcoming/Open/Closed/None is a computed response state based on
  lifecycle plus authoritative server time, never a browser-written state.
  Term creation persists a globally unique payload-bound
  `CreationClientRequestId` and `CreationPayloadHash`. Draft-to-Published is
  available only through the publish command; Published interval/scope values
  are immutable, while valid versioned emergency-close/supersede transitions
  remain available.
- FR-4: A term MUST NOT contain overlapping Published registration windows,
  regardless of scope. A service-ready context has exactly one
  RegistrationOpen term and at most one Teaching term. Zero RegistrationOpen
  terms is an authoritative no-registration-term result; multiple candidate
  terms fail as CONTEXT_UNAVAILABLE. Window containment is
  `OpensAtUtc <= serverNowUtc < ClosesAtUtc`. For the resolved term/scope,
  selection is open first, otherwise earliest upcoming by OpensAtUtc/ID,
  otherwise latest closed by ClosesAtUtc-descending/ID.
- FR-5: Student profile MUST include University ID, program/cohort, GPA,
  earned credits, standing, transcript summary and attempt data, every active
  hold (including non-blocking holds with `blocksRegistration`), provenance,
  data version, and as-of time. Each Development and Testing synthetic student
  MUST populate this same complete existing field set, including linked
  `Student`, `TranscriptAttempt`, `StudentHold`, and
  `StudentTermAcademicState` rows where applicable. Fixture values and stable
  University-ID/ApplicationUser links are determined by seed-profile version
  plus fixture ordinal; no production student data or new demographic fields
  are inferred. Transcript attempts and provenance MUST use the canonical
  default-20/maximum-100 pagination contract. All active holds MUST be returned
  together with a hard maximum of 100; any profile above that cap MUST fail
  closed as not ready. A transcript correction MUST append a new immutable
  attempt that supersedes the current leaf for the same student/course/term and
  MUST NOT overwrite it. Each prior attempt has at most one successor, chains
  are acyclic, and the transcript summary counts only current leaves. Student
  self responses omit hold IDs; the named Admin detail includes them only for
  governed correction.
- FR-6: Registration commands MUST re-resolve time, term, window, student
  state, and holds.
- FR-7: Admin profile corrections MUST require authorization, reason, source,
  optimistic concurrency, and audit. A request contains 1..20 typed operations;
  reason is 10..500 trimmed characters, while source and source-reference values
  are nonblank and at most 200 trimmed characters.
- FR-8: A hold/profile mutation and a registration submission for the same
  student/term MUST participate in one database-backed student-term
  `StudentTermAcademicState` serialization boundary and advance its version.
  Registration specs consume this upstream boundary; SPEC-008 does not depend
  on a downstream registration entity. SPEC-008 MUST prove the public protocol
  with a registration test consumer whose commit callback is not invoked after
  failed revalidation; SPEC-014 proves real seat/enrollment conformance later.
- FR-9: Registration-window publication MUST lock the AcademicTerm and then
  every candidate/existing RegistrationWindow in stable ID order, recheck
  inside the transaction that no Published windows overlap anywhere in that
  term, and reject stale term/window versions. A term, its windows array, and
  its expected-window-version map are each limited to 20 windows. Publication
  uses its required versions and atomic transaction; it creates no second
  idempotency entity.
- FR-10: GET /api/context MUST compose the SPEC-007 identity/session portion
  with a SPEC-008 academic portion containing server UTC time, IANA timezone,
  teaching term, registration term, one nullable canonical
  `RegistrationWindowSummaryDto` containing matched window
  ID/state/open/close/version, the canonical
  available/maintenance/unavailable service state, and
  `supportReferencePath`. SPEC-008 owns endpoint composition and supplies the
  academic values but MUST NOT redefine the SPEC-006 shared DTOs or SPEC-007
  session contract. Missing or ambiguous term/window contributors return 503
  CONTEXT_UNAVAILABLE with no partial response.
- FR-11: Authorized Admin users MUST have bounded term and student lists,
  term/window draft and publication commands, student academic-context reads,
  and explicit profile-correction commands. Every mutation MUST require a
  reason, source/provenance, and audit. Creation additionally requires its
  globally unique payload-bound client request ID; transitions, publication,
  and profile corrections require the applicable expected rowversions. No
  generic bulk overwrite or client-asserted role is accepted. Term commands
  require `AcademicTerms.Manage`; profile commands require the separately
  governed `AcademicProfiles.Manage` permission. The Admin student locator
  requires TermId and a nonblank 3..50-character query and returns minimum locator
  fields. Named detail/correction binds StudentId and TermId. Student self and
  a named permitted Admin are allowed; Lecturer and TeachingAssistant are
  denied. Every endpoint records success, validation, authorization, conflict,
  unavailable, and unexpected-error outcomes. Feature field errors contain at
  most 20 keys, 5 messages/key, and 256 characters/message.

## Non-Functional Requirements

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

## Acceptance Criteria

### AC-1: Device-clock independence (FR-1, FR-6)
Given a device clock is one day ahead<br>
When registration-window state is requested<br>
Then server time and configured term/window determine the state<br>
And the exact opening/closing boundary follows the half-open UTC interval.

### AC-2: No active term (FR-2, FR-4)
Given no registration term matches the student and server instant<br>
When the dashboard loads<br>
Then registration is read-only/unavailable<br>
And a clear no-active-window message is shown.

### AC-3: Hold changes before submit (FR-5, FR-6)
Given a plan was eligible when created<br>
And a blocking hold is added before a registration test consumer attempts its
commit callback<br>
When the consumer locks and re-resolves the SPEC-008 student-term state<br>
Then the consumer returns HOLD_BLOCKED<br>
And its commit callback is not invoked. Actual seat/enrollment conformance is
verified later by dependent SPEC-014.

### AC-4: Governed term/profile edit (FR-3, FR-7)
Given Admin has permission, reason, source and current rowversion<br>
When a valid term/window or academic-profile correction is submitted<br>
Then explicit dates/state/provenance are saved and audited<br>
And a stale rowversion would be rejected<br>
And same-payload term-creation replay returns the existing result while a
different payload for its client request ID is rejected.

### AC-5: Hold mutation races submission (FR-6, FR-8)
Given a registration test consumer races an authorized Admin adding a blocking
hold for the same student and term<br>
When both operations use the SPEC-008 boundary concurrently<br>
Then the operations have one valid serial order<br>
And a consumer that loses the student-term serialization boundary revalidates,
returns HOLD_BLOCKED, and does not invoke its commit callback. SPEC-014 remains
responsible for proving the protocol around real seat/enrollment writes.

### AC-6: Concurrent window publication (FR-3, FR-4, FR-9)
Given two draft windows overlap anywhere in the same term<br>
When two admins publish them concurrently<br>
Then exactly one publication may commit<br>
And the loser receives 409 STALE_VERSION or WINDOW_OVERLAP<br>
And no student matches two permitted registration contexts.

### AC-7: Term and profile quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given fake-clock boundary fixtures, two rebuilds of the same synthetic seed
version, a 25,000-account shared-SQL fixture, and a 10-minute two-replica load
of 300 authenticated context reads per second,
persistence inspection, and
student/staff authorization matrix<br>
When the feature quality gate executes<br>
Then time behavior passes opening/closing boundary tests<br>
And both rebuilds contain the same logical students and complete existing
academic profile values<br>
And dashboard context is at most 300 ms p95 with fewer than 0.1% unexpected
failures<br>
And instants use UTC datetime2 while terms use valid IANA timezone identifiers<br>
And academic data is visible only to self or approved staff scope.

### AC-8: Composed authenticated context (FR-1, FR-2, FR-4, FR-10)
Given an authenticated student with an effective session and one matching
published registration window<br>
When GET /api/context is requested through either application replica<br>
Then the identity fields match SPEC-007 and the academic fields identify the
same server instant, timezone, teaching term, registration term, window ID,
window open/close instants, window version, and computed Open state through the
one canonical nested registration-window summary<br>
And no browser clock or client-selected term changes the result.
And ambiguous term/window configuration returns CONTEXT_UNAVAILABLE rather
than a partial or arbitrary result.

### AC-9: Complete Admin term and profile journeys (FR-3, FR-7, FR-9, FR-11)
Given an authorized Admin opens ADM-02 or ADM-04 with current versions<br>
When the Admin pages/searches records, edits a term/window, publishes a
validated window, or submits a sourced profile correction<br>
Then the matching owner API returns a bounded response or audited mutation<br>
And stale, overlapping, unauthorized, or missing-provenance operations change
nothing and return their stable reason<br>
And student location never runs without a term ID plus bounded search query.

## Edge Cases

- EC-1: Any overlapping Published windows in the same term -> publication
  fails, regardless of scope.
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
- EC-6: Multiple RegistrationOpen or Teaching terms -> fail context composition
  as CONTEXT_UNAVAILABLE; do not choose by browser input or unordered query.
- EC-7: No open window -> select the earliest upcoming, otherwise latest closed,
  using UTC time and ID tie-breaks; no candidate -> state none.

## API Contracts

The complete normative shapes and all ten endpoint outcome matrices are in
[contracts/api.md](contracts/api.md). They include:

- the unchanged shared SPEC-006 context/page/error types and one canonical
  `RegistrationWindowSummaryDto`;
- separate `CreateTermRequest`, `UpdateTermRequest`, and
  `PublishRegistrationWindowRequest` shapes with explicit lifecycle state;
- a payload-bound term-creation client request ID stored on AcademicTerm, and
  expected-rowversion/atomic-transaction retry safety for publication and
  profile correction without a generic idempotency entity;
- independent transcript/provenance pages, a complete active-hold set capped
  at 100, Student-safe holds without identifiers, and named Admin holds with
  correction identifiers;
- no more than 20 windows/version entries or 20 correction operations, bounded
  field errors, bounded strings, and deterministic allow-listed sorts; and
- explicit 200/201, 400, 401, 403, authorized 404, 409, 500, and 503 behavior
  or an explained not-applicable category for each endpoint.

The canonical TypeScript declarations are repeated here verbatim so the
approved requirements and executable contract remain reviewable together:

```typescript
type AcademicTermState =
  | "draft"
  | "registrationOpen"
  | "registrationClosed"
  | "teaching"
  | "completed"
  | "archived";

type RegistrationWindowLifecycleState =
  | "draft"
  | "published"
  | "emergencyClosed"
  | "superseded";

interface RegistrationWindowSummaryDto { // canonical SPEC-006 type
  id: string;
  state: "upcoming" | "open" | "closed";
  opensAtUtc: string;
  closesAtUtc: string;
  rowVersion: string;
}

interface AcademicHoldDto {
  termId: string;
  code: string;
  message: string;
  blocksRegistration: boolean;
  effectiveFromUtc: string;
  effectiveToUtc?: string;
  source: string;
}

interface AdminAcademicHoldDto extends AcademicHoldDto {
  holdId: string;
  sourceReference: string;
}

interface StudentAcademicContextDto {
  universityId: string;
  programCode: string;
  cohort: string;
  currentGpa: number;
  earnedCredits: number;
  standing: string;
  transcriptSummary: {
    attemptedCredits: number;
    earnedCredits: number;
    attemptCount: number;
  };
  transcriptAttempts: Page<{
    attemptId: string;
    supersedesAttemptId?: string;
    courseCode: string;
    termCode: string;
    credits: number;
    grade?: string;
    status: string;
    provenance: string;
  }>;
  activeHolds: AcademicHoldDto[];
  dataVersion: string;
  dataAsOfUtc: string;
  provenance: Page<{
    source: string;
    reference: string;
    importedAtUtc: string;
  }>;
}

interface AdminStudentAcademicContextDto
  extends Omit<StudentAcademicContextDto, "activeHolds"> {
  studentId: string;
  termId: string;
  activeHolds: AdminAcademicHoldDto[];
  studentRowVersion: string;
  studentTermStateRowVersion: string;
}

interface AdminRegistrationWindowDto {
  id: string;
  scopeType: "all-students" | "program" | "cohort";
  scopeValue?: string;
  opensAtUtc: string;
  closesAtUtc: string;
  lifecycleState: RegistrationWindowLifecycleState;
  computedState: "upcoming" | "open" | "closed";
  rowVersion: string;
}

interface AdminTermDto {
  id: string;
  code: string;
  displayName: string;
  timeZoneId: string;
  teachingStartsOn: string;
  teachingEndsOn: string;
  state: AcademicTermState;
  rowVersion: string;
  windows: AdminRegistrationWindowDto[]; // maximum 20
}

interface TermWindowInput {
  id?: string;
  scopeType: "all-students" | "program" | "cohort";
  scopeValue?: string;
  opensAtUtc: string;
  closesAtUtc: string;
  lifecycleState: RegistrationWindowLifecycleState;
}

interface TermInput {
  code: string;
  displayName: string;
  timeZoneId: string;
  teachingStartsOn: string;
  teachingEndsOn: string;
  state: AcademicTermState;
}

interface CreateTermRequest {
  clientRequestId: string;
  reason: string;
  source: string;
  term: TermInput;
  windows: TermWindowInput[]; // maximum 20; new windows must be draft
}

interface UpdateTermRequest {
  expectedTermRowVersion: string;
  expectedWindowRowVersions: Record<string, string>; // maximum 20
  reason: string;
  source: string;
  term: TermInput;
  windows: TermWindowInput[]; // maximum 20
}

interface PublishRegistrationWindowRequest {
  expectedTermRowVersion: string;
  expectedWindowRowVersion: string;
  reason: string;
  source: string;
}

type AcademicProfileCorrectionOperation =
  | { kind: "set-gpa"; currentGpa: number; sourceReference: string }
  | { kind: "set-earned-credits"; earnedCredits: number; sourceReference: string }
  | { kind: "set-standing"; standingCode: string; sourceReference: string }
  | { kind: "upsert-transcript-attempt"; supersedesAttemptId?: string; courseCode: string; termCode: string; credits: number; grade?: string; status: "in-progress" | "passed" | "failed" | "withdrawn"; sourceReference: string }
  | { kind: "upsert-hold"; holdId?: string; code: string; message: string; blocksRegistration: boolean; effectiveFromUtc: string; effectiveToUtc?: string; sourceReference: string }
  | { kind: "remove-hold"; holdId: string; sourceReference: string };

interface AcademicProfileCorrectionRequest {
  termId: string;
  expectedStudentRowVersion: string;
  expectedStudentTermStateRowVersion: string;
  reason: string;
  source: string;
  operations: AcademicProfileCorrectionOperation[]; // 1..20
}

interface AdminStudentLocatorDto {
  studentId: string;
  universityId: string;
  programCode: string;
  cohort: string;
  standing: string;
  dataVersion: string;
}
```

The ten SPEC-008 endpoints are:

- `GET /api/public/context`
- `GET /api/context`
- `GET /api/students/me/academic-context`
- `GET /api/admin/terms`
- `POST /api/admin/terms`
- `PUT /api/admin/terms/{termId}`
- `POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish`
- `GET /api/admin/students`
- `GET /api/admin/students/{studentId}/academic-context`
- `PATCH /api/admin/students/{studentId}/academic-profile`

The public response is the canonical six-field `PublicContextDto`; authenticated
composition extends it only through the canonical shared `AppContextDto`.

The Admin student locator requires `termId` and a 3..50-character query and
returns only minimal locator fields. Detail and correction additionally bind
the named `studentId` and term scope. Authorization occurs before resource or
version disclosure.

The correction request's `termId` identifies the exact
`StudentTermAcademicState` boundary. A transcript correction appends a new
attempt whose normalized `supersedesAttemptId` points to the same
student/course/term current leaf; it never updates the earlier record. The
current summary follows only valid leaves. A hold created or changed by the
request belongs to the supplied term.

Stale writes use canonical `ApiError.currentVersion` for the directly contested
aggregate and bounded `fieldErrors`. Multi-aggregate or overlap conflicts
require a bounded term refetch; no parallel version map or conflicting-window
collection extends the shared error shape.

## Data Models

| Entity | Key fields |
|---|---|
| AcademicTerm | code, teaching dates, timezone, state, rowversion, globally unique CreationClientRequestId and CreationPayloadHash |
| RegistrationWindow | term, normalized scope, OpensAtUtc, ClosesAtUtc, lifecycle, rowversion; maximum 20 per term and no overlapping Published window in the same term |
| Student | program/cohort, GPA, credits, standing, active, rowversion |
| TranscriptAttempt | course/term/grade/status/provenance, optional normalized superseded-attempt reference; immutable, acyclic, same-chain, and unique-successor after append |
| StudentHold | student, term, type, blocking flag, effective period, source |
| StudentTermAcademicState | student, term, rowversion; shared serialization/version boundary consumed by registration |

The synthetic Development/Testing fixture is configuration, not a new entity.
For each fixture ordinal it creates one SPEC-007 ApplicationUser link and the
existing SPEC-008 Student/profile graph with synthetic provenance. Rebuilding a
seed-profile version reproduces the same logical University ID, ProgramCode,
cohort, GPA, credits, standing, transcript attempts, holds, term state, data
version, and as-of time; password hash bytes are outside this spec and need not
be deterministic.

SPEC-008 verifies the student-term boundary with a registration test consumer
and a no-commit callback. Actual seat/enrollment writes and their conformance
are owned by SPEC-014. Recurring class-meeting persistence and its local
day/time representation are owned by SPEC-010.

Implementation must also reconcile `docs/diagrams/CLASS_DIAGRAM.md`: identity
session composition remains at the API boundary, the six SPEC-008 entities and
the shared registration-window context value are represented, and no
Academics-to-Identity implementation dependency is introduced. This planning
refinement records that required delivery without editing the upstream diagram.

## Out of Scope

- OS-1: Computing official grades from assessment events.
- OS-2: Inferring a term from month/date alone.
- OS-3: Browser clock as an authority.
- OS-4: SIS synchronization mechanism until integration is specified.
