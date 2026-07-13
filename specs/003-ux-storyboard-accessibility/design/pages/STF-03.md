# STF-03 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-016<br>
**Implementation task:** SPEC-016/T071<br>
**Design task:** SPEC-003/T242

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STF-03",
  "routeTemplate": "/staff/groups/{groupId}/roster",
  "pageName": "StaffRosterPage.razor",
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
  "purpose": "Show the minimum authorized roster and counts for a Staff member assigned to the group.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Group and assignment summary",
    "Roster counts",
    "Semantic roster table",
    "Pagination",
    "Assignment-removed recovery"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Group/assignment summary, counts, stacked roster rows, pagination, then timetable/dashboard links.",
    "375": "At 375 CSS px, Same order with sortable labels and 44 CSS px paging controls; no unrelated fields are added.",
    "768": "At 768 CSS px, Compact navigation; semantic roster table above paging and assignment-removal recovery.",
    "1024": "At 1024 CSS px, Persistent navigation with group summary beside the paged roster; table remains the primary semantic representation.",
    "1280": "At 1280 CSS px, Bounded roster width with sticky semantic headers only when keyboard access and reading order remain intact.",
    "1920": "At 1920 CSS px, Centered workspace; extra width increases gutters rather than adding unrelated Student data."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "EntityCard",
    "DataTable",
    "Pagination",
    "StatusBadge",
    "Alert",
    "StatePanel",
    "AppLink"
  ],
  "dataContracts": [
    "GET /api/staff/groups/{groupId}/roster"
  ],
  "actions": [
    "Sort roster",
    "Change roster page",
    "Open timetable",
    "Return to Staff dashboard"
  ],
  "navigationTransitions": [
    "Timetable -> STF-02",
    "Dashboard -> STF-01",
    "Unassigned or removed -> SYS-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STF-03-loading-v1",
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
        "STF-03-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STF-03-empty-v1",
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
        "Return to timetable"
      ],
      "testIds": [
        "STF-03-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STF-03-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Assigned group identity, authorized counts, and minimum roster fields",
        "Paging and sort state without unrelated Students or groups",
        "Roster rows and paging metadata exactly as returned by the authorized roster query",
        "Minimum journeys: assigned; unassigned 403; empty; paged; assignment removed"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "STF-03-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "not-applicable",
      "fixture": "STF-03-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "State is not applicable; the read-only roster has no submitted validation interaction."
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "STF-03-COMP-STATE-VALIDATION-ERROR"
      ],
      "reason": "The roster route is read-only and has no submitted validation interaction; unassigned access, removed assignments, and service failures map to unauthorized, stale, or service-error states."
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STF-03-service-error-v1",
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
        "STF-03-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STF-03-unauthorized-v1",
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
        "Return to Staff home"
      ],
      "testIds": [
        "STF-03-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STF-03-session-expired-v1",
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
        "STF-03-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STF-03-stale-v1",
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
        "Return to Staff home"
      ],
      "testIds": [
        "STF-03-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STF-03-offline-v1",
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
        "STF-03-COMP-STATE-OFFLINE"
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
    "Roster heading",
    "Group and assignment summary",
    "Roster table caption and headers",
    "Sortable header controls",
    "Roster rows",
    "Pagination",
    "Timetable, dashboard, and support links"
  ],
  "testIds": [
    "STF-03-CONTRACT-T243",
    "STF-03-COMP-T244",
    "STF-03-E2E-PRIMARY",
    "STF-03-E2E-FAILURE",
    "STF-03-A11Y-T245",
    "STF-03-VIS-T246"
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
| 320 | Group/assignment summary, counts, stacked roster rows, pagination, then timetable/dashboard links. |
| 375 | Same order with sortable labels and 44 CSS px paging controls; no unrelated fields are added. |
| 768 | Compact navigation; semantic roster table above paging and assignment-removal recovery. |
| 1024 | Persistent navigation with group summary beside the paged roster; table remains the primary semantic representation. |
| 1280 | Bounded roster width with sticky semantic headers only when keyboard access and reading order remain intact. |
| 1920 | Centered workspace; extra width increases gutters rather than adding unrelated Student data. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STF-03-assigned-v1` (assigned) | success | assigned group, authorized counts, minimum roster fields, and paging state | unrelated Students, groups, or sensitive profile fields | Sort or change page | `STF-03-COMP-STATE-SUCCESS` | `STF-03-E2E-PRIMARY` |
| `STF-03-unassigned-403-v1` (unassigned 403) | unauthorized | 403 heading and safe Staff-home action | group identity, counts, or roster rows | Return to Staff home | `STF-03-COMP-STATE-UNAUTHORIZED` | `STF-03-E2E-FAILURE` |
| `STF-03-empty-v1` (empty) | empty | empty roster heading, assigned group context, and safe return action | blank table without explanation | Return to timetable | `STF-03-COMP-STATE-EMPTY` | `STF-03-E2E-PRIMARY` |
| `STF-03-paged-v1` (paged) | success | stable sort, page number, total count, and nonduplicated roster rows | duplicate or skipped Students | Change page | `STF-03-COMP-STATE-SUCCESS` | `STF-03-E2E-PRIMARY` |
| `STF-03-assignment-removed-v1` (assignment removed) | stale | assignment-removed reason with no roster content and refresh/home action | stale roster or action | Return to Staff home | `STF-03-COMP-STATE-STALE` | `STF-03-E2E-FAILURE` |


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
