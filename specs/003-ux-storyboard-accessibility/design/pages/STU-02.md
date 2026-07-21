# STU-02 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-011<br>
**Implementation task:** SPEC-011/T040<br>
**Design task:** SPEC-003/T152

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-02",
  "routeTemplate": "/student/subjects",
  "pageName": "SubjectDiscoveryPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-011",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-011"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Discover eligible offerings with complete group, capacity, staffing, meeting, effective credit-limit, and server-reason context.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Search and filter controls",
    "Current/projected credits, default target 18, and effective 12/18 maximum",
    "Eligible and unavailable result summary",
    "Offering cards with complete group bundles and reasons",
    "Pagination or empty recovery",
    "Current-plan navigation"
  ],
  "responsiveWireframes": {
    "320": "Collapsed navigation and heading precede Search, filters, Reset, the current/projected/default-18/effective-12-or-18 credit summary, result summary, and one-column offering cards. Every card exposes each group's capacity/state/selectability/seats, Lecturer, required TA, activity, location, day, and start/end before pagination and recovery.",
    "375": "The 320 DOM order is retained; compact filters may share a row only with 44 CSS px targets. Each offering keeps credit context, policy/source reasons, and complete labelled group bundles without hover-only details.",
    "768": "Compact navigation precedes filters and the effective credit summary above a two-column card grid. Result summary comes before cards; every group bundle keeps capacity, staffing, activity/location/day/time, status, and reasons together before pagination.",
    "1024": "Persistent navigation, a bounded filter rail, and main results are allowed without DOM reordering. Offering cards remain semantic articles and complete group bundles never move into a detached panel; pagination and current-plan navigation follow results.",
    "1280": "The results grid may use three bounded columns. Search/filter state, current/projected credits, default 18, effective 12/18 maximum, and result count remain above; each card exposes all group and eligibility/reason content plus a named details link.",
    "1920": "A centered maximum-width shell preserves the 1280 grid and focus order. Surplus width increases gutters only; group bundles remain bounded and readable, with no hidden staffing/meeting/load context or page-level horizontal scroll."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "SearchFilter",
    "EntityCard",
    "GroupCard",
    "CapacityBreakdown",
    "StatusBadge",
    "Alert",
    "StatePanel",
    "Pagination",
    "Button",
    "AppLink"
  ],
  "dataContracts": [
    "GET /api/student/terms/{termId}/offerings"
  ],
  "actions": [
    "Search GET /api/student/terms/{termId}/offerings",
    "Apply filters through GET /api/student/terms/{termId}/offerings",
    "Reset filters and rerun GET /api/student/terms/{termId}/offerings",
    "Change result page through GET /api/student/terms/{termId}/offerings",
    "Open STU-03 offering details",
    "Open STU-04 current plan"
  ],
  "navigationTransitions": [
    "Offering details -> STU-03",
    "Current plan -> STU-04",
    "Dashboard -> STU-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STU-02-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Available subjects heading with stable landmarks",
        "Named progress status and disabled duplicate command",
        "No false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "STU-02-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STU-02-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: user action",
        "No results heading",
        "Current search/filter summary",
        "No results reason without implying no catalogue",
        "Named Reset filters action"
      ],
      "expectedFocusTarget": "Preserve the active search or filter control while the No results heading is announced",
      "liveRegion": "polite",
      "nextActions": [
        "Reset filters and rerun GET /api/student/terms/{termId}/offerings",
        "Reset filters"
      ],
      "testIds": [
        "STU-02-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STU-02-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
      "Server-returned offering code, title, credits, availability, and eligibility status",
      "Current and projected plan credits, default target 18, and effective maximum 12 or 18 with policy/source reasons",
      "Every group capacity, state, selectability, seats remaining, Lecturer, required TA, activity, location, day, and start/end time",
      "Every unavailable reason and stable reason code",
        "Search/filter/page result count",
        "No client eligibility or capacity decision",
      "Minimum journeys: eligible; unavailable reasons; no results; reset; stale capacity; service error; normal 18; probation 12"
      ],
      "expectedFocusTarget": "Available subjects heading on initial load; preserve the active search, filter, card, or pagination control after refresh",
      "liveRegion": "polite",
      "nextActions": [
        "Open an offering at STU-03",
        "Open current plan at STU-04"
      ],
      "testIds": [
        "STU-02-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STU-02-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary linked to an invalid search/filter value",
        "Stable owner reason",
        "No results changed into success by client filtering",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field, group, or blocker",
      "liveRegion": "assertive",
      "nextActions": [
        "Correct the linked filter",
        "Rerun the offerings GET"
      ],
      "testIds": [
        "STU-02-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STU-02-service-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Safe service-error heading",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown code preserved exactly",
        "Reference ID and retry/support action",
        "No stack trace, SQL text, credential, arbitrary payload, or success",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown-code safe fallback"
      ],
      "expectedFocusTarget": "Keep current focus for a background failure; move to the service-error heading only after a navigation failure",
      "liveRegion": "polite",
      "nextActions": [
        "Retry the failed API explicitly",
        "Open safe support path",
        "Retry offerings GET"
      ],
      "testIds": [
        "STU-02-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STU-02-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Access-denied heading",
        "UNAUTHORIZED or FORBIDDEN preserved exactly",
        "No Student protected data, identifier, cached content, or unauthorized action"
      ],
      "expectedFocusTarget": "Access-denied heading after navigation",
      "liveRegion": "assertive",
      "nextActions": [
        "Return to authorized home or sign in"
      ],
      "testIds": [
        "STU-02-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STU-02-session-expired-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Session-expired heading",
        "SESSION_EXPIRED preserved exactly",
        "Only safe plan/reference ID retained",
        "No protected cached content or automatic success"
      ],
      "expectedFocusTarget": "Session-expired heading, announced assertively once",
      "liveRegion": "assertive",
      "nextActions": [
        "Sign in again",
        "Refetch and revalidate before editing"
      ],
      "testIds": [
        "STU-02-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STU-02-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change heading",
        "GROUP_FULL, WINDOW_CHANGED, or STALE_VERSION",
        "Authoritative refresh and review action",
        "Cached availability, plan, capacity, or success is not reused",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "Rerun GET /api/student/terms/{termId}/offerings",
        "Review changed capacity/reasons",
        "Rerun offerings GET and review"
      ],
      "testIds": [
        "STU-02-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STU-02-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline status for Available subjects heading",
        "Browser connectivity state and retry guidance",
        "No queued, cached, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry GET /api/student/terms/{termId}/offerings when online"
      ],
      "testIds": [
        "STU-02-COMP-STATE-OFFLINE"
      ]
    }
  ],
  "responsiveWidths": [
    320,
    375,
    768,
    1024,
    1280,
    1920
  ],
  "focusOrder": [
    "Skip link",
    "Student role navigation",
    "Available subjects heading",
    "Current/projected/default/effective credit summary and policy reason",
    "Search field",
    "Eligibility, credits, day/time, and availability filters in labelled order",
    "Reset filters button",
    "Result summary",
    "Offering cards in server order with complete group capacity, staffing, activity/location, day/start/end, and details links",
    "Pagination controls",
    "Current plan link",
    "Safe support/reference link"
  ],
  "testIds": [
    "STU-02-CONTRACT-T153",
    "STU-02-COMP-T154",
    "STU-02-E2E-PRIMARY",
    "STU-02-E2E-FAILURE",
    "STU-02-A11Y-T155",
    "STU-02-VIS-T156"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-011": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Collapsed navigation and heading precede Search, filters, Reset, the current/projected/default-18/effective-12-or-18 credit summary, result summary, and one-column offering cards. Every card exposes each group's capacity/state/selectability/seats, Lecturer, required TA, activity, location, day, and start/end before pagination and recovery. |
| 375 | The 320 DOM order is retained; compact filters may share a row only with 44 CSS px targets. Each offering keeps credit context, policy/source reasons, and complete labelled group bundles without hover-only details. |
| 768 | Compact navigation precedes filters and the effective credit summary above a two-column card grid. Result summary comes before cards; every group bundle keeps capacity, staffing, activity/location/day/time, status, and reasons together before pagination. |
| 1024 | Persistent navigation, a bounded filter rail, and main results are allowed without DOM reordering. Offering cards remain semantic articles and complete group bundles never move into a detached panel; pagination and current-plan navigation follow results. |
| 1280 | The results grid may use three bounded columns. Search/filter state, current/projected credits, default 18, effective 12/18 maximum, and result count remain above; each card exposes all group and eligibility/reason content plus a named details link. |
| 1920 | A centered maximum-width shell preserves the 1280 grid and focus order. Surplus width increases gutters only; group bundles remain bounded and readable, with no hidden staffing/meeting/load context or page-level horizontal scroll. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-011/T039. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STU-02-eligible-v1` (eligible) | success | eligible offering code/title/credits, current/projected/default/effective credit context, and every complete capacity/staff/activity/location/day/time group bundle | client-calculated eligibility/load, missing group fields, or hidden server reasons | Open STU-03 details | `STU-02-COMP-STATE-SUCCESS` | `STU-02-E2E-PRIMARY` |
| `STU-02-unavailable-reasons-v1` (unavailable reasons) | success | unavailable reasons and policy/source metadata exactly as returned plus complete nonselectable group bundles | disabled card without text reason, client override, or selectable unavailable status | Review reasons or another offering | `STU-02-COMP-STATE-SUCCESS` | `STU-02-E2E-PRIMARY` |
| `STU-02-no-results-v1` (no results) | empty | no results heading, active criteria, Reset filters, and current-plan link | blank page, invented unavailable reason, or hidden filters | Reset filters | `STU-02-COMP-STATE-EMPTY` | `STU-02-E2E-FAILURE` |
| `STU-02-reset-v1` (reset) | success | cleared criteria, rerun GET result set, and focus returned to Search or result summary | stale filtered cards, duplicate GET command, or lost keyboard focus | Review refreshed results | `STU-02-COMP-STATE-SUCCESS` | `STU-02-E2E-PRIMARY` |
| `STU-02-stale-capacity-v1` (stale capacity) | stale | GROUP_FULL or stale capacity text, refreshed result timestamp, and details/retry action | cached Available badge, client-reserved seat, or success | Rerun offerings GET and review | `STU-02-COMP-STATE-STALE` | `STU-02-E2E-FAILURE` |
| `STU-02-service-error-v1` (service error) | service-error | safe service error, reference ID, Retry, and support path | stack trace, SQL, raw payload, empty-as-success, or cached current results | Retry offerings GET | `STU-02-COMP-STATE-SERVICE-ERROR` | `STU-02-E2E-FAILURE` |
| `STU-02-normal-18-v1` (normal 18) | success | current/projected credits, default target 18, effective maximum 18, and sourced nonblocking load context | GPA-derived browser limit, 12-credit claim, or missing provenance | Review an offering or current plan | `STU-02-COMP-STATE-SUCCESS` | `STU-02-E2E-PRIMARY` |
| `STU-02-probation-12-v1` (probation 12) | success | current/projected credits, default target 18, effective maximum 12, and safe GPA-below-2.0 policy/source reason | presenting 18 as applicable, exposing unrelated profile data, or client-calculated limit | Review load reason and eligible groups | `STU-02-COMP-STATE-SUCCESS` | `STU-02-E2E-PRIMARY` |

## Review notes

- Six annotated layouts are specific to STU-02 and preserve DOM, keyboard,
  action, state, and information order without hiding reasons or boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; commands
  name their endpoint and navigation-only actions never imply a missing write.
- Background stale/offline/service updates preserve focus; submitted validation
  focuses its summary, and route-specific restoration is explicit in focusOrder.
 Approval does not promote the route beyond `design-only`.
- Ahmed ELbamby approved immutable record version 1.0 on 2026-07-13, so its
  design task may close. Approval does not promote the route beyond `design-only`.
