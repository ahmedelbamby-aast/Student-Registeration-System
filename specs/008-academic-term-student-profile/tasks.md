# Tasks: Academic Term and Student Profile

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-008.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-1, FR-6) under tests/acceptance/008-academic-term-student-profile/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-2, FR-4) under tests/acceptance/008-academic-term-student-profile/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-5, FR-6) under tests/acceptance/008-academic-term-student-profile/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-3, FR-7) under tests/acceptance/008-academic-term-student-profile/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: The server MUST expose current UTC time and configured institutional
  timezone, initially Africa/Cairo.
- FR-2: The system MUST distinguish teaching term from registration term.
- FR-3: Terms/windows MUST have explicit dates, states, scope, and rowversion.
- FR-4: At most one permitted registration context MAY match a student at an
  instant.
- FR-5: Student profile MUST include University ID, program/cohort, GPA,
  earned credits, standing, transcript summary, active holds, and provenance.
- FR-6: Registration commands MUST re-resolve time, term, window, student
  state, and holds.
- FR-7: Admin profile corrections MUST require authorization, reason, source,
  optimistic concurrency, and audit.

No task is complete and no implementation file has been created.
