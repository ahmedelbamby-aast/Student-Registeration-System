# Data Model: Identity and Account Lifecycle

## Ownership

SPEC-007 owns `ApplicationUser`, `Staff`, `StudentActivation`,
`AccountRecoveryChallenge`, `StaffMfaChallenge`, `RoleAssignment`, and
`AuthenticationAbuseState`, `IdentityImportBatch`, append-only `SecurityEvent`,
and singleton `AdminSecurityGuard`. SPEC-017 consumes/queries SecurityEvent and
the guard outcome through an upstream
read contract. SPEC-007 consumes the SPEC-018 shared Data Protection key-ring
facility without redefining it.

## Detailed Model

| Entity | Key fields |
|---|---|
| ApplicationUser | normalized login, filtered-unique University ID for a pre-provisioned student identity, enabled/locked state, password hash/provider subject, optional academic/staff link, security stamp, access-failure state, rowversion |
| StudentActivation | hashed token, pre-imported ApplicationUser ID, approved-factor reference, expiry, attempt count, used timestamp, rowversion |
| AccountRecoveryChallenge | hashed token, user ID, approved-channel reference, expiry, attempt count, used timestamp, rowversion |
| StaffMfaChallenge | opaque provider transaction hash/reference, user ID, expiry, attempt count, used timestamp, rowversion |
| RoleAssignment | user, role, effective dates, assigning actor |
| AuthenticationAbuseState | normalized privacy-safe subject/network key, operation, count, window, blocked-until, rowversion |
| IdentityImportBatch | source, content hash, Uploaded/Invalid/Validated/Published/Failed state, rowversion, row errors, idempotent publish result |
| SecurityEvent | immutable event ID, code, safe actor/subject references, reason, redacted before/after summaries for status/role commands, correlation ID, server time, bounded non-secret metadata |
| AdminSecurityGuard | singleton key AdminRole, rowversion; lock before active-Admin recount and role-set mutation |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Activation, recovery, and MFA consumption use a conditional update from
  unused to used and never a read-then-write check.
- University ID is owned by IdentityAccess on ApplicationUser; active role
  assignment constraints are unique in their
  declared scopes; security-stamp validation reads shared SQL-backed state.
- Role replacement locks AdminSecurityGuard, serializes the enabled-Admin
  scope, rechecks the final-enabled-Admin invariant, writes SecurityEvent and
  shared AuditEvent through SPEC-004, and commits in one transaction.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
