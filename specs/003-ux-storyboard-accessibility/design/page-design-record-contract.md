# Page Design Record Contract

**Version:** `page-design-record/1.0`  
**Schema:** `schemas/page-design-record.schema.json`  
**Approval authority:** Ahmed ELbamby

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

The design owner is always SPEC-003. The implementation owner and owner list
must match `.specify/route-manifest.json`; the record cannot transfer runtime
ownership.

## State completeness

The state list contains loading, empty, success, validation-error,
service-error, unauthorized, session-expired, stale, and offline exactly once.
Each state records `applicability`, deterministic `fixture` and
`fixtureVersion`, `expectedContent`, `expectedFocusTarget`, `liveRegion`,
`nextActions`, and state-specific `testIds`. A `not-applicable` state also
requires a reviewed reason. Applicable server reason codes are rendered as
received; unknown codes use the safe fallback design and never become success.

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
