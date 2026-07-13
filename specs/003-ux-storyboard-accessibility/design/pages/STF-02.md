# STF-02 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-016<br>
**Implementation task:** SPEC-016/T069<br>
**Design task:** SPEC-003/T237

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STF-02",
  "routeTemplate": "/staff/timetable",
  "pageName": "StaffTimetablePage.razor",
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
  "purpose": "Present current and historical authorized assignments in equivalent calendar and chronological-list forms.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Current and history selector",
    "Schedule calendar",
    "Chronological schedule list",
    "Assignment and colleague details",
    "Restricted or stale state"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Current/history selector, chronological list before the compact calendar enhancement, assignment detail, then roster/availability links.",
    "375": "At 375 CSS px, Same order with complete room/day/time text and 44 CSS px selector and entry actions.",
    "768": "At 768 CSS px, Compact navigation; calendar and chronological list appear as peer regions with identical assignment content.",
    "1024": "At 1024 CSS px, Persistent navigation with calendar left and chronological list/detail right; unassigned errors replace protected content.",
    "1280": "At 1280 CSS px, Bounded schedule columns with current/history selector above both representations.",
    "1920": "At 1920 CSS px, Centered workspace; additional width never changes list/calendar equivalence or reveals unassigned history."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "ScheduleCalendar",
    "ScheduleList",
    "EntityCard",
    "FormField",
    "StatusBadge",
    "Alert",
    "StatePanel",
    "AppLink"
  ],
  "dataContracts": [
    "GET /api/staff/timetable"
  ],
  "actions": [
    "Select timetable period",
    "Open assigned roster",
    "Open availability",
    "Return to Staff dashboard"
  ],
  "navigationTransitions": [
    "Roster -> STF-03",
    "Availability -> STF-04",
    "Dashboard -> STF-01",
    "Unassigned direct route -> SYS-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STF-02-loading-v1",
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
        "STF-02-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STF-02-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "No assignments heading",
        "Current Staff user has no authorized timetable assignments",
        "Availability and Staff-home actions",
        "No broad assignment search or protected group data"
      ],
      "expectedFocusTarget": "Empty-state heading",
      "liveRegion": "none",
      "nextActions": [
        "Open availability"
      ],
      "testIds": [
        "STF-02-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STF-02-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Authorized current or historical assignments with subject, group, colleagues, room, day, and time",
        "Equivalent calendar and chronological-list content",
        "Assignment and scope data exactly as returned by the authorized timetable query",
        "Minimum journeys: current; history; no assignments; stale; equivalent views"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "STF-02-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "not-applicable",
      "fixture": "STF-02-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "State is not applicable; the reviewed reason is recorded."
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "STF-02-COMP-STATE-VALIDATION-ERROR"
      ],
      "reason": "validation-error has no matching collection, protected-session, or editable interaction on this route; failures use the applicable safe state instead."
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STF-02-service-error-v1",
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
        "STF-02-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STF-02-unauthorized-v1",
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
        "STF-02-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STF-02-session-expired-v1",
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
        "STF-02-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STF-02-stale-v1",
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
        "Refresh timetable"
      ],
      "testIds": [
        "STF-02-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STF-02-offline-v1",
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
        "STF-02-COMP-STATE-OFFLINE"
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
    "Timetable heading",
    "Current or history selector",
    "Schedule calendar entries",
    "Chronological schedule list",
    "Assignment and colleague detail",
    "Assigned-roster, availability, dashboard, and support links"
  ],
  "testIds": [
    "STF-02-CONTRACT-T238",
    "STF-02-COMP-T239",
    "STF-02-E2E-PRIMARY",
    "STF-02-E2E-FAILURE",
    "STF-02-A11Y-T240",
    "STF-02-VIS-T241"
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
| 320 | Current/history selector, chronological list before the compact calendar enhancement, assignment detail, then roster/availability links. |
| 375 | Same order with complete room/day/time text and 44 CSS px selector and entry actions. |
| 768 | Compact navigation; calendar and chronological list appear as peer regions with identical assignment content. |
| 1024 | Persistent navigation with calendar left and chronological list/detail right; unassigned errors replace protected content. |
| 1280 | Bounded schedule columns with current/history selector above both representations. |
| 1920 | Centered workspace; additional width never changes list/calendar equivalence or reveals unassigned history. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STF-02-current-v1` (current) | success | current authorized subject, group, colleagues, room, day, and time in calendar and list | mismatched representations | Open assigned roster | `STF-02-COMP-STATE-SUCCESS` | `STF-02-E2E-PRIMARY` |
| `STF-02-history-v1` (history) | success | historical period label and authorized assignments in equivalent views | current-state actions on history | Select another period | `STF-02-COMP-STATE-SUCCESS` | `STF-02-E2E-PRIMARY` |
| `STF-02-no-assignments-v1` (no assignments) | empty | no assignments heading plus availability and Staff-home actions | assignment, room, colleague, roster data, or broad search access | Open availability | `STF-02-COMP-STATE-EMPTY` | `STF-02-E2E-PRIMARY` |
| `STF-02-stale-v1` (stale) | stale | stale assignment reason and refresh action while focus remains stable | opening removed assignment data | Refresh timetable | `STF-02-COMP-STATE-STALE` | `STF-02-E2E-FAILURE` |
| `STF-02-equivalent-views-v1` (equivalent views) | success | identical group, staff, room, day, and time values in calendar and list | calendar-only information | Switch representation | `STF-02-COMP-STATE-SUCCESS` | `STF-02-E2E-PRIMARY` |


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
