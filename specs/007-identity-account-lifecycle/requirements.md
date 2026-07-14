# SPEC-007: Identity and Account Lifecycle

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** APPROVED<br>
**Owner:** Security Lead<br>
**Reviewers:** Product Owner, Backend, QA, AASTMT identity owner<br>
**Target:** Sprint 1<br>
**Dependencies:** SPEC-003, SPEC-004, SPEC-005, SPEC-006, SPEC-018<br>

## Context

Students require University-ID login and controlled first-time activation.
Admin, Lecturer, and TA need one staff login without a pre-authentication or
self-asserted role selector. A multi-role user may choose only among roles the
server returns after authentication. Blazor client state is not a security
boundary, so identity and authorization are enforced by ASP.NET Core.

Development and Testing database bootstrap generates wholly synthetic,
pre-provisioned accounts, unique University IDs, and initial PIN/password
credentials. Only ASP.NET Core Identity password hashes are persisted.

## Functional Requirements

- FR-1: Student login MUST accept normalized University ID and password.
- FR-2: Student activation MUST only claim a pre-imported Identity-owned
  student identity (`ApplicationUser` with normalized unique University ID).
  In Development and Testing, a guarded bootstrap MUST generate the synthetic
  University ID and initial PIN/password, persist only its ASP.NET Core
  Identity hash, and allow first use to supply that initial credential plus a
  replacement password. The server MUST verify the initial hash, replace it
  with the new-password hash, and activate the existing identity in one atomic
  transition. Password confirmation is client-only. The browser MUST NOT
  create an identity, choose a University ID, or persist plaintext credentials.
- FR-3: Staff MUST use one login and MUST NOT self-register.
- FR-4: The server MUST issue role claims and enforce endpoint/resource
  policies for Student/Admin/Lecturer/TeachingAssistant.
- FR-5: The system MUST support request-and-complete recovery, password
  change, current-session logout, and revoke-all-sessions. Recovery request
  responses MUST be indistinguishable for existing and unknown accounts;
  completion MUST rotate the security stamp and invalidate every earlier
  session on every replica. Recovery proof delivery MUST cross the narrow
  Identity-owned `IAccountRecoveryProofDelivery` port and the request response
  MUST NOT contain the proof. Testing uses an injected in-memory adapter;
  Development may use only a Git-ignored local adapter whose artifacts are
  purged within seven days. Production MUST fail closed until an approved
  institutional adapter is configured.
- FR-6: Demo staff MUST use the shared staff login with a pre-provisioned local
  username and generated password. No MFA or other second factor is required;
  no pre-authentication role selector, self-asserted role, or public staff
  registration is allowed. The server MUST derive effective roles after
  password and account-state checks; any post-authentication context choice
  MUST be a member of that returned role set.
- FR-7: Authentication MUST use a same-origin Secure, HttpOnly, SameSite cookie
  plus antiforgery for every state-changing endpoint, including anonymous
  login, activation, and recovery commands. The host issues the ASP.NET Core
  antiforgery request token through a Secure, SameSite `XSRF-TOKEN` cookie that
  is not an authentication credential; the client echoes it only in the
  `X-XSRF-TOKEN` header. The framework antiforgery cookie remains HttpOnly.
- FR-8: Long-lived tokens MUST NOT be stored in browser local storage.
- FR-9: Login/activation/recovery MUST be rate-limited and safely audited.
- FR-10: First-use activation and recovery proofs MUST be attempt-bounded and
  single-use through an atomic database transition; recovery proofs MUST also
  be opaque, hashed at rest, and time-bounded. Concurrent first-use attempts
  MUST change at most one account state.
- FR-11: Claiming an institutional University ID MUST be protected by a unique
  database constraint so parallel activation requests cannot link it twice.
- FR-12: Lockout counters, rate-limit buckets, used challenges, security
  stamps, role assignments, and session invalidation MUST use shared durable
  state visible to all replicas. Cookie protection keys MUST use the shared
  key-ring contract in SPEC-018; sticky sessions and in-memory-only security
  state MUST NOT be correctness requirements.
- FR-13: AUTH-02 through AUTH-05 and STU-08 MUST consume the SPEC-003 page,
  state, accessibility, and functional-test contracts.
- FR-14: An Admin with the explicit identity-management permission MUST be able
  to submit and inspect a provenance-bearing, idempotent pre-provisioned-user
  import; page/search users; enable or disable an account; and replace
  effective role assignments. Status/role commands require reason and the
  aggregate `expectedRowVersion`, emit audit facts, and lock the Identity-owned singleton
  `AdminSecurityGuard` before serializing the enabled-Admin role scope so two
  concurrent removals cannot eliminate the final enabled Admin. IdentityAccess
  is the sole role-mutation owner; SPEC-017 may delegate to it but MUST NOT
  implement a second role writer or guard.

