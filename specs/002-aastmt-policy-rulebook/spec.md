# Feature Specification: AASTMT Policy Rulebook

**Feature Branch**: 002-aastmt-policy-rulebook
**Created**: 2026-07-12
**Status**: Approved (Gate A demo implementation, 2026-07-13)
**Owner**: Registrar/Policy SME
**Normative detail**: [requirements.md](requirements.md)

## Context

Subject eligibility depends on AASTMT general rules and College-of-AI
overlays. Public documents contain differences and incomplete catalogue data.
Rules must therefore be sourced, effective-dated, explainable, and approved,
not scattered as constants in UI or application code.

The research baseline and unresolved POLICY-Q items are documented in
docs/POLICY_RESEARCH.md.

For this non-production proof of concept, Ahmed ELbamby approved the bounded
`DEMO-POC-2026.1` profile on 2026-07-13. It intentionally demonstrates policy
evaluation with simple rules and a small, sourced curriculum; it is not an
official AASTMT production rulebook.

## User Scenarios and Testing

### User Story 1 - Probation load (FR-1, FR-2, FR-3) (P1)

As a Registrar/Policy SME, I need the Probation load (FR-1, FR-2, FR-3) behavior so that AASTMT Policy Rulebook produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given an active student with GPA 1.99 under an approved general policy<br>
When eligibility is evaluated for a regular-term plan above 12 credits<br>
Then the decision fails with the probation load reason<br>
And cites the governing version/source and 12-credit maximum.
### User Story 2 - Unapproved demo-policy conflict (FR-4, FR-5) (P1)

As a Registrar/Policy SME, I need the Unapproved conflict (FR-4, FR-5) behavior so that AASTMT Policy Rulebook produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given an imported rule conflicts with the approved demo profile by enabling a
waitlist or capacity override<br>
When an admin attempts to publish that conflicting rule<br>
Then publication is blocked<br>
And the rule is shown as requiring a separately approved policy amendment.
### User Story 3 - Historical explainability (FR-3, FR-7) (P2)

As a Registrar/Policy SME, I need the Historical explainability (FR-3, FR-7) behavior so that AASTMT Policy Rulebook produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a registration used policy version 2026.1<br>
When the decision is inspected after version 2026.2 is published<br>
Then the original version, inputs, source and explanation remain available.
### User Story 4 - Typed rule safety (FR-6) (P2)

As a Registrar/Policy SME, I need the Typed rule safety (FR-6) behavior so that AASTMT Policy Rulebook produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given a draft rule contains an unknown rule type or executable expression<br>
When validation is requested<br>
Then validation rejects it<br>
And no executable content is stored or run.
### User Story 5 - Deterministic, sourced policy quality (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Registrar/Policy SME, I need the Deterministic, sourced policy quality (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that AASTMT Policy Rulebook produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given an approved policy version, fixed input, Registrar boundary examples, and
recorded provenance<br>
When the evaluator runs repeatedly under the approved performance fixture<br>
Then every result and reason is identical<br>
And every boundary regression passes<br>
And decision evaluation is at most 100 ms p95 excluding initial data retrieval<br>
And source/access/approval/effective metadata remains auditable.

## Edge Cases

- EC-1: No approved policy matches the student/term -> fail closed and alert
  Admin; do not guess a general rule.
- EC-2: Two sets have equal scope/priority -> publication validation fails.
- EC-3: Source URL becomes unavailable -> retain recorded metadata and flag
  source review; do not alter historical decisions.
- EC-4: A demo catalogue row lacks official-source provenance or an explicit
  synthetic-gap label -> reject catalogue publication.

## Requirements

### Functional Requirements

- FR-1: The system MUST version policy sets by effective dates and academic
  scope.
- FR-2: Approved rules MUST cover registration window, standing, holds, load,
  prerequisites, earned credits, repeats, and conflict/capacity product rules.
  The demo profile MUST enforce configured registration windows, eligible
  standing, absence of blocking holds, and completed prerequisites; accept a
  regular load from 9 through 18 credits with 18 as both the default/recommended
  target and hard normal maximum; limit GPA below 2.0 to 12 credits; allocate
  capacity to the first successful commit with no waitlist or override; block
  every unresolved meeting overlap with travel-time buffering disabled; and
  provide no automatic exception, add/drop, withdrawal, or advisor workflow.
  Its catalogue MUST be the 19-course AASTMT College of Artificial Intelligence
  Data Science snapshot in `docs/DEMO_CURRICULUM.md`, with any gap-filling synthetic row
  clearly labelled as synthetic and never represented as official curriculum.
- FR-3: Every decision MUST return reason code, explanation, policy version,
  input summary, and source.
- FR-4: A draft or unapproved policy set MUST NOT govern student submission.
- FR-5: Conflicting sources MUST be resolved by the Registrar/SME before the
  affected rule is published.
- FR-6: New rule behavior MUST use a reviewed typed rule; arbitrary executable
  policy scripts MUST NOT be stored.
- FR-7: Published policy versions MUST be immutable and superseded, not edited.

### Non-Functional Requirements

- NFR-1: The same input and policy version MUST yield the same result.
- NFR-2: All boundary examples supplied by the Registrar MUST have automated
  regression tests.
- NFR-3: A policy decision query SHOULD complete within 100 ms p95 excluding
  initial data retrieval.
- NFR-4: Source URL, access date, approval actor, and effective period MUST be
  auditable.

### Key Entities

- **PolicyRulebook**: Versioned, scoped, approval-aware rulebook artifact owned by SPEC-002.
- **PolicyRuleDefinition**: Typed, non-executable rule-definition artifact owned by SPEC-002.
- **PolicyBoundaryExample**: Registrar-supplied boundary example artifact owned by SPEC-002.
- **PolicySourceRecord**: Provenance and institutional approval artifact owned by SPEC-002.

Runtime `PolicySet` and `PolicyRule` are owned by SPEC-009, while the durable
decision snapshot is owned by SPEC-015; none is a SPEC-002 runtime entity.

## Success Criteria

- **SC-1**: Every registration decision identifies the governing policy version and reason.
- **SC-2**: All approved policy boundary examples can be evaluated deterministically.
- **SC-3**: No unresolved or unapproved policy value governs a student submission.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.
- `DEMO-POC-2026.1` is approved only by Ahmed for this design-capability demo;
  the public AASTMT sources establish provenance, not production authorization.

## Dependencies

- [SPEC-001](../001-product-charter-rbac/spec.md)

## Frontend Route Ownership

No route is directly owned. Any later UI exposure requires a SPEC-003 route-manifest amendment before implementation.

## Out of Scope

- OS-1: Legal interpretation by software.
- OS-2: Arbitrary scripting/expressions uploaded by users.
- OS-3: Waitlists, capacity/conflict overrides, automatic exceptions, add/drop,
  withdrawal, and Advisor or Deanery approval workflows until separately
  approved.
- OS-4: Automatic dismissal or academic-path decisions.
