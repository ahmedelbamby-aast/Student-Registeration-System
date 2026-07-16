# SPEC-012 Scope Review

**Task:** SPEC-012/T052  
**Reviewed:** 2026-07-16  
**Result:** PASS  
**Focused verification:** 4 scope-quality, 4 acceptance, 1 validate-contract,
and 9 rendered STU-04 tests passed.

This review is bound to the delivered `RegistrationPlanService`,
`Spec012Endpoints`, `ScheduleBuilderPage`, conflict detector and client mapper.
It does not rely on a repository-wide forbidden-word scan. The focused quality
test executes the service and mapper, observes the plan-store interaction, and
then verifies the narrow API/page seams.

| Exclusion | Verified delivered boundary | Runtime and test evidence |
|---|---|---|
| OS-1 | No creation or movement of official staff, room, or institutional schedules | `RegistrationPlanService` receives schedule data only through the read-only `IRegistrationPlanContextReader.ReadAsync`. Its sole write port is `IRegistrationPlanStore`, whose `RegistrationPlanStoreCommand` contains only the student's draft selections, totals, state, conflicts, and validation snapshot. `Spec012Endpoints` exposes only current-plan GET, complete-plan PUT, and validation POST. `ScheduleBuilderPage` calls only the registration-plan client for mutations and does not inject `SchedulingApiClient`. |
| OS-2 | No automatic academic conflict override | `ScheduleConflictDetector` emits `MEETING_OVERLAP`; `ScheduleConflictAction` accepts only `change-group` and `remove-group`. `RegistrationPlanService` keeps any conflict in the returned plan and sets `ReviewBlocked`. `ConflictStateMapper` rejects non-meeting conflict codes and non-canonical actions. The page disables Continue while the server plan is blocked and offers only manual change/remove resolution. There is no ignore, waive, override, or bypass command. |
| OS-3 | Editing and validation perform no seat allocation | The complete-plan PUT persists a draft selection set, not an enrollment or capacity mutation. Capacity and enrolled count are read-only snapshot inputs. The actual non-mutating validation path performs context/store reads and does not call `IRegistrationPlanStore.ReplaceAsync`; focused evidence holds the write count unchanged. `RegistrationPlanStoreCommand` has no seat, enrollment, reservation, decrement, or allocation field. Final capacity revalidation and commit remain SPEC-014 owned. |
| OS-4 | No optimized or ranked alternatives | `RegistrationPlanView` and `RegistrationPlanDto` contain conflicts and manual actions but no recommendations or alternative combinations. The delivered mapper creates an empty alternatives collection for every conflict panel. `ScheduleBuilderPage` therefore renders no optimized option from SPEC-012; its reusable panel merely retains the generic empty slot. Recommendation search, ranking, and explanation remain SPEC-013 owned. |

## Disabled demo travel-buffer rule

The delivered `ScheduleConflictDetector` applies strict half-open overlap only.
Adjacent meetings produce no conflict. Its emitted code is
`MEETING_OVERLAP`; `TRAVEL_BUFFER` is reserved in the domain/API contract for a
future approved policy but is never emitted by this detector. The client mapper
also accepts only the delivered meeting-overlap code, so a browser cannot
invent a travel conflict. No campus/room duration or travel matrix participates
in service preparation, endpoint projection, or page rendering.

Behavior evidence:

- `AC_2Tests.Adjacent_meetings_do_not_overlap_or_emit_disabled_travel_conflicts`
  executes the actual detector and requires an empty result.
- `ScopeEvidenceTests.Delivered_service_keeps_schedule_and_seat_boundaries_read_only`
  executes an overlapping two-group plan and requires the sole emitted code to
  be `MEETING_OVERLAP`.

## Fixed credit-load boundary

`RegistrationPlanService` composes the fixed 18-credit target and fixed
18-credit maximum. A 21-credit replacement is rejected as
`LOAD_ABOVE_MAXIMUM` before the plan store is called; this is a hard maximum,
not an overload workflow. Successful, empty, validated, and authorized stale
plan projections retain the same 18/18 values and sourced load reasons.
`Spec012Endpoints` only projects those server values. `ScheduleBuilderPage`
displays `DefaultTargetCredits`, `MaximumAllowedCredits`, and the returned
reason provenance; it contains no GPA-derived branch and no overload command.

Behavior evidence:

- `ScopeEvidenceTests.Fixed_18_maximum_rejects_excess_instead_of_opening_an_overload_path`
  exercises the actual service, proves the 18/18 response, the blocking reason,
  and zero store writes for 21 credits.
- `AC_4Tests.Student_can_add_change_remove_and_recover_from_stale_advisory_state`
  exercises actual add/change/remove/stale/validate behavior, holds validation
  writes and seat mutations at zero, and verifies sourced 18/18 responses.
- `ScheduleBuilderPageFeatureTests` renders server-provided 18/18 values and
  sourced reason fields and verifies that no GPA content is displayed.

## Focused verification

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec012.ScopeEvidenceTests"

dotnet test tests/StudentRegistration.AcceptanceTests/StudentRegistration.AcceptanceTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec012.AC_2Tests|FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec012.AC_4Tests"

dotnet test tests/StudentRegistration.ContractTests/StudentRegistration.ContractTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.ContractTests.Specs.Spec012.Endpoint03ContractTests"

dotnet test tests/StudentRegistration.E2ETests/StudentRegistration.E2ETests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.E2ETests.Specs.Spec012.ScheduleBuilderPage"
```

`ScopeEvidenceTests` provides the focused T052 proof: actual service behavior,
non-mutating validation, the exact plan-store command boundary, manual-only
conflict actions, the empty alternatives collection, disabled travel output,
fixed 18-credit rejection, and the delivered API/page seams.

No scope leak was found; no runtime change was required for T052.
