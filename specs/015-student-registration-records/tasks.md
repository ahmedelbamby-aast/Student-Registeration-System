# Tasks: Student Registration Records

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-015.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-1, FR-2) under tests/acceptance/015-student-registration-records/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-3) under tests/acceptance/015-student-registration-records/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-5, FR-2) under tests/acceptance/015-student-registration-records/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-7) under tests/acceptance/015-student-registration-records/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.
- [ ] T012 [P] [US5] Add the future failing acceptance test for AC-5 (FR-4, FR-6, FR-8) under tests/acceptance/015-student-registration-records/.
- [ ] T013 [US5] Implement AC-5 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: Successful submission MUST produce a unique receipt/reference.
- FR-2: Receipt MUST include term, course/group, credits, Lecturer/TA, room,
  day/time, policy version, and submission time.
- FR-3: A rejected atomic submission MUST state the reason and that no partial
  enrollment was created.
- FR-4: Students MUST view their current registrations/timetable and historical
  terms.
- FR-5: Authorized staff/admin MAY inspect records within server-enforced scope.
- FR-6: Calendar and printable table/list MUST present equivalent schedule data.
- FR-7: Decision snapshots and historical group details MUST retain their
  original meaning after later edits.
- FR-8: Drop/correction actions MUST be absent until approved policy/workflow
  is specified.

No task is complete and no implementation file has been created.
