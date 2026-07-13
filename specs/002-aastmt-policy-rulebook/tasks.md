# Tasks: AASTMT Policy Rulebook

**Status**: Approved for Gate A demo implementation on 2026-07-13. Execute in dependency order; later release and production gates remain required.
**Implementation progress**: 43/43 tasks complete. The governed rulebook,
schemas, provenance, contract tests, executable acceptance/edge coverage,
measured governance-harness NFR evidence, scope review, and traceability are
verified. The scoped non-production governed-artifact approval is recorded.
**Inputs**: spec.md, requirements.md, plan.md, research.md, data-model.md, contracts/api.md, dependency manifests
**Rule**: Every task names an exact artifact and trace. Checked tasks have
passing evidence and existing artifacts; unchecked tasks remain pending their
runtime owner or release gate.

## Phase 1 - Dependency, Consistency, Readiness, and Final Approval Gates

- [x] T001 [DEP-SPEC-001] Validate the consumed upstream requirements, plan, data model, and API contract at specs/001-product-charter-rbac/ and record the accepted versions in specs/002-aastmt-policy-rulebook/dependency-baseline.md.
- [x] T002 [GATE] Run cross-spec consistency analysis for SPEC-002; verify requirement/acceptance/success-criterion traceability, truthful artifact/runtime ownership, exact architecture paths, endpoint/route contracts, acyclic dependencies, task ordering, and approved Gate A demo-implementation status; record findings and resolutions in specs/002-aastmt-policy-rulebook/checklists/consistency-analysis.md.
- [x] T003 [GATE] After T002 passes, freeze the SPEC-002 requirements, data/API/design contracts, institutional decision states, dependency versions, and executable task baseline in specs/002-aastmt-policy-rulebook/checklists/implementation-readiness.md.
- [x] T004 [GATE] After T003 passes, verify the accountable owner and Ahmed ELbamby's 2026-07-13 Gate A human approval for SPEC-002 in specs/002-aastmt-policy-rulebook/checklists/approval.md as the final planning gate; no test, source, migration, or other implementation task may execute without this approval record.

## Phase 2 - Models and API Contracts

- [x] T005 [ENTITY-PolicyRulebook] [ARTIFACT-OWNER-SPEC-002] Create the future failing version/scope/rule-category/approval and downstream-owner schema checks in tests/StudentRegistration.SpecificationTests/Specs/Spec002/PolicyRulebookSchemaTests.cs.
- [x] T006 [ENTITY-PolicyRulebook] [ARTIFACT-OWNER-SPEC-002] Record the approved PolicyRulebook schema/version target in specs/002-aastmt-policy-rulebook/checklists/policy-rulebook-schema.md after T005 fails for the expected reason (depends on T005); canonical publication remains deferred until all rulebook behavior tests fail as expected.
- [x] T007 [ENTITY-PolicyRuleDefinition] [ENTITY-PolicyBoundaryExample] [ENTITY-PolicySourceRecord] [ARTIFACT-OWNER-SPEC-002] Create the future failing typed-rule, demo-profile boundary, official-curriculum-versus-synthetic-gap provenance, conflict, approval, and SPEC-015 snapshot-owner checks in tests/StudentRegistration.SpecificationTests/Specs/Spec002/PolicyEvidenceSchemaTests.cs.
- [x] T008 [ENTITY-PolicyRuleDefinition] [ARTIFACT-OWNER-SPEC-002] Publish the canonical typed, non-executable rule definitions at specs/002-aastmt-policy-rulebook/schemas/policy-rule-definition.schema.json after T007 fails for the expected reason (depends on T007); SPEC-009 owns runtime evaluation.

- [x] T009 [ENTITY-PolicyBoundaryExample] [ARTIFACT-OWNER-SPEC-002] Publish the canonical approved boundary fixtures, including 9/12/18-credit, prerequisite, capacity, overlap, and disabled-travel-buffer cases, at specs/002-aastmt-policy-rulebook/policy-boundary-examples.md after T007 fails for the expected reason (depends on T007).
- [x] T010 [ENTITY-PolicySourceRecord] [ARTIFACT-OWNER-SPEC-002] Publish the canonical provenance/conflict/approval register at specs/002-aastmt-policy-rulebook/policy-sources.md after T007 fails for the expected reason (depends on T007); record official curriculum URL/access dates, Ahmed's separate demo approval, and every synthetic gap label, while unresolved out-of-profile institutional values remain In Review and fail closed.

## Phase 3 - User-Story Acceptance and Edge Tests

### US1 - Probation load (FR-1, FR-2, FR-3) (P1)

**Goal**: Prove AC-1 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-1 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [x] T011 [AC-1] [FR-1] [FR-2] [FR-3] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-1Tests.cs for AC-1: Probation load (FR-1, FR-2, FR-3): Given an active student with GPA 1.99 under an approved general policy When eligibility is evaluated for a regular-term plan above 12 credits Then the decision fails with the probation load reason And cites the governing version/source and 12-credit maximum.
### US2 - Unapproved demo-policy conflict (FR-4, FR-5) (P1)

