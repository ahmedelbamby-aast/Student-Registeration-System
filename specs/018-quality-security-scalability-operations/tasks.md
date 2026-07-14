# Tasks: Quality, Security, Scalability, and Operations

**Status**: Approved for non-production demo implementation by Ahmed ELbamby on 2026-07-13; dependency/readiness baseline frozen on 2026-07-14.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Phase 1 is complete; every later task names an exact future file and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Planning Baseline and Recorded Gate A Verification

- [x] T001 [GATE] Run and record the constitution-compliance review for SPEC-018 in specs/018-quality-security-scalability-operations/checklists/approval.md; this is planning analysis and does not authorize implementation.
- [x] T002 [DEP-SPEC-003] Validate the consumed upstream requirements, plan, data model, and API contract at specs/003-ux-storyboard-accessibility/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [x] T003 [DEP-SPEC-001] Validate the consumed upstream requirements, plan, data model, and API contract at specs/001-product-charter-rbac/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [x] T004 [DEP-SPEC-004] Validate the consumed upstream requirements, plan, data model, and API contract at specs/004-architecture-engineering-principles/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [x] T005 [DEP-SPEC-005] Validate the consumed upstream requirements, plan, data model, and API contract at specs/005-erd-data-lifecycle/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [x] T006 [DEP-SPEC-006] Validate the consumed upstream requirements, plan, data model, and API contract at specs/006-domain-class-api-contracts/ and record the accepted versions in specs/018-quality-security-scalability-operations/dependency-baseline.md.
- [x] T007 [GATE] Complete dependency validation, cross-spec consistency analysis, model/API/policy/task trace review, and verify the approved SPEC-018 baseline in specs/018-quality-security-scalability-operations/checklists/implementation-readiness.md; record pass/fail and return the package to In Review if this gate fails.
- [x] T008 [GATE] Before any later model, test, source, migration, page, or deployment task, verify Ahmed ELbamby's 2026-07-13 Gate A demo approval recorded in specs/018-quality-security-scalability-operations/clarifications.md remains current; a superseding baseline change returns the package to In Review.

## Phase 2 - Models and API Contracts

