# Identity Admin Command Delegation

## Ownership

SPEC-007 IdentityAccess is the sole owner of `RoleAssignment`,
`AdminSecurityGuard`, role-command validation, persistence, serialization, and
the resulting `SecurityEvent` and shared SPEC-004 `AuditEvent` facts.
SPEC-017 owns no role writer, guard writer, generic Admin command facade, or
generic confirmation facade.

## Canonical command

The ADM-03 page replaces staff roles only through:

`PUT /api/admin/users/{userId}/roles`

The endpoint is implemented by SPEC-007 and requires the exact
`IdentityManagement` authorization policy plus antiforgery validation. Its
`UserRolesRequest` supplies the complete replacement role set, the expected
user rowversion, and a non-empty reason. The Blazor page calls
`IdentityApiClient.ReplaceAdminUserRolesAsync`; it does not call a SPEC-017 or
StaffAdministration role service.

## Final-Admin invariant

For every change that could reduce the enabled Admin set, the SPEC-007 store
locks the singleton `AdminSecurityGuard`, then recounts enabled Admin role
assignments, applies at most one valid mutation, and appends the identity and
shared audit facts in the same SQL transaction. Two replicas revoking
different assignments when exactly two enabled Admins remain may commit at
most one change. The losing command returns HTTP 409 with
`FINAL_ADMIN_REQUIRED`, and at least one enabled Admin remains.

Expected user-rowversion conflicts remain HTTP 409 `STALE_VERSION`. A failed
audit append rolls back the role mutation. SPEC-017 consumes these outcomes
unchanged and does not translate either conflict into success.

## Forbidden SPEC-017 surface

The following are intentionally absent from StaffAdministration:

- a `RoleAssignment` or `AdminSecurityGuard` entity, repository, EF mapping,
  migration contribution, or direct SQL write;
- an `AdminCommandService`, `AdminConfirmationService`, or duplicate Identity
  role endpoint;
- a client-selected role claim or authorization bypass; and
- a break-glass or final-Admin override.

The canonical implementation and concurrency evidence remain owned by
SPEC-007. SPEC-017 adds conformance coverage only.
