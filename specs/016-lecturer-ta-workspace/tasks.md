# Tasks: Lecturer and Teaching Assistant Workspace

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-016.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-2, FR-5) under tests/acceptance/016-lecturer-ta-workspace/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-1, FR-3) under tests/acceptance/016-lecturer-ta-workspace/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-6) under tests/acceptance/016-lecturer-ta-workspace/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-4, FR-7) under tests/acceptance/016-lecturer-ta-workspace/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.
- [ ] T012 [P] [US5] Add the future failing acceptance test for AC-5 (FR-8) under tests/acceptance/016-lecturer-ta-workspace/.
- [ ] T013 [US5] Implement AC-5 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: Lecturer and TA MUST use the shared staff login and shared workspace
  templates.
- FR-2: The API MUST scope assignments/timetable/rosters to the authenticated
  staff user's current assignments.
- FR-3: Lecturer MUST see assigned lecture groups; TA MUST see assigned
  tutorial/lab groups according to server data.
- FR-4: Staff MUST view group code, subject, role partners, room, meeting
  slots, capacity and roster count.
- FR-5: Staff MAY view the minimum authorized roster fields for assigned
  groups.
- FR-6: Staff MUST create/edit own availability before deadline using
  concurrency protection.
- FR-7: Staff MUST NOT manage policy, users, capacity, terms, or unrelated
  rosters.
- FR-8: Availability changes after schedule publication MUST trigger an admin
  warning and MUST NOT silently move a class.

No task is complete and no implementation file has been created.
