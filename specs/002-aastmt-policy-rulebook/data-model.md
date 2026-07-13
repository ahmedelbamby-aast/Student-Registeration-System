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

`DEMO-POC-2026.1` is represented by one `PolicyRulebook` version with typed
definitions for window, standing, hold, prerequisite, load, probation,
capacity, and overlap rules. Its source records distinguish `OfficialAASTMT`
curriculum rows from `SyntheticDemoGap` rows; a synthetic row requires a
human-readable label and rationale and cannot cite AASTMT as its authority.

## Governance Rules

- Every numeric or categorical policy value records source, access date,
  scope, effective period, approval state, and approving actor.
- Unapproved or conflicting values fail closed and cannot govern submission.
- Published versions are immutable and are superseded by new versions.
- The demo profile records Ahmed's demo approval independently of source
  provenance so the two authorities cannot be conflated.
- The default/recommended regular-plan target is 18 credits, the hard normal
  maximum is 18, and the GPA-below-2.0 maximum is 12.
- Travel-time buffering, waitlists, overrides, and workflow states excluded by
  the demo profile have no enabled rule definition.
- Runtime keys, persistence mappings, and transactions are defined by their
  canonical owner specs and SPEC-005 rather than duplicated here.
