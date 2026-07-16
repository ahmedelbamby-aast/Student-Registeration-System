# SPEC-010 NFR-2 Stable Publication-Code Evidence

**Owner:** Ahmed ELbamby  
**Recorded UTC:** 2026-07-16T18:00:00Z  
**Boundary:** Non-production Spec 010 component evidence

## Executable gate

The quality suite executes the canonical `OfferingPublicationValidator` for
100 repeated validations of the same invalid dependency snapshot. Every run
must return the same codes in the same ordinal order. A second direct check
executes `OfferingService.ValidateForPublicationAsync` against an incomplete
activity bundle. Both services may emit only codes from the approved
publication-code registry in the API contract.

The exercised resource-code sequence is:

1. `STALE_DEPENDENCY`
2. `ROOM_CONFLICT`
3. `ROOM_UNAVAILABLE`
4. `ROOM_CAPACITY_TOO_SMALL`
5. `STAFF_CONFLICT`
6. `STAFF_UNAVAILABLE`
7. `INVALID_SLOT`
8. `MISSING_TUTORIAL_OR_LABORATORY`

The incomplete-bundle fixture additionally proves `MISSING_LECTURER`.

## Actionability

| Code | Recovery action |
|---|---|
| `STALE_DEPENDENCY` | Refresh offering, group, room, and staff-term versions before validating again. |
| `ROOM_CONFLICT` | Select a non-overlapping room or meeting interval. |
| `ROOM_UNAVAILABLE` | Select an available room. |
| `ROOM_CAPACITY_TOO_SMALL` | Select a larger room or reduce group capacity without crossing enrollment. |
| `STAFF_CONFLICT` | Change the assigned staff member or meeting interval. |
| `STAFF_UNAVAILABLE` | Select an available staff member or valid declared range. |
| `INVALID_SLOT` | Correct the day/start/end slot. |
| `MISSING_LECTURER` | Assign a Lecturer to the Lecture meeting. |
| `MISSING_TUTORIAL_OR_LABORATORY` | Add the required Tutorial or Laboratory activity and its TA assignment. |

This record covers the codes emitted by these exact canonical fixtures. It
does not invent executions for other approved codes whose focused behavior is
covered by the Spec 010 acceptance, edge, and contract suites.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec010.NFR_2EvidenceTests" -p:TreatWarningsAsErrors=true
```

- 3 passed
- 0 failed
- Repeated validations: 100

Quality test normalized-LF SHA-256: `1F34A85B8A2AB6FD53F1E14FCF898D5A8AA8EC4782300CBCD33C73F87F88A4C0`

**Result: PASS.**
