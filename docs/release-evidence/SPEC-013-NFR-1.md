# SPEC-013 NFR-1 Optimizer Performance Evidence

## Requirement and method

This is the focused executable evidence for SPEC-013 NFR-1 and T057. The
approved load shape contains 8 selected courses with 10 published viable
groups per course: 80 candidates in total. Four courses carry 3 credits and
four carry 1.5 credits, preserving the canonical 18-credit plan target.

The fixture models ordinary 75-minute Sunday/Monday timetable slots. Nine
alternatives for each later course conflict with the preceding course's
feasible slot, while one advances to a free slot. This intentionally exercises
constrained-search pruning without filtering candidates or fabricating an
empty workload. Each invocation calls the real
`ScheduleOptimizer.Optimize` production implementation.

After 10 warm-up invocations, the test times 50 measured runs and calculates
the nearest-rank percentile. Every run must also produce a complete
three-option result, select all eight courses, prune invalid partial
schedules, and satisfy `SearchLimitReached == false`. The executable gate is
**p95 <= 500 ms**.

## Benchmark environment and boundary

The focused run used .NET SDK 10.0.301 on 64-bit Windows 11 Pro with a
12th Gen Intel Core i7-12700H (14 cores, 20 logical processors) and 15.7 GiB
visible memory. This is the approved load shape executed as non-production,
component-level evidence on the recorded local POC environment.

The measurement excludes API, authentication, persistence, browser, network,
and concurrent mixed-traffic costs. It proves the bounded optimizer component
gate and does not replace SPEC-018 system/load evidence.

## Reproduction

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec013.NFR_1EvidenceTests" --logger "console;verbosity=normal" --nologo
```

The executable test emits `SPEC013_NFR001` with the current p95, minimum,
maximum, visited-node, pruned-schedule, option-count, and search-limit values.

Observed focused Release run on 2026-07-17:

- measured runs: 50;
- p95: 2.148 ms (minimum 1.020 ms; maximum 4.878 ms);
- visited nodes per run: 81;
- pruned partial schedules per run: 630;
- complete options per run: 3; and
- search limit reached: false.

**Result: PASS.**
