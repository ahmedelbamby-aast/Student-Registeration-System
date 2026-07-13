# ADM-07 Page Design Record

**Record version:** `draft/1.0`<br>
**Approval status:** Pending Ahmed ELbamby review<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-010<br>
**Implementation task:** SPEC-010/T101<br>
**Design task:** SPEC-003/T217

This complete design draft is governed by `page-design-record/1.1-draft`. It may be
reviewed as design evidence, but it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-07",
  "routeTemplate": "/admin/resources",
  "pageName": "ResourceAdministrationPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-010",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-010",
    "SPEC-017"
  ],
  "actors": [
    "Admin"
  ],
  "purpose": "Manage rooms, inspect imported Staff availability, and handle schedule-impact alerts without overriding Staff-owned availability.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Room filters and table",
    "Imported availability view",
    "Grid and chronological alternative",
    "Impact alerts",
    "Allowed actions and no-override notice"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Room filter, stacked room records/editor, read-only imported availability list, impact alerts, then allowed actions; no Admin override.",
    "375": "At 375 CSS px, Same order with 44 CSS px room and alert controls and complete no-override text before any resolve action.",
    "768": "At 768 CSS px, Compact navigation; room table/editor first and read-only availability/list alternative second; alerts span both columns.",
    "1024": "At 1024 CSS px, Persistent navigation with room management left and imported availability/alerts right; dialog focus returns to its trigger.",
    "1280": "At 1280 CSS px, Bounded room, availability, and alert regions; the chronological alternative precedes any grid-only enhancement.",
    "1920": "At 1920 CSS px, Centered workspace; extra width never separates an alert from revalidate/resolve or weakens the no-override notice."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "SearchFilter",
    "DataTable",
    "Pagination",
    "FormField",
    "ValidationSummary",
    "ScheduleCalendar",
    "ScheduleList",
    "StatusBadge",
    "ConfirmationDialog",
    "Button",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/admin/rooms",
    "POST /api/admin/rooms",
    "PUT /api/admin/rooms/{roomId}",
    "GET /api/admin/staff-availability",
    "GET /api/admin/schedule-impact-alerts",
    "POST /api/admin/schedule-impact-alerts/{alertId}/revalidate",
    "POST /api/admin/schedule-impact-alerts/{alertId}/resolve"
  ],
  "actions": [
    "Create or edit room",
    "Revalidate impact alert",
    "Resolve impact alert",
    "Open offerings"
  ],
  "navigationTransitions": [
    "Actions stay on ADM-07",
    "Offerings -> ADM-06",
    "Dashboard -> ADM-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-07-loading-v1",
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
        "ADM-07-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "ADM-07-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "Empty-state heading",
        "Plain-language reason",
        "Next action",
        "Empty is not a successful command result"
      ],
      "expectedFocusTarget": "Empty-state heading",
      "liveRegion": "none",
      "nextActions": [
        "Create a room or clear filters"
      ],
      "testIds": [
        "ADM-07-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-07-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Room versions, read-only imported Staff availability, and schedule-impact alerts",
        "No-override notice plus current revalidation or resolution status",
        "Success only when serverAccepted is true",
        "Minimum journeys: imported; empty; unavailable; overlap; stale; keyboard text entry"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "ADM-07-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "ADM-07-validation-error-v1",
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
        "Edit the room or meeting"
      ],
      "testIds": [
        "ADM-07-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-07-service-error-v1",
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
        "Retry"
      ],
      "testIds": [
        "ADM-07-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-07-unauthorized-v1",
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
        "ADM-07-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-07-session-expired-v1",
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
        "ADM-07-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-07-stale-v1",
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
        "Refresh and review"
      ],
      "testIds": [
        "ADM-07-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-07-offline-v1",
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
        "ADM-07-COMP-STATE-OFFLINE"
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
    "Resources heading",
    "Room search and result table",
    "Create or edit room fields",
    "Validation summary after failure",
    "Read-only imported availability list before the schedule grid",
    "Impact alerts in chronological order",
    "Revalidate or resolve controls and confirmation",
    "Return focus to the invoking room or alert control",
    "Offerings and support links"
  ],
  "testIds": [
    "ADM-07-CONTRACT-T218",
    "ADM-07-COMP-T219",
    "ADM-07-E2E-PRIMARY",
    "ADM-07-E2E-FAILURE",
    "ADM-07-A11Y-T220",
    "ADM-07-VIS-T221"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1-draft",
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
| 320 | Room filter, stacked room records/editor, read-only imported availability list, impact alerts, then allowed actions; no Admin override. |
| 375 | Same order with 44 CSS px room and alert controls and complete no-override text before any resolve action. |
| 768 | Compact navigation; room table/editor first and read-only availability/list alternative second; alerts span both columns. |
| 1024 | Persistent navigation with room management left and imported availability/alerts right; dialog focus returns to its trigger. |
| 1280 | Bounded room, availability, and alert regions; the chronological alternative precedes any grid-only enhancement. |
| 1920 | Centered workspace; extra width never separates an alert from revalidate/resolve or weakens the no-override notice. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-07-imported-v1` (imported) | success | imported Staff-owned availability, provenance, version, and read-only status | an Admin edit or override control | Inspect schedule impact | `ADM-07-COMP-STATE-SUCCESS` | `ADM-07-E2E-PRIMARY` |
| `ADM-07-empty-v1` (empty) | empty | empty room or availability heading and safe next action | an empty table without explanation | Create a room or clear filters | `ADM-07-COMP-STATE-EMPTY` | `ADM-07-E2E-PRIMARY` |
| `ADM-07-unavailable-v1` (unavailable) | service-error | unavailable data warning, reference ID, retry, and support path | cached availability presented as current | Retry | `ADM-07-COMP-STATE-SERVICE-ERROR` | `ADM-07-E2E-FAILURE` |
| `ADM-07-overlap-v1` (overlap) | validation-error | room overlap intervals, affected groups, and linked correction action | saving the overlap | Edit the room or meeting | `ADM-07-COMP-STATE-VALIDATION-ERROR` | `ADM-07-E2E-FAILURE` |
| `ADM-07-stale-v1` (stale) | stale | STALE_VERSION, changed room/alert data, and refresh action | overwriting current data | Refresh and review | `ADM-07-COMP-STATE-STALE` | `ADM-07-E2E-FAILURE` |
| `ADM-07-keyboard-text-entry-v1` (keyboard text entry) | success | labelled text-range fields, validation, and equivalent non-grid workflow | requiring drag-and-drop or timetable-grid interaction | Save the room edit | `ADM-07-COMP-STATE-SUCCESS` | `ADM-07-E2E-PRIMARY` |


## Review notes

- The six responsive descriptions preserve one information and focus order,
  wrap all reasons, and provide semantic table alternatives where applicable.
- API entries are copied verbatim from `.specify/page-api-manifest.json`;
  server authorization and decisions remain with the named owner specifications.
- Every UI state has a deterministic fixture, content, focus, live-region,
  next-action, and planned test identifier.
- Ahmed ELbamby must approve this exact draft version before its PDR task can
  close. Approval does not promote the route beyond `design-only`.
