# SPEC-008 AUTH-01 frontend evidence

**Route:** `AUTH-01` (`/`)  
**Implementation task:** `SPEC-008/T076`  
**Design contract:** `SPEC-003/T122-T126`, Page Design Record 1.0  
**Fixture:** `frontend-fixture/1.0`  
**Approval:** Ahmed ELbamby, 2026-07-14

## Delivered slice

- `RoleGatewayPage.razor` keeps stable banner, main, and content-info
  landmarks while loading `GET /api/public/context`.
- `AcademicApiClient` strictly deserializes `PublicContextDto`, preserves bounded
  `ApiError` details, and treats missing or malformed success payloads as
  failures.
- Server time, timezone, teaching term, registration term, window state, and
  service state are rendered only from the server response. The page does not
  use browser time as academic authority or render authenticated identity data.
- Student login, Student activation, and Staff login are separate named,
  keyboard-accessible destinations.
- `SERVICE_UNAVAILABLE`, `MAINTENANCE`, `WINDOW_CHANGED`, unknown service
  errors, and offline transport failures fail closed with one guarded retry and
  a safe status/support path.
- The official AASTMT logo is served locally from the previously sourced brand
  asset so the route does not depend on an external runtime request.

## Executed verification

| Evidence family | Result |
|---|---:|
| Client contract (`AUTH-01-CONTRACT-T123`) | 7 passed, 0 failed |
| bUnit component (`AUTH-01-COMP-T124`) | 4 passed, 0 failed |
| Playwright primary/failure journeys | 4 passed, 0 failed |
| Axe, landmarks, keyboard, focus, 400% zoom, six widths | 9 passed, 0 failed |
| Cross-browser visual regression (`AUTH-01-VIS-T126`) | 17 passed, 0 failed |
| Client build | 0 warnings, 0 errors |

The accessibility run initially exposed native fragment navigation that did not
move programmatic focus to `main-content`. The shared external skip-link helper
was added, and the complete accessibility slice then passed.

## Approved visual baseline

Sixteen success-state images were captured at 375, 768, 1280, and 1920 CSS
pixels in Google Chrome Stable, Microsoft Edge Stable, Playwright Firefox, and
Playwright WebKit 26.5. The route waits for the explicit
`[data-route-id='AUTH-01'][data-state='success']` marker before capture.

The authoritative per-target browser metadata and SHA-256 values are recorded
in `tests/StudentRegistration.VisualTests/Baselines/Spec008/AUTH-01/baseline-targets.json`.
The governed registry points to that 16-target approval set and continues to
forbid automatic replacement. Baseline replacement requires the explicit
`SPEC008_AUTH01_BASELINE_APPROVER=Ahmed ELbamby` approval input.

## Scope boundary

This evidence approves only the SPEC-008 contributor implementation for
AUTH-01. It does not claim completion of later registration, timetable,
operations, or production go-live gates.
