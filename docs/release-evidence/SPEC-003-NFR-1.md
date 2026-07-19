# SPEC-003 NFR-1 Accessibility Evidence

**Result: PASS — 2026-07-19.**

The quality gate enumerates all 27 MVP route accessibility suites, rejects a
missing or skipped axe check, and verifies shared landmark, focus-target,
validation-alert, and conflict live-region semantics. Browser runs must report
zero critical or serious axe findings; any missing route file fails closed.

Execution evidence:

- `NFR_1EvidenceTests`: 2/2 passed as part of the 22/22 focused SPEC-003
  quality gate; every one of the 27 route sources contains an executable axe
  assertion and no skipped axe theory/fact.
- The focused ten-route success-state matrix completed 64 browser cases. The
  first run exposed two STF-04 reflow defects; after the table-region fix the
  affected six profiles passed 6/6, leaving every named route/profile green.
- The complete accessibility assembly executed 229 cases: 227 passed. The only
  non-passing items were the explicitly manual SPEC-018 NVDA probe and manual
  signature gate, outside SPEC-003. No SPEC-003 route was skipped or failed.
- Runtime was mandatory (`SRS_ACCESSIBILITY_REQUIRE_RUNTIME=1`); zero critical
  or serious axe finding was accepted.

Evidence sources: `NFR-1EvidenceTests.cs` and the 27 files under
`tests/StudentRegistration.AccessibilityTests/Routes/`.
