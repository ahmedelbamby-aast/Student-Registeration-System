# SPEC-008 ADM-02 frontend evidence

**Route:** `ADM-02` (`/admin/terms`)  
**Implementation task:** `SPEC-008/T080`  
**Design contract:** `SPEC-003/T192-T196`, Page Design Record 1.0  
**Fixture:** `frontend-fixture/1.0`  
**Approval:** Ahmed ELbamby, 2026-07-14

## Delivered slice

- `TermAdministrationPage.razor` provides bounded term discovery, explicit
  create and versioned edit forms, draft-window maintenance, and a separate
  publish confirmation. It never derives an academic term, lifecycle, or
  registration-window state from browser time.
- The page sends the server-owned term/window shapes through the shared
  `AcademicApiClient`, including the create client request ID, reason, source,
  expected term/window versions, and the protected XSRF header.
- Local institutional date-time inputs accept the browser's valid minute,
  second, and fractional-second representations before conversion through the
  configured IANA time zone to explicit UTC request instants.
- Create, update, and publish stay pending until the server returns an accepted
  aggregate. `STALE_VERSION` preserves the safe draft, refetches the bounded
  aggregate, and lists changed server fields. `WINDOW_OVERLAP` renders every
  bounded conflict without claiming success.
- Loading, empty, success, validation-error, service-error, unauthorized,
  session-expired, stale, and offline states retain the frozen
  `ADM-02-COMP-STATE-*` vocabulary. Protected term data is cleared on denied or
  expired-session outcomes.
- The confirmation dialog focuses Cancel when opened, traps keyboard focus,
  closes on Escape, restores focus to its trigger, and validates reason/source
  before any publish request is sent.
- The responsive layout uses the semantic table above 375 CSS pixels and the
  approved card alternative at 375 and below. In list-only state the term list
  spans the available workspace, keeping Review visible without unnecessary
  wide-screen horizontal scrolling.

## Executed verification

| Evidence family | Result |
|---|---:|
| Client contract (`ADM-02-CONTRACT-T193`) | 6 passed, 0 failed |
| bUnit component (`ADM-02-COMP-T194`) | 14 passed, 0 failed |
| Playwright create/invalid/overlap/stale/publish journeys | 5 passed, 0 failed |
| Axe, landmarks, keyboard, focus, 400% zoom, six widths | 8 passed, 0 failed |
| Cross-browser visual regression (`ADM-02-VIS-T196`) | 17 passed, 0 failed |
| Global visual-registry governance | 2 passed, 0 failed |
| Client and route test builds | 0 warnings, 0 errors |

The tests exercised 320, 375, 768, 1024, 1280, and 1920 CSS-pixel layouts,
minimum 44 CSS-pixel command targets, keyboard-only operation, programmatic
validation focus, modal focus restoration, serious/critical axe findings, and
horizontal page reflow.

## Approved visual baseline

Sixteen success-state images were reviewed at 375, 768, 1280, and 1920 CSS
pixels in Google Chrome Stable, Microsoft Edge Stable, Playwright Firefox, and
Playwright WebKit. The final review corrected a list-only desktop grid that had
hidden the Review action behind avoidable horizontal scrolling.

The authoritative metadata and per-artifact SHA-256 values are recorded in
`tests/StudentRegistration.VisualTests/Baselines/Spec008/ADM-02/baseline-targets.json`.
All 16 stored images match that approved manifest. The global registry records
the target-manifest SHA-256 as
`65432a3af5a0629cd44bc3afed763e52f770804e855cc121d1b714421300ad72` and
continues to forbid automatic replacement.

## Scope boundary

This evidence approves only the SPEC-008 term/window owner contribution to
ADM-02. SPEC-017 may later consume the owner APIs for broader operations and
reporting; it does not replace these commands. Production data integration,
Gate B-D, official AASTMT approval, and production go-live remain excluded.
