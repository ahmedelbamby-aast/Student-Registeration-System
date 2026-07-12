# Tasks: Quality, Security, Scalability, and Operations

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-018 in specs/018-quality-security-scalability-operations/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T003 [DEP-SPEC-001] Validate the consumed upstream requirements, plan, data model, and API contract at specs/001-product-charter-rbac/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T004 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T005 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T006 [DEP-SPEC-006] Validate the consumed upstream requirements, plan, data model, and API contract at specs/006-domain-class-api-contracts/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [ ] T007 [GATE] Freeze SPEC-018 requirements, API, data-model, policy approvals, and dependency versions in specs/018-quality-security-scalability-operations/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T008 [P] [ENTITY-HealthSummary] [OWNER-SPEC-018] Create the future failing invariant/schema/serialization checks for canonical HealthSummary ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec018/HealthSummaryModelTests.cs.
- [ ] T009 [ENTITY-HealthSummary] [OWNER-SPEC-018] Deliver the canonical HealthSummary model or governed artifact at src/StudentRegistration.Contracts/Operations/HealthSummary.cs after T008 fails for the expected reason (depends on T008).
- [ ] T010 [P] [ENTITY-OperationalMetric] [OWNER-SPEC-018] Create the future failing invariant/schema/serialization checks for canonical OperationalMetric ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec018/OperationalMetricModelTests.cs.
- [ ] T011 [ENTITY-OperationalMetric] [OWNER-SPEC-018] Deliver the canonical OperationalMetric model or governed artifact at src/StudentRegistration.Contracts/Operations/OperationalMetric.cs after T010 fails for the expected reason (depends on T010).
- [ ] T012 [P] [ENTITY-StructuredLog] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical StructuredLog ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/StructuredLogSchemaTests.cs.
- [ ] T013 [ENTITY-StructuredLog] [ARTIFACT-OWNER] Deliver the canonical StructuredLog model or governed artifact at docs/operations/telemetry-contract.md after T012 fails for the expected reason (depends on T012).
- [ ] T014 [P] [ENTITY-Trace] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical Trace ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/TraceSchemaTests.cs.
- [ ] T015 [ENTITY-Trace] [ARTIFACT-OWNER] Deliver the canonical Trace model or governed artifact at docs/operations/telemetry-contract.md after T014 fails for the expected reason (depends on T014).
- [ ] T016 [P] [ENTITY-BackupEvidence] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical BackupEvidence ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/BackupEvidenceSchemaTests.cs.
- [ ] T017 [ENTITY-BackupEvidence] [ARTIFACT-OWNER] Deliver the canonical BackupEvidence model or governed artifact at docs/release-evidence/schemas/backup-evidence.schema.json after T016 fails for the expected reason (depends on T016).
- [ ] T018 [P] [ENTITY-ReleaseEvidence] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical ReleaseEvidence ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/ReleaseEvidenceSchemaTests.cs.
- [ ] T019 [ENTITY-ReleaseEvidence] [ARTIFACT-OWNER] Deliver the canonical ReleaseEvidence model or governed artifact at docs/release-evidence/schemas/release-evidence.schema.json after T018 fails for the expected reason (depends on T018).
- [ ] T020 [API-Endpoint01] [OWNER-SPEC-018] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/health in specs/018-quality-security-scalability-operations/contracts/api.md.
- [ ] T021 [P] [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/health in tests/StudentRegistration.ContractTests/Specs/Spec018/Endpoint01ContractTests.cs.
- [ ] T022 [API-Endpoint01] [OWNER-SPEC-018] Deliver the sole canonical GET /api/health handler at src/StudentRegistration.Server/Modules/Operations/Endpoints/Spec018Endpoints.cs after T021 fails for the expected reason (depends on T021).
- [ ] T023 [API-Endpoint02] [OWNER-SPEC-018] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/operations/metrics in specs/018-quality-security-scalability-operations/contracts/api.md.
- [ ] T024 [P] [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/operations/metrics in tests/StudentRegistration.ContractTests/Specs/Spec018/Endpoint02ContractTests.cs.
- [ ] T025 [API-Endpoint02] [OWNER-SPEC-018] Deliver the sole canonical GET /api/operations/metrics handler at src/StudentRegistration.Server/Modules/Operations/Endpoints/Spec018Endpoints.cs after T024 fails for the expected reason (depends on T024).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Target load (FR-4, NFR-2, NFR-3, NFR-4) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T025.
- [ ] T026 [P] [AC-1] [FR-4] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-1Tests.cs for AC-1: Target load (FR-4, NFR-2, NFR-3, NFR-4): Given a production-like database and target traffic mix When target load runs for the specified duration Then p95 budgets and unexpected error rate pass And no capacity/duplicate/partial invariant fails.
### US2 - Double and spike load (NFR-2, NFR-4) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T025.
- [ ] T027 [P] [AC-2] [NFR-2] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-2Tests.cs for AC-2: Double and spike load (NFR-2, NFR-4): Given target correctness already passes When 2x target and a short 5x spike run Then invariant correctness remains zero-defect And any graceful degradation is documented against approved thresholds.
### US3 - Restore rehearsal (FR-5, NFR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T025.
- [ ] T028 [P] [AC-3] [FR-5] [NFR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-3Tests.cs for AC-3: Restore rehearsal (FR-5, NFR-7): Given a production-like backup and clean recovery environment When the runbook is executed Then data is restored within RTO And measured data loss is within RPO And integrity/reconciliation checks pass.
### US4 - Accessibility gate (FR-8, NFR-8) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T025.
- [ ] T029 [P] [AC-4] [FR-8] [NFR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-4Tests.cs for AC-4: Accessibility gate (FR-8, NFR-8): Given critical student/staff routes in staging When automated, keyboard, and representative screen-reader tests run Then no serious automated issue or critical/major manual barrier remains.
### US5 - Security release gate (FR-6, FR-9) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T025.
- [ ] T030 [P] [AC-5] [FR-6] [FR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-5Tests.cs for AC-5: Security release gate (FR-6, FR-9): Given dependency, secret, static/dynamic and authorization reviews complete When release readiness is evaluated Then no unresolved critical/high finding remains And protected resources pass negative ownership/role tests.
### US6 - CI quality sequence (FR-1) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T025.
- [ ] T031 [P] [AC-6] [FR-1] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-6Tests.cs for AC-6: CI quality sequence (FR-1): Given a pull request changes application behavior When CI executes Then restore/format/build, unit/architecture, SQL integration/migration, E2E/accessibility and security checks run in the approved order And any required gate failure blocks merge.
### US7 - Complete operational proof (FR-2, FR-3, FR-7, NFR-1, NFR-5, NFR-6, NFR-9) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T025.
- [ ] T032 [P] [AC-7] [FR-2] [FR-3] [FR-7] [NFR-1] [NFR-5] [NFR-6] [NFR-9] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-7Tests.cs for AC-7: Complete operational proof (FR-2, FR-3, FR-7, NFR-1, NFR-5, NFR-6, NFR-9): Given the 25,000-account/5,000-session production-like fixture, two stateless replicas with shared Data Protection keys, observability collectors, and the critical eligibility/conflict/capacity suites When the release evidence pipeline and registration-window soak execute Then every boundary/concurrency test passes And safe health/log/metric/trace signals are available And availability is at least 99.9% during the test window And unexpected failure rate is below 0.1% And critical rule branch coverage is at least 90%.
- [ ] T033 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-1Tests.cs and assert: Observability exporter unavailable -> application remains functional with bounded buffering/fallback logs.
- [ ] T034 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-2Tests.cs and assert: One app instance fails -> load balancer removes it; other instance continues with shared auth keys/database.
- [ ] T035 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-3Tests.cs and assert: SQL unavailable -> fail safely, no partial result; health turns unhealthy and user gets reference ID.
- [ ] T036 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-4Tests.cs and assert: Migration validation differs from production compatibility -> stop deployment before application traffic.
- [ ] T037 [P] [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-5Tests.cs and assert: Load targets prove unrealistic -> rebaseline by approved spec change, never silently relax correctness.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T038 [P] [FR-1] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-1 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: CI MUST run restore, formatting, warnings-as-errors build, unit, architecture, real-SQL integration, migration, E2E, and security checks.
- [ ] T039 [FR-1] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-1 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T038 fails for the expected reason (depends on T038): CI MUST run restore, formatting, warnings-as-errors build, unit, architecture, real-SQL integration, migration, E2E, and security checks.
- [ ] T040 [P] [FR-2] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-2 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: Critical domain rules and capacity logic MUST have automated boundary and concurrency tests before implementation is accepted.
- [ ] T041 [FR-2] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-2 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T040 fails for the expected reason (depends on T040): Critical domain rules and capacity logic MUST have automated boundary and concurrency tests before implementation is accepted.
- [ ] T042 [P] [FR-3] [WORKSTREAM-OBSERVABILITY] Create the future failing FR-3 checks in tests/StudentRegistration.OperationsTests/ObservabilitySignalTests.cs. Test focus: safe health/log/metric/trace signals, correlation, latency, throughput, conflicts, lock waits and mismatch alerts. Prove the requirement against its linked AC/EC fixtures: The application MUST expose authenticated-safe health, logs, metrics, traces, and correlation IDs.
- [ ] T043 [FR-3] [WORKSTREAM-OBSERVABILITY] Deliver FR-3 through the bounded Observability workstream at src/StudentRegistration.Server/Operations/ObservabilityExtensions.cs only after T042 fails for the expected reason (depends on T042): The application MUST expose authenticated-safe health, logs, metrics, traces, and correlation IDs.
- [ ] T044 [P] [FR-4] [WORKSTREAM-OBSERVABILITY] Create the future failing FR-4 checks in tests/StudentRegistration.OperationsTests/ObservabilitySignalTests.cs. Test focus: safe health/log/metric/trace signals, correlation, latency, throughput, conflicts, lock waits and mismatch alerts. Prove the requirement against its linked AC/EC fixtures: Operations MUST monitor latency, throughput, unexpected error rate, business rejection codes, optimizer time, SQL latency, lock waits, deadlocks, capacity conflicts, and counter reconciliation.
- [ ] T045 [FR-4] [WORKSTREAM-OBSERVABILITY] Deliver FR-4 through the bounded Observability workstream at src/StudentRegistration.Server/Operations/ObservabilityExtensions.cs only after T044 fails for the expected reason (depends on T044): Operations MUST monitor latency, throughput, unexpected error rate, business rejection codes, optimizer time, SQL latency, lock waits, deadlocks, capacity conflicts, and counter reconciliation.
- [ ] T046 [P] [FR-5] [WORKSTREAM-RECOVERY-AND-ROLLBACK] Create the future failing FR-5 checks in tests/StudentRegistration.RecoveryTests/RecoveryRehearsalTests.cs. Test focus: backup/restore, migration rollback, application rollback, RPO and RTO evidence. Prove the requirement against its linked AC/EC fixtures: Backup/restore, migration rollback, and application rollback MUST be rehearsed before release.
- [ ] T047 [FR-5] [WORKSTREAM-RECOVERY-AND-ROLLBACK] Deliver FR-5 through the bounded Recovery and rollback workstream at docs/runbooks/RECOVERY_AND_ROLLBACK.md only after T046 fails for the expected reason (depends on T046): Backup/restore, migration rollback, and application rollback MUST be rehearsed before release.
- [ ] T048 [P] [FR-6] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Create the future failing FR-6 checks in tests/StudentRegistration.SecurityTests/SecretAndDataProtectionTests.cs. Test focus: approved secret provider, no source secrets, shared Data Protection keys and cross-replica sessions. Prove the requirement against its linked AC/EC fixtures: Production secrets MUST use an approved secret store and MUST NOT appear in Git/config/logs.
- [ ] T049 [FR-6] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Deliver FR-6 through the bounded Secrets and replica keys workstream at src/StudentRegistration.Server/Operations/SecurityConfiguration.cs only after T048 fails for the expected reason (depends on T048): Production secrets MUST use an approved secret store and MUST NOT appear in Git/config/logs.
- [ ] T050 [P] [FR-7] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Create the future failing FR-7 checks in tests/StudentRegistration.SecurityTests/SecretAndDataProtectionTests.cs. Test focus: approved secret provider, no source secrets, shared Data Protection keys and cross-replica sessions. Prove the requirement against its linked AC/EC fixtures: Application replicas MUST share Data Protection keys and remain stateless.
- [ ] T051 [FR-7] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Deliver FR-7 through the bounded Secrets and replica keys workstream at src/StudentRegistration.Server/Operations/SecurityConfiguration.cs only after T050 fails for the expected reason (depends on T050): Application replicas MUST share Data Protection keys and remain stateless.
- [ ] T052 [P] [FR-8] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-8 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: Critical flows MUST pass automated and manual accessibility tests.
- [ ] T053 [FR-8] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-8 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T052 fails for the expected reason (depends on T052): Critical flows MUST pass automated and manual accessibility tests.
- [ ] T054 [P] [FR-9] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-9 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: Release MUST be blocked by unresolved critical/high security issues, invariant failures, or critical/major core usability defects.
- [ ] T055 [FR-9] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-9 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T054 fails for the expected reason (depends on T054): Release MUST be blocked by unresolved critical/high security issues, invariant failures, or critical/major core usability defects.

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T056 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-1.md: The production-like validation environment MUST support a planning baseline of 25,000 accounts and 5,000 concurrent authenticated sessions, pending S0 rebaseline.
- [ ] T057 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-2.md: The system MUST support 75 submissions/s for 10 min, 200/s for 60 s, and 300 read/s.
- [ ] T058 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-3.md: Catalogue p95 MUST be <= 300 ms, commit p95 MUST be <= 2 s, and optimizer p95 MUST be <= 500 ms for the approved workload.
- [ ] T059 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-4.md: Tests MUST demonstrate zero overbooking, duplicate active offering enrollment, and partial atomic submission.
- [ ] T060 [P] [NFR-5] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-5 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-5EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-5.md: Availability MUST be 99.9% during announced registration windows.
- [ ] T061 [P] [NFR-6] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-6 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-6EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-6.md: Unexpected server failure rate MUST be < 0.1% at target load.
- [ ] T062 [P] [NFR-7] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-7 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-7EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-7.md: RPO MUST be <= 5 minutes and RTO <= 1 hour.
- [ ] T063 [P] [NFR-8] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-8 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-8EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-8.md: Critical flows MUST meet WCAG 2.2 AA.
- [ ] T064 [P] [NFR-9] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-9 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-9EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-9.md: Eligibility/conflict/capacity code SHOULD reach >= 90% branch coverage; coverage never replaces behavior tests.

## Phase 7 - Scope and Release Evidence

- [ ] T065 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-1 remains excluded: Final production hosting/vendor selection.
- [ ] T066 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-2 remains excluded: Kubernetes by default.
- [ ] T067 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-3 remains excluded: 24/7 SLO outside announced registration windows until approved.
- [ ] T068 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-4 remains excluded: Arbitrary collection of student PII in telemetry.
- [ ] T069 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-018-traceability.md and reject release if any row lacks passing evidence.
- [ ] T070 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-018 in docs/release-evidence/SPEC-018-release-approval.md.

No task is complete and no implementation file has been created.