- [x] T009 [ENTITY-HealthSummary] [OWNER-SPEC-018] Create the future failing invariant/schema/serialization checks for canonical HealthSummary ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec018/HealthSummaryModelTests.cs.
- [x] T010 [ENTITY-HealthSummary] [OWNER-SPEC-018] Deliver the canonical HealthSummary model or governed artifact at src/StudentRegistration.Contracts/Operations/HealthSummary.cs after T009 fails for the expected reason (depends on T009).
- [x] T011 [ENTITY-OperationalMetric] [OWNER-SPEC-018] Create the future failing invariant/schema/serialization checks for canonical OperationalMetric ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec018/OperationalMetricModelTests.cs.
- [x] T012 [ENTITY-OperationalMetric] [OWNER-SPEC-018] Deliver the canonical OperationalMetric model or governed artifact at src/StudentRegistration.Contracts/Operations/OperationalMetric.cs after T011 fails for the expected reason (depends on T011).
- [x] T013 [ENTITY-StructuredLog] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical StructuredLog ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/StructuredLogSchemaTests.cs.
- [x] T014 [ENTITY-StructuredLog] [ARTIFACT-OWNER] Deliver the canonical StructuredLog model or governed artifact at docs/operations/telemetry-contract.md after T013 fails for the expected reason (depends on T013).
- [x] T015 [ENTITY-Trace] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical Trace ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/TraceSchemaTests.cs.
- [x] T016 [ENTITY-Trace] [ARTIFACT-OWNER] Deliver the canonical Trace model or governed artifact at docs/operations/telemetry-contract.md after T015 fails for the expected reason (depends on T015).
- [x] T017 [ENTITY-BackupEvidence] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical BackupEvidence ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/BackupEvidenceSchemaTests.cs.
- [x] T018 [ENTITY-BackupEvidence] [ARTIFACT-OWNER] Deliver the canonical BackupEvidence model or governed artifact at docs/release-evidence/schemas/backup-evidence.schema.json after T017 fails for the expected reason (depends on T017).
- [x] T019 [ENTITY-ReleaseEvidence] [ARTIFACT-OWNER] Create the future failing invariant/schema/serialization checks for canonical ReleaseEvidence ownership in tests/StudentRegistration.SpecificationTests/Specs/Spec018/ReleaseEvidenceSchemaTests.cs.
- [x] T020 [ENTITY-ReleaseEvidence] [ARTIFACT-OWNER] Deliver the canonical ReleaseEvidence model or governed artifact at docs/release-evidence/schemas/release-evidence.schema.json after T019 fails for the expected reason (depends on T019).
- [x] T021 [ENTITY-ThreatModel] [ARTIFACT-OWNER] Create the minimal tests/StudentRegistration.SecurityTests project shell and solution entry, then create future failing schema, scope, freshness, mitigation, residual-risk, owner, and review-gate checks in tests/StudentRegistration.SecurityTests/ThreatModelGateTests.cs.
- [x] T022 [ENTITY-ThreatModel] [ARTIFACT-OWNER] Deliver the canonical versioned STRIDE ThreatModel at docs/security/THREAT_MODEL.md after T021 fails, covering every FR-9 trust boundary and release blocker (depends on T021).
- [x] T023 [API-Endpoint01] [OWNER-SPEC-018] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/health in specs/018-quality-security-scalability-operations/contracts/api.md.
- [x] T024 [API-Endpoint01] Verify every documented response and authorization outcome for GET /api/health in tests/StudentRegistration.ContractTests/Specs/Spec018/Endpoint01ContractTests.cs.
- [x] T025 [API-Endpoint01] [FR-3] Create the minimal tests/StudentRegistration.OperationsTests project shell and solution entry, then create future failing safe-summary, SQL-down/degraded, exporter-down, no-topology/secret, and unauthenticated health endpoint behavior tests in tests/StudentRegistration.OperationsTests/Specs/Spec018/Endpoint01BehaviorTests.cs; handler delivery is deferred until linked AC/FR tests fail.
- [x] T026 [API-Endpoint02] [OWNER-SPEC-018] Finalize request, success, validation, authentication, authorization, conflict, rate-limit, and unexpected-error shapes for GET /api/operations/metrics in specs/018-quality-security-scalability-operations/contracts/api.md.
- [x] T027 [API-Endpoint02] Verify every documented response and authorization outcome for GET /api/operations/metrics in tests/StudentRegistration.ContractTests/Specs/Spec018/Endpoint02ContractTests.cs.
- [x] T028 [API-Endpoint02] [FR-3] [FR-4] Create future failing restricted authorization, freshness, cardinality, PII-redaction, degraded-source, and timestamp endpoint behavior tests in tests/StudentRegistration.OperationsTests/Specs/Spec018/Endpoint02BehaviorTests.cs; handler delivery is deferred.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Target load (FR-4, NFR-2, NFR-3, NFR-4) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [x] T029 [SC-1] [AC-1] [FR-4] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-1Tests.cs for AC-1: Target load (FR-4, NFR-2, NFR-3, NFR-4): Given a production-like database and exact NFR-2 mix When 75 submissions/s plus 300 reads/s run for 10 minutes Then p95 budgets and unexpected error rate pass And no capacity/duplicate/partial invariant fails.
### US2 - Approved spike load (NFR-2, NFR-4, NFR-5) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [x] T030 [SC-1] [AC-2] [NFR-2] [NFR-4] [NFR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-2Tests.cs for AC-2: Approved spike load (NFR-2, NFR-4, NFR-5): Given target correctness passes When 200 registration submissions/s run for 60 seconds across at least two stateless API replicas Then invariant correctness remains zero-defect And any graceful degradation is documented against approved POC thresholds.
### US3 - Restore rehearsal (FR-5, NFR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [x] T031 [SC-3] [AC-3] [FR-5] [NFR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-3Tests.cs for AC-3: Restore rehearsal (FR-5, NFR-7): Given a production-like backup and clean recovery environment When the runbook is executed Then data is restored within RTO And measured data loss is within RPO And integrity/reconciliation checks pass.
### US4 - Accessibility gate (FR-8, NFR-8) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [x] T032 [SC-2] [AC-4] [FR-8] [NFR-8] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-4Tests.cs for AC-4: Accessibility gate (FR-8, NFR-8): Given critical student/staff routes in staging When automated, keyboard, and representative screen-reader tests run Then no serious automated issue or critical/major manual barrier remains.
### US5 - Security release gate (FR-6, FR-9) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [x] T033 [AC-5] [FR-6] [FR-9] Create future failing security-release coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-5Tests.cs and tests/StudentRegistration.SecurityTests/DemoCredentialStorageTests.cs for threat approval, negative authorization, and scans proving source, migrations, SQL, fixtures, snapshots, telemetry, and evidence contain no plaintext generated PIN/password or full student profile while ASP.NET Identity hash verification succeeds.
### US6 - CI quality sequence (FR-1) (P3)

