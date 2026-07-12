# Tasks: UX Storyboard and Accessibility

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-003.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-3, FR-4, FR-1) under tests/acceptance/003-ux-storyboard-accessibility/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-5, FR-1) under tests/acceptance/003-ux-storyboard-accessibility/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-2) under tests/acceptance/003-ux-storyboard-accessibility/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-6, FR-7) under tests/acceptance/003-ux-storyboard-accessibility/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: The product MUST implement exactly the 27 MVP route templates in the
  approved storyboard using reusable components.
- FR-2: Every data route MUST implement loading, empty, success, error,
  unauthorized, and stale/concurrent states where applicable.
- FR-3: A hard conflict MUST show icon, Conflict text, involved subjects and
  times, and manual resolution actions.
- FR-4: Submission MUST remain disabled while a hard conflict exists, with the
  reason visible.
- FR-5: Every timetable calendar MUST have an equivalent chronological
  list/table.
- FR-6: Student and shared staff login MUST remain visually and semantically
  distinct.
- FR-7: The authenticated shell MUST display server date/time, term/window,
  user, authorized role context, and session status.

No task is complete and no implementation file has been created.
