# SPEC-007 Identity Persistence Owner Extension

**Contract version:** `identity-persistence-owner-extension/1.1`<br>
**Approved:** 2026-07-14 by Ahmed ELbamby<br>
**Upstream baseline:** SPEC-005 commit
`d88892f97e2164fc3ee60b530187c1f4a658ceb4`

This additive owner contract reconciles SPEC-007's runtime identity lifecycle
with the canonical ERD before mapping delivery. It does not transfer DbContext
or migration ownership, create a second identity model, or weaken any
SPEC-005 invariant.

## Exact auth-schema fields

| Entity | Owner extension |
|---|---|
| ApplicationUser | `SecurityStamp`, `AccessFailedCount`, nullable `LockoutEndUtc`; these and `PasswordHash` are server-only. `Version` is the one public aggregate concurrency token. |
| RoleAssignment | `AssignedByReference`; effective-to is nullable. Its internal rowversion detects direct persistence races while every role-set command also advances ApplicationUser.Version. |
| StudentActivation | unique `ApplicationUserId` FK to Identity-owned ApplicationUser, never academic Student; provisioned/activated timestamps, failed-attempt count, rowversion, and no plaintext credential. |
| AccountRecoveryChallenge | unique token hash, hashed opaque delivery reference, expiry, failed-attempt count, consumed timestamp, and rowversion. |
| AuthenticationAbuseState | `Operation`, failure/window/locked-until state, and rowversion. `SubjectKeyHash` is an HMAC over canonical `operation + subject scope + network scope`, so raw University ID, username, and network address are not stored and the existing unique key is operation-scoped. |
| IdentityImportBatch | requesting user, `ClientRequestId`, source name, source/content hash, state, bounded row-error JSON, bounded final-result JSON, timestamps, and rowversion. `(RequestedByUserId, ClientRequestId)` is additionally unique for atomic idempotency; the source hash remains the approved duplicate-content guard. |
| IdentityImportCandidateRow | immutable batch-owned normalized staging row with `Id`, batch FK, 1-based `Ordinal`, bounded external reference/kind/display name, mutually exclusive student University ID or staff username/number, and canonical comma-delimited allow-listed roles. Unique `(IdentityImportBatchId, Ordinal)`; cascade delete from the batch; no credential, password hash, raw upload, public DTO, or rowversion. |
| SecurityEvent | append-only safe actor/subject references, event code, reason, redacted before/after JSON, bounded metadata, correlation, and server timestamp; no credential or public navigation. |
| AdminSecurityGuard | singleton `Id = 1` plus rowversion. Every disable or role-removal path that could reduce the enabled-Admin set locks and rechecks this row. |

## Transport and persistence boundaries

- Shared browser/API identity DTOs live only under the bounded
  `StudentRegistration.Contracts.Identity` namespace at
  `src/StudentRegistration.Contracts/Identity/AuthenticationContracts.cs` and
  `src/StudentRegistration.Contracts/Identity/AdministrationContracts.cs`.
  They contain no password hash, security stamp, recovery hash, EF navigation,
  or internal rowversion beyond an explicitly authorized `expectedRowVersion`.
- `IdentityAccessModelConfiguration` is SPEC-007's only EF contribution. The
  existing Infrastructure.SqlServer DbContext composes it.
- Import publication uses the server-only `IProvisionedCredentialHandoff`.
  It idempotently prepares a non-visible Development/Testing handoff by import
  ID before SQL commit, completes it after commit, and aborts it on rollback.
  A published retry completes an existing pending handoff. Production has no
  adapter and fails closed until institutional delivery is approved. No secret,
  local path, or handoff reference crosses the public Endpoint 14 DTO.
- SPEC-008 remains the sole writer of migration
  `S1IdentityAcademicFoundation` after SPEC-007 and SPEC-008 mappings exist.
  SPEC-007 tests may inspect its contributed relational model against real SQL
  infrastructure but cannot claim migration-applied parity before SPEC-008.

## Telemetry boundary

The POC uses SPEC-018's existing bounded generic `identity` module and
`request` operation signals; NFR-1's login latency is measured by the external
load harness. SPEC-007 does not add a second collector or edit SPEC-018's
allow-list. No username, University ID, IP address, token, role list, or raw
route/query value is emitted.
