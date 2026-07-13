# SPEC-003: Frontend Page Design, Storyboard, Accessibility and Functional Testing

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** Approved (Gate A demo implementation, 2026-07-13)<br>
**Owner:** UX Lead<br>
**Reviewers:** Product Owner, QA, Student/Admin/Lecturer/TA representatives<br>
**Target:** Sprint 0 design baseline; implemented incrementally with each feature<br>
**Dependencies:** SPEC-001, SPEC-002<br>

## Context

Registration occurs under time pressure and must communicate eligibility,
capacity, stale data, and conflicts without ambiguity. The frontend therefore
needs more than a route list: every page needs a reviewed design record,
reusable component composition, complete state behavior, responsive rules,
accessibility semantics, and executable functional-test coverage.

This specification owns the frontend design system and the design/test contract
for all 27 MVP route templates in docs/STORYBOARD.md. Feature specifications
own their domain behavior and APIs; this specification owns how that behavior
is presented, operated, and verified in the Blazor WebAssembly client.

## Functional Requirements

- FR-1: The product MUST design and deliver exactly the 27 MVP route templates
  listed in the Page Coverage Matrix below.
- FR-2: Before implementation, every route MUST have an approved Page Design
  Record containing its design-owner SPEC, single implementation-owner SPEC,
  contributing SPECs, purpose, authorized actors, information hierarchy,
  responsive wireframes, component inventory, data dependencies, actions,
  navigation transitions, focus order, applicable UI states, and test IDs.
- FR-3: The frontend MUST use a versioned design-token contract covering
  neutral semantic colors, typography, spacing, sizing, borders, focus,
  elevation, motion, breakpoints, and z-index. The official AASTMT logo MUST
  use the unchanged asset sourced from the official `aast.edu` URL recorded in
  `docs/BRAND_ASSETS.md`, MUST preserve its aspect ratio, and MUST be served
  from a provenance-recorded local copy rather than a runtime hotlink. Other
  institutional colors, typefaces, or usage rules MUST NOT be inferred from
  the logo.
- FR-4: Reusable components MUST cover application shell, role navigation,
  buttons, links, form fields, validation summary, search/filter, cards,
  status badges, alerts, loading/empty/error panels, confirmation dialog,
  data table/pagination, group card, capacity indicator, schedule calendar,
  chronological schedule list, conflict panel, and receipt summary.
- FR-5: Every data route MUST explicitly design loading, empty, success,
  validation error, recoverable service error, unauthorized, session-expired,
  stale/concurrent-change, and offline states. A Page Design Record MUST mark a
  state N/A with a reason when the state cannot occur.
- FR-6: A hard schedule conflict MUST show a red X icon, the text Conflict,
  involved subjects/groups, every overlapping day/start/end, explanation, and
  direct manual-resolution actions.
- FR-7: Registration submission MUST remain disabled while a hard conflict or
  other blocking validation exists, and the disabled control MUST expose every
  blocking reason in text.
- FR-8: Every timetable calendar MUST have an equivalent chronological
  list/table containing the same groups, staff, rooms, days, and times.
- FR-9: Student login/activation/recovery and the shared staff login MUST use
  distinct page designs; the staff page MUST NOT provide a client role picker.
- FR-10: The authenticated shell MUST display server date/time and timezone,
  teaching term, registration term/window, user, authorized role context,
  session status, and a safe support/reference path.
- FR-11: Every page MUST define mobile-first layouts at 320, 375, 768, 1024,
  1280, and 1920 CSS pixels, including navigation collapse, table alternatives,
  action placement, content order, and overflow behavior.
- FR-12: Every route MUST have planned automated functional coverage consisting
  of component tests for all interactive states, API-contract integration
  tests, Playwright end-to-end tests for primary and failure journeys,
  automated accessibility checks, and approved-baseline visual regression.
  Page source, contract tests, and E2E execution MUST remain deferred until the
  route's implementation-owner specification and every contributing API/reason
  contract are approved and version-pinned; a design-only record MUST NOT be
  treated as implementation authorization.
- FR-13: The frontend traceability matrix MUST map each route and component to
  its owning SPEC/FR, Page Design Record, implementation task, and functional,
  accessibility, and visual test IDs.
- FR-14: The client MUST treat eligibility, authorization, time/term, conflict,
  capacity, and submission results as server-authoritative and MUST re-render
  stable server reason codes without converting a rejected result into success.

## Page Coverage Matrix

