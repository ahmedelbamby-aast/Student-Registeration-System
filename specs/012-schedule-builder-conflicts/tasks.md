# Tasks: Schedule Builder and Conflicts

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-012.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-2, FR-3, FR-4) under tests/acceptance/012-schedule-builder-conflicts/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-2) under tests/acceptance/012-schedule-builder-conflicts/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-5) under tests/acceptance/012-schedule-builder-conflicts/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-1, FR-6, FR-7, FR-8) under tests/acceptance/012-schedule-builder-conflicts/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: The student MUST add at most one group per course offering to a plan.
- FR-2: The server MUST detect overlap for every meeting slot using strict
  interval logic.
- FR-3: Each conflict MUST identify both groups, subjects, day, times, and
  resolution links.
- FR-4: The UI MUST render a red X plus text/icon-accessible conflict state.
- FR-5: Review/submission MUST be blocked while any hard conflict exists.
- FR-6: Students MUST be able to change/remove groups and see recalculated
  credits/conflicts.
- FR-7: Plans MUST persist server-side and use rowversion.
- FR-8: Capacity displayed in a plan is advisory until final submission.

No task is complete and no implementation file has been created.
