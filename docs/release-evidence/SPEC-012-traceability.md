# SPEC-012 Complete Traceability Evidence

**Requirement set:** SPEC-012 Schedule Builder and Conflicts  
**Task:** T053  
**Recorded:** 2026-07-16  
**Scope:** Requirement-to-task-to-delivered-artifact evidence only

## Evidence boundary

This matrix traces the approved SPEC-012 requirements to the current repository
implementation and executable evidence. The normative sources are
`specs/012-schedule-builder-conflicts/spec.md`,
`specs/012-schedule-builder-conflicts/requirements.md`,
`specs/012-schedule-builder-conflicts/tasks.md`,
`specs/012-schedule-builder-conflicts/dependency-baseline.md`, and
`specs/012-schedule-builder-conflicts/contracts/api.md`.

This document does not change task checkboxes. Their state remains authoritative
in `specs/012-schedule-builder-conflicts/tasks.md`. Gate B-D and production
release approval remains separate. T054 is not claimed, and this evidence does
not represent official AASTMT authorization.

## Functional requirements

| ID | Required behavior | Acceptance / outcome link | Tasks | Delivered implementation | Executable evidence |
|---|---|---|---|---|---|
| FR-1 | At most one group per offering; complete replacement rejects duplicates. | AC-4 | T007, T008, T014, T020, T029, T032, T033, T036, T040, T042, T043 | `src/StudentRegistration.Registration/Domain/RegistrationPlan.cs`; `src/StudentRegistration.Registration/Domain/RegistrationPlanItem.cs`; `src/StudentRegistration.Registration/Application/RegistrationPlanService.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-4Tests.cs`; `tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs`; `tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint02ContractTests.cs`; `tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs` |
| FR-2 | Detect every meeting overlap with strict half-open intervals; adjacency and disabled travel buffer do not conflict. | AC-1, AC-2, SC-1, EC-1, EC-4 | T017, T018, T022, T025, T026, T030, T037, T042, T043 | `src/StudentRegistration.Registration/Domain/ScheduleConflictDetector.cs` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-1Tests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-2Tests.cs`; `tests/StudentRegistration.ApplicationTests/Registration/ScheduleConflictDetectorTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-1Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-4Tests.cs` |
| FR-3 | Conflict contains both groups/subjects, local intervals, exact overlap, safe text, and change/remove actions. | AC-1, SC-1 | T009, T017, T026, T030, T034, T037, T042, T043 | `src/StudentRegistration.Registration/Domain/ScheduleConflict.cs`; `src/StudentRegistration.Registration/Domain/ScheduleConflictDetector.cs`; `src/StudentRegistration.Contracts/Registration/RegistrationPlanContracts.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/ScheduleConflictModelTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-1Tests.cs`; `tests/StudentRegistration.ApplicationTests/Registration/ScheduleConflictDetectorTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-1OutcomeTests.cs` |
| FR-4 | UI renders red X plus visible and icon-accessible conflict meaning. | AC-1 | T017, T031, T038, T042, T043 | `src/StudentRegistration.Client/Features/Scheduling/ConflictStateMapper.cs`; `src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-1Tests.cs`; `tests/StudentRegistration.Client.UnitTests/Scheduling/ConflictStateMapperTests.cs`; `tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs` |
| FR-5 | Hard conflicts block Review/submission and expose accessible recovery. | AC-3, SC-2 | T019, T027, T031, T038, T044, T045 | `src/StudentRegistration.Client/Features/Scheduling/ConflictStateMapper.cs`; `specs/012-schedule-builder-conflicts/contracts/routes/STU-05.md` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-3Tests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-2OutcomeTests.cs`; `tests/StudentRegistration.Client.UnitTests/Scheduling/ConflictStateMapperTests.cs`; `tests/StudentRegistration.E2ETests/Specs/Spec012/RegistrationReviewPageContributorTests.cs` |
| FR-6 | Change/remove recalculates plan; every response uses server-authored target 18, maximum 18, and sourced load reasons with no GPA/overload branch. | AC-4 | T012, T014, T016, T020, T029, T036, T039, T040, T041, T042, T043 | `src/StudentRegistration.Registration/Application/RegistrationPlanService.cs`; `src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs`; `src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor` | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-4Tests.cs`; `tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs`; all three endpoint contract tests; `tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs` |
| FR-7 | One owner/term plan persists with rowversion; authenticated identity is resolved server-side and direct-object access returns no data. | AC-4, EC-2 | T007, T012, T014, T016, T020, T023, T029, T032, T036, T039, T040, T041, T042, T043, T044, T045, T046, T047 | `src/StudentRegistration.Registration/Domain/RegistrationPlan.cs`; `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationPlanSqlServerAdapter.cs`; `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationPlanModelConfiguration.cs`; `src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelConfigurationTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-2Tests.cs`; endpoint contract tests; concurrency tests |
| FR-8 | Capacity is advisory; validation is timestamped/versioned, non-mutating, identifies changed/unavailable selections, and blocks review without reserving seats. | AC-4, EC-3 | T010, T012, T014, T016, T020, T024, T029, T035, T036, T039, T040, T041, T042, T043, T044, T045 | `src/StudentRegistration.Registration/Domain/ValidationSnapshot.cs`; `src/StudentRegistration.Registration/Application/RegistrationPlanService.cs`; `src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationPlanSqlServerAdapter.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/ValidationSnapshotModelTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-3Tests.cs`; `tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint03ContractTests.cs`; concurrency and route tests |

## Non-functional requirements

| ID | Quality gate | Tasks | Evidence test | Recorded evidence |
|---|---|---|---|---|
| NFR-1 | Eight-course, ten-slot conflict recalculation p95 is at most 200 ms. | T021, T048 | `tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-1EvidenceTests.cs` | `docs/release-evidence/SPEC-012-NFR-1.md` |
| NFR-2 | Fixed input produces deterministic conflict output. | T021, T049 | `tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-2EvidenceTests.cs` | `docs/release-evidence/SPEC-012-NFR-2.md` |
| NFR-3 | Calendar and chronological list contain equivalent semantics. | T021, T028, T042, T043, T050 | `tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-3EvidenceTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-3OutcomeTests.cs` | `docs/release-evidence/SPEC-012-NFR-3.md` |
| NFR-4 | Two editors cannot lose an update; stale editor receives current plan through HTTP 409. | T021, T023, T029, T040, T051 | `tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-4EvidenceTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelConfigurationTests.cs` | `docs/release-evidence/SPEC-012-NFR-4.md` |

## Acceptance criteria

| ID | Requirement links | Primary tasks | Direct evidence | Delivery path |
|---|---|---|---|---|
| AC-1 | FR-2, FR-3, FR-4 | T017, T030, T031, T037, T038, T042, T043 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-1Tests.cs` | Detector -> server conflict DTO -> accessible mapper/page |
| AC-2 | FR-2 | T018, T030, T037, T042, T043 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-2Tests.cs` | Half-open detector and disabled travel-buffer rule |
| AC-3 | FR-5 | T019, T027, T031, T038, T044, T045 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-3Tests.cs` | Blocking conflict state and STU-05 contributor contract |
| AC-4 | FR-1, FR-6, FR-7, FR-8 | T020, T029, T036, T039, T040, T041, T042, T043 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-4Tests.cs` | Versioned owner/term service, three endpoints, schedule page |
| AC-5 | NFR-1, NFR-2, NFR-3, NFR-4 | T021, T048, T049, T050, T051 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/AC-5Tests.cs` | Four bounded NFR evidence suites and reports |

