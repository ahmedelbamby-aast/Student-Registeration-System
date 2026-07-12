# Tasks: Architecture and Engineering Principles

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-004.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-2, FR-3, FR-1) under tests/acceptance/004-architecture-engineering-principles/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-2, FR-3) under tests/acceptance/004-architecture-engineering-principles/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-7) under tests/acceptance/004-architecture-engineering-principles/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-4, FR-5, FR-6) under tests/acceptance/004-architecture-engineering-principles/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.
- [ ] T012 [P] [US5] Add the future failing acceptance test for AC-5 (FR-8) under tests/acceptance/004-architecture-engineering-principles/.
- [ ] T013 [US5] Implement AC-5 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: The solution MUST use .NET 10 LTS, ASP.NET Core, Blazor WebAssembly,
  EF Core/LINQ, and SQL Server.
- FR-2: The solution MUST contain IdentityAccess, Academics, Scheduling,
  Registration, and StaffAdministration business modules.
- FR-3: Module internals MUST NOT be referenced across boundaries; interaction
  uses approved interfaces/contracts.
- FR-4: One DbContext/database MUST support atomic registration initially.
- FR-5: API DTOs MUST NOT expose EF entities.
- FR-6: Reads SHOULD use LINQ projection/AsNoTracking; commands use focused
  application services.
- FR-7: Generic repository, microservices, broker, event sourcing, dynamic rule
  DSL, and institution-wide solver MUST NOT be introduced in MVP.
- FR-8: Architectural changes MUST include an ADR and architecture-test update.

No task is complete and no implementation file has been created.
