# Architecture Decision Registry

```yaml
schemaVersion: 1.0
ownerSpec: SPEC-004
approvalVersion: Gate-A-2026-07-13
approvedBy: Ahmed ELbamby
```

This registry contains accepted or superseded architecture decisions governed
by the SPEC-004 `ArchitectureDecision` schema. A proposal or draft has no
decision authority until Ahmed ELbamby approves it and its executable evidence
passes.

## Accepted decisions

| Decision | Status | Decided at | Approved by | Affected boundaries | Architecture-test updates |
|---|---|---|---|---|---|
| [ADR-001: Modular Monolith](ADR-001-modular-monolith.md) | accepted | 2026-07-13 | Ahmed ELbamby | project graph, deployment shape, single database | `ModuleDependencyTests`, `ApprovedStackTests`, `PersistenceBoundaryTests`, `ProhibitedComplexityTests` |

ADR-001 records the context, the one-deployable modular-monolith decision, its
consequences, the simpler alternatives considered, the affected boundaries,
and the architecture-test updates. It is approved for the non-production demo;
it does not approve production SQL topology, key custody, or deployment
authority.

## Proposals without decision authority

| Proposal | Current status | Effect |
|---|---|---|
| [ADR-002: Atomic Seat Allocation](ADR-002-atomic-capacity.md) | Proposed | No accepted ArchitectureDecision entry; its owner spec must complete approval and executable concurrency evidence. |

Proposed records are deliberately excluded from the accepted/superseded schema
set until their owning gate passes.

## Required change workflow

Every module dependency or deployment decision change requires an approved ADR
and an architecture-test update in the same review. The decision record must
state:

1. context and measurable need;
2. the decision and consequences;
3. simpler alternatives considered;
4. affected module/deployment boundaries;
5. the exact tests added or changed; and
6. Ahmed ELbamby's approval date and scope.

CI and review reject the change when any field is absent, when
`ModuleDependencyTests` disagrees with the module-boundary record, or when an
accepted decision lacks its declared executable update. A superseded decision
remains in Git and links to its replacement; history is not rewritten.
