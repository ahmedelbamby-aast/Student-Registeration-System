# SYS-01 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-003<br>
**Implementation task:** SPEC-003/T258<br>
**Design task:** SPEC-003/T252

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "SYS-01",
  "routeTemplate": "/status/{code}",
  "pageName": "SystemStatusPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-003",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-006",
    "SPEC-007",
    "SPEC-008",
    "SPEC-018"
  ],
  "actors": [
    "Public visitor",
    "Student",
    "Admin",
    "Lecturer",
    "Teaching Assistant"
  ],
  "purpose": "Present safe healthy, degraded, unhealthy, forbidden, not-found, session, maintenance, offline, and unexpected status without protected diagnostics.",
  "informationHierarchy": [
    "Safe status heading",
    "User-safe explanation",
    "Reference ID",
    "Primary recovery action",
    "Support, home, or sign-in actions"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Status heading, safe explanation, reference ID, full-width primary recovery action, then home/sign-in/support links; no shell navigation or table.",
    "375": "At 375 CSS px, Same order with bounded readable text, 44 CSS px actions, and the reference adjacent to support guidance.",
    "768": "At 768 CSS px, Centered single status panel; primary and secondary actions may share a row after the complete explanation.",
    "1024": "At 1024 CSS px, Centered bounded panel with no authenticated sidebar; status, reference, and recovery actions remain one landmark.",
    "1280": "At 1280 CSS px, Same bounded panel and reading order with additional outer gutter only.",
    "1920": "At 1920 CSS px, Same maximum readable width; surplus space never reveals diagnostics or separates reference from support."
  },
  "components": [
    "StatePanel",
    "Alert",
    "Button",
    "AppLink"
  ],
  "dataContracts": [
    "GET /api/public/context",
    "GET /api/health"
  ],
  "actions": [
    "Retry safe destination",
    "Open public gateway",
    "Open appropriate sign-in",
    "Return to authorized home",
    "Open safe support path"
  ],
  "navigationTransitions": [
    "Public gateway -> AUTH-01",
    "Student sign-in -> AUTH-02",
    "Staff sign-in -> AUTH-04",
    "Student home -> STU-01",
    "Admin home -> ADM-01",
    "Staff home -> STF-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "SYS-01-loading-v1",
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
        "SYS-01-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "SYS-01-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "State is not applicable; the reviewed reason is recorded."
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "SYS-01-COMP-STATE-EMPTY"
      ],
      "reason": "empty has no matching collection, protected-session, or editable interaction on this route; failures use the applicable safe state instead."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "SYS-01-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Healthy status heading",
        "HealthSummary status healthy",
        "Version and timestampUtc exactly as returned by GET /api/health",
        "No protected diagnostics, topology, or false registration success"
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "polite",
      "nextActions": [
        "Open public gateway"
      ],
      "testIds": [
        "SYS-01-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "not-applicable",
      "fixture": "SYS-01-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "State is not applicable; the reviewed reason is recorded."
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "SYS-01-COMP-STATE-VALIDATION-ERROR"
      ],
      "reason": "validation-error has no matching collection, protected-session, or editable interaction on this route; failures use the applicable safe state instead."
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "SYS-01-service-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Safe service-error heading",
        "Reference ID",
        "Retry or support action",
        "HealthSummary status degraded or unhealthy with safe version and timestampUtc",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown-code safe fallback"
      ],
      "expectedFocusTarget": "Keep current focus for a background failure; move to the service-error heading only after a navigation failure",
      "liveRegion": "polite",
      "nextActions": [
        "Retry",
        "Open safe support path"
      ],
      "testIds": [
        "SYS-01-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "SYS-01-unauthorized-v1",
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
        "SYS-01-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "SYS-01-session-expired-v1",
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
        "Open appropriate sign-in"
      ],
      "testIds": [
        "SYS-01-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "not-applicable",
      "fixture": "SYS-01-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "State is not applicable; the reviewed reason is recorded."
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "SYS-01-COMP-STATE-STALE"
      ],
      "reason": "stale has no matching collection, protected-session, or editable interaction on this route; failures use the applicable safe state instead."
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "SYS-01-offline-v1",
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
        "SYS-01-COMP-STATE-OFFLINE"
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
    "Status page heading",
    "User-safe explanation",
    "Reference ID",
    "Primary retry or sign-in action",
    "Public gateway or authorized home action",
    "Safe support link"
  ],
  "testIds": [
    "SYS-01-CONTRACT-T253",
    "SYS-01-COMP-T254",
    "SYS-01-E2E-PRIMARY",
    "SYS-01-E2E-FAILURE",
    "SYS-01-A11Y-T255",
    "SYS-01-VIS-T256"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-006": "not-pinned",
    "SPEC-007": "not-pinned",
    "SPEC-008": "not-pinned",
    "SPEC-018": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Status heading, safe explanation, reference ID, full-width primary recovery action, then home/sign-in/support links; no shell navigation or table. |
| 375 | Same order with bounded readable text, 44 CSS px actions, and the reference adjacent to support guidance. |
| 768 | Centered single status panel; primary and secondary actions may share a row after the complete explanation. |
| 1024 | Centered bounded panel with no authenticated sidebar; status, reference, and recovery actions remain one landmark. |
| 1280 | Same bounded panel and reading order with additional outer gutter only. |
| 1920 | Same maximum readable width; surplus space never reveals diagnostics or separates reference from support. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `SYS-01-healthy-v1` (healthy) | success | healthy heading plus safe HealthSummary status, version, and timestamp | topology, secrets, protected diagnostics, or registration-success claim | Open public gateway | `SYS-01-COMP-STATE-SUCCESS` | `SYS-01-E2E-PRIMARY` |
| `SYS-01-degraded-v1` (degraded) | service-error | degraded heading plus safe HealthSummary status, version, timestamp, reference, and retry/support action | topology, secrets, raw diagnostics, or a healthy claim | Retry | `SYS-01-COMP-STATE-SERVICE-ERROR` | `SYS-01-E2E-FAILURE` |
| `SYS-01-unhealthy-v1` (unhealthy) | service-error | unavailable heading plus safe HealthSummary status, version, timestamp, reference, and retry/support action | topology, secrets, raw diagnostics, or a degraded/healthy claim | Open safe support path | `SYS-01-COMP-STATE-SERVICE-ERROR` | `SYS-01-E2E-FAILURE` |
| `SYS-01-403-v1` (403) | unauthorized | 403 heading, safe explanation, reference ID, and authorized sign-in/home action | protected resource identifier or content | Return to authorized home | `SYS-01-COMP-STATE-UNAUTHORIZED` | `SYS-01-E2E-FAILURE` |
| `SYS-01-404-v1` (404) | service-error | 404 heading, safe explanation, reference ID, and home action | requested unauthorized identifier or route internals | Return home | `SYS-01-COMP-STATE-SERVICE-ERROR` | `SYS-01-E2E-FAILURE` |
| `SYS-01-expired-v1` (expired) | session-expired | session expired heading, sign-in action, and no protected content | cached authenticated content | Open appropriate sign-in | `SYS-01-COMP-STATE-SESSION-EXPIRED` | `SYS-01-E2E-FAILURE` |
| `SYS-01-maintenance-v1` (maintenance) | service-error | maintenance heading, reference ID, retry timing, and support action | healthy or registration-success claim | Retry or open support | `SYS-01-COMP-STATE-SERVICE-ERROR` | `SYS-01-E2E-FAILURE` |
| `SYS-01-offline-v1` (offline) | offline | offline heading, retry guidance, and no queued-success claim | claiming a write was queued or accepted | Retry when online | `SYS-01-COMP-STATE-OFFLINE` | `SYS-01-E2E-FAILURE` |
| `SYS-01-unexpected-without-stack-trace-v1` (unexpected without stack trace) | service-error | unexpected-error heading, correlation reference, and safe support action | stack trace, SQL text, credential, raw payload, or unauthorized identifier | Open support or return home | `SYS-01-COMP-STATE-SERVICE-ERROR` | `SYS-01-E2E-FAILURE` |


## Review notes

- The six responsive descriptions preserve one information and focus order,
  wrap all reasons, and provide semantic table alternatives where applicable.
- API entries are copied verbatim from `.specify/page-api-manifest.json`;
  server authorization and decisions remain with the named owner specifications.
- Every UI state has a deterministic fixture, content, focus, live-region,
  next-action, and planned test identifier.
 close. Approval does not promote the route beyond `design-only`.
- Ahmed ELbamby approved immutable record version 1.0 on 2026-07-13, so its
  design task may close. Approval does not promote the route beyond `design-only`.
