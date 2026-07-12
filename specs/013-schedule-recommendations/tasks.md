# Tasks: Schedule Recommendations

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-013.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-1, FR-2, FR-4) under tests/acceptance/013-schedule-recommendations/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-5) under tests/acceptance/013-schedule-recommendations/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-7) under tests/acceptance/013-schedule-recommendations/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-6, FR-3) under tests/acceptance/013-schedule-recommendations/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.
- [ ] T012 [P] [US5] Add the future failing acceptance test for AC-5 (FR-8) under tests/acceptance/013-schedule-recommendations/.
- [ ] T013 [US5] Implement AC-5 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: The optimizer MUST choose exactly one published viable group per
  selected course.
- FR-2: It MUST enforce all hard meeting, availability, completeness,
  eligibility, credit, and configured travel-buffer constraints.
- FR-3: It MUST order constrained courses first and prune invalid partial
  schedules.
- FR-4: It SHOULD return up to three distinct feasible schedules.
- FR-5: It MUST score results with approved soft preferences and explain score
  components.
- FR-6: It MUST support cancellation and a configured computation time budget.
- FR-7: If no feasible result exists, it MUST return a useful conflict set and
  manual-resolution path.
- FR-8: Final submission MUST revalidate all results; a recommendation does not
  reserve seats.

No task is complete and no implementation file has been created.
