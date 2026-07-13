# Tasks: Architecture and Engineering Principles

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Dependency, Consistency, Readiness, and Final Approval Gates

- [ ] T001 [DEP-SPEC-001] Validate the consumed upstream requirements, plan, data model, and API contract at specs/001-product-charter-rbac/ and record the accepted versions in specs/004-architecture-engineering-principles/dependency-baseline.md.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/004-architecture-engineering-principles/dependency-baseline.md.
- [ ] T003 [GATE] Run cross-spec consistency analysis for SPEC-004; verify requirement/acceptance/success-criterion traceability, truthful artifact/runtime ownership, exact architecture paths, endpoint/route contracts, acyclic dependencies, task ordering, and planning-only status; record findings and resolutions in specs/004-architecture-engineering-principles/checklists/consistency-analysis.md.
- [ ] T004 [GATE] After T003 passes, freeze the SPEC-004 requirements, data/API/design contracts, institutional decision states, dependency versions, and executable task baseline in specs/004-architecture-engineering-principles/checklists/implementation-readiness.md.
- [ ] T005 [GATE] After T004 passes, record the accountable owner and Ahmed ELbamby's human approval for SPEC-004 in specs/004-architecture-engineering-principles/checklists/approval.md as the final planning gate; no test, source, migration, or other implementation task may execute before this approval.

## Phase 2 - Models and API Contracts

