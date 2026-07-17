# ADM-04 SPEC-017 Contributor Contract

**Contract version:** `spec017-adm04/1.0`
**Route:** `/admin/students`
**Canonical page owner:** SPEC-008
**Canonical page:** `StudentAdministrationPage.razor`

SPEC-017 does not own or edit the canonical Razor page. It pins SPEC-003
ADM-04 Page Design Record 1.0
(`ec0c96f5d1da50dc21c9afd878fdee0bc470976b02f42b59c0e925baa93c22f4`)
and SPEC-008 API contract
(`2e4d6177c66ab60c2214e9fd008c8531c7fecb28dfccfcc804cb50debc61e257`).
The shared route baseline remains `design-only`.

## Governed owner data and action

`GET /api/admin/students` is a bounded, filtered, stable page. `GET
/api/admin/students/{studentId}/academic-context` requires explicit StudentId
and TermId scope and returns only the authorized profile, transcript, holds,
provenance, source timestamps, and rowversions. Both require
`AcademicProfiles.Manage`; role membership alone is insufficient.

`PATCH /api/admin/students/{studentId}/academic-profile` remains the only
correction command. It requires named StudentId/TermId scope, antiforgery,
expected Student and StudentTermState rowversions, a 10-to-500-character
reason, a typed allow-listed operation, and the owner transaction-aware audit
writer. The page preserves `VALIDATION_ERROR`, `STALE_VERSION`,
`INVALID_SUPERSESSION`, `PROFILE_NOT_READY`, `UNAUTHORIZED`, and `FORBIDDEN`.
Stale correction refreshes the selected context and never overwrites current
academic data.

Successful correction displays only the server-returned context. Actor,
reason, timestamp, redacted before/after, and correlation remain available in
the shared audit stream; an audit failure rolls back the correction.

## Excluded contribution

No unrestricted student search, inline ungoverned edit, transcript deletion,
registration/enrollment correction, generic confirmation service, second
academic writer, or protected data on a denied route is added.
