# AUTH-01 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-008<br>
**Implementation task:** SPEC-008/T076<br>
**Design task:** SPEC-003/T122

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "AUTH-01",
  "routeTemplate": "/",
  "pageName": "RoleGatewayPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-008",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-008"
  ],
  "actors": [
    "Public visitor"
  ],
  "purpose": "Show privacy-safe service, teaching-term, and registration-window status with named Student and Staff destinations.",
  "informationHierarchy": [
    "AASTMT identity and page heading",
    "Service and registration status",
    "Student and Staff destination cards",
    "Maintenance or unavailable recovery",
    "Safe support and reference path"
  ],
  "responsiveWireframes": {
    "320": "AASTMT identity and the Role gateway heading lead one column, followed by the public status panel, Student destination card, Staff destination card, and support link. Student login, Student activation, and Staff login are full-width 44 CSS px links; maintenance text wraps and no page-level horizontal scroll is allowed.",
    "375": "The same DOM order as 320 CSS px is retained with separate Student and Staff cards. Student login and activation may share the Student card only when both labels fit without truncation; Retry public context remains immediately after the status panel.",
    "768": "The public status panel spans the content width above a two-column Student/Staff gateway. Maintenance or unavailable recovery stays below the status that caused it; no table or horizontal overflow region is introduced.",
    "1024": "A centered public header is followed by a compact status strip and equal Student/Staff destination cards. The support/reference region remains last and actions stay attached to their owning cards.",
    "1280": "The gateway uses a bounded two-column card layout with the server status strip above it. Surplus width increases whitespace, not line length; retry and support controls remain beside the related safe message.",
    "1920": "A centered maximum-width gateway preserves the 1280 layout and reading order. Cards never stretch edge to edge, the official logo keeps its aspect ratio, and status, destinations, recovery, and support remain one logical sequence."
  },
  "components": [
    "EntityCard",
    "StatusBadge",
    "Alert",
    "StatePanel",
    "AppLink",
    "Button"
  ],
  "dataContracts": [
    "GET /api/public/context"
  ],
  "actions": [
    "Open Student login",
    "Open Student activation",
    "Open Staff login",
    "Retry GET /api/public/context",
    "Open safe support path"
  ],
  "navigationTransitions": [
    "Student login -> AUTH-02",
    "Student activation -> AUTH-03",
    "Staff login -> AUTH-04",
    "Safe status -> SYS-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "AUTH-01-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Role gateway without replacing its landmarks",
        "Named progress status",
        "No false success or protected data"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "AUTH-01-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "AUTH-01-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "empty is not applicable on AUTH-01: GET /api/public/context is a required singleton; an absent payload is a service error rather than a legitimate empty collection."
      ],
      "expectedFocusTarget": "Role gateway heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-01-COMP-STATE-EMPTY"
      ],
      "reason": "GET /api/public/context is a required singleton; an absent payload is a service error rather than a legitimate empty collection."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "AUTH-01-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Server date/time and timezone",
        "Public teaching term, registration window, and service state",
        "Named Student login, Student activation, and Staff login destinations",
        "No authenticated identity, capacity, internal-health, or personal data",
        "Minimum journeys traced: normal, maintenance, unavailable, named navigation",
        "The singleton public context is rendered from the server-returned authoritative response"
      ],
      "expectedFocusTarget": "Role gateway heading on initial navigation; keep focus on the activated destination when only public status refreshes",
      "liveRegion": "polite",
      "nextActions": [
        "Open Student login",
        "Open Student activation",
        "Open Staff login"
      ],
      "testIds": [
        "AUTH-01-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "not-applicable",
      "fixture": "AUTH-01-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "validation-error is not applicable on AUTH-01: The public gateway has no editable form or command payload to validate."
      ],
      "expectedFocusTarget": "Role gateway heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-01-COMP-STATE-VALIDATION-ERROR"
      ],
      "reason": "The public gateway has no editable form or command payload to validate."
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "AUTH-01-service-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Safe service-error heading",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown reason code preserved exactly",
        "Safe reference ID and support path",
        "No stack trace, SQL text, credential, arbitrary payload, or false success",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown-code safe fallback"
      ],
      "expectedFocusTarget": "Keep current focus for a background failure; move to the service-error heading only after a navigation failure",
      "liveRegion": "polite",
      "nextActions": [
        "Retry GET /api/public/context",
        "Open safe support/reference path"
      ],
      "testIds": [
        "AUTH-01-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "not-applicable",
      "fixture": "AUTH-01-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "unauthorized is not applicable on AUTH-01: The route is public and renders no protected resource."
      ],
      "expectedFocusTarget": "Role gateway heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-01-COMP-STATE-UNAUTHORIZED"
      ],
      "reason": "The route is public and renders no protected resource."
    },
    {
      "state": "session-expired",
      "applicability": "not-applicable",
      "fixture": "AUTH-01-session-expired-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "session-expired is not applicable on AUTH-01: The route does not require an authenticated session."
      ],
      "expectedFocusTarget": "Role gateway heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-01-COMP-STATE-SESSION-EXPIRED"
      ],
      "reason": "The route does not require an authenticated session."
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "AUTH-01-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Changed public-context status",
        "WINDOW_CHANGED or refreshed context version preserved exactly",
        "Authoritative refresh and review action",
        "Cached window state is not reused as success",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "Retry GET /api/public/context",
        "Review changed term/window status"
      ],
      "testIds": [
        "AUTH-01-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "AUTH-01-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline Role gateway status",
        "Browser connectivity state, not server success",
        "Retry guidance and safe reference path",
        "No queued, cached, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry GET /api/public/context when online"
      ],
      "testIds": [
        "AUTH-01-COMP-STATE-OFFLINE"
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
    "Role gateway page heading",
    "Server service, teaching-term, and registration-window status",
    "Student login link",
    "Student activation link",
    "Staff login link",
    "Retry public context button when an error state is present",
    "Safe support and reference link"
  ],
  "testIds": [
    "AUTH-01-CONTRACT-T123",
    "AUTH-01-COMP-T124",
    "AUTH-01-E2E-PRIMARY",
    "AUTH-01-E2E-FAILURE",
    "AUTH-01-A11Y-T125",
    "AUTH-01-VIS-T126"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-008": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```


## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | AASTMT identity and the Role gateway heading lead one column, followed by the public status panel, Student destination card, Staff destination card, and support link. Student login, Student activation, and Staff login are full-width 44 CSS px links; maintenance text wraps and no page-level horizontal scroll is allowed. |
| 375 | The same DOM order as 320 CSS px is retained with separate Student and Staff cards. Student login and activation may share the Student card only when both labels fit without truncation; Retry public context remains immediately after the status panel. |
| 768 | The public status panel spans the content width above a two-column Student/Staff gateway. Maintenance or unavailable recovery stays below the status that caused it; no table or horizontal overflow region is introduced. |
| 1024 | A centered public header is followed by a compact status strip and equal Student/Staff destination cards. The support/reference region remains last and actions stay attached to their owning cards. |
| 1280 | The gateway uses a bounded two-column card layout with the server status strip above it. Surplus width increases whitespace, not line length; retry and support controls remain beside the related safe message. |
| 1920 | A centered maximum-width gateway preserves the 1280 layout and reading order. Cards never stretch edge to edge, the official logo keeps its aspect ratio, and status, destinations, recovery, and support remain one logical sequence. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-008/T075. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `AUTH-01-normal-v1` (normal) | success | Server timezone, teaching term, registration window, Available service text, and named Student login, Student activation, and Staff login destinations. | Display name, roles, student data, capacity, internal health, or browser time as academic truth. | Open a named destination. | `AUTH-01-COMP-STATE-SUCCESS` | `AUTH-01-E2E-PRIMARY` |
| `AUTH-01-maintenance-v1` (maintenance) | service-error | MAINTENANCE, safe explanation, reference path, and Retry public context. | Available/open success, protected data, stack trace, SQL text, or invented reopening time. | Retry GET /api/public/context or open support. | `AUTH-01-COMP-STATE-SERVICE-ERROR` | `AUTH-01-E2E-FAILURE` |
| `AUTH-01-unavailable-v1` (unavailable) | service-error | SERVICE_UNAVAILABLE or safe unknown-code fallback, reference ID, and retry guidance. | Successful registration-window claim, stale context presented as current, or queued success. | Retry GET /api/public/context when service returns. | `AUTH-01-COMP-STATE-SERVICE-ERROR` | `AUTH-01-E2E-FAILURE` |
| `AUTH-01-named-navigation-v1` (named navigation) | success | Visible Student login, Student activation, and Staff login names linked to AUTH-02, AUTH-03, and AUTH-04. | Unlabelled role icon, staff role picker, or navigation inferred from client state. | Activate the named link with keyboard or pointer. | `AUTH-01-COMP-STATE-SUCCESS` | `AUTH-01-E2E-PRIMARY` |

## Review notes

- The six responsive descriptions are specific to AUTH-01 and preserve its DOM,
  keyboard, action, and information order at every governed width.
- The API list remains verbatim from `.specify/page-api-manifest.json`; retry
  names its GET endpoint and all other actions are navigation-only.
- Background stale, offline, and service changes keep focus; a navigation-level
  safe failure focuses its heading exactly as required by the reason map.
 close. Approval does not promote the route beyond `design-only`.
- Ahmed ELbamby approved immutable record version 1.0 on 2026-07-13, so its
  design task may close. Approval does not promote the route beyond `design-only`.
