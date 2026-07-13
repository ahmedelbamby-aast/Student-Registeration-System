# ADM-01 Page Design Record

**Record version:** `draft/1.0`<br>
**Approval status:** Pending Ahmed ELbamby review<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-017<br>
**Implementation task:** SPEC-017/T074<br>
**Design task:** SPEC-003/T187

This complete design draft is governed by `page-design-record/1.1-draft`. It may be
reviewed as design evidence, but it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-01",
  "routeTemplate": "/admin",
  "pageName": "AdminDashboardPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-017",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007",
    "SPEC-008",
    "SPEC-010",
    "SPEC-017"
  ],
  "actors": [
    "Admin"
  ],
  "purpose": "Monitor current term, traffic, fill, failures, data quality, and degraded operational state.",
  "informationHierarchy": [
    "Authenticated AppShell context and timestamp",
    "Refresh controls",
    "Key metrics",
    "Fill and failure summaries",
    "Warnings and degraded banner",
    "Administration module navigation"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, One column: heading, server timestamp and pause/resume/refresh controls, metric cards, warning banner, then module links. Every chart has adjacent text.",
    "375": "At 375 CSS px, One column with two-up metric cards only when each card remains 44 CSS px operable; timestamp precedes controls and warnings precede navigation.",
    "768": "At 768 CSS px, Compact navigation and a two-column metric grid; warnings span both columns and module links follow the metrics in DOM order.",
    "1024": "At 1024 CSS px, Persistent navigation, three-column metric grid, separate warning region, and module links after all operational context.",
    "1280": "At 1280 CSS px, Persistent navigation and four bounded metric columns; paused/degraded text stays beside the timestamp and controls.",
    "1920": "At 1920 CSS px, Centered bounded dashboard; surplus width increases gutters only. Reading, focus, warning, and module-link order remains identical."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "EntityCard",
    "StatusBadge",
    "Alert",
    "Button",
    "AppLink",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/context",
    "GET /api/admin/operations/metrics",
    "GET /api/admin/schedule-impact-alerts"
  ],
  "actions": [
    "Pause refresh",
    "Resume refresh",
    "Refresh metrics",
    "Open administration module"
  ],
  "navigationTransitions": [
    "Module navigation -> ADM-02 through ADM-09",
    "Unauthorized -> SYS-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-01-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading heading",
        "Progress status",
        "No false success"
      ],
      "expectedFocusTarget": "Preserve current focus; use the page heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "ADM-01-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "ADM-01-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "State is not applicable; the reviewed reason is recorded."
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "ADM-01-COMP-STATE-EMPTY"
      ],
      "reason": "empty has no matching collection, protected-session, or editable interaction on this route; failures use the applicable safe state instead."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-01-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Server timestamp, active term, traffic, fill, failure, and data-quality metrics",
        "Text equivalents plus live, paused, stale, or degraded status",
        "Metrics and context are rendered from the server-returned authoritative snapshot and its timestamps",
        "Minimum journeys: live; paused refresh; stale; degraded; unauthorized"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "ADM-01-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "not-applicable",
      "fixture": "ADM-01-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "State is not applicable; the reviewed reason is recorded."
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "ADM-01-COMP-STATE-VALIDATION-ERROR"
      ],
      "reason": "validation-error has no matching collection, protected-session, or editable interaction on this route; failures use the applicable safe state instead."
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-01-service-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Safe service-error heading",
        "Reference ID",
        "Retry or support action",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown-code safe fallback"
      ],
      "expectedFocusTarget": "Keep current focus for a background failure; move to the service-error heading only after a navigation failure",
      "liveRegion": "polite",
      "nextActions": [
        "Retry or open support"
      ],
      "testIds": [
        "ADM-01-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-01-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Access-denied heading",
        "Safe navigation action",
        "No protected content",
        "UNAUTHORIZED or FORBIDDEN reason family"
      ],
      "expectedFocusTarget": "Access-denied heading",
      "liveRegion": "assertive",
      "nextActions": [
        "Return to authorized home"
      ],
      "testIds": [
        "ADM-01-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-01-session-expired-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Session-expired heading",
        "Sign-in action",
        "No protected content",
        "SESSION_EXPIRED reason family"
      ],
      "expectedFocusTarget": "Session-expired heading",
      "liveRegion": "assertive",
      "nextActions": [
        "Sign in again"
      ],
      "testIds": [
        "ADM-01-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-01-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change heading",
        "Refresh and review action",
        "No false success",
        "Preserve the owner-contract stale reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus; move to the concurrent-change heading only after failed navigation or a submitted command",
      "liveRegion": "polite",
      "nextActions": [
        "Refresh metrics"
      ],
      "testIds": [
        "ADM-01-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-01-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline heading",
        "Retry guidance",
        "No false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the connection status is announced",
      "liveRegion": "polite",
      "nextActions": [
        "Retry when online"
      ],
      "testIds": [
        "ADM-01-COMP-STATE-OFFLINE"
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
    "Administration navigation",
    "Admin dashboard heading",
    "Server timestamp and pause, resume, or refresh control",
    "Metric cards in DOM order with text equivalents",
    "Warnings and degraded-service alert",
    "Terms, users, students, catalogue, offerings, resources, registrations, and audit links",
    "Safe support link"
  ],
  "testIds": [
    "ADM-01-CONTRACT-T188",
    "ADM-01-COMP-T189",
    "ADM-01-E2E-PRIMARY",
    "ADM-01-E2E-FAILURE",
    "ADM-01-A11Y-T190",
    "ADM-01-VIS-T191"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1-draft",
    "SPEC-007": "not-pinned",
    "SPEC-008": "not-pinned",
    "SPEC-010": "not-pinned",
    "SPEC-017": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "pending-Ahmed-review"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | One column: heading, server timestamp and pause/resume/refresh controls, metric cards, warning banner, then module links. Every chart has adjacent text. |
| 375 | One column with two-up metric cards only when each card remains 44 CSS px operable; timestamp precedes controls and warnings precede navigation. |
| 768 | Compact navigation and a two-column metric grid; warnings span both columns and module links follow the metrics in DOM order. |
| 1024 | Persistent navigation, three-column metric grid, separate warning region, and module links after all operational context. |
| 1280 | Persistent navigation and four bounded metric columns; paused/degraded text stays beside the timestamp and controls. |
| 1920 | Centered bounded dashboard; surplus width increases gutters only. Reading, focus, warning, and module-link order remains identical. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-01-live-v1` (live) | success | live timestamp, active term, traffic, fill rates, failures, and text-equivalent metrics | color-only charts or metrics without timestamps | Pause refresh or open a module | `ADM-01-COMP-STATE-SUCCESS` | `ADM-01-E2E-PRIMARY` |
| `ADM-01-paused-refresh-v1` (paused refresh) | success | paused refresh label, last-updated timestamp, unchanged metrics, and Resume refresh | claiming metrics are live | Resume refresh | `ADM-01-COMP-STATE-SUCCESS` | `ADM-01-E2E-PRIMARY` |
| `ADM-01-stale-v1` (stale) | stale | stale label, source timestamp, stable stale reason, and Refresh metrics | presenting stale data as current | Refresh metrics | `ADM-01-COMP-STATE-STALE` | `ADM-01-E2E-FAILURE` |
| `ADM-01-degraded-v1` (degraded) | service-error | degraded warning, last safe timestamp, reference ID, and retry/support actions | stack trace, raw payload, or false healthy status | Retry or open support | `ADM-01-COMP-STATE-SERVICE-ERROR` | `ADM-01-E2E-FAILURE` |
| `ADM-01-unauthorized-v1` (unauthorized) | unauthorized | safe access-denied heading and authorized-home action | any Admin metric or identifier | Return to authorized home | `ADM-01-COMP-STATE-UNAUTHORIZED` | `ADM-01-E2E-FAILURE` |


## Review notes

- The six responsive descriptions preserve one information and focus order,
  wrap all reasons, and provide semantic table alternatives where applicable.
- API entries are copied verbatim from `.specify/page-api-manifest.json`;
  server authorization and decisions remain with the named owner specifications.
- Every UI state has a deterministic fixture, content, focus, live-region,
  next-action, and planned test identifier.
- Ahmed ELbamby must approve this exact draft version before its PDR task can
  close. Approval does not promote the route beyond `design-only`.