| Area | Required route IDs |
|---|---|
| Public/authentication | AUTH-01, AUTH-02, AUTH-03, AUTH-04, AUTH-05 |
| Student | STU-01, STU-02, STU-03, STU-04, STU-05, STU-06, STU-07, STU-08 |
| Administration | ADM-01, ADM-02, ADM-03, ADM-04, ADM-05, ADM-06, ADM-07, ADM-08, ADM-09 |
| Lecturer/Teaching Assistant | STF-01, STF-02, STF-03, STF-04 |
| System status | SYS-01 |

The route, purpose, owning feature, component composition, states, and minimum
functional tests for each ID are normative in page-matrix.md.

## Non-Functional Requirements

- NFR-1: Every critical route and reusable component MUST conform to WCAG 2.2
  AA with zero unresolved critical or serious automated accessibility finding.
- NFR-2: All actions MUST be keyboard operable in a logical order with a
  visible focus indicator and focus restoration after dialogs/navigation.
- NFR-3: Normal text contrast MUST be at least 4.5:1; large text and interactive
  component boundaries/focus indicators MUST be at least 3:1.
- NFR-4: Pointer targets MUST be at least 44 by 44 CSS pixels except the
  documented WCAG inline/spacing exceptions.
- NFR-5: Core pages MUST reflow at 400% zoom and from 320 through 1920 CSS
  pixels without two-dimensional scrolling except a documented data table that
  has a non-scrolling semantic alternative.
- NFR-6: Student discovery, schedule, and review routes MUST achieve Largest
  Contentful Paint at or below 2.5 seconds at p75 with production compression,
  cold browser cache, a four-core/4-GB client profile, 10-Mbps down/2-Mbps up,
  100-ms round-trip latency, and server APIs meeting their specified p95.
- NFR-7: The POC browser gate MUST pass in current stable Chrome, Edge, and
  Firefox plus a version-pinned Playwright WebKit target. Browser and engine
  builds MUST be recorded in the versioned evidence matrix, and WebKit results
  MUST be labelled WebKit rather than Safari. Actual Safari/macOS verification
  is deferred from the POC and requires a separately approved future browser
  support/release decision.
- NFR-8: Go-live registration usability completion MUST be at least 90% across
  at least eight representative students including novice, keyboard-only, and
  screen-reader participants, plus at least three Admin, three Lecturer, and
  three TA participants for their critical journeys, with no unresolved
  critical or major core-flow usability defect.
- NFR-9: Visual regression MUST reject every unapproved difference from the
  versioned baseline at 375, 768, 1280, and 1920 CSS pixels.
- NFR-10: The MVP MUST be English-first and localization-ready: user-facing
  strings MUST be externalized, formats MUST be culture-aware, layout MUST be
  direction-safe, and user text MUST NOT be embedded as component control
  logic.

## Acceptance Criteria

### AC-1: Complete Page Design Record (FR-1, FR-2, FR-5, FR-11, FR-13)
Given any one of the 27 route IDs is selected for implementation<br>
When its frontend readiness review occurs<br>
Then its approved Page Design Record contains every FR-2 field<br>
And every applicable state and responsive width is designed<br>
And its owning requirement, task, and test IDs are linked.

### AC-2: Versioned design system (FR-3, FR-4, NFR-3, NFR-4)
Given the first page is ready for visual implementation<br>
When the design-system review occurs<br>
Then every required token category and reusable component is documented<br>
And component default/hover/active/focus/disabled/loading/error states use only
approved tokens<br>
And the official-logo source, local-copy provenance, unchanged rendering, and
accessible name are verified<br>
And contrast and pointer-target measurements pass.

### AC-3: Functional state coverage (FR-5, FR-12)
Given a data route declares its complete applicable UI-state set<br>
When component and Playwright test plans are inspected<br>
Then every applicable state has a unique test ID, deterministic fixture, expected content,
focus behavior, and retry/next action<br>
And no applicable state is covered only by a visual snapshot.

### AC-4: Distinct identity pages (FR-9, FR-12, FR-14)
Given student and staff identity journeys are rendered<br>
When their component, contract, authorization, and end-to-end tests execute<br>
Then student login/activation/recovery are distinct from shared staff login<br>
And no staff role picker is rendered<br>
And route manipulation cannot grant a server role.

### AC-5: Student discovery and group details (FR-1, FR-4, FR-12, FR-14)
Given eligible, unavailable, full, stale, and service-error fixtures exist<br>
When STU-02 and STU-03 functional tests execute<br>
Then search/filter results, reasons, credits, capacity, Lecturer, TA, room,
day/time, state changes, and retry actions match the server contracts.

