# Tasks: AASTMT Policy Rulebook

**Status**: Planned only. Do not execute until human approval.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task is unchecked, names an exact future file, and traces to a requirement, criterion, edge case, route, entity, endpoint, dependency, or gate.

## Phase 1 - Approval and Dependency Gates

- [ ] T001 [GATE] Record Ahmed ELbamby's human approval for SPEC-002 in specs/002-aastmt-policy-rulebook/checklists/approval.md before executing any later task.
- [ ] T002 [DEP-SPEC-001] Validate the consumed upstream requirements, plan, data model, and API contract at specs/001-product-charter-rbac/ and record the accepted versions in specs/002-aastmt-policy-rulebook/dependency-baseline.md.
- [ ] T003 [GATE] Freeze SPEC-002 requirements, API, data-model, policy approvals, and dependency versions in specs/002-aastmt-policy-rulebook/checklists/implementation-readiness.md.

## Phase 2 - Models and API Contracts

- [ ] T004 [P] [ENTITY-PolicySet] [CONSUMER-SPEC-009] Verify SPEC-002 consumes the canonical PolicySet at src/StudentRegistration.Domain/Modules/Academics/PolicySet.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec002/PolicySetModelTests.cs.
- [ ] T005 [P] [ENTITY-PolicyRule] [CONSUMER-SPEC-009] Verify SPEC-002 consumes the canonical PolicyRule at src/StudentRegistration.Domain/Modules/Academics/PolicyRule.cs without redefining ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec002/PolicyRuleModelTests.cs.
- [ ] T006 [P] [ENTITY-PolicyDecisionSnapshot] [OWNER-SPEC-002] Create the future failing invariant/schema/serialization checks for canonical PolicyDecisionSnapshot ownership in tests/StudentRegistration.IntegrationTests/Specs/Spec002/PolicyDecisionSnapshotModelTests.cs.
- [ ] T007 [ENTITY-PolicyDecisionSnapshot] [OWNER-SPEC-002] Deliver the canonical PolicyDecisionSnapshot model or governed artifact at src/StudentRegistration.Domain/Modules/Academics/PolicyDecisionSnapshot.cs after T006 fails for the expected reason (depends on T006).

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Probation load (FR-1, FR-2, FR-3) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T008 [P] [AC-1] [FR-1] [FR-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-1Tests.cs for AC-1: Probation load (FR-1, FR-2, FR-3): Given an active student with GPA 1.99 under an approved general policy When eligibility is evaluated for a regular-term plan above 12 credits Then the decision fails with the probation load reason And cites the governing version/source and 12-credit maximum.
### US2 - Unapproved conflict (FR-4, FR-5) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T009 [P] [AC-2] [FR-4] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-2Tests.cs for AC-2: Unapproved conflict (FR-4, FR-5): Given withdrawal week is unresolved between sources When an admin attempts to publish that rule Then publication is blocked And POLICY-Q04 is shown as requiring Registrar approval.
### US3 - Historical explainability (FR-3, FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T010 [P] [AC-3] [FR-3] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-3Tests.cs for AC-3: Historical explainability (FR-3, FR-7): Given a registration used policy version 2026.1 When the decision is inspected after version 2026.2 is published Then the original version, inputs, source and explanation remain available.
### US4 - Typed rule safety (FR-6) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T011 [P] [AC-4] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-4Tests.cs for AC-4: Typed rule safety (FR-6): Given a draft rule contains an unknown rule type or executable expression When validation is requested Then validation rejects it And no executable content is stored or run.
### US5 - Deterministic, sourced policy quality (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T007.
- [ ] T012 [P] [AC-5] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-5Tests.cs for AC-5: Deterministic, sourced policy quality (NFR-1, NFR-2, NFR-3, NFR-4): Given an approved policy version, fixed input, Registrar boundary examples, and recorded provenance When the evaluator runs repeatedly under the approved performance fixture Then every result and reason is identical And every boundary regression passes And decision evaluation is at most 100 ms p95 excluding initial data retrieval And source/access/approval/effective metadata remains auditable.
- [ ] T013 [P] [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-1Tests.cs and assert: No approved policy matches the student/term -> fail closed and alert Admin; do not guess a general rule.
- [ ] T014 [P] [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-2Tests.cs and assert: Two sets have equal scope/priority -> publication validation fails.
- [ ] T015 [P] [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-3Tests.cs and assert: Source URL becomes unavailable -> retain recorded metadata and flag source review; do not alter historical decisions.
- [ ] T016 [P] [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-4Tests.cs and assert: Catalogue references missing course -> reject catalogue publication.

## Phase 4 - Requirement Tests and Bounded Delivery

- [ ] T017 [P] [FR-1] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Create the future failing FR-1 checks in tests/StudentRegistration.DomainTests/Academics/PolicyEvaluatorTests.cs. Test focus: effective scope, fail-closed approval, provenance, deterministic reasons and immutable history. Prove the requirement against its linked AC/EC fixtures: The system MUST version policy sets by effective dates and academic scope.
- [ ] T018 [FR-1] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Deliver FR-1 through the bounded Versioned policy evaluation workstream at src/StudentRegistration.Domain/Modules/Academics/Policies/PolicyEvaluator.cs only after T017 fails for the expected reason (depends on T017): The system MUST version policy sets by effective dates and academic scope.
- [ ] T019 [P] [FR-2] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Create the future failing FR-2 checks in tests/StudentRegistration.DomainTests/Academics/PolicyEvaluatorTests.cs. Test focus: effective scope, fail-closed approval, provenance, deterministic reasons and immutable history. Prove the requirement against its linked AC/EC fixtures: Approved rules MUST cover registration window, standing, holds, load, prerequisites, earned credits, repeats, and conflict/capacity product rules.
- [ ] T020 [FR-2] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Deliver FR-2 through the bounded Versioned policy evaluation workstream at src/StudentRegistration.Domain/Modules/Academics/Policies/PolicyEvaluator.cs only after T019 fails for the expected reason (depends on T019): Approved rules MUST cover registration window, standing, holds, load, prerequisites, earned credits, repeats, and conflict/capacity product rules.
- [ ] T021 [P] [FR-3] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Create the future failing FR-3 checks in tests/StudentRegistration.DomainTests/Academics/PolicyEvaluatorTests.cs. Test focus: effective scope, fail-closed approval, provenance, deterministic reasons and immutable history. Prove the requirement against its linked AC/EC fixtures: Every decision MUST return reason code, explanation, policy version, input summary, and source.
- [ ] T022 [FR-3] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Deliver FR-3 through the bounded Versioned policy evaluation workstream at src/StudentRegistration.Domain/Modules/Academics/Policies/PolicyEvaluator.cs only after T021 fails for the expected reason (depends on T021): Every decision MUST return reason code, explanation, policy version, input summary, and source.
- [ ] T023 [P] [FR-4] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Create the future failing FR-4 checks in tests/StudentRegistration.DomainTests/Academics/PolicyEvaluatorTests.cs. Test focus: effective scope, fail-closed approval, provenance, deterministic reasons and immutable history. Prove the requirement against its linked AC/EC fixtures: A draft or unapproved policy set MUST NOT govern student submission.
- [ ] T024 [FR-4] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Deliver FR-4 through the bounded Versioned policy evaluation workstream at src/StudentRegistration.Domain/Modules/Academics/Policies/PolicyEvaluator.cs only after T023 fails for the expected reason (depends on T023): A draft or unapproved policy set MUST NOT govern student submission.
- [ ] T025 [P] [FR-5] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Create the future failing FR-5 checks in tests/StudentRegistration.DomainTests/Academics/PolicyEvaluatorTests.cs. Test focus: effective scope, fail-closed approval, provenance, deterministic reasons and immutable history. Prove the requirement against its linked AC/EC fixtures: Conflicting sources MUST be resolved by the Registrar/SME before the affected rule is published.
- [ ] T026 [FR-5] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Deliver FR-5 through the bounded Versioned policy evaluation workstream at src/StudentRegistration.Domain/Modules/Academics/Policies/PolicyEvaluator.cs only after T025 fails for the expected reason (depends on T025): Conflicting sources MUST be resolved by the Registrar/SME before the affected rule is published.
- [ ] T027 [P] [FR-6] [WORKSTREAM-TYPED-POLICY-RULE-REGISTRY] Create the future failing FR-6 checks in tests/StudentRegistration.DomainTests/Academics/PolicyRuleTypeRegistryTests.cs. Test focus: known typed rules accepted and unknown or executable rule content rejected. Prove the requirement against its linked AC/EC fixtures: New rule behavior MUST use a reviewed typed rule; arbitrary executable policy scripts MUST NOT be stored.
- [ ] T028 [FR-6] [WORKSTREAM-TYPED-POLICY-RULE-REGISTRY] Deliver FR-6 through the bounded Typed policy rule registry workstream at src/StudentRegistration.Domain/Modules/Academics/Policies/PolicyRuleTypeRegistry.cs only after T027 fails for the expected reason (depends on T027): New rule behavior MUST use a reviewed typed rule; arbitrary executable policy scripts MUST NOT be stored.
- [ ] T029 [P] [FR-7] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Create the future failing FR-7 checks in tests/StudentRegistration.DomainTests/Academics/PolicyEvaluatorTests.cs. Test focus: effective scope, fail-closed approval, provenance, deterministic reasons and immutable history. Prove the requirement against its linked AC/EC fixtures: Published policy versions MUST be immutable and superseded, not edited.
- [ ] T030 [FR-7] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Deliver FR-7 through the bounded Versioned policy evaluation workstream at src/StudentRegistration.Domain/Modules/Academics/Policies/PolicyEvaluator.cs only after T029 fails for the expected reason (depends on T029): Published policy versions MUST be immutable and superseded, not edited.

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [ ] T031 [P] [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-002-NFR-1.md: The same input and policy version MUST yield the same result.
- [ ] T032 [P] [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-002-NFR-2.md: All boundary examples supplied by the Registrar MUST have automated regression tests.
- [ ] T033 [P] [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-002-NFR-3.md: A policy decision query SHOULD complete within 100 ms p95 excluding initial data retrieval.
- [ ] T034 [P] [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-002-NFR-4.md: Source URL, access date, approval actor, and effective period MUST be auditable.

## Phase 7 - Scope and Release Evidence

- [ ] T035 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-002-scope-review.md that OS-1 remains excluded: Legal interpretation by software.
- [ ] T036 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-002-scope-review.md that OS-2 remains excluded: Arbitrary scripting/expressions uploaded by users.
- [ ] T037 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-002-scope-review.md that OS-3 remains excluded: Advisor or Deanery approval workflow until separately approved.
- [ ] T038 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-002-scope-review.md that OS-4 remains excluded: Automatic dismissal or academic-path decisions.
- [ ] T039 [TRACE] Generate the completed FR/NFR/AC/EC/route-to-test evidence matrix at docs/release-evidence/SPEC-002-traceability.md and reject release if any row lacks passing evidence.
- [ ] T040 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-002 in docs/release-evidence/SPEC-002-release-approval.md.

No task is complete and no implementation file has been created.
