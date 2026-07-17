# SPEC-016 Traceability and Passing Evidence

**Generated:** 2026-07-17
**Result:** PASS — every requirement, success criterion, acceptance criterion,
edge case, endpoint, and owned route has named passing automated evidence.

## Functional requirements

| Requirement | Delivery | Passing evidence |
|---|---|---|
| FR-1 shared staff journey | shared StaffApiClient/pages and StaffWorkspaceQueries | AC-2; StaffWorkspaceScopeTests; STF-01 E2E |
| FR-2 server assignment scope | StaffWorkspaceQueries + SqlStaffWorkspaceAdapter | AC-1/AC-2; NFR-2EvidenceTests; authorization 7/7 |
| FR-3 role-specific assignment | active-role Scheduling projection | AC-2; Endpoint01/02 behavior; authorization matrix |
| FR-4 assigned group detail | canonical GroupDto inside StaffAssignmentDto | AC-4; model/contract tests; STF-01/STF-02 E2E |
| FR-5 minimal bounded roster | RosterRowDto + atomic assigned roster reader | AC-1; RosterRowModelTests; Endpoint03 behavior; NFR-3EvidenceTests |
| FR-6 own availability/deadline | StaffAvailabilityFacade + Scheduling port | AC-3/AC-6; endpoint04/05; concurrency tests |
| FR-7 privilege boundary | five exact routes; no Admin/staff admin mutation | AC-4; StaffWorkspaceScopeTests; scope review |
| FR-8 durable schedule impact | serializable SqlStaffAvailabilityPort alert write | AC-5/AC-7; AvailabilityPublicationRaceTests |
| FR-9 canonical complete aggregate | IStaffAvailabilityPort and transaction rules | model/architecture/port tests; concurrency tests |
| FR-10 one local transaction | SQL serializable update/alert/audit commit | AC-6/AC-7; StaffAvailabilityConcurrencyTests |

## Non-functional and success criteria

| Item | Passing evidence |
|---|---|
| NFR-1 <= 300 ms p95 | NFR-1EvidenceTests: 200 samples, p95 0.0154 ms |
| NFR-2 direct-object scope | NFR-2EvidenceTests; StaffWorkspaceScopeTests 7/7 |
| NFR-3 privacy/audit | NFR-3EvidenceTests; exact DTO/model/SQL projection tests |
| NFR-4 keyboard/list-table | NFR-4EvidenceTests; client 10/10; accessibility; E2E |
| SC-1 scoped assignment/roster | AC-1 plus authorization and endpoint tests |
| SC-2 shared journey, separate permissions | AC-2 plus STF-01/STF-02 tests |
| SC-3 no silent class move | AC-5/AC-7 plus publication-race tests |

## Acceptance and edge cases

| Criterion | Passing test |
|---|---|
| AC-1 | `AcceptanceTests/Specs/Spec016/AC-1Tests.cs` |
| AC-2 | `AcceptanceTests/Specs/Spec016/AC-2Tests.cs` |
| AC-3 | `AcceptanceTests/Specs/Spec016/AC-3Tests.cs` |
| AC-4 | `AcceptanceTests/Specs/Spec016/AC-4Tests.cs` |
| AC-5 | `AcceptanceTests/Specs/Spec016/AC-5Tests.cs` |
| AC-6 | `AcceptanceTests/Specs/Spec016/AC-6Tests.cs` |
| AC-7 | `AcceptanceTests/Specs/Spec016/AC-7Tests.cs` |
| AC-8 | `AcceptanceTests/Specs/Spec016/AC-8Tests.cs` |
| EC-1 | `IntegrationTests/Specs/Spec016/EdgeCases/EC-1Tests.cs` |
| EC-2 | `IntegrationTests/Specs/Spec016/EdgeCases/EC-2Tests.cs` |
| EC-3 | `IntegrationTests/Specs/Spec016/EdgeCases/EC-3Tests.cs` |
| EC-4 | `IntegrationTests/Specs/Spec016/EdgeCases/EC-4Tests.cs` |
| EC-5 | `IntegrationTests/Specs/Spec016/EdgeCases/EC-5Tests.cs` |

The focused Acceptance suite passed 8/8 and the edge suite passed 5/5.

## Endpoints and routes

| Contract | Handler/route | Passing evidence |
|---|---|---|
| GET assignments | Spec016Endpoints / STF-01 | Endpoint01 contract/behavior; dashboard component/E2E |
| GET timetable | Spec016Endpoints / STF-02 | Endpoint02 contract/behavior; timetable component/E2E |
| GET roster | Spec016Endpoints / STF-03 | Endpoint03 contract/behavior; roster component/E2E |
| GET availability | Spec016Endpoints / STF-04 | Endpoint04 contract/behavior; availability component/E2E |
| PUT availability | Spec016Endpoints / STF-04 | Endpoint05 contract/behavior; stale/deadline E2E |

Focused passing totals: build 0 warnings/errors; contract 6; application 9;
authorization 7; architecture 3; integration 18; acceptance 8; edge 5;
client unit 10; client route contract 6; accessibility 12; E2E 13; quality 7.

## Dependency-exception closure

The expired readiness exception was limited to SPEC-010 documentation lint.
Implementation and tests consume the exact approved SPEC-010 contract hash,
canonical two-value availability kind, ID/rowversion planning dependency,
Scheduling-owned aggregates, and absent Admin mutation routes. Ahmed Elbamby
performed the required renewed constitutional review on 2026-07-17 and accepts
this bounded closure for SPEC-016 demo evidence only. The unrelated upstream
documentation linter finding remains outside this feature and is not claimed
as globally resolved or production-ready.
