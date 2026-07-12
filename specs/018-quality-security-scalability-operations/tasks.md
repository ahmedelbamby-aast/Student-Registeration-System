# Tasks: Quality Security Scalability and Operations

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md

## Phase 1 - Approval and Contracts

- [ ] T001 Obtain human approval for SPEC-018.
- [ ] T002 Reconfirm upstream dependency versions and institutional policy approvals.
- [ ] T003 Freeze the reviewed data and API contract for the implementation sprint.

## Phase 2 - User Stories

- [ ] T004 [P] [US1] Add the future failing acceptance test for AC-1 (FR-4, FR-2, FR-3) under tests/acceptance/018-quality-security-scalability-operations/.
- [ ] T005 [US1] Implement AC-1 only after approval, using the module paths declared in plan.md.
- [ ] T006 [P] [US2] Add the future failing acceptance test for AC-2 (FR-2, FR-4) under tests/acceptance/018-quality-security-scalability-operations/.
- [ ] T007 [US2] Implement AC-2 only after approval, using the module paths declared in plan.md.
- [ ] T008 [P] [US3] Add the future failing acceptance test for AC-3 (FR-5, FR-7) under tests/acceptance/018-quality-security-scalability-operations/.
- [ ] T009 [US3] Implement AC-3 only after approval, using the module paths declared in plan.md.
- [ ] T010 [P] [US4] Add the future failing acceptance test for AC-4 (FR-8) under tests/acceptance/018-quality-security-scalability-operations/.
- [ ] T011 [US4] Implement AC-4 only after approval, using the module paths declared in plan.md.
- [ ] T012 [P] [US5] Add the future failing acceptance test for AC-5 (FR-6, FR-9) under tests/acceptance/018-quality-security-scalability-operations/.
- [ ] T013 [US5] Implement AC-5 only after approval, using the module paths declared in plan.md.
- [ ] T014 [P] [US6] Add the future failing acceptance test for AC-6 (FR-1) under tests/acceptance/018-quality-security-scalability-operations/.
- [ ] T015 [US6] Implement AC-6 only after approval, using the module paths declared in plan.md.

## Phase 3 - Quality and Release

- [ ] T090 Run unit, integration, concurrency, authorization, accessibility, and performance checks required by requirements.md.
- [ ] T091 Verify every FR and AC has passing evidence and no capacity or authorization invariant regressed.
- [ ] T092 Complete product-owner, policy-owner, QA, security, and operations release gates.

## Requirement Traceability Reference

- FR-1: CI MUST run restore, formatting, warnings-as-errors build, unit,
  architecture, real-SQL integration, migration, E2E, and security checks.
- FR-2: Critical domain rules and capacity logic MUST have automated boundary
  and concurrency tests before implementation is accepted.
- FR-3: The application MUST expose authenticated-safe health, logs, metrics,
  traces, and correlation IDs.
- FR-4: Operations MUST monitor latency, throughput, unexpected error rate,
  business rejection codes, optimizer time, SQL latency, lock waits, deadlocks,
  capacity conflicts, and counter reconciliation.
- FR-5: Backup/restore, migration rollback, and application rollback MUST be
  rehearsed before release.
- FR-6: Production secrets MUST use an approved secret store and MUST NOT
  appear in Git/config/logs.
- FR-7: Application replicas MUST share Data Protection keys and remain
  stateless.
- FR-8: Critical flows MUST pass automated and manual accessibility tests.
- FR-9: Release MUST be blocked by unresolved critical/high security issues,
  invariant failures, or critical/major core usability defects.

No task is complete and no implementation file has been created.
