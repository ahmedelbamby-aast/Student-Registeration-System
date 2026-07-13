# STF-04 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-016<br>
**Implementation task:** SPEC-016/T073<br>
**Design task:** SPEC-003/T247

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STF-04",
  "routeTemplate": "/staff/availability",
  "pageName": "StaffAvailabilityPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-016",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-016"
  ],
  "actors": [
    "Lecturer",
    "Teaching Assistant"
  ],
  "purpose": "Edit owned availability ranges before the deadline without silently moving published classes.",
  "informationHierarchy": [
    "Authenticated AppShell deadline status",
    "Text-range availability editor",
    "Validation summary",
    "Calendar and chronological alternative",
    "Published-impact warning",
    "Save or stale result"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Deadline/status, labelled text-range fields, add/edit/remove controls, validation, chronological alternative, warning, then save.",
    "375": "At 375 CSS px, Same order with each range action adjacent to its range and all overlap/deadline reasons wrapping in full.",
    "768": "At 768 CSS px, Compact navigation; text editor left and equivalent calendar/list right; validation spans both before Save.",
    "1024": "At 1024 CSS px, Persistent navigation with editor and representations side by side; save confirmation restores its trigger.",
    "1280": "At 1280 CSS px, Bounded editor/schedule columns with published-impact warning immediately before Save.",
    "1920": "At 1920 CSS px, Centered workspace; extra width never makes the grid mandatory or separates deadline/warning from affected ranges."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "FormField",
    "ValidationSummary",
    "ScheduleCalendar",
    "ScheduleList",
    "DataTable",
    "ConfirmationDialog",
    "Button",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/staff/availability",
    "PUT /api/staff/availability"
  ],
  "actions": [
    "Add availability range",
    "Edit availability range",
    "Remove availability range",
    "Save availability",
    "Open timetable"
  ],
  "navigationTransitions": [
    "Actions stay on STF-04",
    "Timetable -> STF-02",
    "Dashboard -> STF-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STF-04-loading-v1",
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
        "STF-04-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STF-04-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "Empty-state heading",
        "Plain-language reason",
        "Add availability range action",
        "Empty is not a successful command result"
      ],
      "expectedFocusTarget": "Empty-state heading",
      "liveRegion": "none",
      "nextActions": [
        "Add availability range"
      ],
      "testIds": [
        "STF-04-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STF-04-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Owned availability ranges, configured deadline, save status, and published-group impact warnings",
        "Equivalent text ranges and timetable representation",
        "Success only when serverAccepted is true",
        "Minimum journeys: draft; saved; overlap; deadline; published warning; stale edit"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "STF-04-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STF-04-validation-error-v1",
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
        "Edit the ranges"
      ],
      "testIds": [
        "STF-04-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STF-04-service-error-v1",
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
        "STF-04-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STF-04-unauthorized-v1",
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
        "STF-04-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STF-04-session-expired-v1",
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
        "STF-04-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STF-04-stale-v1",
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
        "Refresh authoritative state",
        "Review changes"
      ],
      "testIds": [
        "STF-04-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STF-04-offline-v1",
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
        "STF-04-COMP-STATE-OFFLINE"
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
    "Staff navigation",
    "Availability heading and deadline status",
    "Text-range editor fields",
    "Add, edit, and remove controls",
    "Validation summary after failure",
    "Calendar and chronological alternative",
    "Published-impact warning",
    "Save confirmation and return to its trigger",
    "Timetable, dashboard, and support links"
  ],
  "testIds": [
    "STF-04-CONTRACT-T248",
    "STF-04-COMP-T249",
    "STF-04-E2E-PRIMARY",
    "STF-04-E2E-FAILURE",
    "STF-04-A11Y-T250",
    "STF-04-VIS-T251"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-016": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Deadline/status, labelled text-range fields, add/edit/remove controls, validation, chronological alternative, warning, then save. |
| 375 | Same order with each range action adjacent to its range and all overlap/deadline reasons wrapping in full. |
| 768 | Compact navigation; text editor left and equivalent calendar/list right; validation spans both before Save. |
| 1024 | Persistent navigation with editor and representations side by side; save confirmation restores its trigger. |
| 1280 | Bounded editor/schedule columns with published-impact warning immediately before Save. |
| 1920 | Centered workspace; extra width never makes the grid mandatory or separates deadline/warning from affected ranges. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STF-04-draft-v1` (draft) | success | unsaved draft ranges, deadline, dirty status, and no server success claim | queued or saved status | Save availability | `STF-04-COMP-STATE-SUCCESS` | `STF-04-E2E-PRIMARY` |
| `STF-04-saved-v1` (saved) | success | saved version, server timestamp, ranges, and confirmation | success before server acceptance | Return to Staff dashboard | `STF-04-COMP-STATE-SUCCESS` | `STF-04-E2E-PRIMARY` |
| `STF-04-overlap-v1` (overlap) | validation-error | all overlapping ranges and linked text fields | saving overlapping ranges | Edit the ranges | `STF-04-COMP-STATE-VALIDATION-ERROR` | `STF-04-E2E-FAILURE` |
| `STF-04-deadline-v1` (deadline) | stale | AVAILABILITY_DEADLINE_PASSED, configured deadline, and read-only recovery | saving after the deadline | Refresh and review | `STF-04-COMP-STATE-STALE` | `STF-04-E2E-FAILURE` |
| `STF-04-published-warning-v1` (published warning) | success | published-group impact warning and explicit no-silent-move statement | silently moving a class | Review timetable impact | `STF-04-COMP-STATE-SUCCESS` | `STF-04-E2E-PRIMARY` |
| `STF-04-stale-edit-v1` (stale edit) | stale | STALE_VERSION, changed ranges, and refresh/review action | overwriting current availability | Refresh and reapply changes | `STF-04-COMP-STATE-STALE` | `STF-04-E2E-FAILURE` |


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
