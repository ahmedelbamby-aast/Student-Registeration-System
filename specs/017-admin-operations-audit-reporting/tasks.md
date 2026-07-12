# Tasks: Admin Operations Audit and Reporting

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-017.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-2, FR-4) under tests/acceptance/017-admin-operations-audit-reporting/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-4, FR-9) under tests/acceptance/017-admin-operations-audit-reporting/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-5, FR-7) under tests/acceptance/017-admin-operations-audit-reporting/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-3) under tests/acceptance/017-admin-operations-audit-reporting/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.
- [ ] T012 [P] [US5] Add the future failing acceptance test for AC-5 (FR-1, FR-6, FR-8) under tests/acceptance/017-admin-operations-audit-reporting/.
- [ ] T013 [US5] Implement AC-5 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: Authorized Admin MUST manage terms/windows, users/roles, student
  records/holds, catalogue/policies, resources, offerings/groups, and imports
  only through feature-spec commands.
- FR-2: Sensitive changes MUST require reason, actor, timestamp, before/after
  summary, correlation ID, and audit event.
- FR-3: The system MUST provide registration-window metrics for traffic,
  success, expected rejections, server failures, fill rates, lock waits, and
  data-quality alerts.
- FR-4: Normal corrections MUST NOT exceed capacity or create timetable
  conflicts.
- FR-5: Exports MUST enforce the same row/data scope and PII minimization as UI.
- FR-6: Admin list/search endpoints MUST be paged, filtered, and safely
  parameterized.
- FR-7: Audit events for sensitive actions MUST be append-only to normal users.
- FR-8: Import/publish/correction MUST use preview and explicit confirmation.
- FR-9: Break-glass behavior MUST NOT exist without a separate approved spec.

No task is complete and no implementation file has been created.
