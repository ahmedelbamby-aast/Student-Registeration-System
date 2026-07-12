# Tasks: Product Charter and RBAC

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-001.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-1, FR-2) under tests/acceptance/001-product-charter-rbac/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-2, FR-3) under tests/acceptance/001-product-charter-rbac/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-6, FR-7) under tests/acceptance/001-product-charter-rbac/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-4, FR-5) under tests/acceptance/001-product-charter-rbac/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: The system MUST support Student, Admin, Lecturer, and
  TeachingAssistant roles.
- FR-2: Students MUST use a student entry point; Admin/Lecturer/TA MUST share a
  staff entry point.
- FR-3: The server MUST derive role and data scope and MUST NOT trust a
  client-selected role.
- FR-4: The system MUST support the end-to-end student flow from login through
  an atomic registration receipt.
- FR-5: The system MUST expose role-scoped staff/admin workspaces.
- FR-6: MVP scope and non-goals MUST match docs/PROJECT_PLAN.md.
- FR-7: Every implementation story MUST trace to an approved spec and
  acceptance criterion.

No task is complete and no implementation file has been created.
