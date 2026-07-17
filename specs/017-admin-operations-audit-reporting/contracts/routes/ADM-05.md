# ADM-05 SPEC-017 Contributor Contract

**Contract version:** `spec017-adm05/1.0`
**Route:** `/admin/catalogue`
**Canonical page owner:** SPEC-009
**Canonical page:** `CatalogueAdministrationPage.razor`

SPEC-017 does not own or edit the canonical Razor page. It pins SPEC-003
ADM-05 Page Design Record 1.0
(`973d6d2430bcb84fa4059497660b3d6b37830d0d16a8a921b3bc744dc9a826d1`)
and SPEC-009 API contract
(`1a809fec654af45577b4ce73407690dddd678dfdffc4f943658c06ef4c10fb58`).
The route remains `design-only` outside this bounded contributor pin.

## Governed owner data and actions

All program, version, draft, import, policy, validation, simulation, and
publication calls go directly to SPEC-009 endpoints under
`CataloguePolicy.Manage`; scoped mutations also require institutional scope
and antiforgery. Lists are paged/filtered/allow-list sorted. Imports are
all-or-nothing and expose bounded row errors, never raw imported content.

Draft/policy edits require expected rowversion and typed allow-listed
operations. Retryable creates and confirmed publications bind a client request
ID to actor, scope, and canonical payload. Validation returns a signed preview
bound to actor, scope, canonical content, dependency versions, and expiry.
Publication explicitly confirms that preview and revalidates/locks the owner
graph before one immutable version and its audit fact commit.

The page preserves `VALIDATION_ERROR`, `PROVENANCE_REQUIRED`,
`PROVENANCE_INVALID`, `UNKNOWN_RULE_TYPE`, `RULE_VALUE_TYPE_MISMATCH`,
`STALE_PREVIEW`, `STALE_VERSION`, `IDEMPOTENCY_KEY_REUSED`,
`PUBLICATION_CONFLICT`, `UNAUTHORIZED`, and `FORBIDDEN`. A stale publication
refreshes and revalidates; simulation never masquerades as publication.
Audit/storage failure rolls back every publication effect.

## Excluded contribution

No executable policy expression, partial import publication, generic Admin
command/confirmation facade, enrollment correction, direct SQL edit, or
success before owner-server acceptance is added.
