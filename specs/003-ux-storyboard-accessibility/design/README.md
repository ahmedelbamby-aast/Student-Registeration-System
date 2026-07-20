# Frontend Design and Traceability Index

**Contract version:** `frontend-design-index/2.0`<br>
**Amendment status:** Approved by Ahmed ELbamby on 2026-07-20

Version 2.0 expands the governed inventory to exactly 30 routes and pins the
unified authenticated visual system. It adds STU-09, ADM-10, and STF-05 and
retains exactly one implementation owner per route. Earlier approved index
versions remain immutable history.

This directory is the governed design source for SPEC-003. It does not replace
runtime feature specifications, source ownership, or browser evidence.

## Authoritative inputs

| Concern | Source |
|---|---|
| Exact route identity and owner list | `.specify/route-manifest.json` |
| Exact reusable component name and source path | `.specify/component-manifest.json` |
| Route API dependency | `.specify/page-api-manifest.json` |
| Endpoint authority owner and explicit composite contributors | `.specify/endpoint-manifest.json` |
| Page record shape | `schemas/page-design-record.schema.json` |
| Frontend test metadata | `schemas/frontend-test-record.schema.json` |
| Approved task writer/dependency | `tasks.md` |

## Design contracts

- `route-inventory.md`: exact 30 routes and single implementation owner.
- `page-design-record-contract.md`: Page Design Record completeness and
  `design-only` versus implementation-ready gates.
- `responsive-layout-contract.md`: six-width reflow and 400% zoom behavior.
- `tokens/contract.md`: neutral primitive, semantic, and component token rules.
- `components/catalogue.md`: current and approved-planned reusable components.
- `components/unified-authenticated-composition.md`: authenticated shell 2.0,
  responsive navigation, adaptive density, and light-only decisions.
- `states/reason-map.md`: server-authoritative reason and UI state mapping.
- `pages/*.md`: one immutable, Ahmed-approved design record per route ID;
  approval remains design-only until every downstream contract is pinned.

## Required route trace row

Each route Page Design Record is the trace row. It must link the route to:

1. its complete owner SPEC and FR/AC references from the route manifest,
   including every authority owner for its page-API entries;
2. the single implementation task in the owner specification;
3. its component test ID and contract test ID;
4. its E2E test ID, including contributor journeys where applicable;
5. its accessibility test ID and visual test ID;
6. deterministic fixture/version, expected outcome, and evidence location.

Test IDs use the stable route prefix: `{ROUTE}-COMP-*`, `{ROUTE}-CONTRACT-*`,
`{ROUTE}-E2E-*`, `{ROUTE}-A11Y-*`, and `{ROUTE}-VIS-*`. A planned identifier is
not pass evidence; the separate evidence record must name the executed version,
browser/engine where relevant, result, actor, and date.

## Required component trace row

Each component-manifest entry maps to SPEC-003 FR-4, its
single canonical Phase-5 source task, its preceding test task, and every route
record that consumes it. Component names and paths are never duplicated in a
second registry.

## Readiness rule

All route records begin `design-only`. That state permits review of layout,
content hierarchy, states, interactions, responsive behavior, accessibility,
and planned tests. It does not permit route source, API binding, or executable
route evidence. The contributor baseline may promote a record only after the
implementation owner and every contributing contract are approved and pinned
to exact immutable versions.
