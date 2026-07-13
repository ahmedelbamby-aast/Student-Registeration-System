# SPEC-008: Academic Term and Student Profile

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** APPROVED<br>
**Owner:** Backend Lead<br>
**Reviewers:** Registrar/Policy SME, Data, QA<br>
**Target:** Sprint 1<br>
**Dependencies:** SPEC-002, SPEC-003, SPEC-005, SPEC-006, SPEC-007, SPEC-018<br>

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
- FR-4: At most one permitted registration context MAY match a student at an
  instant.
- FR-5: Student profile MUST include University ID, program/cohort, GPA,
  earned credits, standing, transcript summary and attempt data, every active
  hold (including non-blocking holds with `blocksRegistration`), provenance,
  data version, and as-of time. Each Development and Testing synthetic student
  MUST populate this same complete existing field set, including linked
  `Student`, `TranscriptAttempt`, `StudentHold`, and
  `StudentTermAcademicState` rows where applicable. Fixture values and stable
  University-ID/ApplicationUser links are determined by seed-profile version
  plus fixture ordinal; no production student data or new demographic fields
  are inferred.
- FR-6: Registration commands MUST re-resolve time, term, window, student
  state, and holds.
- FR-7: Admin profile corrections MUST require authorization, reason, source,
  optimistic concurrency, and audit.
- FR-8: A hold/profile mutation and a registration submission for the same
  student/term MUST participate in one database-backed student-term
  `StudentTermAcademicState` serialization boundary and advance its version.
  Registration specs consume this upstream boundary; SPEC-008 does not depend
  on a downstream registration entity.
- FR-9: Registration-window publication MUST lock the affected term/scope in a
  stable order (AcademicTerm, then RegistrationWindow IDs), recheck overlap
  inside the transaction, and reject stale term/window versions.
- FR-10: GET /api/context MUST compose the SPEC-007 identity/session portion
  with a SPEC-008 academic portion containing server UTC time, IANA timezone,
  teaching term, registration term, matched window ID/state/open/close/version,
  service state, and `supportReferencePath`. SPEC-008 owns the endpoint composition and
  academic portion but MUST NOT redefine the SPEC-007 session contract.
- FR-11: Authorized Admin users MUST have bounded term and student lists,
  term/window draft and publication commands, student academic-context reads,
  and explicit profile-correction commands. Every mutation MUST require
  reason, source/provenance, expected rowversion, and audit; no generic bulk
  overwrite or client-asserted role is accepted.

## Non-Functional Requirements

- NFR-1: Time-dependent behavior MUST use TimeProvider and boundary tests.
- NFR-2: Dashboard context SHOULD load within 300 ms p95 at the SPEC-018
  300-read-requests-per-second target.
- NFR-3: Instants MUST be stored in UTC datetime2; recurring class times use
  DayOfWeek/TimeOnly and term timezone.
- NFR-4: Student academic data MUST be restricted to self and approved staff
  scopes. Non-production fixtures MUST be wholly synthetic, and logs, traces,
  snapshots, and test reports MUST NOT contain a full student profile.

## Acceptance Criteria

### AC-1: Device-clock independence (FR-1, FR-6)
Given a device clock is one day ahead<br>
When registration-window state is requested<br>
Then server time and configured term/window determine the state.

### AC-2: No active term (FR-2, FR-4)
Given no registration term matches the student and server instant<br>
When the dashboard loads<br>
Then registration is read-only/unavailable<br>
And a clear no-active-window message is shown.

### AC-3: Hold changes before submit (FR-5, FR-6)
Given a plan was eligible when created<br>
And a blocking hold is added before submission<br>
When the student submits<br>
Then submission is rejected with the hold reason<br>
And no seat/enrollment changes occur.

### AC-4: Governed term/profile edit (FR-3, FR-7)
Given Admin has permission, reason, source and current rowversion<br>
When a valid term/window or academic-profile correction is submitted<br>
Then explicit dates/state/provenance are saved and audited<br>
And a stale rowversion would be rejected.

### AC-5: Hold mutation races submission (FR-6, FR-8)
Given an eligible student submits while an authorized admin adds a blocking
hold for the same term<br>
When both transactions execute concurrently<br>
Then the operations have one valid serial order<br>
And a submission that loses the student-term serialization boundary
revalidates and returns HOLD_BLOCKED without enrollment changes.

### AC-6: Concurrent window publication (FR-3, FR-4, FR-9)
Given two draft windows overlap for the same term and student scope<br>
When two admins publish them concurrently<br>
Then exactly one publication may commit<br>
And the loser receives 409 STALE_VERSION or WINDOW_OVERLAP<br>
And no student matches two permitted registration contexts.

### AC-7: Term and profile quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given fake-clock boundary fixtures, two rebuilds of the same synthetic seed
version, approved dashboard read load, persistence inspection, and
student/staff authorization matrix<br>
When the feature quality gate executes<br>
Then time behavior passes opening/closing boundary tests<br>
And both rebuilds contain the same logical students and complete existing
academic profile values<br>
And dashboard context is at most 300 ms p95<br>
And instants use UTC datetime2 while recurring meetings use local day/time plus
IANA timezone<br>
And academic data is visible only to self or approved staff scope.

### AC-8: Composed authenticated context (FR-1, FR-2, FR-4, FR-10)
Given an authenticated student with an effective session and one matching
published registration window<br>
When GET /api/context is requested through either application replica<br>
Then the identity fields match SPEC-007 and the academic fields identify the
same server instant, timezone, teaching term, registration term, window ID,
window version, and computed Open state<br>
And no browser clock or client-selected term changes the result.

