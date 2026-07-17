# SPEC-013 Complete Traceability Evidence

**Recorded:** 2026-07-17  
**Scope:** Bounded non-production Schedule Recommendations demo

Release is rejected if any referenced focused suite fails, any row below loses
its evidence, or any Spec 013 task remains unchecked.

## Functional requirements

| Requirement | Acceptance / edge link | Implementation | Executable evidence |
|---|---|---|---|
| FR-1 exactly one published viable group per selected course | AC-1, EC-2 | `ScheduleOptimizer.cs`; `IRecommendationSnapshotReader.cs` | `AC-1Tests.cs`; `ScheduleOptimizerTests.cs`; `EC-2Tests.cs` |
| FR-2 hard meeting, availability, completeness, eligibility, and fixed-credit constraints; no travel guess | AC-1, EC-1, EC-2 | `ScheduleOptimizer.cs`; `ScheduleRecommendationSqlServerAdapter.cs` | `ScheduleOptimizerTests.cs`; `EC-1Tests.cs`; `EC-2Tests.cs` |
| FR-3 constrained-first pruning | AC-8 | `ScheduleOptimizer.cs` | `ScheduleOptimizerTests.cs`; `AC-8Tests.cs`; `NFR-1EvidenceTests.cs`; `NFR-4EvidenceTests.cs` |
| FR-4 up to three distinct schedules | AC-1 | `ScheduleOptimizer.cs` | `ScheduleOptimizerTests.cs`; `AC-1Tests.cs` |
| FR-5 approved versioned lexicographic explanations | AC-2, EC-3, EC-4 | `ScheduleScorer.cs`; `OptimizerConfiguration.cs` | `ScheduleScorerTests.cs`; `AC-2Tests.cs`; `EC-3Tests.cs`; `EC-4Tests.cs` |
| FR-6 cancellation and computation budget | AC-4 | `OptimizationCoordinator.cs` | `OptimizationBudgetTests.cs`; `AC-4Tests.cs`; `NFR-3EvidenceTests.cs` |
| FR-7 inclusion-minimal deterministic diagnostics | AC-3 | `OptimizationCoordinator.cs`; `OptimizationDiagnostic.cs` | `OptimizationBudgetTests.cs`; `AC-3Tests.cs`; `OptimizationDiagnosticModelTests.cs` |
| FR-8 final revalidation and no reservation | AC-5, EC-1 | `RecommendationApplicationService.cs`; `ScheduleRecommendationSqlServerAdapter.cs` | `RecommendationVersionTests.cs`; `AC-5Tests.cs`; `EC-1Tests.cs` |
| FR-9 version-bound encrypted expiring stateless options | AC-6, AC-7 | `RecommendationApplicationService.cs`; `Spec013Endpoints.cs` | `RecommendationVersionTests.cs`; `Endpoint02BehaviorTests.cs`; `AC-6Tests.cs`; `AC-7Tests.cs` |
| FR-10 safe validation and one atomic versioned mutation | AC-6, AC-7, EC-5 | `IRecommendationPlanWriter.cs`; `ScheduleRecommendationSqlServerAdapter.cs` | `RecommendationVersionTests.cs`; `Endpoint02ContractTests.cs`; `EC-5Tests.cs` |

## Non-functional requirements

| Requirement | Automated test | Recorded evidence |
|---|---|---|
| NFR-1 p95 <= 500 ms for 8 x 10 | `tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-1EvidenceTests.cs` | `SPEC-013-NFR-1.md` |
| NFR-2 deterministic ordering | `tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-2EvidenceTests.cs` | `SPEC-013-NFR-2.md` |
| NFR-3 safe bounded expiry | `tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-3EvidenceTests.cs` | `SPEC-013-NFR-3.md` |
| NFR-4 optimizer branch coverage >= 90% | `tests/StudentRegistration.QualityTests/Specs/Spec013/NFR-4EvidenceTests.cs` | `SPEC-013-NFR-4.md` |

## Outcomes, acceptance, and edges

| IDs | Direct evidence |
|---|---|
| SC-1 | `AC-1Tests.cs`, `ScheduleOptimizerTests.cs`, `Endpoint01BehaviorTests.cs` |
| SC-2 | `AC-2Tests.cs`, `ScheduleScorerTests.cs`, `NFR-2EvidenceTests.cs` |
| SC-3 | `AC-3Tests.cs`, `AC-4Tests.cs`, `OptimizationBudgetTests.cs` |
| AC-1 through AC-8 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec013/AC-1Tests.cs` through `AC-8Tests.cs` |
| EC-1 through EC-5 | `tests/StudentRegistration.IntegrationTests/Specs/Spec013/EdgeCases/EC-1Tests.cs` through `EC-5Tests.cs` |

## APIs and route

| Boundary | Contract | Delivery | Evidence |
|---|---|---|---|
| POST recommendations | `specs/013-schedule-recommendations/contracts/api.md` | `Spec013Endpoints.cs` | `Endpoint01ContractTests.cs`; `Endpoint01BehaviorTests.cs` |
| PUT recommended option | same | `Spec013Endpoints.cs`; `RecommendationApplicationService.cs` | `Endpoint02ContractTests.cs`; `Endpoint02BehaviorTests.cs`; `RecommendationVersionTests.cs` |
| STU-04 bounded contributor; SPEC-012 remains owner | `specs/013-schedule-recommendations/contracts/routes/STU-04.md` | `ScheduleRecommendationsPanel.razor`; `RegistrationApiClient.cs`; `ScheduleBuilderPage.razor` | `ScheduleBuilderPageContributorTests.cs` |

## Scope and gate

- T001-T006: approval, dependency baseline, and readiness checklists.
- T007-T020: owned transient models and final API contracts.
- T021-T033: AC/EC future-failing evidence, subsequently satisfied by delivery.
- T034-T053: optimizer, scoring, budget/diagnostic, and protected-apply tests
  and implementation.
- T054-T056: bounded STU-04/API delivery.
- T057-T060: measurable NFR evidence.
- T061-T064: `SPEC-013-scope-review.md`.
- T065: this matrix.
- T066: `SPEC-013-release-approval.md`.

Production deployment, official AASTMT authorization, and final registration
remain outside this evidence boundary.
