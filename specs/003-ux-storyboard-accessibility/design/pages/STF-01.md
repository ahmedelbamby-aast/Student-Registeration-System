# STF-01 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-016<br>
**Implementation task:** SPEC-016/T067<br>
**Design task:** SPEC-003/T232

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STF-01",
  "routeTemplate": "/staff",
  "pageName": "StaffDashboardPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-016",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007",
    "SPEC-008",
    "SPEC-016"
  ],
  "actors": [
    "Lecturer",
    "Teaching Assistant"
  ],
  "purpose": "Show one active server-authorized role context, permit an explicit server-validated context switch, and show only that context's assignments, deadlines, and warnings.",
  "informationHierarchy": [
    "Authenticated AppShell active-role context and authorized context switch",
    "Assignment summary",
    "Deadlines and warnings",
    "Assigned group actions",
    "Timetable, roster, and availability navigation"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, active-role context and authorized switch precede assignment cards, deadlines/warnings, then timetable, assigned-roster, and availability links in one column.",
    "375": "At 375 CSS px, the context selector and confirm action precede assignments; metadata wraps and actions follow each assignment.",
    "768": "At 768 CSS px, Compact navigation and a two-column assignment grid; warnings span both columns before navigation actions.",
    "1024": "At 1024 CSS px, persistent Staff navigation places the active-role selector and deadline summary beside assignment cards; the selector offers only server-returned roles.",
    "1280": "At 1280 CSS px, bounded assignment columns keep Lecturer and TA scopes separate and expressed in text, never as a privilege union.",
    "1920": "At 1920 CSS px, Centered workspace; additional width never reveals unassigned groups or changes authorization scope."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "EntityCard",
    "FormField",
    "StatusBadge",
    "Alert",
    "Button",
    "AppLink",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/context",
    "PUT /api/auth/session/context",
    "GET /api/staff/assignments",
    "GET /api/staff/timetable"
  ],
  "actions": [
    "Switch active role context",
    "Open timetable",
    "Open assigned roster",
    "Open availability"
  ],
  "navigationTransitions": [
    "Admin context -> ADM-01; Lecturer or Teaching Assistant context refreshes STF-01",
    "Timetable -> STF-02",
    "Roster -> STF-03",
    "Availability -> STF-04"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STF-01-loading-v1",
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
        "STF-01-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STF-01-empty-v1",
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
        "Open availability"
      ],
      "testIds": [
        "STF-01-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STF-01-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Server-authorized Staff context status: one active Lecturer or TA context, or role-selection-required choices before assignments load",
        "Multiple Staff claims appear only as authorized switch choices, render no assignments until selection, and are never unioned",
        "Only assigned lecture, tutorial, or laboratory actions and warnings",
        "Success only when serverAccepted is true",
        "Minimum journeys: Lecturer; TA; dual role; no assignment; stale assignment"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Select one authorized context",
        "Continue with an authorized route action"
      ],
      "testIds": [
        "STF-01-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STF-01-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Submitted context-switch validation summary",
        "Only still-authorized roles returned by the server are offered after rejection",
        "No privilege union, client-added claim, or retained rejected context",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary",
      "liveRegion": "assertive",
      "nextActions": [
        "Select one authorized context"
      ],
      "testIds": [
        "STF-01-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STF-01-service-error-v1",
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
        "STF-01-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STF-01-unauthorized-v1",
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
        "STF-01-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STF-01-session-expired-v1",
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
        "STF-01-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STF-01-stale-v1",
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
        "Refresh assignments"
      ],
      "testIds": [
        "STF-01-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STF-01-offline-v1",
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
        "STF-01-COMP-STATE-OFFLINE"
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
    "Staff dashboard heading",
    "Active-role selector and Switch context action",
    "Assignment cards in server order",
    "Deadline and warning alerts",
    "Timetable, assigned-roster, and availability links",
    "Safe support link"
  ],
  "testIds": [
    "STF-01-CONTRACT-T233",
    "STF-01-COMP-T234",
    "STF-01-E2E-PRIMARY",
    "STF-01-E2E-FAILURE",
    "STF-01-A11Y-T235",
    "STF-01-VIS-T236"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-007": "not-pinned",
    "SPEC-008": "not-pinned",
    "SPEC-016": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Active-role context and authorized switch, assignment cards, deadlines/warnings, then timetable, assigned-roster, and availability links in one column. |
| 375 | The context selector and confirm action precede assignments; metadata wraps and actions follow each authorized assignment. |
| 768 | Compact navigation and a two-column assignment grid; warnings span both columns before navigation actions. |
| 1024 | Persistent Staff navigation with the active-role selector and deadline summary beside assignment cards; only server-returned roles appear. |
| 1280 | Bounded assignment columns keep Lecturer and TA scopes separate in text, never as a privilege union. |
| 1920 | Centered workspace; additional width never reveals unassigned groups or changes authorization scope. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STF-01-lecturer-v1` (Lecturer) | success | Lecturer context and only assigned lecture groups, colleagues, deadlines, and actions | TA-only or unassigned groups | Open timetable or assigned roster | `STF-01-COMP-STATE-SUCCESS` | `STF-01-E2E-PRIMARY` |
| `STF-01-ta-v1` (TA) | success | TA context and only assigned tutorial/laboratory groups, colleagues, deadlines, and actions | Lecturer-only or unassigned groups | Open timetable or assigned roster | `STF-01-COMP-STATE-SUCCESS` | `STF-01-E2E-PRIMARY` |
| `STF-01-dual-role-v1` (dual role) | success | role-selection-required status in an authorized session, server-returned Lecturer and TA choices, and no assignments until one context is selected | error announcement before a submitted action, combined assignments, privilege union, or client-created role | Select one authorized context | `STF-01-COMP-STATE-SUCCESS` | `STF-01-E2E-PRIMARY` |
| `STF-01-no-assignment-v1` (no assignment) | empty | no assignment heading and availability/dashboard-safe actions | unrelated groups or Students | Open availability | `STF-01-COMP-STATE-EMPTY` | `STF-01-E2E-PRIMARY` |
| `STF-01-stale-assignment-v1` (stale assignment) | stale | assignment-changed reason and refresh action without stale actions | opening the removed assignment | Refresh assignments | `STF-01-COMP-STATE-STALE` | `STF-01-E2E-FAILURE` |


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
