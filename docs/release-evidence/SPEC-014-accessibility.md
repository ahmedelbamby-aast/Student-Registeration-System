# SPEC-014 STU-05 Automated Accessibility Evidence

**Route:** STU-05 `/student/review`  
**Evidence date:** 2026-07-17  
**Scope:** Bounded non-production demo automation  
**Result:** PASS (11/11 focused checks)

## Measured result

The executable STU-05 route passed the repository's pinned Playwright
Chromium and Deque axe-core checks at 320, 375, 768, 1024, 1280, and 1920 CSS
pixels, plus the 400% effective-mobile-width profile. The checked success,
blocking-conflict, and stale-rejection states had zero critical or serious axe
violations and no horizontal page overflow.

The same run verified:

- exactly one `main` landmark with working skip-link focus;
- 44 CSS-pixel minimum Edit plan and Review and submit targets;
- an accessible modal name, focus containment, Escape dismissal, and focus
  restoration to the Review and submit trigger;
- assertive, textual conflict details linked to the disabled command; and
- focus movement to the concurrent-change heading after a rejected submit,
  with the stable server reason still visible.

## Reproducible command

```powershell
dotnet test tests/StudentRegistration.AccessibilityTests/StudentRegistration.AccessibilityTests.csproj --no-build --no-restore --filter 'FullyQualifiedName~RegistrationReviewPageAccessibility' --verbosity:minimal
```

Observed result: `Passed: 11, Failed: 0, Skipped: 0` in 18 seconds after the
focused client build completed with zero warnings and zero errors.

## Evidence sources

- `tests/StudentRegistration.AccessibilityTests/Routes/RegistrationReviewPageAccessibilityTests.cs`
- `src/StudentRegistration.Client/Pages/RegistrationReviewPage.razor`
- `src/StudentRegistration.Client/wwwroot/css/app.css`
- `specs/003-ux-storyboard-accessibility/design/pages/STU-05.md`

## Boundary

This is automated route-level evidence only. It does not claim a manual NVDA
journey, a human UX/QA sign-off, an institution-wide WCAG certification, or
production authorization. The separate SPEC-018 manual keyboard/screen-reader
gate remains governed by its own evidence record.