**Goal**: Prove AC-6 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-6 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [x] T034 [AC-6] [FR-1] Create future failing CI-sequence coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-6Tests.cs for a uniquely named per-run Testing database, migrations-before-seed, deterministic logical fixture verification, disposal, and no shared Development/production connection before later E2E/accessibility/security gates.
### US7 - Complete operational proof (FR-2, FR-3, FR-7, NFR-1, NFR-5, NFR-6, NFR-9) (P3)

**Goal**: Prove AC-7 as an independently demonstrable slice of Quality, Security, Scalability, and Operations.

**Independent Test**: Execute only the AC-7 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T028.
- [x] T035 [AC-7] [FR-2] [FR-3] [FR-7] [NFR-1] [NFR-5] [NFR-6] [NFR-9] Create future failing complete-operational coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec018/AC-7Tests.cs for the versioned deterministic logical 25,000-account/5,000-session synthetic fixture, two replicas/shared keys, blocking target/spike signals and invariants, plus equivalent logical IDs/profiles across rebuilds without comparing rowversion or salted password-hash bytes.
- [x] T036 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-1Tests.cs and assert: Observability exporter unavailable -> application remains functional with bounded buffering/fallback logs.
- [ ] T037 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-2Tests.cs and assert: One app instance fails -> load balancer removes it; other instance continues with shared auth keys/database.
- [ ] T038 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-3Tests.cs and assert: SQL unavailable -> fail safely, no partial result; health turns unhealthy and user gets reference ID.
- [ ] T039 [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-4Tests.cs and assert that compatibility mismatch or failure to positively validate both Development/Testing environment and connection target stops bootstrap/reset before any seed or destructive mutation.
- [ ] T040 [EC-5] Exercise EC-5 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec018/EdgeCases/EC-5Tests.cs and assert: Load targets prove unrealistic -> rebaseline by approved spec change, never silently relax correctness.

## Phase 4 - Requirement Tests and Bounded Delivery

- [x] T041 [FR-1] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the minimal tests/StudentRegistration.ReleaseTests project shell and solution entry, then create failing FR-1 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs and tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerTestDatabaseFixtureTests.cs for SQL Server 2022 Developer compatibility level 160 through Docker/Testcontainers, approved gate order, unique per-run Testing database creation, migration, versioned seed, readiness verification, and disposal with no Development/production connection reuse.
- [x] T042 [FR-1] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-1 through .github/workflows/ci.yml and tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerTestDatabaseFixture.cs only after T041 fails (depends on T041); the fixture pins SQL Server 2022 Developer compatibility 160, owns per-run isolation/orchestration, and invokes canonical-owner seed contributors only after migrations.
- [x] T043 [FR-2] [WORKSTREAM-CI-AND-RELEASE-GATES] Create failing FR-2 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs and tests/StudentRegistration.IntegrationTests/Infrastructure/NonProductionSeedLifecycleTests.cs for deterministic logical rebuilds, idempotent re-seed, Testing disposal, Development persistence to explicit guarded reset, seven-day Git-ignored local credential/log/export purge, synthetic-only data, partial-bootstrap not-ready state, and rejection outside Development/Testing.
- [x] T044 [FR-2] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver the FR-2 test/evidence gate through .github/workflows/ci.yml only after T043 fails (depends on T043); CI must execute the seed lifecycle, disposal/reset, artifact-expiry, synthetic-only, and environment-guard suite before feature integration/load tests.
- [x] T045 [FR-3] [WORKSTREAM-OBSERVABILITY] Create the future failing FR-3 checks in tests/StudentRegistration.OperationsTests/ObservabilitySignalTests.cs. Test focus: safe health/log/metric/trace signals, correlation, latency, throughput, conflicts, lock waits and mismatch alerts. Prove the requirement against its linked AC/EC fixtures: The application MUST expose authenticated-safe health, logs, metrics, traces, and correlation IDs.
- [x] T046 [FR-3] [WORKSTREAM-OBSERVABILITY] Deliver FR-3 through the bounded Observability workstream at src/StudentRegistration.Api/Operations/ObservabilityExtensions.cs only after T045 fails for the expected reason (depends on T045): The application MUST expose authenticated-safe health, logs, metrics, traces, and correlation IDs.
- [x] T047 [FR-4] [WORKSTREAM-OBSERVABILITY] Create the future failing FR-4 checks in tests/StudentRegistration.OperationsTests/ObservabilitySignalTests.cs. Test focus: safe health/log/metric/trace signals, correlation, latency, throughput, conflicts, lock waits and mismatch alerts. Prove the requirement against its linked AC/EC fixtures: Operations MUST monitor latency, throughput, unexpected error rate, business rejection codes, optimizer time, SQL latency, lock waits, deadlocks, capacity conflicts, and counter reconciliation.
- [x] T048 [FR-4] [WORKSTREAM-OBSERVABILITY] Deliver FR-4 through the bounded Observability workstream at src/StudentRegistration.Api/Operations/ObservabilityExtensions.cs only after T047 fails for the expected reason (depends on T047): Operations MUST monitor latency, throughput, unexpected error rate, business rejection codes, optimizer time, SQL latency, lock waits, deadlocks, capacity conflicts, and counter reconciliation.
- [x] T049 [FR-5] [WORKSTREAM-RECOVERY-AND-ROLLBACK] Create the minimal tests/StudentRegistration.RecoveryTests project shell and solution entry, then create the future failing FR-5 checks in tests/StudentRegistration.RecoveryTests/RecoveryRehearsalTests.cs. Test focus: backup/restore, migration rollback, application rollback, RPO and RTO evidence. Prove the requirement against its linked AC/EC fixtures: Backup/restore, migration rollback, and application rollback MUST be rehearsed before release.
- [x] T050 [FR-5] [WORKSTREAM-RECOVERY-AND-ROLLBACK] Deliver FR-5 through the bounded Recovery and rollback workstream at docs/runbooks/RECOVERY_AND_ROLLBACK.md only after T049 fails for the expected reason (depends on T049): Backup/restore, migration rollback, and application rollback MUST be rehearsed before release.
- [x] T051 [FR-6] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Create failing FR-6 checks in tests/StudentRegistration.SecurityTests/SecretAndDataProtectionTests.cs and tests/StudentRegistration.SecurityTests/DemoCredentialStorageTests.cs for User Secrets/environment-only POC secrets, a generated local certificate outside Git, transient generated credentials, ASP.NET Identity hash-only SQL persistence, and absence of plaintext PIN/password/full profiles in source, migrations, fixtures, snapshots, telemetry, and evidence; no production provider may be implied.
- [x] T052 [FR-6] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Deliver FR-6 through src/StudentRegistration.Api/Operations/SecurityConfiguration.cs only after T051 fails (depends on T051); configuration rejects missing POC secret/certificate inputs, exposes no credential logging path, and permits generated plaintext only as transient input to the canonical identity hasher.
- [x] T053 [FR-7] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Create the future failing FR-7 checks in tests/StudentRegistration.SecurityTests/SecretAndDataProtectionTests.cs. Test focus: SQL-backed shared Data Protection keys protected by the generated local certificate, no source secrets, cross-replica sessions, and no production-provider claim. Prove the requirement against its linked AC/EC fixtures: Application replicas MUST remain stateless and use one shared SQL Server Data Protection key repository protected at rest by the generated local certificate from FR-6. Cross-replica authentication and protected option-token tests MUST pass with no sticky session. This POC mechanism MUST NOT be presented as the undecided production protection provider.
- [ ] T054 [FR-7] [WORKSTREAM-SECRETS-AND-REPLICA-KEYS] Deliver FR-7 through the bounded Secrets and replica keys workstream at src/StudentRegistration.Api/Operations/SecurityConfiguration.cs only after T053 fails for the expected reason (depends on T053): Application replicas MUST remain stateless and use one shared SQL Server Data Protection key repository protected at rest by the generated local certificate from FR-6. Cross-replica authentication and protected option-token tests MUST pass with no sticky session. This POC mechanism MUST NOT be presented as the undecided production protection provider.
- [x] T055 [FR-8] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-8 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering, current Chrome/Edge/Firefox plus pinned WebKit, WebKit-not-Safari labels, and blockers. Prove the requirement against its linked AC/EC fixtures: Critical flows MUST pass automated accessibility checks plus manual keyboard and representative NVDA/Windows screen-reader journeys. A dated evidence record MUST identify tester, assistive technology/version, route, scenario, result, defect links, and UX/QA sign-off; automation alone cannot satisfy this requirement. Browser gates MUST cover current stable Chrome, Edge, and Firefox plus a pinned Playwright WebKit version. WebKit MUST NOT be labeled Safari; actual Safari/macOS validation is deferred.
- [x] T056 [FR-8] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-8 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T055 fails for the expected reason (depends on T055): Critical flows MUST pass automated accessibility checks plus manual keyboard and representative NVDA/Windows screen-reader journeys. A dated evidence record MUST identify tester, assistive technology/version, route, scenario, result, defect links, and UX/QA sign-off; automation alone cannot satisfy this requirement. Browser gates MUST cover current stable Chrome, Edge, and Firefox plus a pinned Playwright WebKit version. WebKit MUST NOT be labeled Safari; actual Safari/macOS validation is deferred.
- [x] T057 [FR-9] [WORKSTREAM-CI-AND-RELEASE-GATES] Create the future failing FR-9 checks in tests/StudentRegistration.ReleaseTests/CiGateDefinitionTests.cs. Test focus: restore/format/build/test/security/accessibility/coverage/invariant gate ordering and blockers. Prove the requirement against its linked AC/EC fixtures: A versioned STRIDE threat model MUST cover trust boundaries, assets, identity/session, authorization/data scope, protected option tokens, registration races, Admin/audit/export, SQL, telemetry, secrets, and deployment. Security review MUST record mitigations, residual risk, and owner. Release MUST be blocked by an unreviewed/stale threat model, unresolved critical/high security issue, invariant failure, or critical/major core usability defect.
- [x] T058 [FR-9] [WORKSTREAM-CI-AND-RELEASE-GATES] Deliver FR-9 through the bounded CI and release gates workstream at .github/workflows/ci.yml only after T057 fails for the expected reason (depends on T057): A versioned STRIDE threat model MUST cover trust boundaries, assets, identity/session, authorization/data scope, protected option tokens, registration races, Admin/audit/export, SQL, telemetry, secrets, and deployment. Security review MUST record mitigations, residual risk, and owner. Release MUST be blocked by an unreviewed/stale threat model, unresolved critical/high security issue, invariant failure, or critical/major core usability defect.


## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

- [x] T059 [API-Endpoint01] [API-Endpoint02] Deliver the canonical handlers for GET /api/health and GET /api/operations/metrics at src/StudentRegistration.Api/Endpoints/Spec018Endpoints.cs only after all contract, safe-signal, authorization, threat-model, shared-key, accessibility, and release-gate tests T023-T058 fail for expected reasons.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T060 [NFR-1] [AUTOMATED-EVIDENCE] Create the minimal tests/StudentRegistration.LoadTests project shell and solution entry, then produce evidence in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-1.md that the versioned synthetic generator creates 25,000 logical accounts/5,000 sessions, reproduces logical identities/profiles across rebuilds, verifies rather than byte-compares salted password hashes, and leaks no plaintext credential/full profile.
- [x] T061 [NFR-2] [AUTOMATED-EVIDENCE] Produce the blocking executable/evidence profiles in tests/StudentRegistration.LoadTests/Specs/Spec018/ExactLoadProfiles.cs and docs/release-evidence/SPEC-018-NFR-2.md for target 10m 75 registrations/s plus 300 reads/s and a 60s 200 registrations/s spike across at least two replicas; any retained 2x/5x/soak profiles are explicitly non-blocking diagnostics.
- [ ] T062 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-3.md: Catalogue p95 MUST be <= 300 ms, commit p95 MUST be <= 2 s, and optimizer p95 MUST be <= 500 ms for the approved workload.
- [ ] T063 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-4.md: Tests MUST demonstrate zero overbooking, zero duplicate active offering enrollment, and zero partial atomic submissions at the mandatory target, 200/s spike, and replica-failover profiles.
- [ ] T064 [NFR-5] [AUTOMATED-EVIDENCE] Run and record the mandatory target and 200/s spike profiles across at least two stateless replicas in tests/StudentRegistration.LoadTests/Specs/Spec018/RequiredReplicaLoadProfiles.cs and docs/release-evidence/SPEC-018-NFR-5.md; any optional 2x, 5x, or 120-minute soak result is diagnostic only and cannot block the POC.
- [ ] T065 [NFR-6] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-6 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-6EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-6.md: Unexpected server failure rate MUST be < 0.1% at target load.
- [ ] T066 [NFR-7] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-7 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-7EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-7.md: RPO MUST be <= 5 minutes and RTO <= 1 hour.
- [ ] T067 [NFR-8] [SC-2] [MANUAL-AND-AUTOMATED-EVIDENCE] Create the minimal tests/StudentRegistration.AccessibilityTests project shell and solution entry, run automated WCAG checks, and record manual keyboard plus representative NVDA/Windows journeys in tests/StudentRegistration.AccessibilityTests/Specs/Spec018/NFR-8EvidenceTests.cs and docs/release-evidence/SPEC-018-screen-reader-manual.md. The manual record MUST include tester, date, NVDA/version, route, scenario, result, defect links, and UX/QA sign-off; release fails if it is absent or unsigned.
- [ ] T068 [NFR-9] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-9 in tests/StudentRegistration.QualityTests/Specs/Spec018/NFR-9EvidenceTests.cs and docs/release-evidence/SPEC-018-NFR-9.md: Eligibility/conflict/capacity code SHOULD reach >= 90% branch coverage; coverage never replaces behavior tests.

## Phase 7 - Scope and Release Evidence

- [x] T069 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-1 remains excluded: Final production hosting/vendor/secret provider and actual Safari/macOS validation.
- [x] T070 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-2 remains excluded: Kubernetes by default.
- [x] T071 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-3 remains excluded: 24/7 SLO outside announced registration windows until approved.
- [x] T072 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-018-scope-review.md that OS-4 remains excluded: Arbitrary collection of student PII in telemetry.
- [ ] T073 [TRACE] Generate the completed FR/NFR/SC/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-018-traceability.md and reject release if any row lacks passing evidence.
- [ ] T074 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-018 in docs/release-evidence/SPEC-018-release-approval.md.

T001-T036, T041-T053, T055-T059, T061, and T069-T072 are complete. T037-T040, T054, T060, T062-T068, and T073-T074 remain dependency- or evidence-gated; skipped or pending evidence cannot authorize release.
