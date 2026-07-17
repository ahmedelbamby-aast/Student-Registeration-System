# ADM-06 SPEC-017 Contributor Contract

**Contract version:** `spec017-adm06/1.0`
**Route:** `/admin/offerings`
**Canonical page owner:** SPEC-010
**Canonical page:** `OfferingAdministrationPage.razor`

SPEC-017 does not own or edit the canonical Razor page. It pins SPEC-003
ADM-06 Page Design Record 1.0
(`8c0134d6cf5fbda8f63e22ce01fbc3c7486d0fcf85da49ab52a56c0533b3119c`)
and SPEC-010 API contract
(`e17932d1088b28ac9738cc874bc269103cd0609e0af35d012a00384703e078c4`).
The route remains `design-only` beyond this contributor contract.

## Governed owner data and actions

`GET /api/admin/offerings`, create, group update, validate, and publish remain
SPEC-010 endpoints. All require Admin plus `Offerings.Manage`; scoped
mutations require institutional scope and antiforgery. The list is bounded and
parameterized. Creates use actor/scope/payload-bound idempotency.

Group updates send offering/group/resource versions and a reason, validate the
complete group graph, preserve `Capacity >= EnrolledCount`, and atomically
append audit with any mutation. Validation writes no publication state and
returns stable findings plus a signed preview bound to actor, offering content,
all dependency versions, and expiry. Publication explicitly confirms that
preview with a client request ID and reason, locks dependencies in stable
order, revalidates, and commits publication plus audit atomically.

The page preserves `VALIDATION_ERROR`, `INVALID_ACTIVITY_ROLE`,
`INVALID_SLOT`, `STALE_PREVIEW`, `STALE_VERSION`, `GROUP_CHANGED`,
`RESOURCE_CONFLICT`, `CAPACITY_BELOW_ENROLLED`, `OFFERING_NOT_VALIDATABLE`,
`IDEMPOTENCY_KEY_REUSED`, `UNAUTHORIZED`, and `FORBIDDEN`. Stale state always
refreshes/reviews; unresolved missing-resource, overlap, staffing, room, or
capacity findings block publication.

## Excluded contribution

No capacity bypass, seat decrement, conflict override, enrollment correction,
Admin availability edit, generic Admin facade, or partial/unaudited
publication is added.