## Non-Functional Requirements

- NFR-1: Login SHOULD respond within 500 ms p95 during a 10-minute profile at
  25 password-login attempts/second across at least two stateless replicas and
  25,000 synthetic accounts: 80% valid, 15% invalid-credential, and 5%
  already-locked requests. Expected generic denials are not errors; unexpected
  errors MUST remain below 1%, with no account-enumeration or shared-state
  inconsistency.
- NFR-2: Authentication errors MUST NOT reveal whether an account exists.
- NFR-3: The demo MUST explicitly configure ASP.NET Core Identity 10 with
  PasswordHasherCompatibilityMode.IdentityV3, at least 100,000 PBKDF2
  iterations, password length 15-128, no character-class composition rule, and
  a versioned common/context-specific password blocklist. Password managers,
  paste, spaces, and Unicode MUST be allowed; arbitrary periodic password
  changes MUST NOT be required. Password login locks after five failures for
  five minutes. Activation and recovery proofs expire after 15 minutes and
  permit at most five failed verifications. The official AASTMT credential
  policy is not source-approved; Production MUST fail closed until an approved,
  versioned institutional policy reconciles or supersedes this demo baseline.
- NFR-4: Every protected endpoint MUST have positive/negative authorization
  tests.

## Acceptance Criteria

### AC-1: Student login (FR-1, FR-4)
Given an activated active student with University ID and password<br>
When valid credentials are submitted on /student/login<br>
Then a secure authenticated session is established<br>
And the server routes only to the student's own context.

### AC-2: Student activation safety (FR-2)
Given no pre-imported Identity-owned institutional student identity matches an entered University ID<br>
When activation is submitted<br>
Then no account is created or linked<br>
And a generic safe response is returned.

### AC-3: Shared staff login (FR-3, FR-4, FR-6)
Given a provisioned staff account with TA claim and valid generated demo
credentials<br>
When the shared staff login succeeds<br>
Then the server supplies TA context<br>
And no second-factor prompt or client parameter can add Lecturer or Admin
permissions.

### AC-4: Antiforgery (FR-7)
Given an authenticated cookie without a valid antiforgery token<br>
When a state-changing request is submitted<br>
Then the request is rejected and no state changes.

### AC-5: Secure lifecycle and abuse control (FR-5, FR-8, FR-9)
Given repeated failed login/recovery attempts for an account<br>
When the approved threshold is reached<br>
Then lockout/rate limiting and safe audit occur<br>
And no long-lived credential is written to browser local storage<br>
And recovery request responses do not disclose whether the account exists.

### AC-6: Parallel activation is single-use (FR-2, FR-10, FR-11)
Given one pre-provisioned inactive University ID and its generated initial
password<br>
When ten first-use requests submit that credential and one new password
concurrently through two application replicas<br>
Then exactly one conditional transition verifies the initial credential,
replaces its hash, marks activation complete, and rotates security state<br>
And every other request receives the same safe already-used result<br>
And no duplicate University ID claim or second identity exists.

### AC-7: Replica-wide invalidation (FR-5, FR-12)
Given a user has sessions routed to two application replicas<br>
When recovery, password change, or revoke-all rotates the security stamp<br>
Then both replicas reject every earlier session<br>
And lockout/rate-limit counters remain consistent across replicas.

### AC-8: Identity route contract (FR-13)
Given AUTH-02 through AUTH-05 and STU-08 Page Design Records<br>
When their component, contract, E2E, accessibility, and visual plans are
reviewed<br>
Then every route/state maps to SPEC-003 and the owning identity FR/AC IDs.

### AC-9: Authentication quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given the SPEC-007 pinned login profile and positive/negative role matrix<br>
When authentication performance, enumeration, configuration, and authorization
tests execute<br>
Then login is at most 500 ms p95<br>
And errors do not reveal account existence<br>
And credential configuration passes the pinned NFR-3 demo baseline while the
unverified AASTMT production boundary stays fail closed<br>
And every protected endpoint permits and denies exactly the documented roles.

### AC-10: Governed Admin user lifecycle (FR-3, FR-4, FR-12, FR-14)
Given an authorized identity Admin has the current aggregate `expectedRowVersion`, reason, and
a validated pre-provisioned import or user change<br>
When the Admin imports users, pages users/import status, disables an account,
or replaces roles<br>
Then one idempotent/versioned/audited result is returned and every replica
observes it<br>
And unauthorized, stale, non-provisioned, or final-enabled-Admin removal
attempts change nothing.

## Edge Cases

