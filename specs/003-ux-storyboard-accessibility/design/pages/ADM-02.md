# ADM-02 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-008<br>
**Implementation task:** SPEC-008/T080<br>
**Design task:** SPEC-003/T192

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-02",
  "routeTemplate": "/admin/terms",
  "pageName": "TermAdministrationPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-008",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-008",
    "SPEC-017"
  ],
  "actors": [
    "Admin"
  ],
  "purpose": "Create and version teaching terms and registration windows and publish only valid contexts.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Term and window list",
    "Selected lifecycle context",
    "Versioned editor",
    "Validation summary",
    "Publish confirmation and result"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Stacked term cards followed by the selected editor, ordered date/timezone fields, validation summary, then full-width save/publish controls.",
    "375": "At 375 CSS px, Same order as 320 with window rows grouped under their term and confirmation actions directly after validation.",
    "768": "At 768 CSS px, Compact navigation; term table above a two-column editor. Window dates stay grouped and publish follows validation findings.",
    "1024": "At 1024 CSS px, Persistent navigation with term list left and selected versioned editor right; publish dialog overlays the editor and restores its trigger.",
    "1280": "At 1280 CSS px, Persistent navigation, bounded term table and editor columns, with lifecycle/status beside the selected term rather than detached.",
    "1920": "At 1920 CSS px, Centered workspace with fixed readable editor width; additional space never separates validation or publish confirmation from the fields."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "DataTable",
    "Pagination",
    "FormField",
    "ValidationSummary",
    "StatusBadge",
    "ConfirmationDialog",
    "Button",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/admin/terms",
    "POST /api/admin/terms",
    "PUT /api/admin/terms/{termId}",
    "POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish"
  ],
  "actions": [
    "Create term",
    "Save term or window through POST or PUT after client field checks and owner-server validation",
    "Publish registration window"
  ],
  "navigationTransitions": [
    "Edits stay on ADM-02",
    "Operations -> ADM-08",
    "Dashboard -> ADM-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-02-loading-v1",
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
        "ADM-02-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "ADM-02-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
      "Empty-state heading",
      "Plain-language reason",
      "Create term action",
      "Empty is not a successful command result"
      ],
      "expectedFocusTarget": "Empty-state heading",
      "liveRegion": "none",
      "nextActions": [
        "Create term"
      ],
      "testIds": [
        "ADM-02-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-02-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Versioned term list, timezone, lifecycle, and registration-window status",
        "Validated editor values and publication state from the server",
        "Success only when serverAccepted is true",
        "Minimum journeys: create; invalid dates; overlap; concurrent edit; publish"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "ADM-02-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "ADM-02-validation-error-v1",
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
        "ADM-02-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-02-service-error-v1",
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
        "Retry",
        "Open safe support path"
      ],
      "testIds": [
        "ADM-02-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-02-unauthorized-v1",
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
        "ADM-02-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-02-session-expired-v1",
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
        "ADM-02-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-02-stale-v1",
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
        "Refresh and review differences"
      ],
      "testIds": [
        "ADM-02-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-02-offline-v1",
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
        "ADM-02-COMP-STATE-OFFLINE"
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
    "Terms heading",
    "Term and window table controls",
    "Selected term editor fields in label order",
    "Validation summary after a failed submit",
    "Create, save, and publish controls",
    "Confirmation dialog title, cancel, then confirm",
    "Return focus to the publish trigger after dialog close",
    "Safe support link"
  ],
  "testIds": [
    "ADM-02-CONTRACT-T193",
    "ADM-02-COMP-T194",
    "ADM-02-E2E-PRIMARY",
    "ADM-02-E2E-FAILURE",
    "ADM-02-A11Y-T195",
    "ADM-02-VIS-T196"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-008": "not-pinned",
    "SPEC-017": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Stacked term cards followed by the selected editor, ordered date/timezone fields, validation summary, then full-width save/publish controls. |
| 375 | Same order as 320 with window rows grouped under their term and confirmation actions directly after validation. |
| 768 | Compact navigation; term table above a two-column editor. Window dates stay grouped and publish follows validation findings. |
| 1024 | Persistent navigation with term list left and selected versioned editor right; publish dialog overlays the editor and restores its trigger. |
| 1280 | Persistent navigation, bounded term table and editor columns, with lifecycle/status beside the selected term rather than detached. |
| 1920 | Centered workspace with fixed readable editor width; additional space never separates validation or publish confirmation from the fields. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-02-create-v1` (create) | success | created term, timezone, lifecycle, row version, and registration-window rows | browser-derived current term or silent defaults | Review or continue editing | `ADM-02-COMP-STATE-SUCCESS` | `ADM-02-E2E-PRIMARY` |
| `ADM-02-invalid-dates-v1` (invalid dates) | validation-error | invalid dates reason, linked start/end fields, and validation summary | saving or publishing invalid dates | Correct dates and retry | `ADM-02-COMP-STATE-VALIDATION-ERROR` | `ADM-02-E2E-FAILURE` |
| `ADM-02-overlap-v1` (overlap) | validation-error | overlap reason and every conflicting registration window | publishing overlapping windows | Edit the conflicting windows | `ADM-02-COMP-STATE-VALIDATION-ERROR` | `ADM-02-E2E-FAILURE` |
| `ADM-02-concurrent-edit-v1` (concurrent edit) | stale | STALE_VERSION, changed fields, refresh action, and preserved safe draft values | overwriting the newer version | Refresh and review differences | `ADM-02-COMP-STATE-STALE` | `ADM-02-E2E-FAILURE` |
| `ADM-02-publish-v1` (publish) | success | published status, effective window, actor, timestamp, and immutable version reference | success before server confirmation | Return to the term list | `ADM-02-COMP-STATE-SUCCESS` | `ADM-02-E2E-PRIMARY` |


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
