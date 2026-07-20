# SPEC-008 NFR-1 Fake-Clock Boundary Evidence

## Requirement and scope

SPEC-008 NFR-1 requires all time-dependent academic-term and student-profile
behavior to use the injected `TimeProvider` and to have executable boundary
tests. This evidence covers that deterministic clock contract only. It does not
claim load, latency, throughput, or multi-replica evidence.

The executable suite is
`tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-1EvidenceTests.cs`.
Its normalized-LF SHA-256 is
`7DAD4C4B05767EBC28124746B44CFAEA31DF0E49B023584228FA4CCCC87192C9`.

## Executed command

Run from the repository root on 2026-07-16:

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec008.NFR_1EvidenceTests" -p:TreatWarningsAsErrors=true
```

## Executable checks

| Check | Measured guarantee |
|---|---|
| Clock injection | `AcademicContextResolver`, `RegistrationWindowService`, `StudentAcademicProfileService`, `AcademicStore`, and `DemoDatabaseInitializer` require `TimeProvider`; their current-time source files contain no direct wall-clock reads. The composition helper preserves a pre-registered fake clock. |
| Registration boundary | At the exact fake opening instant, the window is open: the opening instant is inclusive. At the exact fake closing instant, the window is closed: the closing instant is exclusive. |
| Device independence | Both returned server instants equal the 2042 fake clock and are outside the real device clock interval observed during the test. |
| Fail closed | Multiple current terms and multiple simultaneously open windows deterministically produce `CONTEXT_UNAVAILABLE`, no partial context, and the same message regardless of input order. |
| Student profile boundary | A hold is active at its inclusive start and inactive at its exclusive end; profile reads and the registration boundary command receive the same fake instants. |
| Audit boundary | The term-owner service stamps its audit event with the injected fake instant. |
| Evidence integrity | The suite verifies this document, its own normalized source hash, the measured test counts, and the pending NFR-2 statement below. |

## Measured result

- Focused tests executed: 6
- 6 passed
- 0 failed
- Configuration: Release, warnings treated as errors

**Result: PASS.**

## Deferred performance gate

NFR-2 status: PENDING. The separate T086 run must authenticate 25,000
synthetic students and balance exactly 180,000 context reads through two
stateless API replicas sharing one SQL Server database during a continuous
ten-minute run at 300 reads/second. It must independently record p95 latency
at or below 300 ms and unexpected failures below 0.1%. None of those NFR-2
measurements are inferred from this fake-clock suite.
