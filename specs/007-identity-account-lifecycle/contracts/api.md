# API Contract: Identity and Account Lifecycle

## Feature Contract

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
  activeRole: "Student" | "Admin" | "Lecturer" | "TeachingAssistant";
  sessionState: "active" | "expiring";
  expiresAtUtc: string;
}
interface RecoveryRequest { universityIdOrUserName: string; }
interface RecoveryCompleteRequest { challengeToken: string; newPassword: string; }
interface ChangePasswordRequest { currentPassword: string; newPassword: string; }
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
interface IdentityUserSummaryDto {
  id: string;
  displayName: string;
  loginIdentifier: string;
  enabled: boolean;
  roles: Array<"Student" | "Admin" | "Lecturer" | "TeachingAssistant">;
  rowVersion: string;
}
interface IdentityImportRequest {
  source: string;
  contentHash: string;
  clientRequestId: string;
  users: Array<{
    externalReference: string;
    kind: "student" | "staff";
    universityId?: string;
    userName?: string;
    staffNumber?: string;
    displayName: string;
    roles: Array<"Admin" | "Lecturer" | "TeachingAssistant">;
  }>;
}
interface IdentityImportPublishRequest { expectedRowVersion: string; clientRequestId: string; }
```

Endpoints: POST /api/auth/student/login, POST /api/auth/student/activate,
POST /api/auth/staff/login, POST /api/auth/logout, POST
/api/auth/recovery/request, POST
/api/auth/recovery/complete, POST /api/auth/password/change, POST
/api/auth/sessions/revoke-all and GET /api/auth/session; plus GET /api/admin/users, POST
/api/admin/users/imports, GET /api/admin/users/imports/{importId}, POST
/api/admin/users/imports/{importId}/publish, PATCH
/api/admin/users/{userId}/status, and PUT /api/admin/users/{userId}/roles.

## Endpoint Contracts

Every response body mentioned below uses the shared SPEC006 deterministic JSON
policy. Every error body is the shared privacy-safe `ApiError`. Antiforgery
failure returns `400 ANTIFORGERY_INVALID` before credential, account, resource,
or version evaluation.

### POST /api/auth/student/login

- Authorization: anonymous only; valid antiforgery token required.
- Request: `StudentLoginRequest`; University ID is server-normalized.
- Responses: `200 SessionDto`; `400 VALIDATION_FAILED`; generic
  `401 AUTHENTICATION_FAILED`; `429 RATE_LIMITED`.
- Disabled, locked, unknown, wrong-password, and non-student outcomes share the
  same 401 shape and do not reveal which check failed.

### POST /api/auth/student/activate

- Authorization: anonymous only; valid antiforgery token required.
- Request: `ActivateStudentRequest`; no confirmation or client-selected ID is
  accepted beyond the pre-provisioned University ID claim.
- Responses: `200 SessionDto`; `400 VALIDATION_FAILED`,
  `PASSWORD_REJECTED`, or generic `ACTIVATION_FAILED`; `429 RATE_LIMITED`.
- Unknown, already-used, expired/attempt-exhausted, and wrong-initial-password
  outcomes share `ACTIVATION_FAILED`. One conditional transaction verifies the
  initial hash, replaces it, rotates security state, and consumes activation.

### POST /api/auth/staff/login

- Authorization: anonymous only; valid antiforgery token required.
- Request: `StaffLoginRequest`; there is no role or second-factor field.
- Responses: `200 SessionDto`; `400 VALIDATION_FAILED`; generic
  `401 AUTHENTICATION_FAILED`; `429 RATE_LIMITED`.
- Exactly one role is derived after server-side password, enabled/lockout,
  staff-link, and effective-assignment checks; zero or multiple roles fail closed.

### POST /api/auth/logout

- Authorization: authenticated; valid antiforgery token required.
- Request: no body. Responses: `204`; `401` when unauthenticated.
- Only the current cookie is expired; no long-lived browser credential exists.

### POST /api/auth/recovery/request

- Authorization: anonymous only; valid antiforgery token required.
- Request: `RecoveryRequest`. Response: always the same empty `202`, including
  unknown, disabled, throttled, or temporarily undeliverable subjects.
- No token, delivery reference, account fact, or retry classification appears
  in the HTTP response. A usable challenge commits only after the configured
  delivery port accepts the proof.

### POST /api/auth/recovery/complete

- Authorization: anonymous only; valid antiforgery token required.
- Request: `RecoveryCompleteRequest`.
- Responses: `204`; `400 VALIDATION_FAILED`, `PASSWORD_REJECTED`, or generic
  `CHALLENGE_INVALID`; `429 RATE_LIMITED`.
- Success conditionally consumes the hashed proof, replaces the password hash,
  and rotates security state in one transaction. Expiry, replay, exhaustion,
  mismatch, and concurrent loss share `CHALLENGE_INVALID`.

### POST /api/auth/password/change

- Authorization: authenticated; valid antiforgery token required.
- Request: `ChangePasswordRequest`.
- Responses: `204`; `400 VALIDATION_FAILED`, `PASSWORD_REJECTED`, or generic
  `CURRENT_PASSWORD_INVALID`; `401`; `429 RATE_LIMITED`.
- Success replaces the hash and rotates security state atomically.

### POST /api/auth/sessions/revoke-all

- Authorization: authenticated; valid antiforgery token required.
- Request: no body. Responses: `204`; `401`.
- Success rotates shared security state so every earlier cookie on every
  replica becomes invalid, including the caller's current cookie.

### GET /api/auth/session

- Authorization: authenticated. Request: no body or query.
- Responses: `200 SessionDto`; `401`.
- The DTO is reconstructed from current shared user/role state and never
  exposes a password hash, security stamp, recovery value, or EF entity.

### Retired role-context mutation

No role-context mutation endpoint exists. Every valid account has exactly one
server-derived role. Zero or multiple roles fail closed as
`INVALID_ROLE_CONFIGURATION` and cannot issue or rotate an authenticated cookie.

### GET /api/admin/users

- Authorization: authenticated `Admin` with identity-management permission.
- Query: optional bounded `search`, `page`, `pageSize`, and allow-listed
  `sort`; default is `displayName,id`, page 1, size 20, maximum 100.
- Responses: `200 Page<IdentityUserSummaryDto>`; `400 PAGE_SIZE_INVALID`; `401`;
  `403`. Search and ordering execute server-side with the ID tie-breaker.

### POST /api/admin/users/imports

- Authorization: identity Admin; valid antiforgery token required.
- Request: `IdentityImportRequest`, maximum 500 rows and bounded string fields.
  `(requestedByUserId, clientRequestId)` is the idempotency scope; the server
  recomputes the canonical content hash and also rejects duplicate source
  content. A valid upload persists only normalized batch-owned candidate rows;
  raw upload bytes and credentials are never staged.
- Responses: `202 IdentityImportBatchDto`; `400 IMPORT_INVALID`; `401`; `403`;
  `409 IDEMPOTENCY_KEY_REUSED` or `IMPORT_CONTENT_EXISTS`.
- Imports pre-provision identities only and never accept plaintext passwords.

### GET /api/admin/users/imports/{importId}

- Authorization: identity Admin; the requested batch must be in the caller's
  authorized identity-management scope.
- Responses: `200 IdentityImportBatchDto`; `401`; `403`; authorized `404`.
- Row errors are bounded and safe; no generated credential or raw row payload
  is returned.

### POST /api/admin/users/imports/{importId}/publish

- Authorization: identity Admin; valid antiforgery token required.
- Request: `IdentityImportPublishRequest`; `clientRequestId` is owner/import
  scoped and the server hashes the canonical request.
- Responses: `200 IdentityImportBatchDto`; `400 IMPORT_NOT_VALIDATED`; `401`;
  `403`; authorized `404`; `409 STALE_VERSION` or
  `IDEMPOTENCY_KEY_REUSED`.
- Publication is all-or-nothing. Same-key/same-payload retries replay the
  committed bounded result; cancellation before commit leaves no partial user.
  Development/Testing credentials cross only the server-side
  `IProvisionedCredentialHandoff`: prepare a non-visible import-ID handoff,
  commit SQL, then complete it; rollback aborts it and a published retry
  idempotently completes any pending handoff. Production has no local adapter
  and fails closed. Endpoint 14 never returns a credential, path, or handoff
  reference.

### PATCH /api/admin/users/{userId}/status

- Authorization: identity Admin; valid antiforgery token required.
- Request: `UserStatusRequest`; reason is required and bounded.
- Responses: `200 IdentityUserSummaryDto`; `400 VALIDATION_FAILED`; `401`;
  `403`; authorized `404`; `409 STALE_VERSION` or `FINAL_ADMIN_REQUIRED`.
- A reducing change locks/rechecks `AdminSecurityGuard`; success advances the
  ApplicationUser rowversion, rotates security state, and atomically appends
  safe SecurityEvent and AuditEvent facts.

### PUT /api/admin/users/{userId}/roles

- Authorization: identity Admin; valid antiforgery token required.
- Request: `UserRolesRequest`; roles are de-duplicated from the fixed allow-list
  and reason is required and bounded.
- Responses: `200 IdentityUserSummaryDto`; `400 VALIDATION_FAILED`; `401`;
  `403`; authorized `404`; `409 STALE_VERSION` or `FINAL_ADMIN_REQUIRED`.
- Replacement uses the one ApplicationUser `expectedRowVersion`. A reducing
  change locks/rechecks `AdminSecurityGuard`; success replaces assignments,
  advances the aggregate rowversion, rotates security state, and atomically
  appends safe SecurityEvent and AuditEvent facts.

## Endpoint Semantics

- Student and staff login return `200 SessionDto` after direct password and
  account-state verification. The demo uses no MFA or second-factor endpoint;
  staff roles are derived exclusively from server-side assignments.
- Student activation verifies `initialPassword`, replaces its persisted hash
  with the `newPassword` hash, and consumes activation in one atomic
  transition. Password confirmation is a client-only field and is never sent.
- Recovery request always returns the same `202` envelope. It never returns a
  challenge token. Identity calls `IAccountRecoveryProofDelivery`; Testing
  injects an in-memory adapter and Development may use only the Git-ignored
  bounded local adapter. If delivery cannot complete, no usable challenge is
  committed. Production startup fails closed without an approved institutional
  adapter. The request response MUST NOT contain the proof.
- Challenge expiry, attempt exhaustion, replay, or concurrent second use
  returns a generic `400 CHALLENGE_INVALID` without account disclosure.
- Password change, recovery completion, and revoke-all atomically rotate the
  shared security stamp. Earlier cookies are rejected by every replica.
  Correctness never depends on sticky sessions and in-memory-only security state.
- Session issuance requires exactly one effective role. `SessionDto.activeRole`
  equals that role for `active` or `expiring`; zero or multiple roles fail closed.
- DEC-01 and DEC-02 are resolved for the demo-only local credential model;
  DEC-13 remains a production approval gate.
- Admin lists default to 20 and reject page sizes above 100. Import creation
  binds source/content hash/clientRequestId; publication is all-or-nothing and
  replay-safe over immutable normalized candidate rows. Provisioned credentials
  use the prepare/commit/complete-or-abort handoff above and never cross the API.
  Status/role commands require reason and aggregate `expectedRowVersion`.
  Account disable and role replacement paths that can reduce the enabled-Admin
  set lock the Identity-owned singleton AdminSecurityGuard,
  serialize/recheck that scope, write SecurityEvent plus the shared SPEC-004
  AuditEvent atomically, and return
  `409 FINAL_ADMIN_REQUIRED` rather than permitting concurrent write skew.
  SPEC-017 delegates to this command and owns no competing role writer.
  The SecurityEvent stores the reason and allow-listed redacted before/after
  status or role-set facts, safe actor/subject references, correlation ID, and
  no credential, token, or unbounded request payload.
- Status and role commands use the one ApplicationUser aggregate
  `expectedRowVersion`. A successful status or effective-role-set change
  advances that same version; no public security-stamp or second role-set
  concurrency token exists.

## Demo Credential Configuration

- Passwords are 15-128 characters. Spaces and Unicode are accepted; paste and
  password managers are supported; digit/upper/lower/symbol composition and
  arbitrary periodic rotation are not required.
- New passwords are checked against the versioned common/context-specific
  blocklist. IdentityV3/PBKDF2 uses at least 100,000 iterations.
- Password authentication locks after five failures for five minutes.
  Activation and recovery proofs expire after 15 minutes and allow at most
  five failed verifications. All counters are shared and durable.
- This is a Development/Testing demo baseline, not an official AASTMT policy.
  Production startup fails closed until a versioned institutional policy is
  approved and reconciled.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Permission-protected operations require the exact governed permission claim;
  role membership alone is insufficient. The effective-role allow-list issues
  Student `Context.Read`/`AcademicProfile.ReadOwn`, Admin
  `IdentityAccess.Manage`/`Context.Read`/`AcademicTerms.Manage`/
  `AcademicProfiles.Manage`, and Lecturer/TeachingAssistant only
  `Context.Read`. Role-context selection replaces rather than unions claims.
- Every POST, PUT, PATCH, or DELETE identity endpoint requires a valid
  same-origin antiforgery token, including anonymous login, activation, and
  recovery requests. The host issues the request token through a Secure,
  SameSite `XSRF-TOKEN` cookie and the Blazor client echoes it only as
  `X-XSRF-TOKEN`; ASP.NET Core's antiforgery cookie remains HttpOnly. Neither is
  stored in local storage. Rejection occurs before state change or disclosure.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