- EC-1: University ID already activated -> direct to login/recovery, no second
  account.
- EC-2: Disabled/locked account -> safe generic denial and audit.
- EC-3: User has Lecturer and TA claims -> explicit authorized context switch,
  never privilege union beyond claims.
- EC-4: Session expires during plan edit -> reauthenticate then revalidate plan.
- EC-5: Repeated recovery request -> rate limit while returning generic result.
- EC-6: Activation commits but its response is lost -> retry returns the
  already-used safe result and MUST NOT create another user or role assignment.

## API Contracts

```typescript
interface StudentLoginRequest { universityId: string; password: string; }
interface StaffLoginRequest { userName: string; password: string; }
interface ActivateStudentRequest {
  universityId: string;
  initialPassword: string;
  newPassword: string;
}
interface SessionDto {
  displayName: string;
  roles: Array<"Student" | "Admin" | "Lecturer" | "TeachingAssistant">;
  activeRole: "Student" | "Admin" | "Lecturer" | "TeachingAssistant" | null;
  sessionState: "active" | "expiring" | "role-selection-required";
  expiresAtUtc: string;
}
interface RecoveryRequest { universityIdOrUserName: string; }
interface RecoveryCompleteRequest { challengeToken: string; newPassword: string; }
interface ChangePasswordRequest { currentPassword: string; newPassword: string; }
interface SelectRoleContextRequest {
  role: "Admin" | "Lecturer" | "TeachingAssistant";
}
interface IdentityImportBatchDto {
  id: string;
  source: string;
  contentHash: string;
  state: "uploaded" | "invalid" | "validated" | "published" | "failed";
  rowVersion: string;
  errors: Array<{ row?: number; code: string; message: string }>;
}
interface UserStatusRequest { enabled: boolean; expectedRowVersion: string; reason: string; }
interface UserRolesRequest { roles: Array<"Admin" | "Lecturer" | "TeachingAssistant">; expectedRowVersion: string; reason: string; }
```

Endpoints: POST /api/auth/student/login, POST /api/auth/student/activate,
POST /api/auth/staff/login, POST /api/auth/logout, POST
/api/auth/recovery/request, POST
/api/auth/recovery/complete, POST /api/auth/password/change, POST
/api/auth/sessions/revoke-all, GET /api/auth/session, and PUT
/api/auth/session/context; plus GET /api/admin/users, POST
/api/admin/users/imports, GET /api/admin/users/imports/{importId}, POST
/api/admin/users/imports/{importId}/publish, PATCH
/api/admin/users/{userId}/status, and PUT /api/admin/users/{userId}/roles.
Anonymous activation/recovery responses are generic;
all account mutations use antiforgery and server-side rate limits.

## Data Models

| Entity | Key fields |
|---|---|
| ApplicationUser | Identity fields, normalized unique University ID for student identities, enabled state, optional academic/staff link |
| StudentActivation | unique FK ApplicationUserId -> Identity-owned ApplicationUser.Id, provisioned timestamp, activated timestamp, rowversion; never references Student and has no plaintext PIN/password field |
| AccountRecoveryChallenge | hashed token, subject, expiry, attempts, used timestamp |
| RoleAssignment | user, role, effective dates, assigning actor |
| AuthenticationAbuseState | SubjectKeyHash HMAC over canonical operation + subject/network scope, operation, counters, lockout/rate-limit windows, rowversion |
| IdentityImportBatch | requesting user/clientRequestId, source/content hash, lifecycle, rowversion, bounded row-error JSON, bounded idempotent publication-result JSON |
| SecurityEvent | append-only identity/abuse fact with safe actor/subject references, code, reason and redacted before/after facts for status/role commands, correlation, server time, and bounded non-secret metadata; SPEC-017 may consume/query it |
| AdminSecurityGuard | singleton Admin-role serialization row with rowversion; owned and locked by IdentityAccess role commands |

## Out of Scope

- OS-1: Social login and public staff registration.
- OS-2: Student-created identity without institutional pre-provisioning.
- OS-3: Authorization based only on Blazor route/component visibility.
- OS-4: Final identity-provider integration until AASTMT confirms provider.

## Approval State

- DEC-01 and DEC-02 are resolved for this demo by generated pre-provisioned
  local credentials and password-only authentication with no MFA/2FA.
- DEC-13 must name the production secret provider and certificate custody
  process for the shared Data Protection key ring before production approval;
  it does not block Gate A demo implementation.
- Generated demo credentials MUST remain limited to Development and Testing
  and MUST NOT be presented as an institutional or production identity method.
- Ahmed ELbamby approved Gate A demo implementation on 2026-07-13. Gate B-D,
  release, production deployment, and official AASTMT go-live approvals remain
  separate.
