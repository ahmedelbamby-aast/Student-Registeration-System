# Tasks: Admin Operations, Audit, and Reporting

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-017 in specs/017-admin-operations-audit-reporting/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-007] Validate the consumed upstream requirements, plan, data model, and API contract at specs/007-identity-account-lifecycle/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-008] Validate the consumed upstream requirements, plan, data model, and API contract at specs/008-academic-term-student-profile/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-009] Validate the consumed upstream requirements, plan, data model, and API contract at specs/009-catalog-prerequisites-policy-admin/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-010] Validate the consumed upstream requirements, plan, data model, and API contract at specs/010-offerings-groups-resources/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T007 [DEP-SPEC-014] Validate the consumed upstream requirements, plan, data model, and API contract at specs/014-registration-capacity-concurrency/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T008 [DEP-SPEC-015] Validate the consumed upstream requirements, plan, data model, and API contract at specs/015-student-registration-records/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T009 [DEP-SPEC-016] Validate the consumed upstream requirements, plan, data model, and API contract at specs/016-lecturer-ta-workspace/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T010 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T011 [GATE] Freeze SPEC-017 requirements, API, data-model, policy approvals, and dependency versions in specs/017-admin-operations-audit-reporting/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T012 [P] [ENTITY-AuditEvent] [OWNER-SPEC-017] Create the future failing invariant/schema/serialization checks for canonical AuditEvent ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec017/AuditEventModelTests.cs.
- [ ] T013 [ENTITY-AuditEvent] [OWNER-SPEC-017] Deliver the canonical AuditEvent model or governed artifact at src/StudentRegistration.Domain/Modules/StaffAdministration/AuditEvent.cs after T012 fails for the expected reason (depends on T012).
- [ ] T014 [P] [ENTITY-ImportBatch] [CONSUMER-SPEC-009] Verify SPEC-017 consumes the canonical ImportBatch at src/StudentRegistration.Domain/Modules/Academics/ImportBatch.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec017/ImportBatchModelTests.cs.
- [ ] T015 [P] [ENTITY-ExportJob] [OWNER-SPEC-017] Create the future failing invariant/schema/serialization checks for canonical ExportJob ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec017/ExportJobModelTests.cs.
- [ ] T016 [ENTITY-ExportJob] [OWNER-SPEC-017] Deliver the canonical ExportJob model or governed artifact at src/StudentRegistration.Domain/Modules/StaffAdministration/ExportJob.cs after T015 fails for the expected reason (depends on T015).
- [ ] T017 [P] [ENTITY-OperationalMetric] [CONSUMER-SPEC-018] Verify SPEC-017 consumes the canonical OperationalMetric at src/StudentRegistration.Contracts/Operations/OperationalMetric.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec017/OperationalMetricModelTests.cs.
- [ ] T018 [API-Endpoint01] [OWNER-SPEC-017] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/admin/operations/metrics in specs/017-admin-operations-audit-reporting/contracts/api.md.
- [ ] T019 [P] [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/admin/operations/metrics in tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint01ContractTests.cs.
- [ ] T020 [API-Endpoint01] [OWNER-SPEC-017] Deliver the sole canonical GET /api/admin/operations/metrics handler at src/StudentRegistration.Server/Modules/StaffAdministration/Endpoints/Spec017Endpoints.cs after T019 fails for the expected reason (depends on T019).
- [ ] T021 [API-Endpoint02] [OWNER-SPEC-017] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/admin/audit in specs/017-admin-operations-audit-reporting/contracts/api.md.
- [ ] T022 [P] [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/admin/audit in tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint02ContractTests.cs.
- [ ] T023 [API-Endpoint02] [OWNER-SPEC-017] Deliver the sole canonical GET /api/admin/audit handler at src/StudentRegistration.Server/Modules/StaffAdministration/Endpoints/Spec017Endpoints.cs after T022 fails for the expected reason (depends on T022).
- [ ] T024 [API-Endpoint03] [OWNER-SPEC-017] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for POST /api/admin/exports in specs/017-admin-operations-audit-reporting/contracts/api.md.
- [ ] T025 [P] [API-Endpoint03] Verify every documented response and authorization outcome for POST /api/admin/exports in tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint03ContractTests.cs.
- [ ] T026 [API-Endpoint03] [OWNER-SPEC-017] Deliver the sole canonical POST /api/admin/exports handler at src/StudentRegistration.Server/Modules/StaffAdministration/Endpoints/Spec017Endpoints.cs after T025 fails for the expected reason (depends on T025).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Reasoned correction (FR-2, FR-4) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T027 [P] [AC-1] [FR-2] [FR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-1Tests.cs for AC-1: Reasoned correction (FR-2, FR-4): Given Admin has correction permission and provides a valid reason When a safe correction is committed Then the invariant remains valid And an append-only event records actor, reason, time and before/after summary.
### US2 - Capacity bypass rejected (FR-4, FR-9) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T028 [P] [AC-2] [FR-4] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-2Tests.cs for AC-2: Capacity bypass rejected (FR-4, FR-9): Given a group is full When Admin attempts a normal correction that adds another active enrollment Then the command is rejected And no capacity/enrollment state changes.
### US3 - Audit export scope (FR-5, FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T029 [P] [AC-3] [FR-5] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-3Tests.cs for AC-3: Audit export scope (FR-5, FR-7): Given Admin lacks permission for restricted security events When an audit export is requested Then restricted rows/fields are omitted or request denied And the export action itself is audited.
### US4 - Monitor degradation (FR-3) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T030 [P] [AC-4] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-4Tests.cs for AC-4: Monitor degradation (FR-3): Given server failures or capacity conflicts spike above configured threshold When the admin dashboard refreshes Then a timestamped alert identifies metric, threshold and investigation link.
### US5 - Governed bounded master-data command (FR-1, FR-6, FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T031 [P] [AC-5] [FR-1] [FR-6] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-5Tests.cs for AC-5: Governed bounded master-data command (FR-1, FR-6, FR-8): Given authorized Admin filters a large master-data list and previews a change When the bounded request and confirmed mutation execute Then only a paged parameterized result is returned And the confirmed feature-spec command is validated/audited.
### US6 - Stale preview confirmation (FR-8, FR-10, FR-11) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T032 [P] [AC-6] [FR-8] [FR-10] [FR-11] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-6Tests.cs for AC-6: Stale preview confirmation (FR-8, FR-10, FR-11): Given an admin previews a correction and its dependency version changes When the admin confirms the old token twice with the same idempotency key Then both responses report 409 STALE_PREVIEW And no correction or duplicate audit event commits.
### US7 - Audit failure rolls back mutation (FR-2, FR-12) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T033 [P] [AC-7] [FR-2] [FR-12] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-7Tests.cs for AC-7: Audit failure rolls back mutation (FR-2, FR-12): Given a sensitive change passes validation When audit-event persistence is fault-injected to fail Then the business mutation rolls back And the API returns a generic correlated failure without reporting success.
### US8 - Final Admin safeguard (FR-13) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T034 [P] [AC-8] [FR-13] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-8Tests.cs for AC-8: Final Admin safeguard (FR-13): Given one active Admin role assignment remains When an ordinary admin command attempts to revoke it Then the command returns 409 FINAL_ADMIN_REQUIRED And the assignment remains active.
### US9 - Admin operations quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-9 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-9 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T026.
- [ ] T035 [P] [AC-9] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-9Tests.cs for AC-9: Admin operations quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given target operational metrics, a production-size audit dataset, large export, and positive/negative/concurrent admin command matrix When admin quality tests execute Then metrics are no more than 60 seconds stale and show observation time And audit first page returns within 1 second p95 And export executes asynchronously with bounded resources, expiry, and audit And every admin action passes authorization, audit, concurrency, and anti-forgery checks.
- [ ] T036 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-1Tests.cs and assert: Metrics backend unavailable -> show stale timestamp/degraded state, not fabricated zero.
- [ ] T037 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-2Tests.cs and assert: Export fails/expires -> safe status and authorized retry.
- [ ] T038 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-3Tests.cs and assert: Concurrent admin edit -> 409 current version, no lost update.
- [ ] T039 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-4Tests.cs and assert: Bulk import partially invalid -> preview errors; publish all-or-nothing.
- [ ] T040 [P] [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-5Tests.cs and assert: Admin disables own final Admin role -> require safeguard/second actor according to security approval.
- [ ] T041 [P] [EC-6] Exercise EC-6 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-6Tests.cs and assert: The same idempotency key is reused with a different admin command payload -> return 409 IDEMPOTENCY_KEY_REUSED and execute neither new payload.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T042 [P] [FR-1] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-1 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no break-glass and final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: Authorized Admin MUST manage terms/windows, users/roles, student records/holds, catalogue/policies, resources, offerings/groups, and imports only through feature-spec commands.
- [ ] T043 [FR-1] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Deliver FR-1 through the bounded Governed admin command orchestration workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AdminCommandService.cs only after T042 fails for the expected reason (depends on T042): Authorized Admin MUST manage terms/windows, users/roles, student records/holds, catalogue/policies, resources, offerings/groups, and imports only through feature-spec commands.
- [ ] T044 [P] [FR-2] [WORKSTREAM-ATOMIC-AUDIT-WRITER] Create the future failing FR-2 checks in tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs. Test focus: actor/reason/before-after/correlation, append-only access and business rollback on audit failure. Prove the requirement against its linked AC/EC fixtures: Sensitive changes MUST require reason, actor, timestamp, before/after summary, correlation ID, and audit event.
- [ ] T045 [FR-2] [WORKSTREAM-ATOMIC-AUDIT-WRITER] Deliver FR-2 through the bounded Atomic audit writer workstream at src/StudentRegistration.Infrastructure/Audit/AuditTransactionWriter.cs only after T044 fails for the expected reason (depends on T044): Sensitive changes MUST require reason, actor, timestamp, before/after summary, correlation ID, and audit event.
- [ ] T046 [P] [FR-3] [WORKSTREAM-OPERATIONAL-METRICS-QUERY] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Admin/AdminMetricsTests.cs. Test focus: timestamped traffic/result/failure/fill/lock/data-quality metrics and degraded state. Prove the requirement against its linked AC/EC fixtures: The system MUST provide registration-window metrics for traffic, success, expected rejections, server failures, fill rates, lock waits, and data-quality alerts.
- [ ] T047 [FR-3] [WORKSTREAM-OPERATIONAL-METRICS-QUERY] Deliver FR-3 through the bounded Operational metrics query workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AdminMetricsQuery.cs only after T046 fails for the expected reason (depends on T046): The system MUST provide registration-window metrics for traffic, success, expected rejections, server failures, fill rates, lock waits, and data-quality alerts.
- [ ] T048 [P] [FR-4] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-4 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no break-glass and final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: Normal corrections MUST NOT exceed capacity or create timetable conflicts.
- [ ] T049 [FR-4] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Deliver FR-4 through the bounded Governed admin command orchestration workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AdminCommandService.cs only after T048 fails for the expected reason (depends on T048): Normal corrections MUST NOT exceed capacity or create timetable conflicts.
- [ ] T050 [P] [FR-5] [WORKSTREAM-SCOPED-AUDIT-SEARCH-AND-EXPORT] Create the future failing FR-5 checks in tests/StudentRegistration.AuthorizationTests/AuditExportScopeTests.cs. Test focus: row/field scope, PII minimization, bounded query and asynchronous expiring export. Prove the requirement against its linked AC/EC fixtures: Exports MUST enforce the same row/data scope and PII minimization as UI.
- [ ] T051 [FR-5] [WORKSTREAM-SCOPED-AUDIT-SEARCH-AND-EXPORT] Deliver FR-5 through the bounded Scoped audit search and export workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AuditExportService.cs only after T050 fails for the expected reason (depends on T050): Exports MUST enforce the same row/data scope and PII minimization as UI.
- [ ] T052 [P] [FR-6] [WORKSTREAM-SCOPED-AUDIT-SEARCH-AND-EXPORT] Create the future failing FR-6 checks in tests/StudentRegistration.AuthorizationTests/AuditExportScopeTests.cs. Test focus: row/field scope, PII minimization, bounded query and asynchronous expiring export. Prove the requirement against its linked AC/EC fixtures: Admin list/search endpoints MUST be paged, filtered, and safely parameterized.
- [ ] T053 [FR-6] [WORKSTREAM-SCOPED-AUDIT-SEARCH-AND-EXPORT] Deliver FR-6 through the bounded Scoped audit search and export workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AuditExportService.cs only after T052 fails for the expected reason (depends on T052): Admin list/search endpoints MUST be paged, filtered, and safely parameterized.
- [ ] T054 [P] [FR-7] [WORKSTREAM-ATOMIC-AUDIT-WRITER] Create the future failing FR-7 checks in tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs. Test focus: actor/reason/before-after/correlation, append-only access and business rollback on audit failure. Prove the requirement against its linked AC/EC fixtures: Audit events for sensitive actions MUST be append-only to normal users.
- [ ] T055 [FR-7] [WORKSTREAM-ATOMIC-AUDIT-WRITER] Deliver FR-7 through the bounded Atomic audit writer workstream at src/StudentRegistration.Infrastructure/Audit/AuditTransactionWriter.cs only after T054 fails for the expected reason (depends on T054): Audit events for sensitive actions MUST be append-only to normal users.
- [ ] T056 [P] [FR-8] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-8 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no break-glass and final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: Import/publish/correction MUST use preview and explicit confirmation.
- [ ] T057 [FR-8] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Deliver FR-8 through the bounded Governed admin command orchestration workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AdminCommandService.cs only after T056 fails for the expected reason (depends on T056): Import/publish/correction MUST use preview and explicit confirmation.
- [ ] T058 [P] [FR-9] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-9 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no break-glass and final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: Break-glass behavior MUST NOT exist without a separate approved spec.
- [ ] T059 [FR-9] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Deliver FR-9 through the bounded Governed admin command orchestration workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AdminCommandService.cs only after T058 fails for the expected reason (depends on T058): Break-glass behavior MUST NOT exist without a separate approved spec.
- [ ] T060 [P] [FR-10] [WORKSTREAM-ADMIN-PREVIEW-AND-IDEMPOTENCY] Create the future failing FR-10 checks in tests/StudentRegistration.IntegrationTests/Admin/AdminConfirmationConcurrencyTests.cs. Test focus: required expected version, bound preview, expiry, duplicate confirmation and payload mismatch. Prove the requirement against its linked AC/EC fixtures: Update/delete commands MUST require expected rowversion; retryable creates, imports, corrections, exports, and confirmations MUST require an idempotency key.
- [ ] T061 [FR-10] [WORKSTREAM-ADMIN-PREVIEW-AND-IDEMPOTENCY] Deliver FR-10 through the bounded Admin preview and idempotency workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AdminConfirmationService.cs only after T060 fails for the expected reason (depends on T060): Update/delete commands MUST require expected rowversion; retryable creates, imports, corrections, exports, and confirmations MUST require an idempotency key.
- [ ] T062 [P] [FR-11] [WORKSTREAM-ADMIN-PREVIEW-AND-IDEMPOTENCY] Create the future failing FR-11 checks in tests/StudentRegistration.IntegrationTests/Admin/AdminConfirmationConcurrencyTests.cs. Test focus: required expected version, bound preview, expiry, duplicate confirmation and payload mismatch. Prove the requirement against its linked AC/EC fixtures: Preview tokens MUST bind actor, permission scope, canonical payload, dependency versions, and expiry; confirmation MUST reject any changed input, scope, permission, dependency, or expired token.
- [ ] T063 [FR-11] [WORKSTREAM-ADMIN-PREVIEW-AND-IDEMPOTENCY] Deliver FR-11 through the bounded Admin preview and idempotency workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AdminConfirmationService.cs only after T062 fails for the expected reason (depends on T062): Preview tokens MUST bind actor, permission scope, canonical payload, dependency versions, and expiry; confirmation MUST reject any changed input, scope, permission, dependency, or expired token.
- [ ] T064 [P] [FR-12] [WORKSTREAM-ATOMIC-AUDIT-WRITER] Create the future failing FR-12 checks in tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs. Test focus: actor/reason/before-after/correlation, append-only access and business rollback on audit failure. Prove the requirement against its linked AC/EC fixtures: A sensitive business mutation and its audit event MUST commit in the same local SQL transaction; audit failure MUST roll back the mutation.
- [ ] T065 [FR-12] [WORKSTREAM-ATOMIC-AUDIT-WRITER] Deliver FR-12 through the bounded Atomic audit writer workstream at src/StudentRegistration.Infrastructure/Audit/AuditTransactionWriter.cs only after T064 fails for the expected reason (depends on T064): A sensitive business mutation and its audit event MUST commit in the same local SQL transaction; audit failure MUST roll back the mutation.
- [ ] T066 [P] [FR-13] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-13 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no break-glass and final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: The final-active-Admin role MUST NOT be removed by an ordinary command; removal requires a separately approved two-actor recovery procedure.
- [ ] T067 [FR-13] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Deliver FR-13 through the bounded Governed admin command orchestration workstream at src/StudentRegistration.Server/Modules/StaffAdministration/AdminCommandService.cs only after T066 fails for the expected reason (depends on T066): The final-active-Admin role MUST NOT be removed by an ordinary command; removal requires a separately approved two-actor recovery procedure.

## Phase 5 - Frontend Route Tests and Integration

- [ ] T068 [P] [ADM-01] [UI-CONTRACT-SPEC-003] [FR-3] [FR-6] [AC-4] [AC-9] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-01 in tests/StudentRegistration.E2ETests/Specs/Spec017/AdminDashboardPageFeatureTests.cs.
- [ ] T069 [ADM-01] [UI-CONTRACT-SPEC-003] [FR-3] [FR-6] [AC-4] [AC-9] Deliver the sole canonical Blazor implementation for ADM-01 at src/StudentRegistration.Client/Pages/AdminDashboardPage.razor after T068 and the SPEC-003 contract/component checks fail for expected reasons (depends on T068).
- [ ] T070 [ADM-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-02 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-02.md without editing the canonical Razor page.
- [ ] T071 [P] [ADM-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-02 in tests/StudentRegistration.E2ETests/Specs/Spec017/TermAdministrationPageContributorTests.cs.
- [ ] T072 [ADM-03] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-13] [AC-5] [AC-8] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-03 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-03.md without editing the canonical Razor page.
- [ ] T073 [P] [ADM-03] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-13] [AC-5] [AC-8] Verify the SPEC-017 contribution consumed by ADM-03 in tests/StudentRegistration.E2ETests/Specs/Spec017/UserAdministrationPageContributorTests.cs.
- [ ] T074 [ADM-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-1] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-04 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-04.md without editing the canonical Razor page.
- [ ] T075 [P] [ADM-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-1] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-04 in tests/StudentRegistration.E2ETests/Specs/Spec017/StudentAdministrationPageContributorTests.cs.
- [ ] T076 [ADM-05] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-05 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-05.md without editing the canonical Razor page.
- [ ] T077 [P] [ADM-05] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-05 in tests/StudentRegistration.E2ETests/Specs/Spec017/CatalogueAdministrationPageContributorTests.cs.
- [ ] T078 [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-06 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-06.md without editing the canonical Razor page.
- [ ] T079 [P] [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-06 in tests/StudentRegistration.E2ETests/Specs/Spec017/OfferingAdministrationPageContributorTests.cs.
- [ ] T080 [ADM-07] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-07 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-07.md without editing the canonical Razor page.
- [ ] T081 [P] [ADM-07] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-07 in tests/StudentRegistration.E2ETests/Specs/Spec017/ResourceAdministrationPageContributorTests.cs.
- [ ] T082 [P] [ADM-08] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-4] [FR-8] [FR-10] [FR-11] [FR-12] [AC-1] [AC-2] [AC-4] [AC-6] [AC-7] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-08 in tests/StudentRegistration.E2ETests/Specs/Spec017/RegistrationAdministrationPageFeatureTests.cs.
- [ ] T083 [ADM-08] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-4] [FR-8] [FR-10] [FR-11] [FR-12] [AC-1] [AC-2] [AC-4] [AC-6] [AC-7] Deliver the sole canonical Blazor implementation for ADM-08 at src/StudentRegistration.Client/Pages/RegistrationAdministrationPage.razor after T082 and the SPEC-003 contract/component checks fail for expected reasons (depends on T082).
- [ ] T084 [P] [ADM-09] [UI-CONTRACT-SPEC-003] [FR-2] [FR-5] [FR-6] [FR-7] [FR-10] [FR-12] [AC-3] [AC-7] [AC-9] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-09 in tests/StudentRegistration.E2ETests/Specs/Spec017/AuditAdministrationPageFeatureTests.cs.
- [ ] T085 [ADM-09] [UI-CONTRACT-SPEC-003] [FR-2] [FR-5] [FR-6] [FR-7] [FR-10] [FR-12] [AC-3] [AC-7] [AC-9] Deliver the sole canonical Blazor implementation for ADM-09 at src/StudentRegistration.Client/Pages/AuditAdministrationPage.razor after T084 and the SPEC-003 contract/component checks fail for expected reasons (depends on T084).

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T086 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-017-NFR-1.md: Operational metrics SHOULD be no more than 60 seconds stale and show observation timestamp.
- [ ] T087 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-017-NFR-2.md: Audit search SHOULD return first page within 1 second p95 at approved retention volume.
- [ ] T088 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-017-NFR-3.md: Export generation MUST be asynchronous/bounded for large data and expire securely.
- [ ] T089 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-017-NFR-4.md: Admin actions MUST have authorization, audit, concurrency, and validation tests.

## Phase 7 - Scope and Release Evidence

- [ ] T090 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-017-scope-review.md that OS-1 remains excluded: Unrestricted super-admin and unaudited direct database edits.
- [ ] T091 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-017-scope-review.md that OS-2 remains excluded: Break-glass capacity/conflict override.
- [ ] T092 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-017-scope-review.md that OS-3 remains excluded: Business-intelligence warehouse.
- [ ] T093 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-017-scope-review.md that OS-4 remains excluded: Long-term report replica until primary impact is measured.
- [ ] T094 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-017-traceability.md and reject release if any row lacks passing evidence.
- [ ] T095 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-017 in docs/release-evidence/SPEC-017-release-approval.md.

No task is complete and no implementation file has been created.
