# Data Model: AASTMT Policy Rulebook

## Owned Governance Artifacts

- **PolicyRulebook**: Versioned, scoped, approval-aware rulebook artifact.
- **PolicyRuleDefinition**: Typed, non-executable rule-definition artifact.
- **PolicyBoundaryExample**: Registrar-supplied, approved boundary fixture.
- **PolicySourceRecord**: Provenance, authority, access, conflict, and approval record.

Runtime `PolicySet`/`PolicyRule` belong to SPEC-009 and durable decision
snapshots to SPEC-015.

## Detailed Model

| Concept | Artifact role | Runtime owner | Required data |
|---|---|---|---|
| PolicyRulebook | Governed artifact | SPEC-002; consumed by SPEC-009 | version, scope, priority, effective dates, rule categories, approval state/actor |
| PolicyRuleDefinition | Governed typed-rule artifact | SPEC-002; consumed by SPEC-009 | type key, validated configuration schema, reason code, source reference |
| PolicyBoundaryExample | Governed boundary fixture | SPEC-002; consumed by SPEC-009 | input, expected reason/result, boundary label, approval |
| PolicySourceRecord | Governed provenance artifact | SPEC-002; consumed by SPEC-009/SPEC-015 | source reference, access date, authority, affected rules, approval state |

## Governance Rules

- Every numeric or categorical policy value records source, access date,
  scope, effective period, approval state, and approving actor.
- Unapproved or conflicting values fail closed and cannot govern submission.
- Published versions are immutable and are superseded by new versions.
- Runtime keys, persistence mappings, and transactions are defined by their
  canonical owner specs and SPEC-005 rather than duplicated here.
