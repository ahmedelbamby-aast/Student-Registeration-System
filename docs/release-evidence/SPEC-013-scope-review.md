# SPEC-013 Scope Review

**Tasks:** T061, T062, T063, T064  
**Reviewed:** 2026-07-17  
**Baseline:** `c76af079d9029ab70459bee7c0a176a2fa3bac58` plus the current SPEC-013 worktree  
**Result:** PASS — 4 scope-quality, 1 AC-5 acceptance, and 2 EC-1
integration tests passed

This review is limited to the four explicit SPEC-013 exclusions. It inspects
the delivered optimizer, application ports, SQL adapter, API/route manifests,
persistence manifest, and focused tests. It does not claim broader release
approval and does not modify production behavior.

| Task | Exclusion | Verified delivered boundary | Evidence |
|---|---|---|---|
| T061 / OS-1 | ML- or AI-based ranking is excluded | `ScheduleOptimizer` is bounded deterministic search and `ScheduleScorer` uses the approved explicit lexicographic factors: preference violations, idle minutes, teaching days, then the stable group tuple. No ML framework, model, inference engine, training path, or prediction API appears in the delivered source/project dependency surface. | `OptimizerConfiguration` enforces the canonical factor order. `ScopeReviewEvidenceTests.Optimizer_is_bounded_deterministic_code_without_ml_or_ortools` checks the actual algorithm sources, project declarations, and referenced assemblies. |
| T062 / OS-2 | Institution-wide timetable generation and published-resource mutation are excluded | The SPEC-013 endpoint manifest contains only recommendation search and apply-to-student-plan operations. The only contributed route is the existing student schedule page. `IRecommendationSnapshotReader` has only `ReadAsync`; `IRecommendationPlanWriter` has only `ReplaceAsync`. `ScheduleRecommendationSqlServerAdapter` reads published offerings/groups/meetings with `AsNoTracking` and writes only through `plan.ReplaceSelections`. | The route remains implementation-owned by SPEC-012. The adapter contains no offering-publication, resource-availability, group-update, bulk-update, or bulk-delete command. The persistence manifest contains no SPEC-013 persistence contribution or migration. |
| T063 / OS-3 | Guaranteed seats and seat reservation are excluded | Capacity and enrolled count are recommendation viability inputs only. Applying a protected option replaces the student's draft registration-plan selections. The write command has no seat, reservation, enrollment, capacity, room, staff, or resource field. There is no reservation method, enrollment creation, or capacity/enrolled-count mutation in the recommendation service or SQL adapter. | `ScopeReviewEvidenceTests.Applying_an_option_changes_the_plan_without_reserving_a_seat`, `AC-5`, and `EC-1` bind the boundary to the delivered application and adapter. This is explicitly **no seat reservation**; final capacity revalidation and enrollment commit remain outside SPEC-013. |
| T064 / OS-4 | OR-Tools is excluded until benchmark evidence justifies it | The registration domain project reports `No packages were found` for direct or transitive package references. Repository project declarations and the relevant runtime assembly references contain no Google OR-Tools dependency. The delivered bounded deterministic search is retained. | The dependency/source scan returned no `Google.OrTools`, solver-model, ML, or inference-framework matches. No benchmark-driven dependency approval was found, so no solver package was introduced. |

## Inspected surfaces

- `ScheduleOptimizer`, `ScheduleScorer`, and `OptimizerConfiguration` for the
  ranking mechanism, stable ordering, three-option limit, and search bound.
- `RecommendationApplicationService`, `IRecommendationSnapshotReader`, and
  `IRecommendationPlanWriter` for public application/write authority.
- `ScheduleRecommendationSqlServerAdapter` for query tracking and mutations.
- `.specify/endpoint-manifest.json`, `.specify/route-manifest.json`, and
  `.specify/persistence-manifest.json` for owned APIs, UI routes, entities, and
  migrations.
- `AC-5Tests`, `EC-1Tests`, and `ScopeReviewEvidenceTests` for executable scope
  evidence.

The manifest result is deliberately narrow: SPEC-013 owns POST
`/api/student/terms/{termId}/registration-plan/recommendations` and PUT
`/api/student/terms/{termId}/registration-plan/recommended-option`; it
contributes to STU-04 `/student/schedule`, whose implementation owner remains
SPEC-012; and it has no SPEC-013 persistence contribution or migration.

## Focused verification

```powershell
rg -n --glob '*.cs' --glob '*.csproj' 'Google\.OrTools|Microsoft\.ML|ML\.NET|TensorFlow|TorchSharp|OnnxRuntime|PredictionEngine|CpSolver|CpModel|MachineLearning' src
# Result: no matches.

rg -n --glob '*Migration*.cs' --glob '*Migrations*.cs' 'ScheduleOption|OptimizationDiagnostic|OptimizerConfiguration|Recommendation' src
# Result: no matches.

dotnet list src/StudentRegistration.Registration/StudentRegistration.Registration.csproj package --include-transitive
# Result: [net10.0]: No packages were found for this framework.

dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec013.ScopeReviewEvidenceTests"
# Result: 4 passed, 0 failed.

dotnet test tests/StudentRegistration.AcceptanceTests/StudentRegistration.AcceptanceTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.AcceptanceTests.Specs.Spec013.AC_5Tests"
# Result: 1 passed, 0 failed.

dotnet test tests/StudentRegistration.IntegrationTests/StudentRegistration.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.IntegrationTests.Specs.Spec013.EdgeCases.EC_1Tests"
# Result: 2 passed, 0 failed.
```

The focused quality test is the durable evidence for all four exclusions. Its
checks bind the findings to delivered types, source seams, and manifests rather
than treating a prose review as sufficient evidence.
