# Page Design Record Contract

**Version:** `page-design-record/1.1-draft`<br>
**Schema:** `schemas/page-design-record.schema.json`  
**Approval authority:** Ahmed ELbamby<br>
**Amendment status:** Pending Ahmed ELbamby review

Version 1.1 drafts the task-link, endpoint-composite-authority, exact journey
label, route-owned state/action trace, and trigger-aware live-region rules added
while preparing the first 27 Page Design Records. The already approved 1.0
contract remains immutable; this draft does not become an approved pin until
Ahmed reviews it.
The closed JSON field shape did not change, so each record correctly retains
`schemaVersion: "1.0"`; the separate contract/index version carries this
governance amendment.

Each canonical route has one governed record named by route ID under
`design/pages/`. The record describes the page before its implementation owner
writes route source. It is design and traceability metadata, not a SQL entity.

## Required metadata

Every record supplies these schema fields exactly once:

- `schemaVersion`, `routeId`, `routeTemplate`, and canonical `pageName`;
- `designOwnerSpec`, one `implementationOwnerSpec`, and complete `ownerSpecs`;
- authorized `actors`, concise `purpose`, ordered `informationHierarchy`, and
  the six-width `responsiveWireframes`;
- canonical `components`, consumed `dataContracts`, user `actions`, and
  `navigationTransitions`;
- nine `states`, exact `responsiveWidths`, keyboard `focusOrder`, and governed
  `testIds`;
- `contributorContractVersions`, `readinessState`, and `approvalVersion`.

The Markdown wrapper also names its SPEC-003 design task and the single
canonical runtime delivery as `**Design task:** SPEC-003/TNNN` and
`**Implementation task:** SPEC-NNN/TNNN`. Those trace links are not test IDs
and do not extend the closed machine-readable schema.

The design owner is always SPEC-003. The implementation owner and owner list
must match `.specify/route-manifest.json`; the record cannot transfer runtime
ownership. Every API in `.specify/page-api-manifest.json` must resolve in
`.specify/endpoint-manifest.json`, and that endpoint authority owner must be
present in the route and record owner lists with an FR/AC link. Any explicit
composite contributor on the endpoint entry is subject to the same rule.

## State completeness

The state list contains loading, empty, success, validation-error,
service-error, unauthorized, session-expired, stale, and offline exactly once.
Each state records `applicability`, deterministic `fixture` and
`fixtureVersion`, `expectedContent`, `expectedFocusTarget`, `liveRegion`,
`nextActions`, and state-specific `testIds`. A `not-applicable` state also
requires a reviewed reason. Applicable server reason codes are rendered as
received; unknown codes use the safe fallback design and never become success.

Every required `empty` and `validation-error` fixture declares exactly one
trigger marker in `expectedContent`. `Trigger: initial navigation` uses no live
region and moves focus to the page or state heading. `Trigger: user action`
uses a polite live region and preserves the user's current focus.
`Trigger: submitted form failure` uses an assertive live region and moves focus
to the validation summary. A `not-applicable` state declares no trigger marker.
This keeps announcements dependent on the interaction that caused the state
rather than on a route-wide assumption.

Each record expands the page-matrix minimum journeys in a normative Markdown
trace table. Every row names its deterministic fixture, canonical state,
required and forbidden content/actions, next action, component test ID, and
primary or failure E2E ID. Multiple journey fixtures may map to one of the nine
canonical states without adding an undeclared schema state. The fixture cell
uses `` `ROUTE-slug-v1` (exact page-matrix journey label) `` so every minimum
journey resolves exactly once. When only one journey maps to a canonical state,
its table next action must appear verbatim in that state's machine-readable
`nextActions`.

## Readiness states

- `design-only` permits storyboard and Page Design Record review; it does not authorize route source,
  route API binding, or executable route tests.
- `implementation-ready` requires the implementation owner and every
  contributor to be approved, with each exact immutable version recorded in
  `contributorContractVersions` and reconciled in the route contributor
  baseline. Any contributor change returns the row to `design-only`.

Shared SPEC-003 tokens and components may be built independently when they
accept presentation-ready parameters and perform no role, eligibility,
capacity, conflict, or submission decision.

## Approval and change control

`approvalVersion` identifies the reviewed record version. Ahmed ELbamby is the
only demo approval authority. Route identity, ownership, contract dependency,
state, responsive, focus, or test-plan changes require a new immutable record
version and another approval entry; approved versions are never silently
rewritten.
