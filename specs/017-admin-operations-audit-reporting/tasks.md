# Tasks: Admin Operations, Audit, and Reporting

**Status**: Approved for non-production demo implementation by Ahmed ELbamby on 2026-07-13; tasks remain unstarted.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Planning Baseline and Recorded Gate A Verification

- [ ] T001 [GATE] Run and record the constitution-compliance review for SPEC-017 in specs/017-admin-operations-audit-reporting/checklists/approval.md; this is planning analysis and does not authorize implementation.
- [ ] T002 [DEP-SPEC-003] [DEP-SPEC-004] Validate the frontend contract at specs/003-ux-storyboard-accessibility/ and the upstream atomic-audit/architecture contract at specs/004-architecture-engineering-principles/; record both accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-007] Validate the consumed upstream requirements, plan, data model, and API contract at specs/007-identity-account-lifecycle/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-008] Validate the consumed upstream requirements, plan, data model, and API contract at specs/008-academic-term-student-profile/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-009] Validate the consumed upstream requirements, plan, data model, and API contract at specs/009-catalog-prerequisites-policy-admin/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-010] Validate the consumed upstream requirements, plan, data model, and API contract at specs/010-offerings-groups-resources/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T007 [DEP-SPEC-014] Validate the consumed upstream requirements, plan, data model, and API contract at specs/014-registration-capacity-concurrency/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T008 [DEP-SPEC-015] Validate the consumed upstream requirements, plan, data model, and API contract at specs/015-student-registration-records/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T009 [DEP-SPEC-016] Validate the consumed upstream requirements, plan, data model, and API contract at specs/016-lecturer-ta-workspace/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T010 [DEP-SPEC-018] Validate the consumed upstream requirements, plan, data model, and API contract at specs/018-quality-security-scalability-operations/ and record the accepted versions in specs/017-admin-operations-audit-reporting/dependency-baseline.md.
- [ ] T011 [GATE] Complete dependency validation, cross-spec consistency analysis, model/API/policy/task trace review, and verify the approved SPEC-017 baseline in specs/017-admin-operations-audit-reporting/checklists/implementation-readiness.md; record pass/fail and return the package to In Review if this gate fails.
- [ ] T012 [GATE] Before any later model, test, source, migration, page, or deployment task, verify Ahmed ELbamby's 2026-07-13 Gate A demo approval recorded in specs/017-admin-operations-audit-reporting/clarifications.md remains current; a superseding baseline change returns the package to In Review.

## Phase 2 - Models and API Contracts

