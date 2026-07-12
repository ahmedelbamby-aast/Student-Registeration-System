# Feature Specification: Academic Term and Student Profile

**Feature Branch**: 008-academic-term-student-profile
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Backend Lead
**Normative detail**: [requirements.md](requirements.md)

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
Then server time and configured term/window determine the state.
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
And a blocking hold is added before submission<br>
When the student submits<br>
Then submission is rejected with the hold reason<br>
And no seat/enrollment changes occur.
### User Story 4 - Governed term/profile edit (FR-3, FR-7) (P2)

As a Student or authorized registrar, I need the Governed term/profile edit (FR-3, FR-7) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given Admin has permission, reason, source and current rowversion<br>
When a valid term/window or academic-profile correction is submitted<br>
Then explicit dates/state/provenance are saved and audited<br>
And a stale rowversion would be rejected.
### User Story 5 - Hold mutation races submission (FR-6, FR-8) (P3)

As a Student or authorized registrar, I need the Hold mutation races submission (FR-6, FR-8) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given an eligible student submits while an authorized admin adds a blocking
hold for the same term<br>
When both transactions execute concurrently<br>
Then the operations have one valid serial order<br>
And a submission that loses the student-term serialization boundary
revalidates and returns HOLD_BLOCKED without enrollment changes.
### User Story 6 - Concurrent window publication (FR-3, FR-4, FR-9) (P3)

As a Student or authorized registrar, I need the Concurrent window publication (FR-3, FR-4, FR-9) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given two draft windows overlap for the same term and student scope<br>
When two admins publish them concurrently<br>
Then exactly one publication may commit<br>
And the loser receives 409 STALE_VERSION or WINDOW_OVERLAP<br>
And no student matches two permitted registration contexts.
### User Story 7 - Term and profile quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Student or authorized registrar, I need the Term and profile quality gate (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Academic Term and Student Profile produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

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

## Requirements

### Functional Requirements

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

### Non-Functional Requirements

- NFR-1: Time-dependent behavior MUST use TimeProvider and boundary tests.
- NFR-2: Dashboard context SHOULD load within 300 ms p95 at the SPEC-018
  300-read-requests-per-second target.
- NFR-3: Instants MUST be stored in UTC datetime2; recurring class times use
  DayOfWeek/TimeOnly and term timezone.
- NFR-4: Student academic data MUST be restricted to self and approved staff
  scopes.

### Key Entities

- **AcademicTerm**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationWindow**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Student**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **TranscriptAttempt**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StudentHold**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Registration availability is determined only by authoritative institutional time and approved windows.
- **SC-2**: Every eligibility decision uses a complete, sourced academic profile.
- **SC-3**: No overlapping active registration context can apply to the same student.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-002](../002-aastmt-policy-rulebook/spec.md)
- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
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
