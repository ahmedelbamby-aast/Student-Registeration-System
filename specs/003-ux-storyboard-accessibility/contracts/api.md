# API Contract: Frontend Page Design, Storyboard, Accessibility and Functional Testing

## Feature Contract

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

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
