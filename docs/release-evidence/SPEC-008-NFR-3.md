# SPEC-008 NFR-3 UTC and IANA Timezone Evidence

**Owner:** Ahmed ELbamby

**Recorded UTC:** 2026-07-16T00:00:00Z

## Requirement and boundary

SPEC-008 persists its nine academic instants as UTC `datetime2` values and
requires every academic term to carry a resolvable IANA timezone identifier.
The delivered model uses a value converter that restores `DateTimeKind.Utc`
when SQL Server materializes a `datetime2`, whose wire value does not retain a
.NET `DateTimeKind` flag. Academic-term teaching boundaries remain `date`
values because they are calendar dates rather than instants.

Recurring `DayOfWeek`/`TimeOnly` persistence evidence remains SPEC-010-owned.
This result neither adds recurring-meeting persistence to SPEC-008 nor claims
the downstream scheduling gate.

## Executable verification

The focused suite verifies the delivered runtime model, migration operations,
domain entity, and context resolver rather than relying on prose or source-text
claims alone.

| Executable check | Verified result |
|---|---|
| Runtime SQL mapping | The exact nine SPEC-008 instant properties map to `datetime2`; only `StudentHold.EffectiveToUtc` is nullable. |
| Converter round-trip | A value with sub-millisecond ticks survives provider conversion, simulated SQL materialization with `DateTimeKind.Unspecified`, and conversion back with identical ticks and `DateTimeKind.Utc`. The nullable converter also preserves null. |
| Foundation migration | The six `academics` tables contain the same nine `datetime2` instant columns. `AcademicTerms.TimeZoneId` is required `nvarchar(100)`. |
| IANA term persistence | `Africa/Cairo` is preserved exactly by `AcademicTerm` and its EF property mapping and resolves through `TimeZoneInfo`. A Windows identifier and an unknown identifier are rejected. |
| Context resolution | A matching `Africa/Cairo` term resolves successfully. A valid but institutionally mismatched `Europe/London` term fails closed with `CONTEXT_UNAVAILABLE` and no partial context. |
| Evidence integrity | The test verifies the hashes below, required statements and measured counts, and rejects unresolved template markers. |

## Source binding

- NFR-3 test normalized-LF SHA-256: `5C5DC113037F5BBB48FB840ECCA7C2D98FD3AC9A7353832FA769C0F35188AAD2`
- Academic mapping normalized-LF SHA-256: `7BC820B5FD42035989E2B4B877D294CD3FAB8F5322D55B20E478B85F1F6EF62A`
- AcademicTerm normalized-LF SHA-256: `9365097054269C1F1889BCACD791B126E00E963C0093AB825B56113CC61C8F7D`
- Context resolver normalized-LF SHA-256: `839B769AECA89D31DA7DCAA40C6D435422E35F9D0B19970CF34B7C713F43E0CE`
- Foundation migration normalized-LF SHA-256: `45A64F2FCF73953E57F2F8A6CDCB818D30B231E1A2A1339CF1D369113F593ADF`

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec008.NFR_3EvidenceTests" -p:TreatWarningsAsErrors=true
```

- Focused tests: 5 passed
- Failures: 0 failed
- Configuration: Release, warnings treated as errors

**Result: PASS.**
