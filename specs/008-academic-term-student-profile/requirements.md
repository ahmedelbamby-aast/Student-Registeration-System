# SPEC-008: Academic Term and Student Profile

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Backend Lead<br>
**Reviewers:** Registrar/Policy SME, Data, QA<br>
**Target:** Sprint 1<br>
**Dependencies:** SPEC-002, SPEC-003, SPEC-005, SPEC-007, SPEC-018<br>

## Context

Eligibility needs authoritative server time, the permitted registration term,
and a trusted academic profile. Device time and stale client profile data must
never open a window or change an academic decision.

## Functional Requirements

- FR-1: The server MUST expose current UTC time and configured institutional
  timezone, initially Africa/Cairo.
- FR-2: The system MUST distinguish teaching term from registration term.
- FR-3: Terms/windows MUST have explicit dates, states, scope, and rowversion.
- FR-4: At most one permitted registration context MAY match a student at an
  instant.
- FR-5: Student profile MUST include University ID, program/cohort, GPA,
  earned credits, standing, transcript summary, active holds, and provenance.
- FR-6: Registration commands MUST re-resolve time, term, window, student
  state, and holds.
- FR-7: Admin profile corrections MUST require authorization, reason, source,
  optimistic concurrency, and audit.
- FR-8: A hold/profile mutation and a registration submission for the same
  student/term MUST participate in one database-backed student-term
  serialization boundary and advance its aggregate version.
- FR-9: Registration-window publication MUST lock the affected term/scope in a
  stable order, recheck overlap inside the transaction, and reject a stale
  expected version.

## Non-Functional Requirements

- NFR-1: Time-dependent behavior MUST use TimeProvider and boundary tests.
- NFR-2: Dashboard context SHOULD load within 300 ms p95 at the SPEC-018
  300-read-requests-per-second target.
- NFR-3: Instants MUST be stored in UTC datetime2; recurring class times use
  DayOfWeek/TimeOnly and term timezone.
- NFR-4: Student academic data MUST be restricted to self and approved staff
  scopes.

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
Given fake-clock boundary fixtures, approved dashboard read load, persistence
inspection, and student/staff authorization matrix<br>
When the feature quality gate executes<br>
Then time behavior passes opening/closing boundary tests<br>
And dashboard context is at most 300 ms p95<br>
And instants use UTC datetime2 while recurring meetings use local day/time plus
IANA timezone<br>
And academic data is visible only to self or approved staff scope.

## Edge Cases

- EC-1: Overlapping active windows for same scope -> publication fails.
- EC-2: Missing GPA/provenance -> affected policy decision fails closed.
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
  blockingHolds: Array<{ code: string; message: string }>;
  dataAsOfUtc: string;
  provenance: string;
}
interface PublicAcademicContextDto {
  serverTimeUtc: string;
  timeZoneId: string;
  teachingTermLabel?: string;
  registrationTermLabel?: string;
  registrationWindowState: "open" | "upcoming" | "closed" | "none";
  serviceState: "available" | "maintenance" | "unavailable";
}
```

Endpoints: GET /api/public/context, GET /api/context, and GET
/api/students/me/academic-context; admin mutation contracts live in SPEC-017.
The public response contains no user, role, student, capacity, or
internal-health data.

## Data Models

| Entity | Key fields |
|---|---|
| AcademicTerm | code, teaching dates, timezone, state, rowversion |
| RegistrationWindow | term, scope, OpensUtc, ClosesUtc |
| Student | program/cohort, GPA, credits, standing, active, rowversion |
| TranscriptAttempt | course/term/grade/status/provenance |
| StudentHold | type, blocking flag, effective period, source |

## Out of Scope

- OS-1: Computing official grades from assessment events.
- OS-2: Inferring a term from month/date alone.
- OS-3: Browser clock as an authority.
- OS-4: SIS synchronization mechanism until integration is specified.