### AC-6: Accessible schedule conflict (FR-6, FR-7, FR-8, NFR-1, NFR-2)
Given two selected groups overlap<br>
When STU-04 and STU-05 render using keyboard and screen-reader fixtures<br>
Then a red X and the word Conflict identify the issue<br>
And subject/group/day/start/end and resolution actions are announced<br>
And equivalent calendar/list data is present<br>
And submission remains disabled with every blocking reason.

### AC-7: Admin page functionality (FR-1, FR-5, FR-12, FR-14)
Given valid, invalid, stale-rowversion, unauthorized, and concurrent-edit
fixtures exist for ADM-02 through ADM-09<br>
When their functional tests execute<br>
Then preview, validation, confirmation, pagination, conflict recovery, audit,
and server-authority behaviors match the owning feature contracts.

### AC-8: Lecturer and TA page functionality (FR-1, FR-8, FR-12, FR-14)
Given Lecturer-only, TA-only, dual-role, unassigned, stale-assignment, and
availability-deadline fixtures exist<br>
When STF-01 through STF-04 functional tests execute<br>
Then shared components display only server-authorized assignments and actions<br>
And timetable calendar/list content is equivalent.

### AC-9: Responsive and accessible route matrix (FR-11, FR-12, NFR-1, NFR-2, NFR-5)
Given each of the 27 routes is rendered at every required responsive width and
at 400% zoom<br>
When keyboard, automated accessibility, and responsive functional suites run<br>
Then no critical action or reason is clipped or unreachable<br>
And focus order follows the Page Design Record<br>
And only documented table exceptions scroll in two dimensions.

### AC-10: Cross-browser visual gate (FR-3, FR-12, NFR-7, NFR-9)
Given an approved versioned visual baseline and POC browser matrix exist<br>
When primary and error states run in current stable Chrome, Edge, and Firefox,
plus the pinned Playwright WebKit target, at every visual width<br>
Then functional assertions pass in every POC browser target<br>
And WebKit evidence is labeled WebKit rather than Safari<br>
And any visual difference blocks the gate until approved or corrected.

### AC-11: Server-authoritative stale response (FR-10, FR-14)
Given the shell shows an open window and available group from an earlier read<br>
When the server returns WINDOW_CLOSED, GROUP_FULL, or PLAN_CHANGED<br>
Then the client presents the stable reason and reference path<br>
And does not show success or enable submission from cached state.

### AC-12: Usability release gate (NFR-8, NFR-10)
Given the approved representative UAT sample completes the registration
journey with localization-ready labels<br>
When completion and severity results are calculated<br>
Then at least 90% complete the journey<br>
And no critical or major core-flow usability defect remains open.

### AC-13: Frontend performance budget (NFR-6)
Given production-like Blazor assets, API fixtures, device/network profile, and
student discovery, schedule, and review routes<br>
When page performance is measured across the approved sample<br>
Then Largest Contentful Paint is at most 2.5 seconds at p75 for each route.

### AC-14: Public gateway and student dashboard (FR-1, FR-5, FR-10, FR-12, FR-14)
Given public available/maintenance fixtures and student open/upcoming/closed,
hold, and incomplete-profile fixtures<br>
When AUTH-01 and STU-01 functional journeys execute<br>
Then the privacy-safe gateway shows server term/window/service state and named
Student/Staff destinations<br>
And the dashboard shows only authenticated server context, academic summary,
blocking reasons, and the correct start/resume action.

### AC-15: Result, history, and account journeys (FR-1, FR-5, FR-8, FR-12, FR-14)
Given accepted/rejected/lost-response, empty/current/history/archive, and
account/session fixtures<br>
When STU-06, STU-07, and STU-08 functional journeys execute<br>
Then atomic outcome and no-partial semantics, equivalent timetable/list,
history, security actions, ownership denial, and retry behavior match their
owning server contracts.

### AC-16: Admin dashboard functionality (FR-1, FR-5, FR-12, FR-14)
Given live, paused-refresh, stale, degraded, and unauthorized metrics fixtures<br>
When ADM-01 functional journeys execute<br>
Then timestamps, staleness, text-equivalent metrics, pause/resume, warnings,
and authorization behavior match SPEC-017/SPEC-018 contracts.

### AC-17: Safe system status functionality (FR-1, FR-5, FR-12, FR-14)
Given 403, 404, session-expired, maintenance, offline, and unexpected-error
fixtures<br>
When SYS-01 functional journeys execute<br>
Then the correct safe heading, next action, and reference ID are shown<br>
And no stack trace, SQL text, credential, or unauthorized identifier appears.

## Edge Cases

