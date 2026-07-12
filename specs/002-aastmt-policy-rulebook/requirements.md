# SPEC-002: AASTMT Policy Rulebook

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** AASTMT Registrar/Policy SME<br>
**Reviewers:** Product Owner, Data Lead, QA Lead<br>
**Target:** Sprint 0; maintained thereafter<br>
**Dependencies:** SPEC-001<br>

## Context

Subject eligibility depends on AASTMT general rules and College-of-AI
overlays. Public documents contain differences and incomplete catalogue data.
Rules must therefore be sourced, effective-dated, explainable, and approved,
not scattered as constants in UI or application code.

The research baseline and unresolved POLICY-Q items are documented in
docs/POLICY_RESEARCH.md.

## Functional Requirements

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

## Non-Functional Requirements

- NFR-1: The same input and policy version MUST yield the same result.
- NFR-2: All boundary examples supplied by the Registrar MUST have automated
  regression tests.
- NFR-3: A policy decision query SHOULD complete within 100 ms p95 excluding
  initial data retrieval.
- NFR-4: Source URL, access date, approval actor, and effective period MUST be
  auditable.

## Acceptance Criteria

### AC-1: Probation load (FR-1, FR-2, FR-3)
Given an active student with GPA 1.99 under an approved general policy<br>
When eligibility is evaluated for a regular-term plan above 12 credits<br>
Then the decision fails with the probation load reason<br>
And cites the governing version/source and 12-credit maximum.

### AC-2: Unapproved conflict (FR-4, FR-5)
Given withdrawal week is unresolved between sources<br>
When an admin attempts to publish that rule<br>
Then publication is blocked<br>
And POLICY-Q04 is shown as requiring Registrar approval.

### AC-3: Historical explainability (FR-3, FR-7)
Given a registration used policy version 2026.1<br>
When the decision is inspected after version 2026.2 is published<br>
Then the original version, inputs, source and explanation remain available.

### AC-4: Typed rule safety (FR-6)
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

## API Contracts

```typescript
interface PolicyDecisionDto {
  eligible: boolean;
  policyVersion: string;
  evaluatedAtUtc: string;
  results: Array<{
    reasonCode: string;
    passed: boolean;
    explanation: string;
    sourceUrl: string;
    overridePossible: boolean;
  }>;
}
```

Endpoints: POST /api/admin/policies/{id}/simulate and GET
/api/student/offerings/{id}/eligibility.

## Data Models

| Entity | Required data |
|---|---|
| PolicySet | version, scope, priority, effective dates, approval state/actor |
| PolicyRule | typed rule, reason code, validated config, source URL/access date |
| PolicyDecisionSnapshot | version, input summary, result list, evaluated time |

## Out of Scope

- OS-1: Legal interpretation by software.
- OS-2: Arbitrary scripting/expressions uploaded by users.
- OS-3: Advisor or Deanery approval workflow until separately approved.
- OS-4: Automatic dismissal or academic-path decisions.
