# SPEC-001 NFR-1 Accessibility Evidence

**Requirement:** `SPEC-001/NFR-1`
**Recorded:** 2026-07-19
**Automated evidence result:** PASS
**Production authority: not granted**

## Measured result

The committed `SPEC-018-NFR-8-browser-matrix.json` records **36/36** passing
critical-route/browser combinations and zero failures. Nine critical routes
(AUTH-02, AUTH-04, STU-02, STU-04, STU-05, STU-06, STF-01, STF-03, and STF-04)
were exercised across Chrome, Edge, Firefox, and pinned Playwright WebKit.
WebKit is explicitly not represented as Safari.

Each combination checks axe serious-or-worse findings, keyboard focus and
semantics, 44 CSS pixel interactive targets, and responsive overflow. The
result contains zero serious-or-worse findings. Executable static gates also
check WCAG contrast ratios, focus-visible behavior, forced-color support,
reduced-motion behavior, landmarks, live regions, and validation-summary
focus recovery.

The objective Windows integration probe used official NVDA 2026.1.1 with
headed Chrome, produced 16 speech events and two focus events, and completed
its keyboard probe with exit code zero.

## Evidence boundary

This task is the automated evidence task requested by SPEC-001/T033. A manual
Windows/NVDA usability sign-off is not claimed. The signed manual accessibility
release gate and actual Safari execution remain independently governed by
SPEC-018; their absence does not get relabeled as automated proof. This
SPEC-001 evidence therefore closes the measurable automated charter gate only,
not the broader system production-release gate.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec001.NFR_1EvidenceTests"
dotnet test tests/StudentRegistration.AccessibilityTests/StudentRegistration.AccessibilityTests.csproj --configuration Release --filter "FullyQualifiedName~StudentRegistration.AccessibilityTests.Specs.Spec018.Nfr8EvidenceTests&FullyQualifiedName!~Manual_keyboard_and_nvda_journeys_are_executed_and_signed"
```
