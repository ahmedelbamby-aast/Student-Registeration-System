# ADM-02 SPEC-017 Contributor Contract

**Contract version:** `spec017-adm02/1.0`
**Route:** `/admin/terms`
**Canonical page owner:** SPEC-008
**Canonical page:** `TermAdministrationPage.razor`

SPEC-017 does not own or edit the canonical Razor page. This contribution is
bounded by immutable SPEC-003 Page Design Record ADM-02 version 1.0
(`9b67a03ccb9dd12c7a63ae62f70c7f0f520333bebc1be891da9ac2cca7dc1d76`)
and the SPEC-008 API contract
(`2e4d6177c66ab60c2214e9fd008c8531c7fecb28dfccfcc804cb50debc61e257`).
The route remains `design-only`; this file pins only the SPEC-017 contribution.

## Governed owner actions

| Action | Owner endpoint | Authority and evidence |
|---|---|---|
| Page/filter terms | `GET /api/admin/terms` | `AcademicTerms.Manage`; page 1/20, maximum 100, allow-listed sort |
| Create term | `POST /api/admin/terms` | `AcademicTerms.Manage`, antiforgery, payload-bound client request ID, reason/source, atomic audit |
| Save term/window | `PUT /api/admin/terms/{termId}` | `AcademicTerms.Manage`, antiforgery, expected term and child rowversions, reason/source, atomic audit |
| Publish window | `POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish` | `AcademicTerms.Manage`, antiforgery, explicit confirmation, expected term/window rowversions, reason/source, atomic audit |

The page calls SPEC-008 directly. It preserves `VALIDATION_ERROR`,
`TERM_CODE_EXISTS`, `TERM_STATE_CONFLICT`, `WINDOW_OVERLAP`, `STALE_VERSION`,
and `IDEMPOTENCY_KEY_REUSED`; 401/403 remain `UNAUTHORIZED`/`FORBIDDEN`.
Stale results refetch the bounded aggregate, show changed fields/current
versions, preserve safe draft values, and never overwrite the winner.

Create retry uses the owner idempotency contract. Updates use expected
rowversions and refetch before retry. Window publication uses the owner
confirmation plus current dependency versions; SPEC-017 does not invent a
generic `AdminConfirmationService` or accept a generic Admin mutation request.
Audit failure cannot be reported as success and rolls back the owner mutation.

## Excluded contribution

No browser-authored current term/time, enrollment correction, capacity
override, direct database edit, generic Admin command facade, or unaudited
publication action is added. Success appears only after the SPEC-008 server
accepts the command and returns the current immutable version.