**Goal**: Prove AC-2 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-2 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [x] T012 [AC-2] [FR-4] [FR-5] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-2Tests.cs for AC-2: Unapproved demo-policy conflict (FR-4, FR-5): Given an imported rule conflicts with the approved demo profile by enabling a waitlist or capacity override When an admin attempts to publish that conflicting rule Then publication is blocked And the rule is shown as requiring a separately approved policy amendment.
### US3 - Historical explainability (FR-3, FR-7) (P2)

**Goal**: Prove AC-3 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-3 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [x] T013 [AC-3] [FR-3] [FR-7] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-3Tests.cs for AC-3: Historical explainability (FR-3, FR-7): Given a registration used policy version 2026.1 When the decision is inspected after version 2026.2 is published Then the original version, inputs, source and explanation remain available.
### US4 - Typed rule safety (FR-6) (P2)

**Goal**: Prove AC-4 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-4 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [x] T014 [AC-4] [FR-6] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-4Tests.cs for AC-4: Typed rule safety (FR-6): Given a draft rule contains an unknown rule type or executable expression When validation is requested Then validation rejects it And no executable content is stored or run.
### US5 - Deterministic, sourced policy quality (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

**Goal**: Prove AC-5 as an independently demonstrable slice of AASTMT Policy Rulebook.

**Independent Test**: Execute only the AC-5 Given/When/Then fixture with its declared data and dependency doubles.

**Dependencies**: Approval/dependency/model/API baseline through T010.
- [x] T015 [AC-5] [NFR-1] [NFR-2] [NFR-3] [NFR-4] Create the future failing Given/When/Then coverage in tests/StudentRegistration.AcceptanceTests/Specs/Spec002/AC-5Tests.cs for AC-5: Deterministic, sourced policy quality (NFR-1, NFR-2, NFR-3, NFR-4): Given an approved policy version, fixed input, Registrar boundary examples, and recorded provenance When the evaluator runs repeatedly under the approved performance fixture Then every result and reason is identical And every boundary regression passes And decision evaluation is at most 100 ms p95 excluding initial data retrieval And source/access/approval/effective metadata remains auditable.
- [x] T016 [EC-1] Exercise EC-1 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-1Tests.cs and assert: No approved policy matches the student/term -> fail closed and alert Admin; do not guess a general rule.
- [x] T017 [EC-2] Exercise EC-2 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-2Tests.cs and assert: Two sets have equal scope/priority -> publication validation fails.
- [x] T018 [EC-3] Exercise EC-3 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-3Tests.cs and assert: Source URL becomes unavailable -> retain recorded metadata and flag source review; do not alter historical decisions.
- [x] T019 [EC-4] Exercise EC-4 with fault/boundary injection in tests/StudentRegistration.IntegrationTests/Specs/Spec002/EdgeCases/EC-4Tests.cs and assert: A demo catalogue row lacks official-source provenance or an explicit synthetic-gap label -> reject catalogue publication.

## Phase 4 - Requirement Tests and Bounded Delivery

