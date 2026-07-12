# Feature Specification: AASTMT Policy Rulebook

**Feature Branch**: 002-aastmt-policy-rulebook
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Registrar/Policy SME
**Normative detail**: [requirements.md](requirements.md)

## Context

Subject eligibility depends on AASTMT general rules and College-of-AI
overlays. Public documents contain differences and incomplete catalogue data.
Rules must therefore be sourced, effective-dated, explainable, and approved,
not scattered as constants in UI or application code.

The research baseline and unresolved POLICY-Q items are documented in
docs/POLICY_RESEARCH.md.

## User Scenarios and Testing

### User Story 1 - Probation load (FR-1, FR-2, FR-3) (P1)

As a Registrar/Policy SME, I need the Probation load (FR-1, FR-2, FR-3) behavior so that AASTMT Policy Rulebook produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given an active student with GPA 1.99 under an approved general policy<br>
When eligibility is evaluated for a regular-term plan above 12 credits<br>
Then the decision fails with the probation load reason<br>
And cites the governing version/source and 12-credit maximum.
### User Story 2 - Unapproved conflict (FR-4, FR-5) (P1)

As a Registrar/Policy SME, I need the Unapproved conflict (FR-4, FR-5) behavior so that AASTMT Policy Rulebook produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given withdrawal week is unresolved between sources<br>
When an admin attempts to publish that rule<br>
Then publication is blocked<br>
And POLICY-Q04 is shown as requiring Registrar approval.
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

## Edge Cases

- EC-1: No approved policy matches the student/term -> fail closed and alert
  Admin; do not guess a general rule.
- EC-2: Two sets have equal scope/priority -> publication validation fails.
- EC-3: Source URL becomes unavailable -> retain recorded metadata and flag
  source review; do not alter historical decisions.
- EC-4: Catalogue references missing course -> reject catalogue publication.

## Requirements

### Functional Requirements

- FR-1: The system MUST version policy sets by effective dates and academic
  scope.
- FR-2: Approved rules MUST cover registration window, standing, holds, load,
  prerequisites, earned credits, repeats, and conflict/capacity product rules.
- FR-3: Every decision MUST return reason code, explanation, policy version,
  input summary, and source.
- FR-4: A draft or unapproved policy set MUST NOT govern student submission.
- FR-5: Conflicting sources MUST be resolved by the Registrar/SME before the
  affected rule is published.
- FR-6: New rule behavior MUST use a reviewed typed rule; arbitrary executable
  policy scripts MUST NOT be stored.
- FR-7: Published policy versions MUST be immutable and superseded, not edited.

### Key Entities

- **PolicySet**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicyRule**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicyDecisionSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Every registration decision identifies the governing policy version and reason.
- **SC-2**: All approved policy boundary examples can be evaluated deterministically.
- **SC-3**: No unresolved or unapproved policy value governs a student submission.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-001](../001-product-charter-rbac/spec.md)

## Out of Scope

- OS-1: Legal interpretation by software.
- OS-2: Arbitrary scripting/expressions uploaded by users.
- OS-3: Advisor or Deanery approval workflow until separately approved.
- OS-4: Automatic dismissal or academic-path decisions.
