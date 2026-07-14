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
  activeRole: "Student" | "Admin" | "Lecturer" | "TeachingAssistant" | null;
  sessionState: "active" | "expiring" | "role-selection-required";
  expiresAtUtc: string;
}
interface RecoveryRequest { universityIdOrUserName: string; }
interface RecoveryCompleteRequest { challengeToken: string; newPassword: string; }
interface ChangePasswordRequest { currentPassword: string; newPassword: string; }
interface SelectRoleContextRequest { role: "Admin" | "Lecturer" | "TeachingAssistant"; }
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
  adapter.
- Challenge expiry, attempt exhaustion, replay, or concurrent second use
  returns a generic `400 CHALLENGE_INVALID` without account disclosure.
- Password change, recovery completion, and revoke-all atomically rotate the
  shared security stamp. Earlier cookies are rejected by every replica.
- Session-context selection accepts only a role already present in the
  effective server role set and rotates the cookie; it cannot add claims.
- When a multi-role staff member has not selected an active context, protected
  navigation returns the canonical `role-selection-required` service state.
  In that state `SessionDto.activeRole` is null; it is non-null when
  `sessionState` is `active` or `expiring`. A selected role must be present in
  `roles`.
- DEC-01 and DEC-02 are resolved for the demo-only local credential model;
  DEC-13 remains a production approval gate.
- Admin lists default to 20 and reject page sizes above 100. Import creation
  binds source/content hash/clientRequestId; publication is all-or-nothing and
  replay-safe. Status/role commands require reason and aggregate `expectedRowVersion`.
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
