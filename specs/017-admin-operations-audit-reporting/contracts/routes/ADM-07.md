# ADM-07 SPEC-017 Contributor Contract

**Contract version:** `spec017-adm07/1.0`
**Route:** `/admin/resources`
**Canonical page owner:** SPEC-010
**Canonical page:** `ResourceAdministrationPage.razor`

SPEC-017 does not own or edit the canonical Razor page. It pins SPEC-003
ADM-07 Page Design Record 1.0
(`5618e66874503874cffed16e1f5962e5795ed274322c9b10f6ee8ac2b4f8af44`)
and SPEC-010 API contract
(`e17932d1088b28ac9738cc874bc269103cd0609e0af35d012a00384703e078c4`).
The route remains `design-only` outside this SPEC-017 contribution.

## Governed owner data and actions

Room list/create/update, bounded `GET /api/admin/staff-availability`, impact
alert list/revalidate/resolve all remain SPEC-010 endpoints under Admin plus
`Offerings.Manage`; scoped mutations also require antiforgery. Lists are
paged, filtered, parameterized, and stable sorted.

Room create uses actor/scope/payload-bound idempotency. Room update requires
current rowversion and reason and atomically writes any durable impact alert
plus privacy-safe audit. Alert revalidation requires current alert and
group/room/staff-availability dependency versions. Resolution requires the
current alert rowversion, a reason, and unchanged latest passing dependency
snapshot; it never silently resolves stale state.

The page preserves `VALIDATION_ERROR`, `STALE_VERSION`, `GROUP_CHANGED`,
`RESOURCE_CONFLICT`, `ROOM_CODE_EXISTS`, `ROOM_CAPACITY_CONFLICT`,
`REVALIDATION_REQUIRED`, `REVALIDATION_FAILED`, `ALERT_NOT_OPEN`,
`IDEMPOTENCY_KEY_REUSED`, `UNAUTHORIZED`, and `FORBIDDEN`.

Staff availability is a read-only complete range projection. Selecting an
availability ID and rowversion copies only an immutable planning dependency;
it does not mutate `StaffTermAvailability`. Admin does not receive
`Availability.ManageOwn` through this route.

## Excluded contribution

There is no Admin availability mutation/correction/override endpoint,
permission, editable range control, notification workflow, correction audit,
room/conflict override, enrollment correction, or generic Admin facade.
