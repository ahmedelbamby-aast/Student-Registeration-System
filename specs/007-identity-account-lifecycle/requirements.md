# SPEC-007: Identity and Account Lifecycle

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Security Lead<br>
**Reviewers:** Product Owner, Backend, QA, AASTMT identity owner<br>
**Target:** Sprint 1<br>
**Dependencies:** SPEC-004, SPEC-005, SPEC-006, SPEC-018<br>

## Context

Students require University-ID login and controlled first-time activation.
Admin, Lecturer, and TA need one staff login without a role selector. Blazor
client state is not a security boundary, so identity and authorization are
enforced by ASP.NET Core.

## Functional Requirements

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

## Non-Functional Requirements

- NFR-1: Login SHOULD respond within 500 ms p95 at approved load excluding MFA
  provider latency.
- NFR-2: Authentication errors MUST NOT reveal whether an account exists.
- NFR-3: Password/credential configuration MUST follow current ASP.NET Core
  Identity and AASTMT security policy.
- NFR-4: Every protected endpoint MUST have positive/negative authorization
  tests.

## Acceptance Criteria

### AC-1: Student login (FR-1, FR-4)
Given an activated active student with University ID and password<br>
When valid credentials are submitted on /student/login<br>
Then a secure authenticated session is established<br>
And the server routes only to the student's own context.

### AC-2: Student activation safety (FR-2)
Given no pre-imported student record matches an entered University ID<br>
When activation is submitted<br>
Then no account is created or linked<br>
And a generic safe response is returned.

### AC-3: Shared staff login (FR-3, FR-4, FR-6)
Given a staff account with TA claim and valid MFA<br>
When staff login succeeds<br>
Then the server supplies TA context<br>
And no client parameter can add Lecturer or Admin permissions.

### AC-4: Antiforgery (FR-7)
Given an authenticated cookie without a valid antiforgery token<br>
When a state-changing request is submitted<br>
Then the request is rejected and no state changes.

### AC-5: Secure lifecycle and abuse control (FR-5, FR-8, FR-9)
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

## API Contracts

```typescript
interface StudentLoginRequest { universityId: string; password: string; }
interface StaffLoginRequest { userName: string; password: string; mfaCode?: string; }
interface ActivateStudentRequest {
  universityId: string;
  activationCode: string;
  password: string;
}
interface SessionDto {
  displayName: string;
  roles: Array<"Student" | "Admin" | "Lecturer" | "TeachingAssistant">;
  expiresAtUtc: string;
}
```

Endpoints: POST /api/auth/student/login, POST /api/auth/student/activate,
POST /api/auth/staff/login, POST /api/auth/logout, POST /api/auth/recovery,
GET /api/auth/session.

## Data Models

| Entity | Key fields |
|---|---|
| ApplicationUser | Identity fields, enabled state, student/staff link |
| StudentActivation | hashed one-time token, expiry, used timestamp |
| RoleAssignment | user, role, effective dates, assigning actor |
| SecurityAudit | event code, actor/subject, time, safe metadata |

## Out of Scope

- OS-1: Social login and public staff registration.
- OS-2: Student-created identity without institutional pre-provisioning.
- OS-3: Authorization based only on Blazor route/component visibility.
- OS-4: Final identity-provider integration until AASTMT confirms provider.
