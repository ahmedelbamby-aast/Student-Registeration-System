# SPEC-003 NFR-2 Keyboard and Focus Evidence

**Result: PASS — 2026-07-19.**

Every route suite is required to exercise keyboard and focus behavior. The
shared runtime CSS supplies a visible `:focus-visible` outline and route/dialog
sources expose programmatic focus targets and restoration behavior. A route
without both keyboard and focus assertions fails the quality gate.

Execution evidence:

- `NFR_2EvidenceTests`: 2/2 passed in the 22/22 focused quality run.
- All 27 route suites contain keyboard and programmatic-focus assertions.
- The ten requested success-state matrices exercised skip navigation, logical
  focus, visible focus outlines, dialog focus restoration, and equivalent
  calendar/list or table representations across six viewport/zoom profiles.
- The STU-05 dialog test proved Escape restoration to the invoking control and
  rejection focus transfer to the status heading.
