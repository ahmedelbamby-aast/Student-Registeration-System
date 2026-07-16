# SPEC-008 ADM-04 frontend evidence

**Route:** `ADM-04` (`/admin/students`)<br>
**Implementation task:** `SPEC-008/T082`<br>
**Design contract:** `SPEC-003/T202-T206`, Page Design Record 1.0<br>
**Fixture:** `frontend-fixture/1.0`<br>
**Approval:** Ahmed ELbamby, 2026-07-14
**Verification run:** 2026-07-16

## Delivered slice

- `StudentAdministrationPage.razor` performs a bounded 3-to-50-character
  Student search inside the server-selected academic term. It retains the
  canonical page metadata, exposes labelled previous/next navigation, and
  never offers an empty-query Student enumeration.
- Search results contain only authorized locator fields. Opening one result
  loads the sourced GPA, credits, standing, transcript, complete active-hold
  set, provenance, source timestamp, Student version, and Student-term version.
- Academic values remain read-only until the explicit correction dialog is
  opened. A correction requires a trimmed 10-to-500-character reason, sourced
  provenance, the term ID, both expected row versions, and a typed operation.
  The existing academic API client supplies the protected XSRF header.
- Client validation prevents invalid or duplicate submissions. Server field
  validation remains linked to the relevant inputs. `PROFILE_NOT_READY` is a
  validation state; `STALE_VERSION` closes the command and requires an
  authorized refetch before another decision.
- Unauthorized and expired-session paths purge the Student result set,
  selected profile, search values, pagination metadata, correction draft, and
  pending Student-linked state before rendering safe navigation. Lecturer and
  Teaching Assistant access remain denied by the server-owned endpoint policy.
- An offline correction remains in the modal so keyboard focus is preserved;
  the page explicitly says no change was queued. Loading, empty, success,
  validation-error, service-error, unauthorized, session-expired, stale, and
  offline states use the frozen `ADM-04-COMP-STATE-*` vocabulary.
- At narrow widths, tables have readable label/value alternatives with the
  same actions and values. At 1024 CSS pixels and above, the bounded Student
  list remains left of the academic/provenance detail without changing DOM or
  focus order. The correction dialog becomes full-screen at the narrowest
  layout and restores focus to its trigger.
- The page honestly identifies SPEC-017 registration/audit results as a future
  owner contribution. It does not fabricate downstream data or introduce a
  reverse dependency.

## Executed verification

| Evidence family | Result |
|---|---:|
| Client build | 0 warnings, 0 errors |
| Client contract (`ADM-04-CONTRACT-T203`) | 4 passed, 0 failed |
| bUnit component (`ADM-04-COMP-T204`) | 6 passed, 0 failed |
| Playwright frozen contract and functional journeys | 9 passed, 0 failed |
| Axe, keyboard, focus, screen-reader, 400% zoom, six widths | 9 passed, 0 failed |
| Cross-browser visual regression (`ADM-04-VIS-T206`) | 18 passed, 0 failed |
| Global visual-registry governance | 2 passed, 0 failed |

The functional suite proves a sourced read, no results, 21-result pagination,
server validation, stale mutation, reason-required rejection,
`PROFILE_NOT_READY`, and protected-state purge. The mutation assertions verify
the XSRF header, term ID, normalized reason, source and source reference,
operation kind/value, and both expected versions.

The accessibility suite exercises 320, 375, 768, 1024, 1280, and 1920 CSS
pixels, serious/critical axe findings, page overflow, named landmarks and
regions, semantic table/card alternatives, skip-link behavior, keyboard-only
dialog operation, validation focus, trigger focus restoration, and the 400%
effective mobile-width case.

## Approved visual baseline

Sixteen success-state images were reviewed at 375, 768, 1280, and 1920 CSS
pixels in Google Chrome Stable, Microsoft Edge Stable, Playwright Firefox, and
Playwright WebKit. The reviewed layouts show the locally served AASTMT logo,
term-scoped locator, responsive pagination, sourced profile, narrow card
alternatives, bounded desktop list/detail layout, and honest downstream state.

The authoritative target metadata and per-image SHA-256 values are recorded in
`tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-04/baseline-targets.json`.
All 16 stored images match that manifest. The global registry records the
target-manifest SHA-256 as
`a328536996ac8631ee7d5e713e4a60eb3dc581fbc5ce471d8daa521f132f6f82`
and forbids automatic replacement.

## Scope boundary

This evidence approves only the SPEC-008 academic-profile contribution to
ADM-04. SPEC-017 still owns broader registration administration, audit, and
reporting. Production data integration, production deployment, Gates B-D,
official AASTMT approval, and production go-live remain excluded.
