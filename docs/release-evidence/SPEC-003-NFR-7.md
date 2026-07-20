# SPEC-003 NFR-7 Browser Matrix Evidence

**Result: PASS for the approved POC browser scope (verified 19 July 2026).**

The executable browser matrix in
`tests/StudentRegistration.E2ETests/browser-matrix.json` records the exact
builds exercised on Microsoft Windows 11 Pro 10.0.26200 x64:

| Required target | Executed build | Evidence scope |
|---|---:|---|
| Google Chrome Stable | 150.0.7871.125 | Current-stable Chromium/Blink target passed. |
| Microsoft Edge Stable | 150.0.4078.83 | Current-stable Edge/Chromium target passed. |
| Mozilla Firefox Release | 152.0.6 | Current-stable system Firefox smoke passed through geckodriver 0.36.0. |
| Playwright WebKit | 26.5 | Bundled by Microsoft.Playwright 1.61.0; this is not Apple Safari. |

The current-stable Firefox 152.0.6 smoke loaded the `STU-02`
`/student/subjects` route and reached its deterministic `service-error` state
with the expected **Available subjects** heading. The service-error state is
intentional for this browser-only smoke because a live API was not part of the
fixture; it verifies that the current Firefox build starts, loads the published
Blazor client, and renders the route's defined recovery state. The captured
screenshot SHA-256 is
`ca3c0f8df7433c1451e64f5708faad7bec2e49140e4dfc25b9a652cf5013129f`,
and the observed user agent identifies `Firefox/152.0`.

Firefox visual-regression baselines are a separate evidence stream. They were
captured with Playwright's bundled Firefox 151.0, not with system Firefox
152.0.6. Those visual artifacts therefore must not be described as screenshots
from the current-stable Firefox build; current-stable compatibility is evidenced
by the WebDriver smoke above.

Actual Safari on macOS remains outside the approved POC environment and is
recorded as deferred and `not-passed`. Passing Playwright WebKit 26.5 does not
claim Safari/macOS compatibility.
