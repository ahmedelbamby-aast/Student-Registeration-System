# SPEC-013 NFR-3 Safe Time-Budget Evidence

**Requirement:** SPEC-013/NFR-3  
**Task:** SPEC-013/T059  
**Recorded:** 2026-07-17  
**Focused verification:** PASS - deterministic runtime and evidence checks

## Evidence boundary

The quality test constructs the real `OptimizationCoordinator`,
`ScheduleOptimizer`, and `ScheduleScorer`. An injected deterministic
`TimeProvider` advances to the configured deadline after the optimizer has
verified one complete option. The coordinator returns the stable `time-budget`
status, retains only that complete two-course option, and returns no partial
selection or misleading diagnostic.

The returned task is already complete when observed by the test. Production
uses the injected deadline as a cooperative search stop and contains no
`Task.Run`, timer-backed worker, or detached background operation. A separate
assertion proves caller cancellation propagates before the clock or optimizer
starts work.

## Verification command

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec013.NFR_3EvidenceTests"
```

## Explicit non-claims

- This is deterministic bounded-control-flow evidence, not a wall-clock performance claim.
- The 500 ms p95 target is separate SPEC-013/NFR-1 evidence.
- Final registration revalidation and release authorization remain separate gates.

**Result: PASS.**
