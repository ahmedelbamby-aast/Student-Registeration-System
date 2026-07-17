# SPEC-013 NFR-4 Optimizer Branch-Coverage Evidence

**Requirement:** SPEC-013/NFR-4  
**Task:** SPEC-013/T060  
**Recorded:** 2026-07-17  
**Focused verification:** PASS - focused quality tests plus Coverlet measurement

## Evidence boundary

The focused scenario matrix executes `ScheduleOptimizer` branches covering
validation and normalization plus:

- argument validation and candidate identity normalization;
- identical and inconsistent duplicate candidates;
- empty, missing, unpublished, non-viable, and out-of-scope candidates;
- immediate no-solution behavior;
- cooperative budget and caller cancellation exits;
- under-credit leaves, over-credit pruning, and meeting-overlap pruning;
- constrained-course ordering and three-option retention; and
- the 100,000-node search limit.

## Coverlet result

The focused command uses the repository's `coverlet.collector` and Cobertura
output. The measured `ScheduleOptimizer` class `branch-rate` is recorded after
the executable suite runs. This document does not substitute the scenario
matrix for a Coverlet percentage or claim coverage for the scorer,
coordinator, endpoints, or other classes.

The focused run on 2026-07-17 reported `branch-rate="0.9722"` (97.22%) and
`line-rate="1"` for `StudentRegistration.Registration.Domain.ScheduleOptimizer`.
The measured branch result exceeds the required 90% threshold.

## Verification command

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec013.NFR_4EvidenceTests" --collect:"XPlat Code Coverage" --results-directory artifacts/TestResults/Spec013Nfr4
```

The generated `coverage.cobertura.xml` must report a `ScheduleOptimizer`
`branch-rate` of at least `0.90` before this evidence remains valid.

## Explicit non-claims

- The percentage is scoped to `ScheduleOptimizer` branches.
- Runtime p95 is separate SPEC-013/NFR-1 evidence.
- Repository-wide coverage and release authorization remain separate gates.

**Result: PASS.**