- [ ] T013 [ENTITY-AuditEvent] [CONSUMER-SPEC-004] [ENTITY-SecurityEvent] [CONSUMER-SPEC-007] [ENTITY-AdminSecurityGuard] Verify SPEC-017 consumes canonical AuditEvent at src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditEvent.cs, canonical SecurityEvent at src/StudentRegistration.IdentityAccess/Domain/SecurityEvent.cs, and canonical AdminSecurityGuard at src/StudentRegistration.IdentityAccess/Domain/AdminSecurityGuard.cs without redefining or mutating them in tests/StudentRegistration.IntegrationTests/Specs/Spec017/AuditSourceConsumptionTests.cs.
- [ ] T014 [FR-2] Create future failing source-merge, redaction, chronological ordering, correlation, and scoped-visibility checks in tests/StudentRegistration.ContractTests/Specs/Spec017/AuditSourceMergeContractTests.cs after validating the consumed sources in T013.
- [ ] T015 [ENTITY-ImportBatch] [CONSUMER-SPEC-009] Verify SPEC-017 consumes the canonical ImportBatch at src/StudentRegistration.Academics/Domain/ImportBatch.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec017/ImportBatchModelTests.cs.
- [ ] T016 [ENTITY-ExportJob] [OWNER-SPEC-017] Create future failing SQL model/lifecycle tests in tests/StudentRegistration.IntegrationTests/Specs/Spec017/ExportJobModelTests.cs for owner/scope/request binding, Pending/Running/Complete/Failed/Expired, 60-second renewable lease, max three attempts, unique artifact, expiry, and rowversion.
- [ ] T017 [ENTITY-ExportJob] [OWNER-SPEC-017] Deliver the canonical durable ExportJob and lease transitions at src/StudentRegistration.StaffAdministration/Domain/ExportJob.cs after T016 fails (depends on T016).
- [ ] T018 [ENTITY-OperationalMetric] [CONSUMER-SPEC-018] Verify SPEC-017 consumes the canonical OperationalMetric at src/StudentRegistration.Contracts/Operations/OperationalMetric.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec017/OperationalMetricModelTests.cs.
- [ ] T019 [FR-13] Create future failing authorization/delegation and two-replica write-skew checks in tests/StudentRegistration.IntegrationTests/Specs/Spec017/IdentityAdminDelegationTests.cs proving the Admin page uses the SPEC-007 role endpoint/command, at most one concurrent revocation commits, the loser receives FINAL_ADMIN_REQUIRED, and SPEC-017 adds no generic facade or RoleAssignment/AdminSecurityGuard writer.
- [ ] T020 [FR-13] Publish the narrow Identity Admin-command delegation contract at specs/017-admin-operations-audit-reporting/contracts/identity-admin-delegation.md after T019 fails; no StaffAdministration role writer is delivered (depends on T019).
- [ ] T021 [PERSISTENCE-MAPPING] [ENTITY-ExportJob] [MIGRATION-S7StaffAdminOperations] Create future failing real-SQL EF mapping tests in tests/StudentRegistration.IntegrationTests/Persistence/AdministrationAuditModelConfigurationTests.cs for ExportJob lease constraints and no duplicate AuditEvent/AdminSecurityGuard/SecurityEvent mapping, plus incremental-migration/update/rollback/snapshot parity tests in tests/StudentRegistration.IntegrationTests/Persistence/S7StaffAdminOperationsMigrationTests.cs.
- [ ] T022 [PERSISTENCE-MAPPING] [ENTITY-ExportJob] [MIGRATION-S7StaffAdminOperations] Deliver the bounded ExportJob mapping at src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AdministrationAuditModelConfiguration.cs, generate src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713070000_StaffAdminOperations.cs, and update src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs after T021 and upstream slice migrations pass; SPEC-004 remains the sole DbContext writer (depends on T021 and upstream migrations).
- [ ] T023 [API-Endpoint01] [OWNER-SPEC-017] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/admin/operations/metrics in specs/017-admin-operations-audit-reporting/contracts/api.md.
- [ ] T024 [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/admin/operations/metrics in tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint01ContractTests.cs.
- [ ] T025 [API-Endpoint01] [FR-3] [NFR-1] Create future failing metrics freshness/degraded/scope endpoint behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec017/Endpoint01BehaviorTests.cs; handler delivery is deferred until linked AC/FR/NFR tests fail.
- [ ] T026 [API-Endpoint02] [OWNER-SPEC-017] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/admin/audit in specs/017-admin-operations-audit-reporting/contracts/api.md.
- [ ] T027 [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/admin/audit in tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint02ContractTests.cs.
- [ ] T028 [API-Endpoint02] [FR-2] [FR-6] [FR-7] Create future failing bounded audit query, redacted before/after, correlation, field/row scope, and append-only endpoint behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec017/Endpoint02BehaviorTests.cs; handler delivery is deferred.
- [ ] T029 [API-Endpoint03] [API-Endpoint04] [API-Endpoint05] [OWNER-SPEC-017] Finalize the complete export lifecycle in specs/017-admin-operations-audit-reporting/contracts/api.md for POST /api/admin/exports, GET /api/admin/exports/{jobId}, and GET /api/admin/exports/{jobId}/download, including idempotency, owner/scope reauthorization, status, retry, lease, one artifact, expiry, and audit.
- [ ] T030 [API-Endpoint03] [API-Endpoint04] [API-Endpoint05] Verify POST /api/admin/exports in tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint03ContractTests.cs, GET /api/admin/exports/{jobId} in tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint04ContractTests.cs, and GET /api/admin/exports/{jobId}/download in tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint05ContractTests.cs, covering authorization, idempotency, 202/retry, completion, failure, 410 expiry, and audit.
- [ ] T031 [API-Endpoint03] [API-Endpoint04] [API-Endpoint05] [FR-5] [FR-10] Create future failing lifecycle and two-replica endpoint/worker behavior tests in tests/StudentRegistration.ApplicationTests/Specs/Spec017/ExportLifecycleBehaviorTests.cs; handler/worker delivery is deferred until all linked AC/FR/NFR tests fail.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Reasoned sensitive mutation (FR-2, FR-4) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T032 [SC-1] [AC-1] [FR-2] [FR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-1Tests.cs for AC-1: Reasoned sensitive mutation (FR-2, FR-4): Given Admin has the owning feature permission and provides a valid reason When a safe feature-spec master-data mutation is committed Then the invariant remains valid And an append-only event records actor, reason, time and before/after summary.
### US2 - Capacity bypass rejected (FR-4, FR-9) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T033 [SC-2] [AC-2] [FR-4] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-2Tests.cs for AC-2: Capacity bypass rejected (FR-4, FR-9): Given a group has active enrollments at its current capacity When Admin attempts a SPEC-010 capacity reduction below EnrolledCount Then the owning feature command rejects it And no capacity/enrollment state changes.
### US3 - Audit export scope (FR-5, FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T034 [AC-3] [FR-5] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-3Tests.cs for AC-3: Audit export scope (FR-5, FR-7): Given Admin lacks permission for restricted security events When an audit export is requested Then restricted rows/fields are omitted or request denied And the export action itself is audited.
### US4 - Monitor degradation (FR-3) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T035 [SC-3] [AC-4] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-4Tests.cs for AC-4: Monitor degradation (FR-3): Given server failures or capacity conflicts spike above configured threshold When the admin dashboard refreshes Then a timestamped alert identifies metric, threshold and investigation link.
### US5 - Governed bounded master-data command (FR-1, FR-6, FR-8) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T036 [AC-5] [FR-1] [FR-6] [FR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-5Tests.cs for AC-5: Governed bounded master-data command (FR-1, FR-6, FR-8): Given authorized Admin filters a large master-data list and previews a change When the bounded request and confirmed mutation execute Then only a paged parameterized result is returned And the confirmed feature-spec command is validated/audited.
### US6 - Stale preview confirmation (FR-8, FR-10, FR-11) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T037 [AC-6] [FR-8] [FR-10] [FR-11] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-6Tests.cs for AC-6: Stale preview confirmation (FR-8, FR-10, FR-11): Given an Admin previews an offering publication and its dependency version changes When the admin confirms the old token twice with the same idempotency key Then both responses report 409 STALE_PREVIEW And no publication or duplicate audit event commits.
### US7 - Audit failure rolls back mutation (FR-2, FR-12) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T038 [AC-7] [FR-2] [FR-12] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-7Tests.cs for AC-7: Audit failure rolls back mutation (FR-2, FR-12): Given a sensitive change passes validation When audit-event persistence is fault-injected to fail Then the business mutation rolls back And the API returns a generic correlated failure without reporting success.
### US8 - Final Admin safeguard (FR-13) (P3)

**Goal**: Prove AC-8 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-8 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T039 [AC-8] [FR-13] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-8Tests.cs for AC-8: Final Admin safeguard (FR-13): Given exactly two active Admin assignments remain When two replicas concurrently revoke different assignments Then both serialize through AdminSecurityGuard, at most one commits, the loser returns 409 FINAL_ADMIN_REQUIRED, and at least one active Admin remains.
### US9 - Admin operations quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-9 as an independently demonstrable slice of Admin Operations, Audit, and Reporting.

**Independent Test**: Execute only the AC-9 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T031.
- [ ] T040 [AC-9] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-9Tests.cs for AC-9: Admin operations quality gate (NFR-1, NFR-2, NFR-3, NFR-4): Given target operational metrics, a production-size audit dataset, large export, and positive/negative/concurrent admin command matrix When admin quality tests execute Then metrics are no more than 60 seconds stale and show observation time And audit first page returns within 1 second p95 And two replicas competing for one export publish exactly one artifact through a durable lease with authorized status/download, secure expiry, and audit And every admin action passes authorization, audit, concurrency, and anti-forgery checks.
- [ ] T041 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-1Tests.cs and assert: Metrics backend unavailable -> show stale timestamp/degraded state, not fabricated zero.
- [ ] T042 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-2Tests.cs and assert: Export fails/expires -> safe status and authorized retry.
- [ ] T043 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-3Tests.cs and assert: Concurrent admin edit -> 409 current version, no lost update.
- [ ] T044 [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-4Tests.cs and assert: Bulk import partially invalid -> preview errors; publish all-or-nothing.
- [ ] T045 [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-5Tests.cs and assert: Admin disables own final Admin role -> require safeguard/second actor according to security approval.
- [ ] T046 [EC-6] Exercise EC-6 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-6Tests.cs and assert: The same idempotency key is reused with a different admin command payload -> return 409 IDEMPOTENCY_KEY_REUSED and execute neither new payload.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T047 [FR-1] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-1 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no correction workflow and serialized final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: Authorized Admin MUST manage terms/windows, users/roles, student records/holds, catalogue/policies, resources, offerings/groups, and imports only through feature-spec commands.
- [ ] T048 [FR-1] Record the page-to-canonical-owner endpoint/command matrix and passing FR-1 evidence in docs/release-evidence/SPEC-017-FR-1-delegation.md after T047 fails and owner contracts are baselined.
- [ ] T049 [FR-2] [WORKSTREAM-AUDIT-AGGREGATION-AND-ATOMICITY-CONFORMANCE] Create the future failing merge and conformance checks in tests/StudentRegistration.IntegrationTests/Audit/AuditAggregationConformanceTests.cs for shared AuditEvent, Identity SecurityEvent, actor/reason/redacted-before-after/correlation/source fields, and SPEC-004 rollback semantics.
- [ ] T050 [FR-2] Publish the reviewed merged-stream/redaction contract at specs/017-admin-operations-audit-reporting/contracts/audit-source-merge.md after T049 fails; source delivery remains blocked on FR-7/FR-12 tests (depends on T049).
- [ ] T051 [FR-3] [WORKSTREAM-OPERATIONAL-METRICS-QUERY] Create the future failing FR-3 checks in tests/StudentRegistration.IntegrationTests/Admin/AdminMetricsTests.cs. Test focus: timestamped traffic/result/failure/fill/lock/data-quality metrics and degraded state. Prove the requirement against its linked AC/EC fixtures: The system MUST provide registration-window metrics for traffic, success, expected rejections, server failures, fill rates, lock waits, and data-quality alerts.
- [ ] T052 [FR-3] [WORKSTREAM-OPERATIONAL-METRICS-QUERY] Deliver FR-3 through the bounded Operational metrics query workstream at src/StudentRegistration.StaffAdministration/Application/AdminMetricsQuery.cs only after T051 fails for the expected reason (depends on T051): The system MUST provide registration-window metrics for traffic, success, expected rejections, server failures, fill rates, lock waits, and data-quality alerts.
- [ ] T053 [FR-4] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-4 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no correction workflow and serialized final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: Admin orchestration MUST NOT expose enrollment correction, drop, withdrawal, or seat-decrement commands in MVP. Delegated master-data commands MUST preserve capacity and timetable invariants and cannot bypass the owning feature module.
- [ ] T054 [FR-4] Record the prohibited correction/drop/withdraw/seat-decrement and no-bypass evidence in docs/release-evidence/SPEC-017-FR-4-excluded-actions.md after T053 fails.
- [ ] T055 [FR-5] [WORKSTREAM-SCOPED-AUDIT-SEARCH-AND-EXPORT] Create the future failing FR-5 checks in tests/StudentRegistration.AuthorizationTests/AuditExportScopeTests.cs. Test focus: row/field scope, PII minimization, bounded query, durable lease claim, status/download and asynchronous expiring export. Prove the requirement against its linked AC/EC fixtures: Exports MUST enforce the same row/data scope and PII minimization as UI and use an explicit request/status/download lifecycle. ExportJob MUST be durable, owner/scope/request-bound, expiring, and claimed by workers through a conditional SQL lease; only the current lease owner may publish one artifact, and request/download actions MUST be audited.
- [ ] T056 [FR-5] [WORKSTREAM-SCOPED-AUDIT-SEARCH-AND-EXPORT] Deliver FR-5 through the bounded Scoped audit search and export workstream at src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs only after T055 fails for the expected reason (depends on T055): Exports MUST enforce the same row/data scope and PII minimization as UI and use an explicit request/status/download lifecycle. ExportJob MUST be durable, owner/scope/request-bound, expiring, and claimed by workers through a conditional SQL lease; only the current lease owner may publish one artifact, and request/download actions MUST be audited.
- [ ] T057 [FR-6] [WORKSTREAM-SCOPED-AUDIT-SEARCH-AND-EXPORT] Create the future failing FR-6 checks in tests/StudentRegistration.AuthorizationTests/AuditExportScopeTests.cs. Test focus: row/field scope, PII minimization, bounded query, durable lease claim, status/download and asynchronous expiring export. Prove the requirement against its linked AC/EC fixtures: Admin list/search endpoints MUST be paged, filtered, and safely parameterized.
- [ ] T058 [FR-6] [WORKSTREAM-SCOPED-AUDIT-SEARCH-AND-EXPORT] Deliver FR-6 through the bounded Scoped audit search and export workstream at src/StudentRegistration.StaffAdministration/Application/AuditExportService.cs only after T057 fails for the expected reason (depends on T057): Admin list/search endpoints MUST be paged, filtered, and safely parameterized.
- [ ] T059 [FR-7] [WORKSTREAM-AUDIT-AGGREGATION-AND-ATOMICITY-CONFORMANCE] Create the future failing append-only authorization checks for both consumed streams in tests/StudentRegistration.IntegrationTests/Audit/AuditAggregationConformanceTests.cs.
- [ ] T060 [FR-7] Record the passing append-only authorization evidence for both consumed streams in docs/release-evidence/SPEC-017-audit-append-only.md after T059 and the single T050 delivery complete.
- [ ] T061 [FR-8] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-8 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no correction workflow and serialized final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: Import, publication, and other approved sensitive feature-spec mutations MUST use preview and explicit confirmation; this does not authorize enrollment correction.
- [ ] T062 [FR-8] Record owner-endpoint preview/confirmation conformance evidence in docs/release-evidence/SPEC-017-FR-8-owner-preview.md after T061 fails.
- [ ] T063 [FR-9] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the future failing FR-9 checks in tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs. Test focus: feature delegation, preview/confirmation, no capacity/conflict bypass, no correction workflow and serialized final-Admin safeguard. Prove the requirement against its linked AC/EC fixtures: Break-glass behavior MUST NOT exist without a separate approved spec.
- [ ] T064 [FR-9] Record the absence of break-glass routes/services in docs/release-evidence/SPEC-017-FR-9-no-break-glass.md after T063 fails.
- [ ] T065 [FR-10] [WORKSTREAM-ADMIN-PREVIEW-AND-IDEMPOTENCY] Create the future failing FR-10 checks in tests/StudentRegistration.IntegrationTests/Admin/AdminConfirmationConcurrencyTests.cs. Test focus: required expected version, bound preview, expiry, duplicate confirmation, payload mismatch and final-Admin write-skew prevention. Prove the requirement against its linked AC/EC fixtures: Update/delete commands MUST require expected rowversion; retryable creates, imports, exports, and confirmed feature-spec mutations MUST require an idempotency key.
- [ ] T066 [FR-10] Record the canonical owner-endpoint expected-version/idempotency matrix in docs/release-evidence/SPEC-017-FR-10-command-metadata.md after T065 fails.
- [ ] T067 [FR-11] [WORKSTREAM-ADMIN-PREVIEW-AND-IDEMPOTENCY] Create the future failing FR-11 checks in tests/StudentRegistration.IntegrationTests/Admin/AdminConfirmationConcurrencyTests.cs. Test focus: required expected version, bound preview, expiry, duplicate confirmation, payload mismatch and final-Admin write-skew prevention. Prove the requirement against its linked AC/EC fixtures: Preview tokens MUST bind actor, permission scope, canonical payload, dependency versions, and expiry; confirmation MUST reject any changed input, scope, permission, dependency, or expired token.
- [ ] T068 [FR-10] [FR-11] [WORKSTREAM-ADMIN-PREVIEW-AND-IDEMPOTENCY] Deliver the owner-endpoint preview/version/idempotency conformance record at docs/architecture/admin-confirmation-conformance.md only after T065 and T067 fail (depends on T065, T067); explicitly prohibit a generic AdminConfirmationService.
- [ ] T069 [FR-12] [WORKSTREAM-AUDIT-AGGREGATION-AND-ATOMICITY-CONFORMANCE] Create conformance/fault checks in tests/StudentRegistration.IntegrationTests/Audit/AuditAggregationConformanceTests.cs proving feature mutations use the SPEC-004 writer and that audit failure rolls back business state; reject a second SPEC-017 writer.
- [ ] T070 [FR-2] [FR-7] [FR-12] [WORKSTREAM-AUDIT-AGGREGATION-AND-ATOMICITY-CONFORMANCE] Deliver the single read-only merged audit projection and atomicity-conformance query at src/StudentRegistration.StaffAdministration/Application/AuditEventQueries.cs only after T049, T059, and T069 fail (depends on T049, T059, T069); expose no update/delete path and do not deliver a transaction writer.
- [ ] T071 [FR-13] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Create the failing tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs checks that every Admin-role change delegates to SPEC-007, preserves its AdminSecurityGuard/FINAL_ADMIN_REQUIRED result, and adds no RoleAssignment or guard writer in StaffAdministration.
- [ ] T072 [FR-1] [FR-4] [FR-8] [FR-9] [FR-13] [WORKSTREAM-GOVERNED-ADMIN-COMMAND-ORCHESTRATION] Deliver the page-to-owner delegation and excluded-facade conformance record at docs/architecture/admin-command-delegation-conformance.md only after T047, T053, T061, T063, and T071 fail (depends on T047, T053, T061, T063, T071); explicitly prohibit AdminCommandService and preserve SPEC-007's role result.


## Phase 5 - Frontend Route Tests and Integration

- [ ] T073 [ADM-01] [UI-CONTRACT-SPEC-003] [FR-3] [FR-6] [AC-4] [AC-9] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-01 in tests/StudentRegistration.E2ETests/Specs/Spec017/AdminDashboardPageFeatureTests.cs.
- [ ] T074 [ADM-01] [UI-CONTRACT-SPEC-003] [FR-3] [FR-6] [AC-4] [AC-9] Deliver the sole canonical Blazor implementation for ADM-01 at src/StudentRegistration.Client/Pages/AdminDashboardPage.razor after T073 and the SPEC-003 contract/component checks fail for expected reasons (depends on T073).
- [ ] T075 [ADM-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-02 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-02.md without editing the canonical Razor page.
- [ ] T076 [ADM-02] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-02 in tests/StudentRegistration.E2ETests/Specs/Spec017/TermAdministrationPageContributorTests.cs.
- [ ] T077 [ADM-03] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-13] [AC-5] [AC-8] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-03 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-03.md without editing the canonical Razor page.
- [ ] T078 [ADM-03] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-13] [AC-5] [AC-8] Verify the SPEC-017 contribution consumed by ADM-03 in tests/StudentRegistration.E2ETests/Specs/Spec017/UserAdministrationPageContributorTests.cs.
- [ ] T079 [ADM-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-1] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-04 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-04.md without editing the canonical Razor page.
- [ ] T080 [ADM-04] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-1] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-04 in tests/StudentRegistration.E2ETests/Specs/Spec017/StudentAdministrationPageContributorTests.cs.
- [ ] T081 [ADM-05] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-05 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-05.md without editing the canonical Razor page.
- [ ] T082 [ADM-05] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-05 in tests/StudentRegistration.E2ETests/Specs/Spec017/CatalogueAdministrationPageContributorTests.cs.
- [ ] T083 [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-06 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-06.md without editing the canonical Razor page.
- [ ] T084 [ADM-06] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-06 in tests/StudentRegistration.E2ETests/Specs/Spec017/OfferingAdministrationPageContributorTests.cs.
- [ ] T085 [ADM-07] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Finalize SPEC-017 data, actions, stable reasons, authorization, and stale/concurrent contribution for ADM-07 at specs/017-admin-operations-audit-reporting/contracts/routes/ADM-07.md without editing the canonical Razor page.
- [ ] T086 [ADM-07] [UI-CONTRACT-SPEC-003] [FR-1] [FR-2] [FR-8] [FR-10] [FR-11] [FR-12] [AC-5] [AC-6] [AC-7] Verify the SPEC-017 contribution consumed by ADM-07 in tests/StudentRegistration.E2ETests/Specs/Spec017/ResourceAdministrationPageContributorTests.cs.
- [ ] T087 [ADM-08] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-4] [FR-8] [FR-10] [FR-11] [FR-12] [AC-1] [AC-2] [AC-4] [AC-6] [AC-7] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-08 in tests/StudentRegistration.E2ETests/Specs/Spec017/RegistrationAdministrationPageFeatureTests.cs.
- [ ] T088 [ADM-08] [UI-CONTRACT-SPEC-003] [FR-2] [FR-3] [FR-4] [FR-8] [FR-10] [FR-11] [FR-12] [AC-1] [AC-2] [AC-4] [AC-6] [AC-7] Deliver the sole canonical Blazor implementation for ADM-08 at src/StudentRegistration.Client/Pages/RegistrationAdministrationPage.razor after T087 and the SPEC-003 contract/component checks fail for expected reasons (depends on T087).
- [ ] T089 [ADM-09] [UI-CONTRACT-SPEC-003] [FR-2] [FR-5] [FR-6] [FR-7] [FR-10] [FR-12] [AC-3] [AC-7] [AC-9] Create the future failing primary, negative, stale/concurrent, authorization, and server-reason journeys for ADM-09 in tests/StudentRegistration.E2ETests/Specs/Spec017/AuditAdministrationPageFeatureTests.cs.
- [ ] T090 [ADM-09] [UI-CONTRACT-SPEC-003] [FR-2] [FR-5] [FR-6] [FR-7] [FR-10] [FR-12] [AC-3] [AC-7] [AC-9] Deliver the sole canonical Blazor implementation for ADM-09 at src/StudentRegistration.Client/Pages/AuditAdministrationPage.razor after T089 and the SPEC-003 contract/component checks fail for expected reasons (depends on T089).

- [ ] T091 [API-Endpoint01] [API-Endpoint02] [API-Endpoint03] [API-Endpoint04] [API-Endpoint05] Deliver the canonical handlers for GET /api/admin/operations/metrics, GET /api/admin/audit, POST /api/admin/exports, GET /api/admin/exports/{jobId}, and GET /api/admin/exports/{jobId}/download at src/StudentRegistration.StaffAdministration/Endpoints/Spec017Endpoints.cs only after all contract, authorization, audit-atomicity, lease, final-Admin write-skew, acceptance, workstream, and E2E tests T023-T090 fail for expected reasons.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T092 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-017-NFR-1.md: Operational metrics SHOULD be no more than 60 seconds stale and show observation timestamp.
- [ ] T093 [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-017-NFR-2.md: Audit search SHOULD return first page within 1 second p95 at approved retention volume.
- [ ] T094 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-017-NFR-3.md: Export generation MUST be asynchronous/bounded for large data and expire securely.
- [ ] T095 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-017-NFR-4.md: Admin actions MUST have authorization, audit, concurrency, and validation tests.

## Phase 7 - Scope and Release Evidence

- [ ] T096 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-017-scope-review.md that OS-1 remains excluded: Unrestricted super-admin and unaudited direct database edits.
- [ ] T097 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-017-scope-review.md that OS-2 remains excluded: Break-glass capacity/conflict override and any enrollment correction, drop, withdrawal, or seat-decrement workflow.
- [ ] T098 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-017-scope-review.md that OS-3 remains excluded: Business-intelligence warehouse.
- [ ] T099 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-017-scope-review.md that OS-4 remains excluded: Long-term report replica until primary impact is measured.
- [ ] T100 [TRACE] Generate the completed FR/NFR/SC/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-017-traceability.md and reject release if any row lacks passing evidence.
- [ ] T101 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-017 in docs/release-evidence/SPEC-017-release-approval.md.

No task is complete and no implementation file has been created.
