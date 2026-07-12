# Tasks: Architecture and Engineering Principles

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-004 in specs/004-architecture-engineering-principles/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-001] Validate the consumed upstream requirements, plan, data model, and API contract at specs/001-product-charter-rbac/ and record the accepted versions in specs/004-architecture-engineering-principles/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/004-architecture-engineering-principles/dependency-baseline.md.
- [ ] T004 [GATE] Freeze SPEC-004 requirements, API, data-model, policy approvals, and dependency versions in specs/004-architecture-engineering-principles/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T005 [P] [ENTITY-ModuleBoundary] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical ModuleBoundary ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec004/ModuleBoundarySchemaTests.cs.
- [ ] T006 [ENTITY-ModuleBoundary] [ARTIFACT-OWNER] Deliver the canonical ModuleBoundary model or governed artifact at docs/architecture/module-boundaries.md after T005 fails for the expected reason (depends on T005).
- [ ] T007 [P] [ENTITY-ArchitectureDecision] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical ArchitectureDecision ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec004/ArchitectureDecisionSchemaTests.cs.
- [ ] T008 [ENTITY-ArchitectureDecision] [ARTIFACT-OWNER] Deliver the canonical ArchitectureDecision model or governed artifact at docs/adr/README.md after T007 fails for the expected reason (depends on T007).
- [ ] T009 [P] [ENTITY-DependencyRule] [OWNER-SPEC-004] Create the future failing invariant/schema/serialization checks for canonical DependencyRule ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec004/DependencyRuleModelTests.cs.
- [ ] T010 [ENTITY-DependencyRule] [OWNER-SPEC-004] Deliver the canonical DependencyRule model or governed artifact at tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs after T009 fails for the expected reason (depends on T009).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Forbidden dependency (FR-2, FR-3, NFR-1) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [ ] T011 [P] [AC-1] [FR-2] [FR-3] [NFR-1] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-1Tests.cs for AC-1: Forbidden dependency (FR-2, FR-3, NFR-1): Given Academics has an internal implementation type When Registration directly references that type Then the architecture test fails the build.
### US2 - Horizontal instance (NFR-2, NFR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [ ] T012 [P] [AC-2] [NFR-2] [NFR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-2Tests.cs for AC-2: Horizontal instance (NFR-2, NFR-3): Given two application instances share SQL and Data Protection keys When an authenticated user sends consecutive requests to different instances Then authorization and plan state remain correct.
### US3 - Complexity gate (FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [ ] T013 [P] [AC-3] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-3Tests.cs for AC-3: Complexity gate (FR-7): Given a proposal adds a message broker before a durable external consumer exists When architecture review occurs Then the proposal is rejected or moved to a separately approved ADR/spec.
### US4 - Persistence and DTO boundary (FR-4, FR-5, FR-6) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [ ] T014 [P] [AC-4] [FR-4] [FR-5] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-4Tests.cs for AC-4: Persistence and DTO boundary (FR-4, FR-5, FR-6): Given an offering read and an atomic registration command When architecture/code review runs Then both use the single approved DbContext transaction boundary where needed And the read uses a projected DTO rather than exposing an EF entity.
### US5 - Architecture change governance (FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [ ] T015 [P] [AC-5] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-5Tests.cs for AC-5: Architecture change governance (FR-8): Given a pull request changes a module dependency or deployment decision When CI and review run Then an approved ADR and updated architecture test are required.
### US6 - Required stack and domain purity (FR-1, NFR-4) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [ ] T016 [P] [AC-6] [FR-1] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-6Tests.cs for AC-6: Required stack and domain purity (FR-1, NFR-4): Given the solution manifest and compiled dependency graph When architecture conformance tests execute Then the solution uses the approved .NET/ASP.NET Core/Blazor/EF Core/SQL Server stack And Domain projects reference none of ASP.NET, Blazor, EF Core, or SQL Server.
- [ ] T017 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-1Tests.cs and assert: A module needs an additional read -> add a narrow query interface, not direct table ownership leakage.
- [ ] T018 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-2Tests.cs and assert: Cross-module transaction emerges -> keep it in the single DbContext or stop for an architectural review.
- [ ] T019 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-3Tests.cs and assert: A read cache is stale -> final registration never trusts it.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T020 [P] [FR-1] [WORKSTREAM-APPROVED-SOLUTION-STACK] Create the future failing FR-1 checks in tests/StudentRegistration.ArchitectureTests/ApprovedStackTests.cs. Test focus: pinned .NET, ASP.NET Core, Blazor WebAssembly, EF Core, LINQ and SQL Server references. Prove the requirement against its linked AC/EC fixtures: The solution MUST use .NET 10 LTS, ASP.NET Core, Blazor WebAssembly, EF Core/LINQ, and SQL Server.
- [ ] T021 [FR-1] [WORKSTREAM-APPROVED-SOLUTION-STACK] Deliver FR-1 through the bounded Approved solution stack workstream at global.json only after T020 fails for the expected reason (depends on T020): The solution MUST use .NET 10 LTS, ASP.NET Core, Blazor WebAssembly, EF Core/LINQ, and SQL Server.
- [ ] T022 [P] [FR-2] [WORKSTREAM-MODULE-BOUNDARIES] Create the future failing FR-2 checks in tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs. Test focus: declared modules, allowed interfaces, no internals crossing and no cycles. Prove the requirement against its linked AC/EC fixtures: The solution MUST contain IdentityAccess, Academics, Scheduling, Registration, and StaffAdministration business modules.
- [ ] T023 [FR-2] [WORKSTREAM-MODULE-BOUNDARIES] Deliver FR-2 through the bounded Module boundaries workstream at src/StudentRegistration.Server/Architecture/ModuleRegistration.cs only after T022 fails for the expected reason (depends on T022): The solution MUST contain IdentityAccess, Academics, Scheduling, Registration, and StaffAdministration business modules.
- [ ] T024 [P] [FR-3] [WORKSTREAM-MODULE-BOUNDARIES] Create the future failing FR-3 checks in tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs. Test focus: declared modules, allowed interfaces, no internals crossing and no cycles. Prove the requirement against its linked AC/EC fixtures: Module internals MUST NOT be referenced across boundaries; interaction uses approved interfaces/contracts.
- [ ] T025 [FR-3] [WORKSTREAM-MODULE-BOUNDARIES] Deliver FR-3 through the bounded Module boundaries workstream at src/StudentRegistration.Server/Architecture/ModuleRegistration.cs only after T024 fails for the expected reason (depends on T024): Module internals MUST NOT be referenced across boundaries; interaction uses approved interfaces/contracts.
- [ ] T026 [P] [FR-4] [WORKSTREAM-PERSISTENCE-AND-QUERY-BOUNDARY] Create the future failing FR-4 checks in tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs. Test focus: single atomic context, projections, AsNoTracking reads and focused transactions. Prove the requirement against its linked AC/EC fixtures: One DbContext/database MUST support atomic registration initially.
- [ ] T027 [FR-4] [WORKSTREAM-PERSISTENCE-AND-QUERY-BOUNDARY] Deliver FR-4 through the bounded Persistence and query boundary workstream at src/StudentRegistration.Infrastructure/Persistence/StudentRegistrationDbContext.cs only after T026 fails for the expected reason (depends on T026): One DbContext/database MUST support atomic registration initially.
- [ ] T028 [P] [FR-5] [WORKSTREAM-DTO-ISOLATION] Create the future failing FR-5 checks in tests/StudentRegistration.ArchitectureTests/DtoIsolationTests.cs. Test focus: API contracts never expose EF entities or persistence navigation graphs. Prove the requirement against its linked AC/EC fixtures: API DTOs MUST NOT expose EF entities.
- [ ] T029 [FR-5] [WORKSTREAM-DTO-ISOLATION] Deliver FR-5 through the bounded DTO isolation workstream at src/StudentRegistration.Contracts/SharedContracts.cs only after T028 fails for the expected reason (depends on T028): API DTOs MUST NOT expose EF entities.
- [ ] T030 [P] [FR-6] [WORKSTREAM-PERSISTENCE-AND-QUERY-BOUNDARY] Create the future failing FR-6 checks in tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs. Test focus: single atomic context, projections, AsNoTracking reads and focused transactions. Prove the requirement against its linked AC/EC fixtures: Reads SHOULD use LINQ projection/AsNoTracking; commands use focused application services.
- [ ] T031 [FR-6] [WORKSTREAM-PERSISTENCE-AND-QUERY-BOUNDARY] Deliver FR-6 through the bounded Persistence and query boundary workstream at src/StudentRegistration.Infrastructure/Persistence/StudentRegistrationDbContext.cs only after T030 fails for the expected reason (depends on T030): Reads SHOULD use LINQ projection/AsNoTracking; commands use focused application services.
- [ ] T032 [P] [FR-7] [WORKSTREAM-COMPLEXITY-AND-ADR-GOVERNANCE] Create the future failing FR-7 checks in tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs. Test focus: rejected infrastructure remains absent and every boundary change has ADR plus test update. Prove the requirement against its linked AC/EC fixtures: Generic repository, microservices, broker, event sourcing, dynamic rule DSL, and institution-wide solver MUST NOT be introduced in MVP.
- [ ] T033 [FR-7] [WORKSTREAM-COMPLEXITY-AND-ADR-GOVERNANCE] Deliver FR-7 through the bounded Complexity and ADR governance workstream at docs/adr/README.md only after T032 fails for the expected reason (depends on T032): Generic repository, microservices, broker, event sourcing, dynamic rule DSL, and institution-wide solver MUST NOT be introduced in MVP.
- [ ] T034 [P] [FR-8] [WORKSTREAM-COMPLEXITY-AND-ADR-GOVERNANCE] Create the future failing FR-8 checks in tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs. Test focus: rejected infrastructure remains absent and every boundary change has ADR plus test update. Prove the requirement against its linked AC/EC fixtures: Architectural changes MUST include an ADR and architecture-test update.
- [ ] T035 [FR-8] [WORKSTREAM-COMPLEXITY-AND-ADR-GOVERNANCE] Deliver FR-8 through the bounded Complexity and ADR governance workstream at docs/adr/README.md only after T034 fails for the expected reason (depends on T034): Architectural changes MUST include an ADR and architecture-test update.

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T036 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-004-NFR-1.md: Architecture tests MUST fail on forbidden module references/cycles.
- [ ] T037 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-004-NFR-2.md: Application instances MUST be stateless except for shared database and approved key/config stores.
- [ ] T038 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-004-NFR-3.md: The architecture MUST support at least two application replicas.
- [ ] T039 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-004-NFR-4.md: Domain projects MUST have no dependency on ASP.NET, Blazor, EF, or SQL.

## Phase 7 - Scope and Release Evidence

- [ ] T040 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-004-scope-review.md that OS-1 remains excluded: Independent module deployments in MVP.
- [ ] T041 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-004-scope-review.md that OS-2 remains excluded: Kubernetes and service mesh.
- [ ] T042 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-004-scope-review.md that OS-3 remains excluded: Separate read/write databases.
- [ ] T043 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-004-scope-review.md that OS-4 remains excluded: Distributed transaction protocol.
- [ ] T044 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-004-traceability.md and reject release if any row lacks passing evidence.
- [ ] T045 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-004 in docs/release-evidence/SPEC-004-release-approval.md.

No task is complete and no implementation file has been created.