- EC-1: Status changes while keyboard focus is in a group card -> announce the
  update politely without stealing focus or changing the selected group.
- EC-2: Session expires with an unsaved plan -> preserve only the safe plan
  identifier, reauthenticate, fetch the server plan, and revalidate before
  rendering editable state.
- EC-3: 400% zoom or 320-pixel width -> critical actions remain reachable and
  labels do not truncate blocking reasons.
- EC-4: Reduced motion enabled -> disable nonessential animation and preserve
  equivalent state feedback.
- EC-5: JavaScript/WebAssembly startup fails or the user is offline -> render a
  static recoverable status with retry guidance; never display a false success.
- EC-6: API returns an unknown reason code -> render the generic safe message,
  correlation/reference ID, and retry/support action; log no sensitive payload.
- EC-7: A visual baseline changes intentionally -> require UX approval,
  documented affected routes/states, and a versioned baseline update.
- EC-8: A table exceeds the viewport -> retain headers and keyboard access and
  provide the specified stacked/list alternative.
- EC-9: Browser autofill populates identity fields -> labels remain visible,
  values remain reviewable, and secret values are not exposed.
- EC-10: Rapid double activation of a command button -> disable while pending
  and rely on the command idempotency contract; only one result is presented.

## API Contracts

```typescript
interface PageDesignRecord {
  schemaVersion: string;
  routeId: string;
  routeTemplate: string;
  pageName: string;
  designOwnerSpec: string;
  implementationOwnerSpec: string;
  ownerSpecs: string[];
  actors: string[];
  purpose: string;
  informationHierarchy: string[];
  responsiveWireframes: Record<string, string>;
  components: string[];
  dataContracts: string[];
  actions: string[];
  navigationTransitions: string[];
  states: UiStateCase[];
  responsiveWidths: number[];
  focusOrder: string[];
  testIds: string[];
  contributorContractVersions: Record<string, string>;
  readinessState: "design-only" | "implementation-ready";
  approvalVersion: string;
}
interface UiStateCase {
  state: "loading" | "empty" | "success" | "validation-error" |
    "service-error" | "unauthorized" | "session-expired" | "stale" | "offline";
  applicability: "required" | "not-applicable";
  reason?: string;
  fixture: string;
  fixtureVersion: string;
  expectedContent: string[];
  expectedFocusTarget: string;
  liveRegion: "none" | "polite" | "assertive";
  nextActions: string[];
  testIds: string[];
}
interface FrontendTestRecord {
  testId: string;
  routeId: string;
  requirementIds: string[];
  type: "component" | "contract" | "e2e" | "accessibility" | "visual";
  fixture: string;
  fixtureVersion: string;
  expectedOutcome: string;
}
```

The UI consumes the versioned contracts in SPEC-006 through SPEC-017,
including `GET /api/public/context` and the authenticated application-context
resource owned by SPEC-008. This method/path is a consumed dependency, not an
endpoint implemented or owned by SPEC-003.
Stable error/reason codes MUST map to designed states; the client MUST retain
unknown-code fallback behavior. SPEC-003 owns no server endpoint.

## Data Models

| View/design model | Required data |
|---|---|
| FrontendAppContextView | Server time/timezone, teaching term, registration term/window, display name, authorized roles, nullable active role only during role-selection-required, session state/expiry, service state, supportReferencePath |
| UiStatus | Stable code, heading, message, severity, next actions, reference ID |
| ConflictView | Subjects/groups, every overlap slot, alternatives, resolution links |
| PageDesignRecord | Route, design owner, single implementation owner, contributors, layout/component/state/interaction/responsive/accessibility/test contract and approval version |
| DesignTokenSet | Version, approval, color/type/spacing/size/border/focus/elevation/motion/breakpoint/z-index tokens |
| FrontendTestRecord | Test ID, route, requirement links, type, fixture, expected outcome |

## Out of Scope

- OS-1: Inventing additional institutional brand colors, typefaces, logos, or
  usage rules beyond the sourced official AASTMT logo, or recoloring, cropping,
  stretching, or distorting that logo.
- OS-2: Arabic translation and RTL delivery; the MVP remains English-first and
  localization-ready, and a separately approved localization specification is
  required before delivery.
- OS-3: Native mobile applications.
- OS-4: Drag-and-drop as the only schedule-editing interaction.
- OS-5: Client-side authorization, eligibility, capacity, or commit decisions.
- OS-6: Creating frontend source or executable tests before Gate A approval.
  Gate A was approved on 2026-07-13; implementation now follows FR-12's
  route-owner and contributor-contract gates.
