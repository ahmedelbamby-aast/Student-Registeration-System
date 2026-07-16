# SPEC-012 NFR-3 Calendar/List Semantic Equivalence Evidence

**Requirement:** SPEC-012/NFR-3  
**Task:** SPEC-012/T050  
**Recorded:** 2026-07-16  
**Focused verification:** PASS - 4 quality tests and 9 rendered STU-04 browser tests passed on 2026-07-16

## Evidence boundary

This is focused client-state evidence only. `ConflictStateMapper` copies one
server-authoritative schedule into the same canonical meeting collection used
for both calendar and chronological-list projections. It does not recalculate
meeting overlaps or make an academic, authorization, capacity, or submission
decision.

The focused tests compare every meeting's:

- meeting ID;
- subject code and title;
- group code and activity kind;
- supplied Lecturer and Teaching Assistant names;
- room/location;
- day and start/end local time;
- authoritative timezone context;
- conflict text; and
- owning-route link and accessible name.

The test additionally requires the calendar and list properties to reference
the same canonical meeting collection. `ScheduleBuilderPage` binds both
components to the same `_meetingItems` instance. The rendered STU-04 browser
proof reads every `data-meeting-id` from both views and requires the arrays to
be equal, including the expected two-meeting conflict fixture. This prevents
either presentation from filtering, enriching, reordering, or independently
recalculating the schedule.

## Verification command

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec012.NFR_3EvidenceTests" --no-restore

dotnet test tests/StudentRegistration.E2ETests/StudentRegistration.E2ETests.csproj --filter "FullyQualifiedName~StudentRegistration.E2ETests.Specs.Spec012.ScheduleBuilderPage" --no-restore
```

## Explicit non-claims

- Policy and final-submission behavior remain separate evidence from this NFR-3 proof.
- No visual-release completion is claimed by this focused equivalence proof.
- SPEC-003 contributor pinning and Gate B-D/release approval remain separate.

The focused command passed after the shared persistence and earlier quality
cycles completed. This PASS applies only to the semantic-equivalence boundary
described above.