## Edge cases

| ID | Expected behavior | Tasks | Direct evidence | Supporting implementation |
|---|---|---|---|---|
| EC-1 | One conflicting slot makes a multi-slot group pair conflict. | T022, T030, T037 | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-1Tests.cs` | `src/StudentRegistration.Registration/Domain/ScheduleConflictDetector.cs` |
| EC-2 | Stale rowversion rejects the write and returns current owner plan. | T023, T029, T036, T040, T051 | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-2Tests.cs` | SQL adapter, service, endpoint, and NFR-4 evidence |
| EC-3 | Full/unpublished/closed/cancelled selection blocks review with actions and no substitution. | T024, T029, T036, T041 | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-3Tests.cs` | Service selection issues and non-mutating validate endpoint |
| EC-4 | Duplicate meeting identity is defensively de-duplicated. | T025, T030, T037 | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/EdgeCases/EC-4Tests.cs` | `src/StudentRegistration.Registration/Domain/ScheduleConflictDetector.cs` |

## Success criteria

| ID | Measurable outcome | Tasks | Direct evidence | Requirement coverage |
|---|---|---|---|---|
| SC-1 | Every overlap is identified before Review. | T026, T030, T037 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-1OutcomeTests.cs` | FR-2, FR-3; AC-1; EC-1, EC-4 |
| SC-2 | Every unresolved hard conflict blocks with accessible guidance. | T027, T031, T038, T044, T045 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-2OutcomeTests.cs` | FR-4, FR-5; AC-3 |
| SC-3 | Calendar and list expose equivalent schedule information. | T028, T042, T043, T050 | `tests/StudentRegistration.AcceptanceTests/Specs/Spec012/SC-3OutcomeTests.cs` | NFR-3; AC-5; STU-04 |

