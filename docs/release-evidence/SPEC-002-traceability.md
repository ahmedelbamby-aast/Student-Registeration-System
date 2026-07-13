# SPEC-002 Complete Traceability Evidence

**Feature:** AASTMT Policy Rulebook  
**Profile:** `DEMO-POC-2026.1`  
**Evidence date:** 2026-07-13  
**Evidence scope:** governed artifacts and executable governance-reference tests  
**Runtime/system release is not claimed:** downstream replay remains owned by
SPEC-009, SPEC-011, SPEC-015, and SPEC-018 under
`SPEC-002-CONTRACT-TEST-BOUNDARY.md`.

## Admission rule

Every SPEC-002 FR, NFR, AC, EC, and SC row below has an authoritative artifact
and executable evidence at the scope owned by SPEC-002. `PASS` means the
governed rulebook/reference contract passed; it does not substitute for the
owning server, API, persistence, concurrency, or production-like release gate.
A missing row or failing test rejects the SPEC-002 artifact release.

## Functional requirements

| ID | Governed requirement | Authoritative artifact | Executable evidence | Status |
|---|---|---|---|---|
| FR-1 | Effective-dated, scoped policy versions | `contracts/policy-versioning.md`; `policy-rules.md` | `PolicyRulebookSchemaTests`; `PolicyRulebookTests`; `EC-1Tests`; `EC-2Tests` | PASS |
| FR-2 | Complete bounded demo rule categories and values | `contracts/policy-rule-coverage.md`; `policy-boundary-examples.md`; `docs/DEMO_CURRICULUM.md` | `PolicyRuleCoverageTests`; `AC-1Tests`; `NFR-2EvidenceTests` | PASS |
| FR-3 | Complete explainable decision/provenance shape | `contracts/policy-decision.md` | `PolicyDecisionContractTests`; `AC-1Tests`; `AC-3Tests`; `NFR-4EvidenceTests` | PASS |
| FR-4 | Unapproved policy cannot govern | `contracts/policy-approval-gate.md` | `PolicyApprovalGateTests`; `AC-2Tests`; `EC-1Tests` | PASS |
| FR-5 | Conflicting sources/scopes fail closed until resolved | `contracts/source-resolution.md` | `PolicySourceResolutionTests`; `AC-2Tests`; `EC-2Tests`; `EC-3Tests` | PASS |
| FR-6 | Closed typed-rule registry; no executable policy content | `schemas/policy-rule-definition.schema.json`; `schemas/policy-rule-types.json` | `PolicyRuleTypeRegistryTests`; `AC-4Tests` | PASS |
| FR-7 | Published versions and historical evidence are immutable | `policy-rules.md`; `contracts/policy-versioning.md` | `PublishedPolicyImmutabilityTests`; `AC-3Tests`; `EC-3Tests` | PASS |

## Non-functional requirements

| ID | Governed requirement | Measured/automated evidence | Result and downstream boundary | Status |
|---|---|---|---|---|
| NFR-1 | Identical input/version produces identical complete result | `NFR-1EvidenceTests.cs`; `SPEC-002-NFR-1.md` | 19 baselines plus 475 complete-decision comparisons, zero differences; SPEC-009 replay required | PASS |
| NFR-2 | Every approved Registrar boundary has regression coverage | `NFR-2EvidenceTests.cs`; `SPEC-002-NFR-2.md` | All PB-01 through PB-19 relevant inputs, typed inputs, results, reasons, and special effects verified; SPEC-009 replay required | PASS |
| NFR-3 | Decision evaluation reference target is p95 at most 100 ms | `NFR-3EvidenceTests.cs`; `SPEC-002-NFR-3.md` | Informational reference-oracle benchmark passes; runtime release performance remains pending SPEC-009/SPEC-018 | PASS (reference evidence only) |
| NFR-4 | Source/access/approval/effective metadata is auditable | `NFR-4EvidenceTests.cs`; `SPEC-002-NFR-4.md` | 12 canonical source rows, 96 fields, and 189 decision bindings verified; SPEC-015 persistence replay required | PASS |

## Acceptance criteria

| ID | Scenario | Executable evidence | Status |
|---|---|---|---|
| AC-1 | GPA 1.99 plan above 12 credits blocks with version/source/maximum | `AcceptanceTests/Specs/Spec002/AC-1Tests.cs` | PASS |
| AC-2 | Waitlist/override conflict blocks publication and requires amendment | `AcceptanceTests/Specs/Spec002/AC-2Tests.cs` | PASS |
| AC-3 | Successor publication does not rewrite original inputs/source/explanation | `AcceptanceTests/Specs/Spec002/AC-3Tests.cs` | PASS |
| AC-4 | Unknown rule type or executable expression is rejected and not stored | `AcceptanceTests/Specs/Spec002/AC-4Tests.cs` | PASS |
| AC-5 | Deterministic, sourced boundaries meet the reference performance target | `AcceptanceTests/Specs/Spec002/AC-5Tests.cs`; NFR evidence suite | PASS |

## Edge cases

| ID | Fault/boundary | Executable evidence | Status |
|---|---|---|---|
| EC-1 | No matching policy fails immediately with no guessed version/rules and alerts Admin | `IntegrationTests/Specs/Spec002/EdgeCases/EC-1Tests.cs` | PASS |
| EC-2 | Two actual equal-scope/priority/effective candidates reject publication/evaluation | `IntegrationTests/Specs/Spec002/EdgeCases/EC-2Tests.cs` | PASS |
| EC-3 | Source outage preserves complete historical source metadata and marks current review | `IntegrationTests/Specs/Spec002/EdgeCases/EC-3Tests.cs` | PASS |
| EC-4 | Missing official, synthetic-credit, gap-label, or disclaimer provenance rejects catalogue row | `IntegrationTests/Specs/Spec002/EdgeCases/EC-4Tests.cs` | PASS |

## Success criteria

| ID | Success criterion | Evidence | Status |
|---|---|---|---|
| SC-1 | Every governed decision identifies a stable reason and selected version or explicit `NOT_SELECTED` sentinel | AC-1/AC-3; EC-1/EC-2; `NFR-4EvidenceTests` | PASS |
| SC-2 | All approved boundary examples evaluate deterministically | `NFR-1EvidenceTests`; `NFR-2EvidenceTests` | PASS |
| SC-3 | No unresolved or unapproved value governs submission | `PolicyApprovalGateTests`; AC-2/AC-4; EC-1/EC-2 | PASS |

## Frontend route boundary

| Boundary | Evidence | Status |
|---|---|---|
| No direct frontend route is owned by SPEC-002 | `spec.md` Frontend Route Ownership; SPEC-003 route ownership is unchanged | PASS |

## Test and artifact gate

The executable reference is intentionally isolated under
`tests/StudentRegistration.TestSupport/Spec002`. Application/runtime projects
do not reference it. The exact ownership and replay rule is recorded in
`docs/release-evidence/SPEC-002-CONTRACT-TEST-BOUNDARY.md`.

The content-bound Release run is recorded in `SPEC-002-test-run.json`: 6
acceptance, 9 integration, 5 quality, and 28 specification tests passed; 0
failed and 0 skipped (48 total). Ahmed ELbamby's T043 approval is the only
remaining gate for the governed SPEC-002 artifact. Repository-wide validation
is repeated after that decision and before commit.
