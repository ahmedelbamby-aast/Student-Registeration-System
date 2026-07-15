# SPEC-008 STU-01 frontend evidence

**Route:** `STU-01` (`/student`)  
**Implementation task:** `SPEC-008/T078`  
**Design contract:** `SPEC-003/T147-T151`, Page Design Record 1.0  
**Fixture:** `frontend-fixture/1.0`  
**Approval authority:** Ahmed ELbamby

## Delivered slice

- `StudentDashboardPage.razor` uses the authenticated `AppShell`, official
  locally served AASTMT logo, Student role navigation, and stable banner, main,
  and content-info landmarks.
- `GET /api/context` and `GET /api/students/me/academic-context` are requested
  together through the shared `AcademicApiClient`. Server time, IANA timezone,
  term, window, profile, GPA, credits, standing, and holds are rendered only
  from those owner responses; browser time is not an academic authority.
- The route implements the complete loading, empty, success, validation-error,
  service-error, unauthorized, session-expired, stale, and offline state
  vocabulary with the immutable `STU-01-COMP-STATE-*` identifiers.
- The approved open, upcoming, closed, no-term, registration-hold, and
  incomplete-profile journeys fail closed. Every blocking hold is rendered
  before and programmatically associated with the disabled primary action.
- Open registration exposes one `Start registration` transition. Upcoming,
  closed, no-term, hold, and incomplete-profile states expose no registration
  navigation. The later Resume decision remains dependent on a server-owned
  registration-record contributor and is not inferred in the browser.
- Refresh is guarded by one `_isRefreshing` pending state, refetches both
  authoritative resources, and cannot issue duplicate commands.
- The future `GET /api/student/registrations/current/timetable` contributor is
  explicitly rendered as unavailable until SPEC-015 delivers and pins its
  contract. No sample, cached, or fabricated timetable is presented as live.

## Executed verification and visual approval

| Evidence family | Result |
|---|---:|
| Client contract (`STU-01-CONTRACT-T148`) | 16 passed, 0 failed |
| bUnit component (`STU-01-COMP-T149`) | 6 passed, 0 failed |
| Playwright owner metadata and six primary/failure journeys | 7 passed, 0 failed |
| Axe, landmarks, keyboard, focus, 400% zoom, six widths | 10 passed, 0 failed |
| Visual target manifest guard | 1 passed, 0 failed |
| Approved cross-browser visual artifacts | 16 present; 16 SHA-256 values verified |
| Cross-browser visual regression (`STU-01-VIS-T151`) | 17 passed, 0 failed, 0 skipped |
| Global visual-registry governance | 2 passed, 0 failed, 0 skipped; 0 warnings |
| Client build | 0 warnings, 0 errors |

The route was exercised at 320, 375, 768, 1024, 1280, and 1920 CSS pixels.
The accessibility suite verifies serious/critical axe findings, horizontal
reflow, 44 CSS-pixel targets, skip-link behavior, initial heading focus,
landmark uniqueness, region names, logical interactive order, and the 400%
effective mobile-width case.

Representative Chrome mobile/desktop, Firefox mobile, and WebKit desktop
artifacts were visually reviewed after capture. They preserve the approved
content hierarchy, readable wrapping, visible focus, official logo, primary
action placement, and explicit unavailable timetable state without horizontal
overflow.

The final verification commands were:

```powershell
dotnet build src\StudentRegistration.Client\StudentRegistration.Client.csproj --no-restore --nologo
dotnet test tests\StudentRegistration.Client.ContractTests\StudentRegistration.Client.ContractTests.csproj --no-restore --nologo --filter "FullyQualifiedName~StudentDashboardPageContractTests"
dotnet test tests\StudentRegistration.Client.UnitTests\StudentRegistration.Client.UnitTests.csproj --no-restore --nologo --filter "FullyQualifiedName~StudentDashboardPageComponentTests"
dotnet test tests\StudentRegistration.E2ETests\StudentRegistration.E2ETests.csproj --no-restore --nologo --filter "FullyQualifiedName~StudentDashboardPage"
dotnet test tests\StudentRegistration.AccessibilityTests\StudentRegistration.AccessibilityTests.csproj --no-restore --nologo --filter "FullyQualifiedName~StudentDashboardPage"
dotnet test tests\StudentRegistration.VisualTests\StudentRegistration.VisualTests.csproj --no-restore --nologo --filter "FullyQualifiedName~StudentDashboardPage"
dotnet test tests\StudentRegistration.SpecificationTests\StudentRegistration.SpecificationTests.csproj --no-restore --nologo --filter "FullyQualifiedName~VisualBaselineManifestTests"
```

## Governed visual handoff

The visual suite targets the approved open-state fixture at 375, 768, 1280,
and 1920 CSS pixels in Google Chrome Stable, Microsoft Edge Stable, Playwright
Firefox, and Playwright WebKit. It waits for both the
`[data-route-id='STU-01'][data-state='success']` route marker and the honest
`current-timetable-unavailable` contributor region before capture.

Ahmed ELbamby's approval is recorded in
`tests/StudentRegistration.VisualTests/Baselines/Spec008/STU-01/baseline-targets.json`.
The manifest governs 16 reviewed PNG artifacts, records their browser/runtime,
viewport, fixture, token, operating-system, approval, and SHA-256 provenance,
and forbids automatic replacement. All 16 stored artifact hashes were verified
against the approved route manifest. The route manifest itself is registered in
the global baseline registry with SHA-256
`89c913e0780e88d91aa7e55b34c70d0cdb363824e2641c90405eedef9cff0d12`.

## Scope boundary

This evidence covers the SPEC-008 contribution to STU-01 only. SPEC-015 still
owns the current timetable and durable registration-record contribution, and
the later discovery, schedule, submission, production-data, Gate B-D, official
AASTMT, and production go-live approvals are not claimed here.
