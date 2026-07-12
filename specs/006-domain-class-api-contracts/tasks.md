# Tasks: Domain Classes and API Contracts

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-006.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-3) under tests/acceptance/006-domain-class-api-contracts/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-2) under tests/acceptance/006-domain-class-api-contracts/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-1, FR-2) under tests/acceptance/006-domain-class-api-contracts/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-4, FR-7) under tests/acceptance/006-domain-class-api-contracts/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.
- [ ] T012 [P] [US5] Add the future failing acceptance test for AC-5 (FR-5, FR-6) under tests/acceptance/006-domain-class-api-contracts/.
- [ ] T013 [US5] Implement AC-5 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: Endpoints MUST delegate business decisions to focused application
  services.
- FR-2: Contracts MUST use DTOs/value identifiers and MUST NOT serialize EF
  entities or password/security internals.
- FR-3: Errors MUST use stable machine code, safe message, correlation ID, and
  optional field details.
- FR-4: Mutation endpoints MUST support cancellation and appropriate
  idempotency/concurrency tokens.
- FR-5: Listing endpoints MUST use bounded pagination.
- FR-6: API versioning policy MUST be defined before the first breaking change.
- FR-7: Domain code MUST use TimeProvider abstraction for current time.

No task is complete and no implementation file has been created.
