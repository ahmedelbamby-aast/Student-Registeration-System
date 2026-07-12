# Feature Specification: Identity and Account Lifecycle

**Feature Branch**: 007-identity-account-lifecycle
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Security Lead
**Normative detail**: [requirements.md](requirements.md)

## Context

Students require University-ID login and controlled first-time activation.
Admin, Lecturer, and TA need one staff login without a role selector. Blazor
client state is not a security boundary, so identity and authorization are
enforced by ASP.NET Core.

## User Scenarios and Testing

### User Story 1 - Student login (FR-1, FR-4) (P1)

As a Student or staff user, I need the Student login (FR-1, FR-4) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given an activated active student with University ID and password<br>
When valid credentials are submitted on /student/login<br>
Then a secure authenticated session is established<br>
And the server routes only to the student's own context.
### User Story 2 - Student activation safety (FR-2) (P1)

As a Student or staff user, I need the Student activation safety (FR-2) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given no pre-imported student record matches an entered University ID<br>
When activation is submitted<br>
Then no account is created or linked<br>
And a generic safe response is returned.
### User Story 3 - Shared staff login (FR-3, FR-4, FR-6) (P2)

As a Student or staff user, I need the Shared staff login (FR-3, FR-4, FR-6) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a staff account with TA claim and valid MFA<br>
When staff login succeeds<br>
Then the server supplies TA context<br>
And no client parameter can add Lecturer or Admin permissions.
### User Story 4 - Antiforgery (FR-7) (P2)

As a Student or staff user, I need the Antiforgery (FR-7) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given an authenticated cookie without a valid antiforgery token<br>
When a state-changing request is submitted<br>
Then the request is rejected and no state changes.
### User Story 5 - Secure lifecycle and abuse control (FR-5, FR-8, FR-9) (P3)

As a Student or staff user, I need the Secure lifecycle and abuse control (FR-5, FR-8, FR-9) behavior so that Identity and Account Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given repeated failed login/recovery attempts for an account<br>
When the approved threshold is reached<br>
Then lockout/rate limiting and safe audit occur<br>
And no long-lived credential is written to browser local storage.

## Edge Cases

- EC-1: University ID already activated -> direct to login/recovery, no second
  account.
- EC-2: Disabled/locked account -> safe generic denial and audit.
- EC-3: User has Lecturer and TA claims -> explicit authorized context switch,
  never privilege union beyond claims.
- EC-4: Session expires during plan edit -> reauthenticate then revalidate plan.
- EC-5: Repeated recovery request -> rate limit while returning generic result.

## Requirements

### Functional Requirements

- FR-1: Student login MUST accept normalized University ID and password.
- FR-2: Student activation MUST only claim a pre-imported student record after
  verification through an approved institutional factor.
- FR-3: Staff MUST use one login and MUST NOT self-register.
- FR-4: The server MUST issue role claims and enforce endpoint/resource
  policies for Student/Admin/Lecturer/TeachingAssistant.
- FR-5: The system MUST support secure recovery, lockout, logout, and
  invalidate-all-sessions.
- FR-6: Staff MUST use MFA before production.
- FR-7: Authentication MUST use a same-origin Secure, HttpOnly, SameSite cookie
  plus antiforgery for mutations.
- FR-8: Long-lived tokens MUST NOT be stored in browser local storage.
- FR-9: Login/activation/recovery MUST be rate-limited and safely audited.

### Key Entities

- **ApplicationUser**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StudentActivation**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RoleAssignment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SecurityAudit**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Authorized students and staff reach only their permitted context.
- **SC-2**: Account activation cannot claim an unknown or already-claimed institutional identity.
- **SC-3**: Authentication and recovery failures reveal no account-existence information.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-004](../004-architecture-engineering-principles/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Out of Scope

- OS-1: Social login and public staff registration.
- OS-2: Student-created identity without institutional pre-provisioning.
- OS-3: Authorization based only on Blazor route/component visibility.
- OS-4: Final identity-provider integration until AASTMT confirms provider.
