# ADM-09 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-017<br>
**Implementation task:** SPEC-017/T090<br>
**Design task:** SPEC-003/T227

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-09",
  "routeTemplate": "/admin/audit",
  "pageName": "AuditAdministrationPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-017",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-017"
  ],
  "actors": [
    "Admin"
  ],
  "purpose": "Search immutable audit events and request or download authorized scoped exports.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Scoped filters",
    "Paged audit results",
    "Immutable event detail",
    "Export job status",
    "Authorized download or expiry result"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Scope filters, stacked audit results, selected immutable event, export status, then authorized download/support.",
    "375": "At 375 CSS px, Same order with event fields wrapping and download action only beside a ready authorized job.",
    "768": "At 768 CSS px, Compact navigation; paged audit table above event detail and export status.",
    "1024": "At 1024 CSS px, Persistent navigation with results left and immutable detail/export status right; focus returns to Request export after dialog/result.",
    "1280": "At 1280 CSS px, Bounded result/detail columns with filter scope and correlation reference visible before event payload.",
    "1920": "At 1920 CSS px, Centered workspace; surplus width never exposes extra fields or separates export authorization from download."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "SearchFilter",
    "DataTable",
    "Pagination",
    "EntityCard",
    "StatusBadge",
    "Button",
    "AppLink",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/admin/audit",
    "POST /api/admin/exports",
    "GET /api/admin/exports/{jobId}",
    "GET /api/admin/exports/{jobId}/download"
  ],
  "actions": [
    "Search audit",
    "Change page",
    "Open event detail",
    "Request export",
    "Download ready export"
  ],
  "navigationTransitions": [
    "Actions stay on ADM-09",
    "Restricted or expired -> SYS-01",
    "Dashboard -> ADM-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-09-loading-v1",
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
        "ADM-09-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "ADM-09-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: user action",
        "Empty-state heading",
        "Plain-language reason",
        "Next action",
        "Empty is not a successful command result"
      ],
      "expectedFocusTarget": "Preserve the active audit filter or Clear filters control while the empty result heading is announced",
      "liveRegion": "polite",
      "nextActions": [
        "Clear filters"
      ],
      "testIds": [
        "ADM-09-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-09-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Scoped immutable audit events with actor, action, target, timestamp, and correlation reference",
        "Export job status and authorized download availability",
        "Success only when serverAccepted is true",
        "Minimum journeys: empty; pagination; queued; ready; failed; expired; restricted"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "ADM-09-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "ADM-09-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary",
        "Field or action reason",
        "No false success",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary",
      "liveRegion": "assertive",
      "nextActions": [
        "Review highlighted input",
        "Retry the action"
      ],
      "testIds": [
        "ADM-09-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-09-service-error-v1",
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
        "Request a new export"
      ],
      "testIds": [
        "ADM-09-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-09-unauthorized-v1",
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
        "Return to Admin home"
      ],
      "testIds": [
        "ADM-09-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-09-session-expired-v1",
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
        "ADM-09-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-09-stale-v1",
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
        "Request a new export"
      ],
      "testIds": [
        "ADM-09-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-09-offline-v1",
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
        "ADM-09-COMP-STATE-OFFLINE"
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
    "Audit heading",
    "Scope and search filters",
    "Audit result table",
    "Pagination",
    "Selected immutable event detail",
    "Request-export control",
    "Export job status and authorized download",
    "Safe support link"
  ],
  "testIds": [
    "ADM-09-CONTRACT-T228",
    "ADM-09-COMP-T229",
    "ADM-09-E2E-PRIMARY",
    "ADM-09-E2E-FAILURE",
    "ADM-09-A11Y-T230",
    "ADM-09-VIS-T231"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-017": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Scope filters, stacked audit results, selected immutable event, export status, then authorized download/support. |
| 375 | Same order with event fields wrapping and download action only beside a ready authorized job. |
| 768 | Compact navigation; paged audit table above event detail and export status. |
| 1024 | Persistent navigation with results left and immutable detail/export status right; focus returns to Request export after dialog/result. |
| 1280 | Bounded result/detail columns with filter scope and correlation reference visible before event payload. |
| 1920 | Centered workspace; surplus width never exposes extra fields or separates export authorization from download. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-09-empty-v1` (empty) | empty | empty audit result heading, active filters, and clear-filter action | blank table without explanation | Clear filters | `ADM-09-COMP-STATE-EMPTY` | `ADM-09-E2E-PRIMARY` |
| `ADM-09-pagination-v1` (pagination) | success | stable sort, page number, total count, and immutable event rows | duplicate or skipped events | Change page | `ADM-09-COMP-STATE-SUCCESS` | `ADM-09-E2E-PRIMARY` |
| `ADM-09-queued-v1` (queued) | success | queued export status, job reference, timestamp, and refresh action | download before authorization and readiness | Refresh export status | `ADM-09-COMP-STATE-SUCCESS` | `ADM-09-E2E-PRIMARY` |
| `ADM-09-ready-v1` (ready) | success | ready status, expiry, scope, and authorized download | unscoped download | Download export | `ADM-09-COMP-STATE-SUCCESS` | `ADM-09-E2E-PRIMARY` |
| `ADM-09-failed-v1` (failed) | service-error | failed status, safe reason, reference ID, and retry action | stack trace or partial export | Request a new export | `ADM-09-COMP-STATE-SERVICE-ERROR` | `ADM-09-E2E-FAILURE` |
| `ADM-09-expired-v1` (expired) | stale | expired status and new-export action | expired download link | Request a new export | `ADM-09-COMP-STATE-STALE` | `ADM-09-E2E-FAILURE` |
| `ADM-09-restricted-v1` (restricted) | unauthorized | safe restricted heading and authorized-home action | audit event fields or export identifiers | Return to Admin home | `ADM-09-COMP-STATE-UNAUTHORIZED` | `ADM-09-E2E-FAILURE` |


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