- [ ] T006 [ENTITY-ModuleBoundary] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical ModuleBoundary ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec004/ModuleBoundarySchemaTests.cs.
- [ ] T007 [ENTITY-ModuleBoundary] [ARTIFACT-OWNER] Record the approved ModuleBoundary schema/version at specs/004-architecture-engineering-principles/schemas/module-boundary.schema.json after T006 fails for the expected reason (depends on T006); canonical publication is deferred until module-boundary behavior tests fail as expected.
- [ ] T008 [ENTITY-ArchitectureDecision] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical ArchitectureDecision ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec004/ArchitectureDecisionSchemaTests.cs.
- [ ] T009 [ENTITY-ArchitectureDecision] [ARTIFACT-OWNER] Record the approved ArchitectureDecision schema/version at specs/004-architecture-engineering-principles/schemas/architecture-decision.schema.json after T008 fails for the expected reason (depends on T008); canonical ADR publication is deferred until governance behavior tests fail as expected.
- [ ] T010 [ENTITY-DependencyRule] [ENTITY-AuditEvent] [ENTITY-AuditWritePort] [OWNER-SPEC-004] Create the future failing DependencyRule, append-only AuditEvent, and transaction-aware AuditWritePort model/contract checks in tests/StudentRegistration.IntegrationTests/Specs/Spec004/DependencyRuleModelTests.cs, tests/StudentRegistration.IntegrationTests/Specs/Spec004/AuditEventModelTests.cs, and tests/StudentRegistration.ContractTests/Shared/AuditWritePortTests.cs.
- [ ] T011 [ENTITY-DependencyRule] [ENTITY-AuditEvent] [ENTITY-AuditWritePort] [OWNER-SPEC-004] Deliver the canonical DependencyRule at tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs, canonical AuditEvent at src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditEvent.cs, and canonical AuditWritePort at src/StudentRegistration.Contracts/Auditing/IAuditEventWriter.cs after T010 fails (depends on T010).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Forbidden dependency (FR-2, FR-3, NFR-1) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T011.
- [ ] T012 [AC-1] [FR-2] [FR-3] [NFR-1] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-1Tests.cs for AC-1: Forbidden dependency (FR-2, FR-3, NFR-1): Given Academics has an internal implementation type When Registration directly references that type Then the architecture test fails the build.
### US2 - Horizontal instance (NFR-2, NFR-3) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T011.
- [ ] T013 [AC-2] [NFR-2] [NFR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-2Tests.cs for AC-2: Horizontal instance (NFR-2, NFR-3): Given two application instances share SQL and Data Protection keys When an authenticated user sends consecutive requests to different instances Then authorization and plan state remain correct.
### US3 - Complexity gate (FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T011.
- [ ] T014 [AC-3] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-3Tests.cs for AC-3: Complexity gate (FR-7): Given a proposal adds a message broker before a durable external consumer exists When architecture review occurs Then the proposal is rejected or moved to a separately approved ADR/spec.
### US4 - Persistence and DTO boundary (FR-4, FR-5, FR-6) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T011.
- [ ] T015 [AC-4] [FR-4] [FR-5] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-4Tests.cs for AC-4: Persistence and DTO boundary (FR-4, FR-5, FR-6): Given an offering read and an atomic registration command When architecture/code review runs Then both use the single approved DbContext transaction boundary where needed And the read uses a projected DTO rather than exposing an EF entity.
### US5 - Architecture change governance (FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T011.
- [ ] T016 [AC-5] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-5Tests.cs for AC-5: Architecture change governance (FR-8): Given a pull request changes a module dependency or deployment decision When CI and review run Then an approved ADR and updated architecture test are required.
### US6 - Required stack and domain purity (FR-1, NFR-4) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Architecture and Engineering Principles.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T011.
- [ ] T017 [AC-6] [FR-1] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-6Tests.cs for the approved stack, exact project-per-business-module solution shape, absence of generic Server/Domain/Application/Infrastructure projects, and framework-free business-module Domain code.
- [ ] T018 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-1Tests.cs and assert: A module needs an additional read -> add a narrow query interface, not direct table ownership leakage.
- [ ] T019 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-2Tests.cs and assert: Cross-module transaction emerges -> keep it in the single DbContext or stop for an architectural review.
- [ ] T020 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-3Tests.cs and assert: A read cache is stale -> final registration never trusts it.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T021 [FR-1] [WORKSTREAM-APPROVED-SOLUTION-STACK] Create the future failing FR-1 checks in tests/StudentRegistration.ArchitectureTests/ApprovedStackTests.cs. Test focus: pinned .NET, ASP.NET Core, Blazor WebAssembly, EF Core, LINQ and SQL Server references. Prove the requirement against its linked AC/EC fixtures: The solution MUST use .NET 10 LTS, ASP.NET Core, Blazor WebAssembly, EF Core/LINQ, and SQL Server.
- [ ] T022 [FR-1] [WORKSTREAM-APPROVED-SOLUTION-STACK] Deliver FR-1 through the bounded Approved solution stack workstream at global.json only after T021 fails for the expected reason (depends on T021): The solution MUST use .NET 10 LTS, ASP.NET Core, Blazor WebAssembly, EF Core/LINQ, and SQL Server.
- [ ] T023 [FR-2] [WORKSTREAM-MODULE-BOUNDARIES] Create the future failing exact project-shape checks in tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs. Test focus: declared business-module projects, allowed interfaces, no internals crossing and no cycles.
- [ ] T024 [FR-2] [WORKSTREAM-MODULE-BOUNDARIES] Create the exact Client, Api, Contracts, IdentityAccess, Academics, Scheduling, Registration, StaffAdministration, and Infrastructure.SqlServer project composition at src/StudentRegistration.Api/Composition/ModuleRegistration.cs only after T023 fails for the expected reason (depends on T023); do not create generic Server, Domain, Application, or Infrastructure projects.
- [ ] T025 [FR-3] [WORKSTREAM-MODULE-BOUNDARIES] Create the future failing FR-3 checks in tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs. Test focus: declared business-module projects, allowed interfaces, no internals crossing and no cycles. Prove the requirement against its linked AC/EC fixtures.
- [ ] T026 [FR-2] [FR-3] [ENTITY-ModuleBoundary] [WORKSTREAM-MODULE-BOUNDARIES] Deliver the bounded Module boundaries workstream and publish the canonical ModuleBoundary at docs/architecture/module-boundaries.md only after T023 and T025 fail for their expected reasons (depends on T023, T025); T024 remains the single ModuleRegistration source writer.
- [ ] T027 [FR-4] [WORKSTREAM-PERSISTENCE-AND-QUERY-BOUNDARY] Create the future failing Infrastructure.SqlServer boundary checks in tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs. Test focus: single Infrastructure.SqlServer atomic context, projections, AsNoTracking reads and focused transactions.
- [ ] T028 [FR-4] [WORKSTREAM-PERSISTENCE-AND-QUERY-BOUNDARY] Create the single composition DbContext owned by Infrastructure.SqlServer at src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs only after T027 fails for the expected reason (depends on T027); owner feature specs contribute mappings and no other spec writes this file.
- [ ] T029 [FR-5] [WORKSTREAM-DTO-ISOLATION] Create the future failing FR-5 checks in tests/StudentRegistration.ArchitectureTests/DtoIsolationTests.cs. Test focus: API contracts never expose EF entities or persistence navigation graphs. Prove the requirement against its linked AC/EC fixtures: API DTOs MUST NOT expose EF entities.
- [ ] T030 [FR-5] [WORKSTREAM-DTO-ISOLATION] Deliver FR-5 through the bounded DTO isolation workstream at src/StudentRegistration.Contracts/SharedContracts.cs only after T029 fails for the expected reason (depends on T029): API DTOs MUST NOT expose EF entities.
- [ ] T031 [FR-6] [WORKSTREAM-PERSISTENCE-AND-QUERY-BOUNDARY] Create the future failing FR-6 checks in tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs. Test focus: single Infrastructure.SqlServer atomic context, projections, AsNoTracking reads and focused transactions. Prove the requirement against its linked AC/EC fixtures.
- [ ] T032 [FR-4] [FR-6] [WORKSTREAM-PERSISTENCE-AND-QUERY-BOUNDARY] Deliver the bounded Persistence and query boundary workstream at docs/architecture/persistence-boundary.md only after T027 and T031 fail for their expected reasons (depends on T027, T031); T028 remains the single DbContext source writer.
- [ ] T033 [FR-7] [WORKSTREAM-COMPLEXITY-AND-ADR-GOVERNANCE] Create the future failing FR-7 checks in tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs. Test focus: rejected infrastructure remains absent and every boundary change has ADR plus test update. Prove the requirement against its linked AC/EC fixtures: Generic repository, microservices, broker, event sourcing, dynamic rule DSL, and institution-wide solver MUST NOT be introduced in MVP.
- [ ] T034 [FR-7] [WORKSTREAM-COMPLEXITY-AND-ADR-GOVERNANCE] Publish the prohibited-complexity conformance record at specs/004-architecture-engineering-principles/contracts/prohibited-complexity.md only after T033 fails for the expected reason (depends on T033).
- [ ] T035 [FR-8] [WORKSTREAM-COMPLEXITY-AND-ADR-GOVERNANCE] Create the future failing FR-8 checks in tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs. Test focus: rejected infrastructure remains absent and every boundary change has ADR plus test update. Prove the requirement against its linked AC/EC fixtures: Architectural changes MUST include an ADR and architecture-test update.
- [ ] T036 [FR-7] [FR-8] [ENTITY-ArchitectureDecision] [WORKSTREAM-COMPLEXITY-AND-ADR-GOVERNANCE] Deliver the bounded Complexity and ADR governance workstream and publish the canonical ArchitectureDecision registry at docs/adr/README.md only after T033 and T035 fail for their expected reasons (depends on T033, T035).

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T037 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-004-NFR-1.md: Architecture tests MUST fail on forbidden module references/cycles.
- [ ] T038 [NFR-2] [AUTOMATED-EVIDENCE] Produce two-replica, cross-instance authentication/plan-state evidence without sticky sessions and verify approved encrypted-at-rest, least-privilege, rotated shared Data Protection keys in tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-004-NFR-2.md; fail closed when the production repository/encryption authority lacks Security/DevOps approval.
- [ ] T039 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-004-NFR-3.md: The architecture MUST support at least two application replicas.
- [ ] T040 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence in tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-004-NFR-4.md that business-module Domain code references no ASP.NET, Blazor, EF Core, or SQL Server type/namespace.

## Phase 7 - Scope and Release Evidence

- [ ] T041 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-004-scope-review.md that OS-1 remains excluded: Independent module deployments in MVP.
- [ ] T042 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-004-scope-review.md that OS-2 remains excluded: Kubernetes and service mesh.
- [ ] T043 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-004-scope-review.md that OS-3 remains excluded: Separate read/write databases.
- [ ] T044 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-004-scope-review.md that OS-4 remains excluded: Distributed transaction protocol.
- [ ] T045 [AC-7] [FR-9] [ENTITY-AuditEvent] [ENTITY-AuditWritePort] [PERSISTENCE-MAPPING] [WORKSTREAM-ATOMIC-AUDIT-FOUNDATION] Create the future failing same-transaction success/fault/rollback and no-SPEC-017-dependency suite in tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs plus append-only mapping/model parity checks in tests/StudentRegistration.IntegrationTests/Persistence/AuditEventPersistenceTests.cs.
- [ ] T046 [FR-9] [ENTITY-AuditEvent] [ENTITY-AuditWritePort] [PERSISTENCE-MAPPING] [WORKSTREAM-ATOMIC-AUDIT-FOUNDATION] Deliver the shared transaction-aware writer at src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditTransactionWriter.cs and its EF configuration at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AuditEventModelConfiguration.cs only after T045 fails; it joins but never commits the caller transaction (depends on T045).
- [ ] T047 [TRACE] [SC-1] [SC-2] [SC-3] Generate the completed FR/NFR/AC/EC/SC/route-to-test evidence matrix at docs/release-evidence/SPEC-004-traceability.md and reject release if any row lacks passing evidence, including exact project-shape, atomic-audit, and single-writer DbContext proof.
- [ ] T048 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-004 in docs/release-evidence/SPEC-004-release-approval.md.

No task is complete and no implementation file has been created.
