# SPEC-008: Academic Term and Student Profile

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Backend Lead<br>
**Reviewers:** Registrar/Policy SME, Data, QA<br>
**Target:** Sprint 1<br>

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

## Non-Functional Requirements

- NFR-1: Time-dependent behavior MUST use TimeProvider and boundary tests.
- NFR-2: Dashboard context SHOULD load within 300 ms p95 at approved read load.
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

## Edge Cases

- EC-1: Overlapping active windows for same scope -> publication fails.
- EC-2: Missing GPA/provenance -> affected policy decision fails closed.
- EC-3: Daylight/timezone rule changes -> UTC window remains unambiguous and
  display uses configured timezone library.
- EC-4: Stale admin edit -> 409 with current rowversion.

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
```

Endpoints: GET /api/context, GET /api/students/me/academic-context; admin
mutation contracts live in SPEC-017.

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
