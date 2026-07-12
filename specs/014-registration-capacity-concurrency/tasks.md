# Tasks: Registration Capacity and Concurrency

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-014.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-4, FR-6, FR-1) under tests/acceptance/014-registration-capacity-concurrency/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-4, FR-6) under tests/acceptance/014-registration-capacity-concurrency/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-2, FR-7) under tests/acceptance/014-registration-capacity-concurrency/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-8, FR-1) under tests/acceptance/014-registration-capacity-concurrency/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.
- [ ] T012 [P] [US5] Add the future failing acceptance test for AC-5 (FR-3, FR-10) under tests/acceptance/014-registration-capacity-concurrency/.
- [ ] T013 [US5] Implement AC-5 only after approval, using the module paths declared in plan.md.
- [ ] T014 [P] [US6] Add the future failing acceptance test for AC-6 (FR-5, FR-9, FR-11) under tests/acceptance/014-registration-capacity-concurrency/.
- [ ] T015 [US6] Implement AC-6 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: Submission MUST resolve the student from authenticated server identity.
- FR-2: Submission MUST accept PlanId, plan rowversion, term, and a
  client-generated idempotency key.
- FR-3: The server MUST revalidate window, student/holds, policy version,
  eligibility, credit load, duplicates, group state, and timetable.
- FR-4: Every selected group seat MUST be allocated by a conditional atomic SQL
  update within the enrollment transaction.
- FR-5: Group IDs MUST be allocated in stable sorted order to reduce deadlocks.
- FR-6: If any allocation or invariant fails, the whole transaction MUST roll
  back and create no active partial enrollment.
- FR-7: The same student/term/idempotency key MUST return the original result
  and MUST NOT allocate again.
- FR-8: Unique/check constraints MUST be final guards for duplicates/capacity.
- FR-9: Drops MUST transition enrollment and decrement capacity transactionally.
- FR-10: Expected business conflicts MUST return 409 with stable reason codes.
- FR-11: The system MUST reconcile EnrolledCount to active Enrollment and alert
  on mismatch.

No task is complete and no implementation file has been created.
