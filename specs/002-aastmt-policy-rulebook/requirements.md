# SPEC-002: AASTMT Policy Rulebook

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** Approved (Gate A demo implementation, 2026-07-13; owner amendment approved 2026-07-20)<br>
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

For this non-production proof of concept, Ahmed ELbamby approved the bounded
`DEMO-POC-2026.1` profile on 2026-07-13. It intentionally demonstrates policy
evaluation with simple rules and a small, sourced curriculum; it is not an
official AASTMT production rulebook.

The 2026-07-20 owner amendment adds first-term automatic enrollment,
capacity-consuming pending subject approvals, and the CGPA-qualified 19-21
credit overload boundary described normatively below.

## Functional Requirements

- FR-1: The system MUST version policy sets by effective dates and academic
  scope.
- FR-2: Approved rules MUST cover registration window, standing, holds, load,
  prerequisites, earned credits, repeats, and conflict/capacity product rules.
  The demo profile MUST enforce configured registration windows, eligible
  standing, absence of blocking holds, and completed prerequisites; accept a
  regular load from 9 through 18 credits with 18 as both the default/recommended
  target and normal maximum; limit GPA below 2.0 to 12 credits; permit 19
  through 21 credits only when authoritative CGPA is at least 3.00 and every
  selected subject receives the required scoped approval; reject more than 21;
  automatically enroll matching required first-program-term prerequisite
  roots; hold capacity for each term-two-or-later self-service subject until
  the complete plan is approved/rejected or the window closes; block every
  unresolved meeting overlap with travel-time buffering disabled; and provide
  no prerequisite waiver, capacity override, add/drop, withdrawal, or generic
  advisor workflow.
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

### AC-2: Unapproved demo-policy conflict (FR-4, FR-5)
Given an imported rule conflicts with the approved demo profile by enabling a
waitlist or capacity override<br>
When an admin attempts to publish that conflicting rule<br>
Then publication is blocked<br>
And the rule is shown as requiring a separately approved policy amendment.

### AC-3: Historical explainability (FR-3, FR-7)
Given a registration used policy version 2026.1<br>
When the decision is inspected after version 2026.2 is published<br>
Then the original version, inputs, source and explanation remain available.

### AC-4: Typed rule safety (FR-6)
Given a draft rule contains an unknown rule type or executable expression<br>
When validation is requested<br>
Then validation rejects it<br>
And no executable content is stored or run.

### AC-5: Deterministic, sourced policy quality (NFR-1, NFR-2, NFR-3, NFR-4)
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

## API Contracts

```typescript
interface PolicyDecisionDto {
  eligible: boolean;
  policyVersion: string;
  evaluatedAtUtc: string;
  inputSummary: Record<string, string>;
  approvedBy: string;
  effectiveFromUtc: string;
  effectiveToUtc?: string;
  results: Array<{
    reasonCode: string;
    passed: boolean;
    explanation: string;
    sourceUrl: string;
    sourceAccessedOn: string;
    overridePossible: boolean;
  }>;
}
```

`POST /api/admin/policies/{policySetId}/simulate` is owned and delivered by
SPEC-009. `GET /api/student/offerings/{offeringId}/eligibility` is owned and
delivered by SPEC-011. SPEC-002 contributes the decision/provenance contract to
both endpoints and does not implement either handler.

## Data Models

| Concept | Role in SPEC-002 | Canonical runtime owner | Required data |
|---|---|---|---|
| PolicyRulebook | Governed artifact | SPEC-002; consumed by SPEC-009 | version, scope, priority, effective dates, rule categories, approval state/actor |
| PolicyRuleDefinition | Governed typed-rule artifact | SPEC-002; consumed by SPEC-009 | type key, validated configuration schema, reason code, source reference |
| PolicyBoundaryExample | Governed boundary fixture | SPEC-002; consumed by SPEC-009 | input, expected reason/result, boundary label, approval |
| PolicySourceRecord | Governed provenance artifact | SPEC-002; consumed by SPEC-009/SPEC-015 | source URL/reference, access date, field-level authority/classification, affected rules, approval state |

Runtime `PolicySet`/`PolicyRule` are owned by SPEC-009 and durable decision
snapshots by SPEC-015.

## Out of Scope

- OS-1: Legal interpretation by software.
- OS-2: Arbitrary scripting/expressions uploaded by users.
- OS-3: Waitlists, capacity/conflict overrides, automatic exceptions, add/drop,
  withdrawal, and generic Advisor or Deanery workflows. The bounded 2026-07-20
  subject-line approval and seat-hold amendment is in scope.
- OS-4: Automatic dismissal or academic-path decisions.
