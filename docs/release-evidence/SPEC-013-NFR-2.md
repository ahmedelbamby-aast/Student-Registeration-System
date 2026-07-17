# SPEC-013 NFR-2 Deterministic Ordering Evidence

## Requirement and method

This is focused executable evidence for SPEC-013 NFR-2 and T058. It calls
`ScheduleOptimizer.Optimize` with the same 8-course by 10-group fixture,
approved `1.0.0` optimizer configuration, and identical preferences and
credit target used by NFR-1.

The first production result establishes the ordered baseline. For 50
repetitions, the optimizer runs once in original candidate order and once in
reversed candidate order, producing 100 comparisons. Every returned option
signature contains its rank, ordered course, offering, and group identifiers,
and all four score components with their values and explanations. The ordered
course sequence and complete option signatures must be exactly equal to the
baseline on every comparison, and no run may reach the search limit.

## Boundary

This is deterministic component-level evidence for the real production
optimizer and scorer. It proves that input enumeration order cannot change
the same-input/configuration recommendation order. Correlation suppression,
token protection, atomic apply, persistence, and UI rendering retain their
separate Spec 013 evidence.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec013.NFR_2EvidenceTests" --logger "console;verbosity=normal" --nologo
```

The executable test emits `SPEC013_NFR002` with the repetition, comparison,
and option counts.

Observed focused Release run on 2026-07-17:

- repetitions: 50;
- original/reversed comparisons: 100;
- ordered complete options per result: 3;
- ordering mismatches: 0; and
- search-limit events: 0.

**Result: PASS.**
