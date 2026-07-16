# SPEC-012 NFR-1 Conflict Recalculation Evidence

## Requirement and method

This is the focused executable evidence for SPEC-012 NFR-1 and T048. The
quality test builds 8 selected courses/groups with 10 meeting slots per group
using the immutable Registration-owned `SelectedScheduleMeeting` projection.
After ten warm-up runs, it
executes 100 measured recalculations through
`ScheduleConflictDetector.Detect`, sorts the elapsed samples, and evaluates
the nearest-rank 95th percentile.

The feature gate is p95 <= 200 ms. Every measured run also asserts the
expected 280 exact meeting overlaps so a fast but incomplete calculation
cannot pass.

## Boundary

This is component-level evidence for the pure, server-side overlap detector.
It excludes plan persistence, endpoint/network/browser latency, concurrent
editing, seat allocation, and mixed traffic. Those concerns retain their own
SPEC-012 tasks or the SPEC-018 system/load gates; this focused measurement
does not replace them.

## Execution

Command:

`dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec012.NFR_1EvidenceTests" --nologo`

The test emits the current machine-local measurement as `SPEC012_NFR001` and
enforces the threshold on every run. A static hardware-specific number is not
used as a substitute for the executable assertion.

Observed focused run on 2026-07-16:

- groups: 8;
- meeting slots per group: 10;
- measured recalculations: 100; and
- p95: 3.843 ms against the 200 ms limit.

**Result: PASS.**