## Frontend routes

| Route ID | Ownership and contribution | Tasks | Delivered artifact | Evidence |
|---|---|---|---|---|
| STU-04 | SPEC-012 implements `/student/schedule`; it presents the server plan, calendar/list, conflicts, fixed 18/18 load state, and recovery actions. | T001, T042, T043 | `src/StudentRegistration.Client/Pages/ScheduleBuilderPage.razor` | `tests/StudentRegistration.E2ETests/Specs/Spec012/ScheduleBuilderPageFeatureTests.cs` |
| STU-05 | SPEC-012 contributes validated blockers/actions/snapshot to `/student/review`; SPEC-014 retains canonical page and submission ownership. | T001, T044, T045 | `specs/012-schedule-builder-conflicts/contracts/routes/STU-05.md` | `tests/StudentRegistration.E2ETests/Specs/Spec012/RegistrationReviewPageContributorTests.cs` |

## Feature-owned entities

| Entity ID | Tasks | Domain artifact | Persistence / evidence |
|---|---|---|---|
| ENTITY-RegistrationPlan | T007, T032, T046, T047 | `src/StudentRegistration.Registration/Domain/RegistrationPlan.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelConfigurationTests.cs` |
| ENTITY-RegistrationPlanItem | T008, T033, T046, T047 | `src/StudentRegistration.Registration/Domain/RegistrationPlanItem.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanItemModelTests.cs`; parent-owned EF mapping |
| ENTITY-ScheduleConflict | T009, T034, T046, T047 | `src/StudentRegistration.Registration/Domain/ScheduleConflict.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/ScheduleConflictModelTests.cs`; JSON persistence mapping |
| ENTITY-ValidationSnapshot | T010, T035, T046, T047 | `src/StudentRegistration.Registration/Domain/ValidationSnapshot.cs` | `tests/StudentRegistration.IntegrationTests/Specs/Spec012/ValidationSnapshotModelTests.cs`; JSON persistence mapping |

Shared persistence is implemented by
`src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationPlanModelConfiguration.cs`
and the production port adapter is
`src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationPlanSqlServerAdapter.cs`.

## API endpoints

