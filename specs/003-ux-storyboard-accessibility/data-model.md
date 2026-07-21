# Data Model: Frontend Page Design, Storyboard, Accessibility and Functional Testing

## Owned Client Models and Governed Artifacts

- **FrontendAppContextView**, **UiStatus**, and **ConflictView** are client-only presentation models owned by SPEC-003. They do not become domain or persistence entities.
- **PageDesignRecord**, **DesignTokenSet**, and **FrontendTestRecord** are governed design/test metadata owned by SPEC-003 and are not SQL entities.

## Detailed Model

| View/design model | Required data |
|---|---|
| FrontendAppContextView | Server time/timezone, teaching term, registration term/window, display name, exactly one authorized active role, session state/expiry, service state, supportReferencePath |
| UiStatus | Stable code, heading, message, severity, next actions, reference ID |
| ConflictView | Subjects/groups, every overlap slot, alternatives, resolution links |
| PageDesignRecord | Route, design owner, single implementation owner, contributors, layout/component/state/interaction/responsive/accessibility/test contract and approval version |
| DesignTokenSet | Version, approval, color/type/spacing/size/border/focus/elevation/motion/breakpoint/z-index tokens |
| FrontendTestRecord | Test ID, route, requirement links, type, fixture, expected outcome |

## Integrity Rules

- Every PageDesignRecord MUST declare a schema version, one route ID/template pair, nonempty ownerSpecs, exactly one design owner, exactly one canonical implementation owner, contributor contract versions, readiness state, and an approval version.
- Route IDs, templates, and canonical page names MUST be unique and MUST match route-manifest.json, page-matrix.md, and the owning feature specifications.
- Every required responsive width and applicable UI state MUST be present; a not-applicable state requires a reviewed reason.
- DesignTokenSet versions MUST be immutable after approval. Neutral semantic tokens MUST meet the specified contrast rules, and pages/components MUST reference approved tokens rather than guessed literal values. The official AASTMT logo is a separate provenance-recorded local asset governed by `docs/BRAND_ASSETS.md`; its approval does not authorize inferred colors or typography.
- Every FrontendTestRecord MUST have a unique test ID, existing route ID, valid owning requirement/criterion IDs, deterministic fixture version, evidence type, and expected outcome.
- Each route MUST trace to its PageDesignRecord, ownerSpecs, contributing API/reason contracts, implementation task, component tests, client-contract tests, Playwright journeys, axe/keyboard evidence, visual baselines, and manual evidence where required.
- `implementation-ready` requires approved, version-pinned implementation-owner and contributor contracts; `design-only` never authorizes page source or executable integration work.
- Every visual baseline MUST record route, state, viewport, browser/browser-engine build, operating-system image, token version, fixture version, approval actor, and approval date; automatic baseline replacement is prohibited. The POC matrix contains current stable Chrome, Edge, and Firefox plus pinned Playwright WebKit, labels WebKit only as WebKit, and records actual Safari/macOS as deferred rather than passed.
- PageDesignRecord, DesignTokenSet, and FrontendTestRecord are governed design/test metadata, not SQL entities, unless a separately approved runtime requirement introduces persistence.
