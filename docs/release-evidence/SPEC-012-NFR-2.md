# SPEC-012 NFR-2 Deterministic Conflict Evidence

## Requirement and method

This is the focused executable evidence for SPEC-012 NFR-2 and T049. One
fixed input contains three selected groups whose meeting intervals produce
three exact overlaps. The test captures stable conflict signatures containing
the code, both groups and meeting intervals, day, exact intersection, message,
and ordered resolution actions.

The same data is evaluated with:

- 50 repeated recalculations; and
- reversed and rotated group order.

Every result must equal the first ordered signature set.
Only `MEETING_OVERLAP` is enabled; `TRAVEL_BUFFER is not emitted` by the
approved demo implementation.

## Boundary

The evidence calls `ScheduleConflictDetector.Detect` directly. It proves
deterministic conflict calculation without claiming plan concurrency,
persistence, endpoint, UI, or recommendation behavior.

## Execution

Command:

`dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec012.NFR_2EvidenceTests" --nologo`

Observed focused run on 2026-07-16:

- three expected conflict signatures were produced;
- all 50 repeated recalculations matched the baseline ordering and content;
- reversed and rotated inputs produced the same ordered result; and
- no travel-buffer conflict was emitted.

**Result: PASS.**
