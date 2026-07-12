# Tasks: Offerings Groups and Resources

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-010.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-1, FR-2, FR-3) under tests/acceptance/010-offerings-groups-resources/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-3) under tests/acceptance/010-offerings-groups-resources/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-4) under tests/acceptance/010-offerings-groups-resources/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-5, FR-6, FR-7) under tests/acceptance/010-offerings-groups-resources/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: Admin MUST create term course offerings and one or more section groups.
- FR-2: Each group MUST have code, capacity, state, meeting slots, room(s), and
  required Lecturer/TA assignments before publish.
- FR-3: Publish validation MUST reject staff overlap/unavailability, room
  overlap/unavailability, room capacity below group capacity, invalid slots,
  missing roles, and duplicate offering/group codes.
- FR-4: Students MUST NOT select full, unpublished, cancelled, or closed
  groups.
- FR-5: Capacity MUST NOT be set below active EnrolledCount.
- FR-6: Staff assignments/availability and resource changes MUST be
  optimistic-concurrency protected and audited.
- FR-7: Publication MUST be transactional.

No task is complete and no implementation file has been created.
