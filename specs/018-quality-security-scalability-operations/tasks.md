# Tasks: Quality, Security, Scalability, and Operations

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Planning Analysis, Dependency Baseline, and Final Human Approval

- [ ] T001 [GATE] Run and record the constitution-compliance review for SPEC-018 in specs/018-quality-security-scalability-operations/checklists/approval.md; this is planning analysis and does not authorize implementation.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-001] Validate the consumed upstream requirements, plan, data model, and API contract at specs/001-product-charter-rbac/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-006] Validate the consumed upstream requirements, plan, data model, and API contract at specs/006-domain-class-api-contracts/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T007 [GATE] Complete dependency validation, cross-spec consistency analysis, model/API/policy/task trace review, and freeze SPEC-018 in specs/018-quality-security-scalability-operations/checklists/implementation-readiness.md; record pass/fail and keep the package In Review. This readiness task does not authorize implementation.
- [ ] T008 [GATE] Only after T001-T007 pass, record Ahmed ELbamby's human approval in specs/018-quality-security-scalability-operations/checklists/approval.md and update the package status to Approved; no later model, test, source, migration, page, or deployment task may begin before this final planning gate is complete.

## Phase 2 - Models and API Contracts

- [ ] T009 [ENTITY-HealthSummary] [OWNER-SPEC-018] Create the future failing invariant/schema/serialization checks for canonical HealthSummary ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec018/HealthSummaryModelTests.cs.
- [ ] T010 [ENTITY-HealthSummary] [OWNER-SPEC-018] Deliver the canonical HealthSummary model or governed artifact at src/StudentRegistration.Contracts/Operations/HealthSummary.cs after T009 fails for the expected reason (depends on T009).
- [ ] T011 [ENTITY-OperationalMetric] [OWNER-SPEC-018] Create the future failing invariant/schema/serialization checks for canonical OperationalMetric ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec018/OperationalMetricModelTests.cs.
- [ ] T012 [ENTITY-OperationalMetric] [OWNER-SPEC-018] Deliver the canonical OperationalMetric model or governed artifact at src/StudentRegistration.Contracts/Operations/OperationalMetric.cs after T011 fails for the expected reason (depends on T011).
- [ ] T013 [ENTITY-StructuredLog] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical StructuredLog ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/StructuredLogSchemaTests.cs.
- [ ] T014 [ENTITY-StructuredLog] [ARTIFACT-OWNER] Deliver the canonical StructuredLog model or governed artifact at docs/operations/telemetry-contract.md after T013 fails for the expected reason (depends on T013).
- [ ] T015 [ENTITY-Trace] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical Trace ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/TraceSchemaTests.cs.
- [ ] T016 [ENTITY-Trace] [ARTIFACT-OWNER] Deliver the canonical Trace model or governed artifact at docs/operations/telemetry-contract.md after T015 fails for the expected reason (depends on T015).
- [ ] T017 [ENTITY-BackupEvidence] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical BackupEvidence ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/BackupEvidenceSchemaTests.cs.
- [ ] T018 [ENTITY-BackupEvidence] [ARTIFACT-OWNER] Deliver the canonical BackupEvidence model or governed artifact at docs/release-evidence/schemas/backup-evidence.schema.json after T017 fails for the expected reason (depends on T017).
- [ ] T019 [ENTITY-ReleaseEvidence] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical ReleaseEvidence ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/ReleaseEvidenceSchemaTests.cs.
- [ ] T020 [ENTITY-ReleaseEvidence] [ARTIFACT-OWNER] Deliver the canonical ReleaseEvidence model or governed artifact at docs/release-evidence/schemas/release-evidence.schema.json after T019 fails for the expected reason (depends on T019).
- [ ] T021 [ENTITY-ThreatModel] [ARTIFACT-OWNER] Create future failing schema, scope, freshness, mitigation, residual-risk, owner, and review-gate checks in tests/StudentRegistration.SecurityTests/ThreatModelGateTests.cs.
- [ ] T022 [ENTITY-ThreatModel] [ARTIFACT-OWNER] Deliver the canonical versioned STRIDE ThreatModel at docs/security/THREAT_MODEL.md after T021 fails, covering every FR-9 trust boundary and release blocker (depends on T021).
- [ ] T023 [API-Endpoint01] [OWNER-SPEC-018] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/health in specs/018-quality-security-scalability-operations/contracts/api.md.
- [ ] T024 [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/health in tests/StudentRegistration.ContractTests/Specs/Spec018/Endpoint01ContractTests.cs.
- [ ] T025 [API-Endpoint01] [FR-3] Create future failing safe-summary, SQL-down/degraded, exporter-down, no-topology/secret, and unauthenticated health endpoint behavior tests in tests/StudentRegistration.OperationsTests/Specs/Spec018/Endpoint01BehaviorTests.cs; handler delivery is deferred until linked AC/FR tests fail.
- [ ] T026 [API-Endpoint02] [OWNER-SPEC-018] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/operations/metrics in specs/018-quality-security-scalability-operations/contracts/api.md.
- [ ] T027 [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/operations/metrics in tests/StudentRegistration.ContractTests/Specs/Spec018/Endpoint02ContractTests.cs.
- [ ] T028 [API-Endpoint02] [FR-3] [FR-4] Create future failing restricted authorization, freshness, cardinality, PII-redaction, degraded-source, and timestamp endpoint behavior tests in tests/StudentRegistration.OperationsTests/Specs/Spec018/Endpoint02BehaviorTests.cs; handler delivery is deferred.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Target load (FR-4, NFR-2, NFR-3, NFR-4) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [ ] T029 [SC-1] [AC-1] [FR-4] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-1Tests.cs for AC-1: Target load (FR-4, NFR-2, NFR-3, NFR-4): Given a production-like database and exact NFR-2 mix When 75 submissions/s plus 300 reads/s run for 10 minutes Then p95 budgets and unexpected error rate pass And no capacity/duplicate/partial invariant fails.
### US2 - Double and spike load (NFR-2, NFR-4) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [ ] T030 [SC-1] [AC-2] [NFR-2] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-2Tests.cs for AC-2: Double and spike load (NFR-2, NFR-4): Given target correctness passes When 150 submissions/s plus 600 reads/s run for 10 minutes and 375 submissions/s plus 1,500 reads/s run for 60 seconds using the exact NFR-2 mixes Then invariant correctness remains zero-defect And any graceful degradation is documented against approved thresholds.
### US3 - Restore rehearsal (FR-5, NFR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [ ] T031 [SC-3] [AC-3] [FR-5] [NFR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-3Tests.cs for AC-3: Restore rehearsal (FR-5, NFR-7): Given a production-like backup and clean recovery environment When the runbook is executed Then data is restored within RTO And measured data loss is within RPO And integrity/reconciliation checks pass.
### US4 - Accessibility gate (FR-8, NFR-8) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [ ] T032 [SC-2] [AC-4] [FR-8] [NFR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-4Tests.cs for AC-4: Accessibility gate (FR-8, NFR-8): Given critical student/staff routes in staging When automated, keyboard, and representative screen-reader tests run Then no serious automated issue or critical/major manual barrier remains.
### US5 - Security release gate (FR-6, FR-9) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [ ] T033 [AC-5] [FR-6] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-5Tests.cs for AC-5: Security release gate (FR-6, FR-9): Given the versioned STRIDE model plus dependency, secret, static/dynamic and authorization reviews are complete When release readiness is evaluated Then the threat model has owner/mitigation/residual-risk approval and no unresolved critical/high finding remains And protected resources pass negative ownership/role tests.
### US6 - CI quality sequence (FR-1) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [ ] T034 [AC-6] [FR-1] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-6Tests.cs for AC-6: CI quality sequence (FR-1): Given a pull request changes application behavior When CI executes Then restore/format/build, unit/architecture, SQL integration/migration, E2E/accessibility and security checks run in the approved order And any required gate failure blocks merge.
### US7 - Complete operational proof (FR-2, FR-3, FR-7, NFR-1, NFR-5, NFR-6, NFR-9) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [ ] T035 [AC-7] [FR-2] [FR-3] [FR-7] [NFR-1] [NFR-5] [NFR-6] [NFR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-7Tests.cs for AC-7: Complete operational proof (FR-2, FR-3, FR-7, NFR-1, NFR-5, NFR-6, NFR-9): Given the 25,000-account/5,000-session production-like fixture, two stateless replicas with shared Data Protection keys, observability collectors, and the critical eligibility/conflict/capacity suites When the release evidence pipeline and 120-minute target-mix registration-window soak execute Then every boundary/concurrency test passes And safe health/log/metric/trace signals are available And availability is at least 99.9% during the test window And unexpected failure rate is below 0.1% And critical rule branch coverage is at least 90%.
- [ ] T036 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-1Tests.cs and assert: Observability exporter unavailable -> application remains functional with bounded buffering/fallback logs.
- [ ] T037 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-2Tests.cs and assert: One app instance fails -> load balancer removes it; other instance continues with shared auth keys/database.
- [ ] T038 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-3Tests.cs and assert: SQL unavailable -> fail safely, no partial result; health turns unhealthy and user gets reference ID.
- [ ] T039 [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-4Tests.cs and assert: Migration validation differs from production compatibility -> stop deployment before application traffic.
- [ ] T040 [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-5Tests.cs and assert: Load targets prove unrealistic -> rebaseline by approved spec change, never silently relax correctness.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T041 [FR-1] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-1 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: CI MUST run restore, formatting, warnings-as-errors build, unit, architecture, real-SQL integration, migration, E2E, and security checks.
- [ ] T042 [FR-1] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-1 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T041 fails for the expected reason (depends on T041): CI MUST run restore, formatting, warnings-as-errors build, unit, architecture, real-SQL integration, migration, E2E, and security checks.
- [ ] T043 [FR-2] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-2 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: Critical domain rules and capacity logic MUST have automated boundary and concurrency tests before implementation is accepted.
- [ ] T044 [FR-2] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-2 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T043 fails for the expected reason (depends on T043): Critical domain rules and capacity logic MUST have automated boundary and concurrency tests before implementation is accepted.
- [ ] T045 [FR-3] [WORKSTREAM-OBSERVABILITY] Create the future failing FR-3 checks in tests/StudentRegistration.OperationsTests/ObservabilitySignalTests.cs. Test focus: safe health/log/metric/trace signals, correlation, latency, throughput, conflicts, lock waits and mismatch alerts. Prove the requirement against its linked AC/EC fixtures: The application MUST expose authenticated-safe health, logs, metrics, traces, and correlation IDs.
- [ ] T046 [FR-3] [WORKSTREAM-OBSERVABILITY] Deliver FR-3 through the bounded Observability workstream at src/StudentRegistration.Api/Operations/ObservabilityExtensions.cs only after T045 fails for the expected reason (depends on T045): The application MUST expose authenticated-safe health, logs, metrics, traces, and correlation IDs.
- [ ] T047 [FR-4] [WORKSTREAM-OBSERVABILITY] Create the future failing FR-4 checks in tests/StudentRegistration.OperationsTests/ObservabilitySignalTests.cs. Test focus: safe health/log/metric/trace signals, correlation, latency, throughput, conflicts, lock waits and mismatch alerts. Prove the requirement against its linked AC/EC fixtures: Operations MUST monitor latency, throughput, unexpected error rate, business rejection codes, optimizer time, SQL latency, lock waits, deadlocks, capacity conflicts, and counter reconciliation.
- [ ] T048 [FR-4] [WORKSTREAM-OBSERVABILITY] Deliver FR-4 through the bounded Observability workstream at src/StudentRegistration.Api/Operations/ObservabilityExtensions.cs only after T047 fails for the expected reason (depends on T047): Operations MUST monitor latency, throughput, unexpected error rate, business rejection codes, optimizer time, SQL latency, lock waits, deadlocks, capacity conflicts, and counter reconciliation.
- [ ] T049 [FR-5] [WORKSTREAM-RECOVERY-AND-ROLLBACK] Create the future failing FR-5 checks in tests/StudentRegistration.RecoveryTests/RecoveryRehearsalTests.cs. Test focus: backup/restore, migration rollback, application rollback, RPO and RTO evidence. Prove the requirement against its linked AC/EC fixtures: Backup/restore, migration rollback, and application rollback MUST be rehearsed before release.
- [ ] T050 [FR-5] [WORKSTREAM-RECOVERY-AND-ROLLBACK] Deliver FR-5 through the bounded Recovery and rollback workstream at docs/runbooks/RECOVERY_AND_ROLLBACK.md only after T049 fails for the expected reason (depends on T049): Backup/restore, migration rollback, and application rollback MUST be rehearsed before release.
- [ ] T051 [FR-6] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Create the future failing FR-6 checks in tests/StudentRegistration.SecurityTests/SecretAndDataProtectionTests.cs. Test focus: approved secret provider, SQL-backed protected shared Data Protection keys, no source secrets and cross-replica sessions. Prove the requirement against its linked AC/EC fixtures: Production secrets and the Data Protection at-rest protection certificate/key MUST come from the approved secret-store interface and MUST NOT appear in Git, checked-in configuration, or logs. Production startup MUST fail closed when required secret/key material is unavailable.
- [ ] T052 [FR-6] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Deliver FR-6 through the bounded Secrets and replica keys workstream at src/StudentRegistration.Api/Operations/SecurityConfiguration.cs only after T051 fails for the expected reason (depends on T051): Production secrets and the Data Protection at-rest protection certificate/key MUST come from the approved secret-store interface and MUST NOT appear in Git, checked-in configuration, or logs. Production startup MUST fail closed when required secret/key material is unavailable.
- [ ] T053 [FR-7] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Create the future failing FR-7 checks in tests/StudentRegistration.SecurityTests/SecretAndDataProtectionTests.cs. Test focus: approved secret provider, SQL-backed protected shared Data Protection keys, no source secrets and cross-replica sessions. Prove the requirement against its linked AC/EC fixtures: Application replicas MUST remain stateless and use one shared SQL Server Data Protection key repository, encrypted at rest by certificate/key material obtained through FR-6. Cross-replica authentication and protected option-token tests MUST pass with no sticky session.
- [ ] T054 [FR-7] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Deliver FR-7 through the bounded Secrets and replica keys workstream at src/StudentRegistration.Api/Operations/SecurityConfiguration.cs only after T053 fails for the expected reason (depends on T053): Application replicas MUST remain stateless and use one shared SQL Server Data Protection key repository, encrypted at rest by certificate/key material obtained through FR-6. Cross-replica authentication and protected option-token tests MUST pass with no sticky session.
- [ ] T055 [FR-8] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-8 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: Critical flows MUST pass automated accessibility checks plus manual keyboard and representative NVDA/Windows screen-reader journeys. A dated evidence record MUST identify tester, assistive technology/version, route, scenario, result, defect links, and UX/QA sign-off; automation alone cannot satisfy this requirement.
- [ ] T056 [FR-8] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-8 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T055 fails for the expected reason (depends on T055): Critical flows MUST pass automated accessibility checks plus manual keyboard and representative NVDA/Windows screen-reader journeys. A dated evidence record MUST identify tester, assistive technology/version, route, scenario, result, defect links, and UX/QA sign-off; automation alone cannot satisfy this requirement.
- [ ] T057 [FR-9] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-9 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: A versioned STRIDE threat model MUST cover trust boundaries, assets, identity/session, authorization/data scope, protected option tokens, registration races, Admin/audit/export, SQL, telemetry, secrets, and deployment. Security review MUST record mitigations, residual risk, and owner. Release MUST be blocked by an unreviewed/stale threat model, unresolved critical/high security issue, invariant failure, or critical/major core usability defect.
- [ ] T058 [FR-9] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-9 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T057 fails for the expected reason (depends on T057): A versioned STRIDE threat model MUST cover trust boundaries, assets, identity/session, authorization/data scope, protected option tokens, registration races, Admin/audit/export, SQL, telemetry, secrets, and deployment. Security review MUST record mitigations, residual risk, and owner. Release MUST be blocked by an unreviewed/stale threat model, unresolved critical/high security issue, invariant failure, or critical/major core usability defect.


## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

- [ ] T059 [API-Endpoint01] [API-Endpoint02] Deliver the canonical handlers for GET /api/health and GET /api/operations/metrics at src/StudentRegistration.Api/Endpoints/Spec018Endpoints.cs only after all contract, safe-signal, authorization, threat-model, shared-key, accessibility, and release-gate tests T023-T058 fail for expected reasons.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T060 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-1.md: The production-like validation environment MUST support a planning baseline of 25,000 accounts and 5,000 concurrent authenticated sessions, pending S0 rebaseline.
- [ ] T061 [NFR-2] [AUTOMATED-EVIDENCE] Produce executable/evidence profiles in tests/StudentRegistration.LoadTests/Specs/Spec018/ExactLoadProfiles.cs and docs/release-evidence/SPEC-018-NFR-2.md for target 10m 75+300/s, 2x 10m 150+600/s, burst 60s 200+300/s, and 5x 60s 375+1500/s; enforce read mix 50/25/15/10 and submission mix 70/20/10 exactly.
- [ ] T062 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-3.md: Catalogue p95 MUST be <= 300 ms, commit p95 MUST be <= 2 s, and optimizer p95 MUST be <= 500 ms for the approved workload.
- [ ] T063 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-4.md: Tests MUST demonstrate zero overbooking, zero duplicate active offering enrollment, and zero partial atomic submissions at target, 2x, burst, 5x spike, failover, and soak load.
- [ ] T064 [NFR-5] [AUTOMATED-EVIDENCE] Run and record a 120-minute target-mix soak across at least two replicas in tests/StudentRegistration.LoadTests/Specs/Spec018/RegistrationWindowSoak.cs and docs/release-evidence/SPEC-018-NFR-5.md; measure availability >=99.9% and unexpected failures <0.1% over that exact window.
- [ ] T065 [NFR-6] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-6 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-6EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-6.md: Unexpected server failure rate MUST be < 0.1% at target load.
- [ ] T066 [NFR-7] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-7 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-7EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-7.md: RPO MUST be <= 5 minutes and RTO <= 1 hour.
- [ ] T067 [NFR-8] [SC-2] [MANUAL-AND-AUTOMATED-EVIDENCE] Run automated WCAG checks and record manual keyboard plus representative NVDA/Windows journeys in tests/StudentRegistration.AccessibilityTests/Specs/Spec018/NFR-8EvidenceTests.cs and docs/release-evidence/SPEC-018-screen-reader-manual.md. The manual record MUST include tester, date, NVDA/version, route, scenario, result, defect links, and UX/QA sign-off; release fails if it is absent or unsigned.
- [ ] T068 [NFR-9] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-9 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-9EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-9.md: Eligibility/conflict/capacity code SHOULD reach >= 90% branch coverage; coverage never replaces behavior tests.

## Phase 7 - Scope and Release Evidence

- [ ] T069 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-1 remains excluded: Final production hosting/vendor selection.
- [ ] T070 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-2 remains excluded: Kubernetes by default.
- [ ] T071 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-3 remains excluded: 24/7 SLO outside announced registration windows until approved.
- [ ] T072 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-4 remains excluded: Arbitrary collection of student PII in telemetry.
- [ ] T073 [TRACE] Generate the completed FR/NFR/SC/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-018-traceability.md and reject release if any row lacks passing evidence.
- [ ] T074 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-018 in docs/release-evidence/SPEC-018-release-approval.md.

No task is complete and no implementation file has been created.