| Endpoint ID | Route and semantics | Contract/delivery tasks | Handler and DTO artifacts | Contract evidence |
|---|---|---|---|---|
| API-Endpoint01 | `GET /api/student/terms/{termId}/registration-plan`; owner/term current or empty plan, no seat mutation. | T011, T012, T039 | `src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs`; `src/StudentRegistration.Contracts/Registration/RegistrationPlanContracts.cs` | `tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint01ContractTests.cs` |
| API-Endpoint02 | `PUT /api/student/terms/{termId}/registration-plan`; atomic complete replacement with expected rowversion and stale-current 409. | T013, T014, T040 | same endpoint/DTO artifacts plus SQL adapter | `tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint02ContractTests.cs` |
| API-Endpoint03 | `POST /api/student/terms/{termId}/registration-plan/validate`; no body, non-mutating fresh validation, no seat mutation. | T015, T016, T041 | same endpoint/DTO artifacts plus service/context port | `tests/StudentRegistration.ContractTests/Specs/Spec012/Endpoint03ContractTests.cs` |

## Task-to-evidence index

This index proves that every task through the traceability task has a named
planning, test, delivery, or evidence artifact. Checkbox state is not inferred
from this table.

| Tasks | Evidence boundary |
|---|---|
| T001, T002, T003, T004 | Dependency evidence is recorded in `specs/012-schedule-builder-conflicts/dependency-baseline.md`. |
| T005, T006 | Readiness and Gate A records are under `specs/012-schedule-builder-conflicts/checklists/`; later release gates remain separate. |
| T007, T008, T009, T010 | Four entity model suites map to the four domain artifacts listed above. |
| T011, T013, T015 | Three finalized endpoint contracts are in `specs/012-schedule-builder-conflicts/contracts/api.md`. |
| T012, T014, T016 | Endpoint 01/02/03 contract tests are mapped in the API table. |
| T017, T018, T019, T020, T021 | AC-1 through AC-5 acceptance suites are mapped in the acceptance table. |
| T022, T023, T024, T025 | EC-1 through EC-4 integration suites are mapped in the edge-case table. |
| T026, T027, T028 | SC-1 through SC-3 outcome suites are mapped in the success-criteria table. |
| T029 | Consolidated plan behavior is verified by `tests/StudentRegistration.IntegrationTests/Registration/RegistrationPlanConcurrencyTests.cs`. |
| T030 | Detector behavior is verified by `tests/StudentRegistration.ApplicationTests/Registration/ScheduleConflictDetectorTests.cs`. |
| T031 | Accessible state mapping is verified by `tests/StudentRegistration.Client.UnitTests/Scheduling/ConflictStateMapperTests.cs`. |
| T032, T033, T034, T035 | The four canonical domain artifacts are mapped in the entity table. |
| T036, T037, T038 | Service, detector, and client state delivery artifacts are mapped in the FR tables. |
| T039, T040, T041 | The three delivered handler operations are mapped in the API table. |
| T042, T043 | STU-04 journey and page delivery are mapped in the route table. |
| T044, T045 | STU-05 contribution contract and contributor test are mapped in the route table. |
| T046, T047 | Real-SQL mapping/migration evidence and EF delivery are mapped in the entity section. |
| T048, T049, T050, T051 | NFR-1 through NFR-4 executable suites and dated evidence are mapped in the NFR table. |
| T052 | Scope evidence artifact: `docs/release-evidence/SPEC-012-scope-review.md`; its task state remains owned by the task list. |
| T053 | This trace matrix and its focused quality evidence test provide the requested artifacts; its task state remains owned by the task list. |

## Artifact coverage checked by T053

The focused trace test also requires these delivered artifacts to remain
present and named by this evidence:

- `tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelConfigurationTests.cs`
- `tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-1EvidenceTests.cs`
- `tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-2EvidenceTests.cs`
- `tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-3EvidenceTests.cs`
- `tests/StudentRegistration.QualityTests/Specs/Spec012/NFR-4EvidenceTests.cs`
- `docs/release-evidence/SPEC-012-NFR-1.md`
- `docs/release-evidence/SPEC-012-NFR-2.md`
- `docs/release-evidence/SPEC-012-NFR-3.md`
- `docs/release-evidence/SPEC-012-NFR-4.md`

The traceability test itself is
`tests/StudentRegistration.QualityTests/Specs/Spec012/TraceabilityEvidenceTests.cs`.
It verifies the full identifier inventory, checks every mapped artifact exists,
and enforces the release-approval boundary above.
