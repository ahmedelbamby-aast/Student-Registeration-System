# API Contract: Identity and Account Lifecycle

## Feature Contract

```typescript
interface StudentLoginRequest { universityId: string; password: string; }
interface StaffLoginRequest { userName: string; password: string; }
interface ActivateStudentRequest {
  universityId: string;
  activationCode: string;
  password: string;
}
interface SessionDto {
  displayName: string;
  roles: Array<"Student" | "Admin" | "Lecturer" | "TeachingAssistant">;
  activeRole: "Student" | "Admin" | "Lecturer" | "TeachingAssistant" | null;
  sessionState: "active" | "expiring" | "role-selection-required";
  expiresAtUtc: string;
  securityStampVersion: string;
}
interface MfaChallengeDto { challengeId: string; expiresAtUtc: string; providerDisplayName: string; }
interface MfaVerifyRequest { challengeId: string; providerProof: string; }
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
interface UserStatusRequest { enabled: boolean; expectedUserRowVersion: string; reason: string; }
interface UserRolesRequest { roles: Array<"Admin" | "Lecturer" | "TeachingAssistant">; expectedUserRowVersion: string; expectedRoleSetVersion: string; reason: string; }
```

Endpoints: POST /api/auth/student/login, POST /api/auth/student/activate,
POST /api/auth/staff/login, POST /api/auth/staff/mfa/verify, POST
/api/auth/logout, POST /api/auth/recovery/request, POST
/api/auth/recovery/complete, POST /api/auth/password/change, POST
/api/auth/sessions/revoke-all, GET /api/auth/session, and PUT
/api/auth/session/context; plus GET /api/admin/users, POST
/api/admin/users/imports, GET /api/admin/users/imports/{importId}, POST
/api/admin/users/imports/{importId}/publish, PATCH
/api/admin/users/{userId}/status, and PUT /api/admin/users/{userId}/roles.

## Endpoint Semantics

- Student login returns `200 SessionDto`; staff login returns `202
  MfaChallengeDto` until `/staff/mfa/verify` succeeds. No password-only staff
  session is issued.
- Recovery request always returns the same `202` envelope. It never returns a
  challenge token; the approved institutional channel delivers the proof.
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
- DEC-01, DEC-02, and DEC-13 remain fail-closed production approval gates.
- Admin lists default to 20 and reject page sizes above 100. Import creation
  binds source/content hash/clientRequestId; publication is all-or-nothing and
  replay-safe. Status/role commands require reason and expected versions.
  Role replacement locks the Identity-owned singleton AdminSecurityGuard,
  serializes/rechecks the enabled-Admin scope, writes SecurityEvent plus the
  shared SPEC-004 AuditEvent atomically, and returns
  `409 FINAL_ADMIN_REQUIRED` rather than permitting concurrent write skew.
  SPEC-017 delegates to this command and owns no competing role writer.
  The SecurityEvent stores the reason and allow-listed redacted before/after
  status or role-set facts, safe actor/subject references, correlation ID, and
  no credential, token, or unbounded request payload.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