- [x] T020 [FR-1] [FR-2] [FR-3] [FR-4] [FR-5] [FR-7] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Create the future failing consolidated rulebook checks in tests/StudentRegistration.SpecificationTests/Spec002/PolicyRulebookTests.cs. Test focus: effective scope, `DEMO-POC-2026.1` rules, fail-closed approval, official-versus-synthetic provenance, deterministic reasons and immutable history consumed by owning feature specs.
- [x] T021 [FR-1] [WORKSTREAM-RULEBOOK-CONTRACT] Publish the version/scope/effective-period contract at specs/002-aastmt-policy-rulebook/contracts/policy-versioning.md only after T020 fails for the expected reason (depends on T020).
- [x] T022 [FR-2] [WORKSTREAM-RULEBOOK-CONTRACT] Create the future failing required-rule-category coverage checks in tests/StudentRegistration.SpecificationTests/Policy/PolicyRuleCoverageTests.cs for configured window, standing, holds, prerequisites, regular 9-18/default-18 load, GPA-below-2.0 maximum 12, first-commit capacity, hard overlap blocking, zero travel buffer, excluded workflows, and the 19-course `docs/DEMO_CURRICULUM.md` provenance labels.
- [x] T023 [FR-2] [WORKSTREAM-RULEBOOK-CONTRACT] Publish the required typed-rule category catalogue and bounded `DEMO-POC-2026.1` values at specs/002-aastmt-policy-rulebook/contracts/policy-rule-coverage.md only after T022 fails for the expected reason (depends on T022).
- [x] T024 [FR-3] [WORKSTREAM-RULEBOOK-CONTRACT] Create the future failing decision/provenance shape checks in tests/StudentRegistration.SpecificationTests/Policy/PolicyDecisionContractTests.cs.
- [x] T025 [FR-3] [WORKSTREAM-RULEBOOK-CONTRACT] Publish the reason/explanation/version/input/source decision contract at specs/002-aastmt-policy-rulebook/contracts/policy-decision.md only after T024 fails for the expected reason (depends on T024).
- [x] T026 [FR-4] [WORKSTREAM-RULEBOOK-CONTRACT] Create the future failing approval-state/fail-closed checks in tests/StudentRegistration.SpecificationTests/Policy/PolicyApprovalGateTests.cs.
- [x] T027 [FR-4] [WORKSTREAM-RULEBOOK-CONTRACT] Publish the approval and fail-closed rules at specs/002-aastmt-policy-rulebook/contracts/policy-approval-gate.md only after T026 fails for the expected reason (depends on T026).
- [x] T028 [FR-5] [WORKSTREAM-RULEBOOK-CONTRACT] Create the future failing source-conflict resolution checks in tests/StudentRegistration.SpecificationTests/Policy/PolicySourceResolutionTests.cs.
- [x] T029 [FR-5] [WORKSTREAM-RULEBOOK-CONTRACT] Publish the Registrar-perspective source-conflict gate at specs/002-aastmt-policy-rulebook/contracts/source-resolution.md only after T028 fails for the expected reason (depends on T028); the bounded Ahmed-approved demo values are distinguished from source-derived values, and every unresolved out-of-profile POLICY-Q remains In Review and fail closed.
- [x] T030 [FR-6] [WORKSTREAM-TYPED-POLICY-RULE-REGISTRY] Create the future failing typed-rule/no-executable-content checks in tests/StudentRegistration.SpecificationTests/Spec002/PolicyRuleTypeRegistryTests.cs. Test focus: known typed rules accepted and unknown or executable rule content rejected.
- [x] T031 [FR-6] [WORKSTREAM-TYPED-POLICY-RULE-REGISTRY] Deliver the bounded Typed policy rule registry workstream at specs/002-aastmt-policy-rulebook/schemas/policy-rule-types.json only after T030 fails for the expected reason (depends on T030); SPEC-009 implements the runtime registry.
- [x] T032 [FR-7] [WORKSTREAM-RULEBOOK-CONTRACT] Create the future failing published-version immutability checks in tests/StudentRegistration.SpecificationTests/Policy/PublishedPolicyImmutabilityTests.cs.
- [x] T033 [FR-1] [FR-2] [FR-3] [FR-4] [FR-5] [FR-7] [ENTITY-PolicyRulebook] [WORKSTREAM-VERSIONED-POLICY-EVALUATION] Deliver the bounded Versioned policy evaluation workstream and publish the canonical PolicyRulebook with immutable `DEMO-POC-2026.1` at specs/002-aastmt-policy-rulebook/policy-rules.md only after T020, T022, T024, T026, T028, and T032 fail for their expected reasons (depends on T020, T022, T024, T026, T028, T032); SPEC-009 owns runtime PolicySet/PolicyRule evaluation.

## Phase 5 - Frontend Route Tests and Integration

No direct frontend route is owned by this specification; frontend integration remains governed by SPEC-003.

## Phase 6 - Measurable Non-Functional Evidence

- [x] T034 [NFR-1] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-1 in tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-1EvidenceTests.cs and docs/release-evidence/SPEC-002-NFR-1.md: The same input and policy version MUST yield the same result.
- [x] T035 [NFR-2] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-2 in tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-2EvidenceTests.cs and docs/release-evidence/SPEC-002-NFR-2.md: All boundary examples supplied by the Registrar MUST have automated regression tests.
- [x] T036 [NFR-3] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-3 in tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-3EvidenceTests.cs and docs/release-evidence/SPEC-002-NFR-3.md: A policy decision query SHOULD complete within 100 ms p95 excluding initial data retrieval.
- [x] T037 [NFR-4] [AUTOMATED-EVIDENCE] Produce measurable automated release evidence for NFR-4 in tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-4EvidenceTests.cs and docs/release-evidence/SPEC-002-NFR-4.md: Source URL, access date, approval actor, and effective period MUST be auditable.

## Phase 7 - Scope and Release Evidence

- [x] T038 [OS-1] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-002-scope-review.md that OS-1 remains excluded: Legal interpretation by software.
- [x] T039 [OS-2] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-002-scope-review.md that OS-2 remains excluded: Arbitrary scripting/expressions uploaded by users.
- [x] T040 [OS-3] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-002-scope-review.md that OS-3 remains excluded: Waitlists, capacity/conflict overrides, automatic exceptions, add/drop, withdrawal, and Advisor or Deanery approval workflows until separately approved.
- [x] T041 [OS-4] Inspect source, contracts, migrations, routes, and tests and record in docs/release-evidence/SPEC-002-scope-review.md that OS-4 remains excluded: Automatic dismissal or academic-path decisions.
- [x] T042 [TRACE] [SC-1] [SC-2] [SC-3] Generate the completed FR/NFR/AC/EC/SC/route-to-test evidence matrix at docs/release-evidence/SPEC-002-traceability.md and reject release if any row lacks passing evidence.
- [x] T043 [GATE] Record product owner, domain owner, QA, security, accessibility, data/concurrency, and operations approvals applicable to SPEC-002 in docs/release-evidence/SPEC-002-release-approval.md.

All forty-three governance, executable-test, scope-review, traceability, and
scoped release-approval tasks are complete.
