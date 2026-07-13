# ADM-06 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-010<br>
**Implementation task:** SPEC-010/T099<br>
**Design task:** SPEC-003/T212

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-06",
  "routeTemplate": "/admin/offerings",
  "pageName": "OfferingAdministrationPage.razor",
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
  "purpose": "Configure offerings and groups, capacity, Lecturer and TA assignments, rooms, meetings, validation, and publication.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Offering list and status",
    "Offering editor",
    "Group and capacity details",
    "Staffing, resources, and meetings",
    "Validation findings",
    "Publish confirmation"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Offering list, selected editor, group/capacity cards, staff/room/meeting controls, findings, then publish confirmation.",
    "375": "At 375 CSS px, Same order with one group per section and complete resource reasons immediately before the affected controls.",
    "768": "At 768 CSS px, Compact navigation; offering table above a two-column editor. Group details and resource validation remain paired.",
    "1024": "At 1024 CSS px, Persistent navigation with offering list left and editor right; meeting alternatives use a chronological list before any grid.",
    "1280": "At 1280 CSS px, Bounded list/editor layout with validation panel adjacent to groups and publish after all blocking findings.",
    "1920": "At 1920 CSS px, Centered workspace; wide space never separates capacity, staff, room, or meeting errors from their fields."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "SearchFilter",
    "DataTable",
    "Pagination",
    "GroupCard",
    "CapacityIndicator",
    "FormField",
    "ValidationSummary",
    "StatusBadge",
    "ConfirmationDialog",
    "Button",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/admin/offerings",
    "GET /api/groups/{groupId}",
    "POST /api/admin/offerings",
    "PUT /api/admin/groups/{groupId}",
    "POST /api/admin/offerings/{offeringId}/validate",
    "POST /api/admin/offerings/{offeringId}/publish"
  ],
  "actions": [
    "Create offering",
    "Edit group",
    "Validate offering",
    "Publish offering",
    "Open resources"
  ],
  "navigationTransitions": [
    "Actions stay on ADM-06",
    "Resources -> ADM-07",
    "Catalogue -> ADM-05",
    "Dashboard -> ADM-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-06-loading-v1",
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
        "ADM-06-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "ADM-06-empty-v1",
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
        "Retry or change criteria"
      ],
      "testIds": [
        "ADM-06-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-06-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offering, group, capacity, Lecturer, TA, room, and meeting-slot details",
        "Validation and publication status with every blocking resource reason",
        "Success only when serverAccepted is true",
        "Minimum journeys: missing resource; overlap; capacity mismatch; stale edit; publish"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Return to offering list"
      ],
      "testIds": [
        "ADM-06-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "ADM-06-validation-error-v1",
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
        "ADM-06-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-06-service-error-v1",
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
        "ADM-06-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-06-unauthorized-v1",
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
        "ADM-06-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-06-session-expired-v1",
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
        "ADM-06-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-06-stale-v1",
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
        "ADM-06-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-06-offline-v1",
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
        "ADM-06-COMP-STATE-OFFLINE"
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
    "Offerings heading",
    "Offering search and list",
    "Offering editor fields",
    "Group and capacity controls",
    "Lecturer, TA, room, and meeting controls",
    "Validation summary and linked blocking fields",
    "Publish dialog and return to its trigger",
    "Resources, catalogue, and support links"
  ],
  "testIds": [
    "ADM-06-CONTRACT-T213",
    "ADM-06-COMP-T214",
    "ADM-06-E2E-PRIMARY",
    "ADM-06-E2E-FAILURE",
    "ADM-06-A11Y-T215",
    "ADM-06-VIS-T216"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-010": "not-pinned",
    "SPEC-017": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Offering list, selected editor, group/capacity cards, staff/room/meeting controls, findings, then publish confirmation. |
| 375 | Same order with one group per section and complete resource reasons immediately before the affected controls. |
| 768 | Compact navigation; offering table above a two-column editor. Group details and resource validation remain paired. |
| 1024 | Persistent navigation with offering list left and editor right; meeting alternatives use a chronological list before any grid. |
| 1280 | Bounded list/editor layout with validation panel adjacent to groups and publish after all blocking findings. |
| 1920 | Centered workspace; wide space never separates capacity, staff, room, or meeting errors from their fields. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-06-missing-resource-v1` (missing resource) | validation-error | missing Lecturer, TA, room, or meeting resource and linked field | publishing the offering | Assign every required resource | `ADM-06-COMP-STATE-VALIDATION-ERROR` | `ADM-06-E2E-FAILURE` |
| `ADM-06-overlap-v1` (overlap) | validation-error | all overlapping staff/room meeting intervals and affected groups | publishing an overlapping schedule | Edit meetings or resources | `ADM-06-COMP-STATE-VALIDATION-ERROR` | `ADM-06-E2E-FAILURE` |
| `ADM-06-capacity-mismatch-v1` (capacity mismatch) | validation-error | group capacity mismatch, configured values, and linked capacity field | silently changing capacity | Correct capacity | `ADM-06-COMP-STATE-VALIDATION-ERROR` | `ADM-06-E2E-FAILURE` |
| `ADM-06-stale-edit-v1` (stale edit) | stale | STALE_VERSION, changed offering fields, and refresh/review action | overwriting the newer offering | Refresh and review | `ADM-06-COMP-STATE-STALE` | `ADM-06-E2E-FAILURE` |
| `ADM-06-publish-v1` (publish) | success | published offering, groups, capacity, Lecturer, TAs, rooms, meetings, actor, and timestamp | success with any unresolved blocker | Return to offering list | `ADM-06-COMP-STATE-SUCCESS` | `ADM-06-E2E-PRIMARY` |


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
