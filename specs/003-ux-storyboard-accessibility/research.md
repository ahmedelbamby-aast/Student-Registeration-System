# Research: Frontend Page Design, Storyboard, Accessibility and Functional Testing

## Decisions

### Modular boundary
**Decision**: Own this capability in the Client module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface PageDesignRecord {
  routeId: string;
  routeTemplate: string;
  designOwnerSpec: string;
  implementationOwnerSpec: string;
  ownerSpecs: string[];
  actors: string[];
  components: string[];
  dataContracts: string[];
  actions: string[];
  states: UiStateCase[];
  responsiveWidths: number[];
  focusOrder: string[];
  testIds: string[];
  approvalVersion: string;
}
interface UiStateCase {
  state: "loading" | "empty" | "success" | "validation-error" |
    "service-error" | "unauthorized" | "session-expired" | "stale" | "offline";
  applicability: "required" | "not-applicable";
  reason?: string;
  expectedFocusTarget?: string;
  liveRegion?: "none" | "polite" | "assertive";
}
interface FrontendTestRecord {
  testId: string;
  routeId: string;
  requirementIds: string[];
  type: "component" | "contract" | "e2e" | "accessibility" | "visual";
  fixture: string;
  expectedOutcome: string;
}
```

The UI consumes the versioned contracts in SPEC-006 through SPEC-017,
including `GET /api/public/context` and the authenticated application-context
resource owned by SPEC-008. This method/path is a consumed dependency, not an
endpoint implemented or owned by SPEC-003.
Stable error/reason codes MUST map to designed states; the client MUST retain
unknown-code fallback behavior. SPEC-003 owns no server endpoint.

### Frontend verification stack
**Decision**: Use bUnit for Blazor component behavior, Microsoft.Playwright for browser journeys, axe-core for automated accessibility, and Playwright screenshot comparison for visual regression.
**Rationale**: These layers separate fast component feedback, client-contract behavior, real navigation, accessibility signals, and pixel-level review without treating snapshots as functional assertions.
**Alternatives rejected**: Browser-only testing, snapshot-only testing, and manual-only accessibility review because each leaves important behavior unverified.

### Browser evidence and version pinning
**Decision**: Pin CI operating-system images and exact browser builds in a versioned browser matrix. Use Chromium, Firefox, and WebKit automation for repeatable coverage, then require a separately recorded run on actual stable Safari on pinned macOS for Safari support.
**Rationale**: Playwright WebKit is useful compatibility evidence but is not the shipping Safari browser. Version-pinned provenance makes failures and visual baselines reproducible.
**Alternatives rejected**: Floating latest browsers and labeling WebKit automation as Safari certification.

### Deterministic fixtures and visual baselines
**Decision**: Version fixtures for server time, term, role, policy, rowversion, capacity, reason codes, and UI states. Store visual baselines by route/state/browser/viewport with token version and UX approval; mask only reviewed nondeterministic regions.
**Rationale**: Registration state is time- and concurrency-sensitive. Uncontrolled clocks, data, or animation create misleading failures and unreviewable baseline churn.
**Alternatives rejected**: Production-data copies, arbitrary screenshot tolerances, and automatic baseline replacement.

### Flake and evidence policy
**Decision**: A first-run failure fails the gate; a retry can collect diagnostics only. Critical journeys cannot be quarantined, and every intermittent failure requires an owned defect before release. Machine reports and manual Safari, screen-reader, keyboard, and usability evidence are retained together.
**Rationale**: Retrying until green hides race, timing, and accessibility defects in the system's highest-risk journeys.
**Alternatives rejected**: Pass-on-retry, unowned quarantine, and unsupported claims based only on generated reports.

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
