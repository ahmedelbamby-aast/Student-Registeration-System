# Data Model: Identity and Account Lifecycle

## Ownership

SPEC-007 owns `ApplicationUser`, `Staff`, `StudentActivation`,
`AccountRecoveryChallenge`, `RoleAssignment`, and
`AuthenticationAbuseState`, `IdentityImportBatch`, append-only `SecurityEvent`,
and singleton `AdminSecurityGuard`. SPEC-017 consumes/queries SecurityEvent and
the guard outcome through an upstream
  read contract. SPEC-007 consumes the SPEC-004 SQL-backed Data Protection
  foundation under SPEC-018 security/operations governance without redefining
  either boundary.

## Detailed Model

| Entity | Key fields |
|---|---|
| ApplicationUser | normalized login, filtered-unique synthetic University ID for a pre-provisioned demo student identity, enabled state, ASP.NET Core Identity password hash, security stamp, access-failure count, nullable lockout end, optional academic/staff link, rowversion |
| StudentActivation | unique FK ApplicationUserId -> Identity-owned ApplicationUser.Id, provisioned timestamp, activated timestamp, failed-attempt state, rowversion; never references Student and has no plaintext PIN/password column |
| AccountRecoveryChallenge | hashed token, user ID, hashed opaque delivery reference, expiry, failed-attempt count, used timestamp, rowversion |
| RoleAssignment | user, role, effective dates, assigning actor reference, internal rowversion |
| AuthenticationAbuseState | SubjectKeyHash HMAC over canonical operation + subject/network scope, operation, count, window, blocked-until, rowversion |
| IdentityImportBatch | requesting user/clientRequestId, source, content hash, Uploaded/Invalid/Validated/Published/Failed state, rowversion, bounded row-error JSON, bounded idempotent publish-result JSON |
| SecurityEvent | immutable event ID, code, safe actor/subject references, reason, redacted before/after summaries for status/role commands, correlation ID, server time, bounded non-secret metadata |
| AdminSecurityGuard | singleton integer Id = 1, rowversion; lock before any account-status or role-set mutation that could reduce enabled Admins |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Activation and recovery consumption use a conditional update from
  unused to used and never a read-then-write check.
- StudentActivation references the Identity-owned ApplicationUser, never the
  downstream academic Student. Activation verifies the issued credential,
  replaces the password hash, and marks activation used in one transaction.
- University ID is owned by IdentityAccess on ApplicationUser; active role
  assignment constraints are unique in their
  declared scopes. Status and role-set mutations advance the one ApplicationUser
  aggregate rowversion; there is no independent role-set version. Security-stamp
  validation reads shared SQL-backed state.
- Account disable and role-replacement paths that can reduce the enabled-Admin
  set lock AdminSecurityGuard, serialize that scope, recheck the
  final-enabled-Admin invariant, write SecurityEvent and shared AuditEvent
  through SPEC-004, and commit in one transaction.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