### AC-9: Complete Admin term and profile journeys (FR-3, FR-7, FR-9, FR-11)
Given an authorized Admin opens ADM-02 or ADM-04 with current versions<br>
When the Admin pages/searches records, edits a term/window, publishes a
validated window, or submits a sourced profile correction<br>
Then the matching owner API returns a bounded response or audited mutation<br>
And stale, overlapping, unauthorized, or missing-provenance operations change
nothing and return their stable reason.

## Edge Cases

- EC-1: Overlapping active windows for same scope -> publication fails.
- EC-2: Missing GPA/provenance in an imported record, or any missing required
  existing academic-profile field in a synthetic seed row -> the affected
  policy decision fails closed and the seed profile is not marked ready.
- EC-3: Daylight/timezone rule changes -> UTC window remains unambiguous and
  display uses configured timezone library.
- EC-4: Stale admin edit -> 409 with current rowversion.
- EC-5: A scheduled window closes while a request is in flight -> the
  server-received timestamp governs the scheduled cutoff, while an emergency
  administrative closure/version change blocks every uncommitted request.

## API Contracts

```typescript
interface StudentAcademicContextDto {
  universityId: string;
  programCode: string;
  cohort: string;
  currentGpa: number;
  earnedCredits: number;
  standing: string;
  transcriptSummary: { attemptedCredits: number; earnedCredits: number; attemptCount: number };
  transcriptAttempts: Array<{ courseCode: string; termCode: string; credits: number; grade?: string; status: string; provenance: string }>;
  activeHolds: Array<{ code: string; message: string; blocksRegistration: boolean; effectiveFromUtc: string; effectiveToUtc?: string; source: string }>;
  dataVersion: string;
  dataAsOfUtc: string;
  provenance: Array<{ source: string; reference: string; importedAtUtc: string }>;
}
type PublicAcademicContextDto = PublicContextDto; // canonical SPEC-006 public shape
interface AcademicAppContextDto {
  serverTimeUtc: string;
  timeZoneId: string;
  teachingTerm?: TermSummaryDto;
  registrationTerm?: TermSummaryDto;
  registrationWindow?: { id: string; state: "upcoming" | "open" | "closed"; opensAtUtc: string; closesAtUtc: string; rowVersion: string };
  serviceState: "available" | "maintenance" | "unavailable";
  supportReferencePath: string;
}
interface TermMutationRequest {
  expectedTermRowVersion?: string;
  expectedWindowRowVersions: Record<string, string>;
  reason: string;
  source: string;
  term: {
    code: string;
    displayName: string;
    timeZoneId: string;
    teachingStartsOn: string;
    teachingEndsOn: string;
  };
  windows: Array<{
    id?: string;
    scopeType: "all-students" | "program" | "cohort";
    scopeValue?: string;
    opensAtUtc: string;
    closesAtUtc: string;
  }>;
}
type AcademicProfileCorrectionOperation =
  | { kind: "set-gpa"; currentGpa: number; sourceReference: string }
  | { kind: "set-earned-credits"; earnedCredits: number; sourceReference: string }
  | { kind: "set-standing"; standingCode: string; sourceReference: string }
  | { kind: "upsert-transcript-attempt"; attemptId?: string; courseCode: string; termCode: string; credits: number; grade?: string; status: "in-progress" | "passed" | "failed" | "withdrawn"; sourceReference: string }
  | { kind: "upsert-hold"; holdId?: string; code: string; message: string; blocksRegistration: boolean; effectiveFromUtc: string; effectiveToUtc?: string; sourceReference: string }
  | { kind: "remove-hold"; holdId: string; sourceReference: string };
interface AcademicProfileCorrectionRequest {
  expectedStudentRowVersion: string;
  expectedStudentTermStateRowVersion: string;
  reason: string;
  source: string;
  operations: AcademicProfileCorrectionOperation[];
}
```

`TermSummaryDto` is the canonical SPEC-006 shared contract with its
Draft/RegistrationOpen/RegistrationClosed/Teaching/Completed/Archived state
vocabulary. SPEC-008 contributes AcademicTerm values to that type without
redefining it.

Endpoints: GET /api/public/context, GET /api/context, GET
/api/students/me/academic-context, GET /api/admin/terms, POST
/api/admin/terms, PUT /api/admin/terms/{termId}, POST
/api/admin/terms/{termId}/registration-windows/{windowId}/publish, GET
/api/admin/students, GET /api/admin/students/{studentId}/academic-context, and
PATCH /api/admin/students/{studentId}/academic-profile. The public response
contains no user, role, student, capacity, or internal-health data. SPEC-017
may orchestrate audit/report views but does not duplicate these owner
handlers.

## Data Models

| Entity | Key fields |
|---|---|
| AcademicTerm | code, teaching dates, timezone, state, rowversion |
| RegistrationWindow | term, scope, OpensUtc, ClosesUtc |
| Student | program/cohort, GPA, credits, standing, active, rowversion |
| TranscriptAttempt | course/term/grade/status/provenance |
| StudentHold | type, blocking flag, effective period, source |
| StudentTermAcademicState | student, term, rowversion; shared serialization/version boundary consumed by registration |

The synthetic Development/Testing fixture is configuration, not a new entity.
For each fixture ordinal it creates one SPEC-007 ApplicationUser link and the
existing SPEC-008 Student/profile graph with synthetic provenance. Rebuilding a
seed-profile version reproduces the same logical University ID, ProgramCode,
cohort, GPA, credits, standing, transcript attempts, holds, term state, data
version, and as-of time; password hash bytes are outside this spec and need not
be deterministic.

## Out of Scope

- OS-1: Computing official grades from assessment events.
- OS-2: Inferring a term from month/date alone.
- OS-3: Browser clock as an authority.
- OS-4: SIS synchronization mechanism until integration is specified.
