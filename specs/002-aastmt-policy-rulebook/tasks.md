# Tasks: AASTMT Policy Rulebook

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-002.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-1, FR-2, FR-3) under tests/acceptance/002-aastmt-policy-rulebook/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-4, FR-5) under tests/acceptance/002-aastmt-policy-rulebook/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-3, FR-7) under tests/acceptance/002-aastmt-policy-rulebook/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-6) under tests/acceptance/002-aastmt-policy-rulebook/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: The system MUST version policy sets by effective dates and academic
  scope.
- FR-2: Approved rules MUST cover registration window, standing, holds, load,
  prerequisites, earned credits, repeats, and conflict/capacity product rules.
- FR-3: Every decision MUST return reason code, explanation, policy version,
  input summary, and source.
- FR-4: A draft or unapproved policy set MUST NOT govern student submission.
- FR-5: Conflicting sources MUST be resolved by the Registrar/SME before the
  affected rule is published.
- FR-6: New rule behavior MUST use a reviewed typed rule; arbitrary executable
  policy scripts MUST NOT be stored.
- FR-7: Published policy versions MUST be immutable and superseded, not edited.

No task is complete and no implementation file has been created.
