# ADM-03 SPEC-017 Contributor Contract

**Contract version:** `spec017-adm03/1.0`
**Route:** `/admin/users`
**Canonical page owner:** SPEC-007
**Canonical page:** `UserAdministrationPage.razor`

SPEC-017 does not own or edit the canonical Razor page. This contribution pins
SPEC-003 ADM-03 Page Design Record 1.0
(`ddf4d3064d33d5faa1a65ed9e63c8146b3700a2cfe0f42bcca7876b6f6c5dbff`)
and SPEC-007 API contract
(`28b2cd5b9e1ecc3152cd6bf5ba2ba48bb85255a94c7ad44fdce72a9c04997e5b`).
The route remains `design-only` until all owner/contributor runtime pins pass.

## Governed owner actions

`GET /api/admin/users`, the import lifecycle, `PATCH
/api/admin/users/{userId}/status`, and `PUT
/api/admin/users/{userId}/roles` remain SPEC-007 endpoints. They require the
exact `IdentityAccess.Manage`/Identity-management authorization policy;
mutations also require antiforgery. Lists are bounded and parameterized.

Status and role changes send a non-empty reason and expected user rowversion.
Imports bind their client request ID to actor, payload, and scope; publish uses
the current import rowversion. The page preserves `STALE_VERSION`,
`FINAL_ADMIN_REQUIRED`, `IDEMPOTENCY_KEY_REUSED`, `IMPORT_CONTENT_EXISTS`,
`IMPORT_NOT_VALIDATED`, `UNAUTHORIZED`, and `FORBIDDEN` without translating a
conflict into success. Successful status/role changes state that the server
accepted and audited the command.

Every Admin-role grant/revocation delegates through the SPEC-007 command.
Identity locks `AdminSecurityGuard`, recounts enabled Admins, mutates
`RoleAssignment`, and appends `SecurityEvent` plus shared `AuditEvent` in one
transaction. Two-replica write skew returns 409 `FINAL_ADMIN_REQUIRED` to the
loser and leaves at least one enabled Admin.

## Excluded contribution

SPEC-017 adds no `RoleAssignment` writer, guard writer, identity repository,
generic Admin command/confirmation facade, credential display, partial import
publication, client role claim, or final-Admin override.
