# STU-01 Page Design Record

**Record version:** `draft/1.0`<br>
**Approval status:** Pending Ahmed ELbamby review<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-008<br>
**Implementation task:** SPEC-008/T078<br>
**Design task:** SPEC-003/T147

This complete design draft is governed by `page-design-record/1.1-draft`. It may be
reviewed as design evidence, but it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-01",
  "routeTemplate": "/student",
  "pageName": "StudentDashboardPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-008",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007",
    "SPEC-008",
    "SPEC-015"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Present authoritative academic context, blockers, registration state, and the correct start or resume action.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Academic summary",
    "Holds and profile blockers",
    "Term, window, and registration summary",
    "Primary registration action",
    "Timetable, history, and account links"
  ],
  "responsiveWireframes": {
    "320": "Collapsed Student navigation is followed by dashboard heading, server context, academic summary, holds/profile blockers, term/window status, one Start or Resume action, compact current timetable list, then registrations and account links. Blocker text wraps in full.",
    "375": "The 320 DOM order is retained with 44 CSS px Start/Resume and navigation targets. Academic metrics may pair only when labels and values fit; holds remain before the action they disable.",
    "768": "Compact navigation precedes a two-column summary: academic profile and term/window status. Blockers span both columns, Start/Resume follows them, and the chronological timetable list remains below.",
    "1024": "Persistent Student navigation, a main academic/registration column, and a complementary current-timetable summary are allowed. DOM order still puts every blocker before Start/Resume.",
    "1280": "The dashboard uses bounded summary cards with term/window and academic state adjacent. Start/Resume remains attached to its blocking explanation; timetable and account/history links follow.",
    "1920": "A centered maximum-width shell preserves the 1280 hierarchy. Surplus width increases gutters only; no hold, status, timetable item, or primary action moves outside logical reading order."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "EntityCard",
    "StatusBadge",
    "Alert",
    "ScheduleList",
    "Button",
    "AppLink",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/context",
    "GET /api/students/me/academic-context",
    "GET /api/student/registrations/current/timetable"
  ],
  "actions": [
    "Refresh GET /api/context, GET /api/students/me/academic-context, and GET /api/student/registrations/current/timetable",
    "Navigate to STU-02 to start registration",
    "Navigate to STU-04 to resume registration",
    "Open STU-07 current registrations",
    "Open STU-08 account"
  ],
  "navigationTransitions": [
    "Start -> STU-02",
    "Resume -> STU-04",
    "Registrations -> STU-07",
    "Account -> STU-08"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STU-01-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Student dashboard heading with stable landmarks",
        "Named progress status and disabled duplicate command",
        "No false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "STU-01-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STU-01-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "No current teaching or registration term",
        "Server-controlled no term explanation",
        "Registration action absent or disabled",
        "Safe support/reference path"
      ],
      "expectedFocusTarget": "Student dashboard heading",
      "liveRegion": "none",
      "nextActions": [
        "Refresh all dashboard GET resources",
        "Open safe support",
        "Refresh context or open support"
      ],
      "testIds": [
        "STU-01-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STU-01-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Authenticated server context and academic summary",
        "Term/window status and exactly one correct Start or Resume action",
        "Current timetable summary from the server",
        "Every hold or incomplete-profile blocker before the action",
        "Minimum journeys: open; upcoming; closed; no term; hold; incomplete profile",
        "Dashboard state is rendered only from the authoritative server-returned context and academic summary"
      ],
      "expectedFocusTarget": "Student dashboard heading on initial navigation; preserve the activated Start/Resume link during background refresh",
      "liveRegion": "polite",
      "nextActions": [
        "Navigate to STU-02 when Start is authorized",
        "Navigate to STU-04 when Resume is authorized",
        "Open STU-07 or STU-08"
      ],
      "testIds": [
        "STU-01-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STU-01-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "Validation summary for REGISTRATION_HOLD or incomplete profile",
        "Every stable blocker and safe explanation",
        "Start/Resume disabled with aria-describedby blocker text",
        "No browser-time or client-policy override",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Student dashboard heading",
      "liveRegion": "none",
      "nextActions": [
        "Open the server-supplied blocker/support action",
        "Refresh dashboard context"
      ],
      "testIds": [
        "STU-01-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STU-01-service-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Safe service-error heading",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown code preserved exactly",
        "Reference ID and retry/support action",
        "No stack trace, SQL text, credential, arbitrary payload, or success",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown-code safe fallback"
      ],
      "expectedFocusTarget": "Keep current focus for a background failure; move to the service-error heading only after a navigation failure",
      "liveRegion": "polite",
      "nextActions": [
        "Retry the failed API explicitly",
        "Open safe support path"
      ],
      "testIds": [
        "STU-01-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STU-01-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Access-denied heading",
        "UNAUTHORIZED or FORBIDDEN preserved exactly",
        "No Student protected data, identifier, cached content, or unauthorized action"
      ],
      "expectedFocusTarget": "Access-denied heading after navigation",
      "liveRegion": "assertive",
      "nextActions": [
        "Return to authorized home or sign in"
      ],
      "testIds": [
        "STU-01-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STU-01-session-expired-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Session-expired heading",
        "SESSION_EXPIRED preserved exactly",
        "Only safe plan/reference ID retained",
        "No protected cached content or automatic success"
      ],
      "expectedFocusTarget": "Session-expired heading, announced assertively once",
      "liveRegion": "assertive",
      "nextActions": [
        "Sign in again",
        "Refetch and revalidate before editing"
      ],
      "testIds": [
        "STU-01-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STU-01-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change heading",
        "WINDOW_CHANGED, WINDOW_CLOSED, or STALE_VERSION",
        "Authoritative refresh and review action",
        "Cached availability, plan, capacity, or success is not reused",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "Refresh all dashboard GET resources",
        "Review changed term/window/timetable",
        "Refresh dashboard or open support"
      ],
      "testIds": [
        "STU-01-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STU-01-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline status for Student dashboard heading",
        "Browser connectivity state and retry guidance",
        "No queued, cached, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry all dashboard GET resources when online"
      ],
      "testIds": [
        "STU-01-COMP-STATE-OFFLINE"
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
    "Student role navigation",
    "Student dashboard heading",
    "Server date, timezone, teaching term, registration term/window, session, and service context",
    "Academic summary",
    "Holds and incomplete-profile blockers",
    "Start registration or Resume registration action",
    "Current timetable chronological list",
    "Current registrations link",
    "Account link",
    "Safe support/reference link"
  ],
  "testIds": [
    "STU-01-CONTRACT-T148",
    "STU-01-COMP-T149",
    "STU-01-E2E-PRIMARY",
    "STU-01-E2E-FAILURE",
    "STU-01-A11Y-T150",
    "STU-01-VIS-T151"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1-draft",
    "SPEC-007": "not-pinned",
    "SPEC-008": "not-pinned",
    "SPEC-015": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "pending-Ahmed-review"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Collapsed Student navigation is followed by dashboard heading, server context, academic summary, holds/profile blockers, term/window status, one Start or Resume action, compact current timetable list, then registrations and account links. Blocker text wraps in full. |
| 375 | The 320 DOM order is retained with 44 CSS px Start/Resume and navigation targets. Academic metrics may pair only when labels and values fit; holds remain before the action they disable. |
| 768 | Compact navigation precedes a two-column summary: academic profile and term/window status. Blockers span both columns, Start/Resume follows them, and the chronological timetable list remains below. |
| 1024 | Persistent Student navigation, a main academic/registration column, and a complementary current-timetable summary are allowed. DOM order still puts every blocker before Start/Resume. |
| 1280 | The dashboard uses bounded summary cards with term/window and academic state adjacent. Start/Resume remains attached to its blocking explanation; timetable and account/history links follow. |
| 1920 | A centered maximum-width shell preserves the 1280 hierarchy. Surplus width increases gutters only; no hold, status, timetable item, or primary action moves outside logical reading order. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-008/T077. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STU-01-open-v1` (open) | success | server-open window, academic summary, no blocker, and correct Start or Resume registration action | browser clock deciding open state, both Start and Resume, or cached success | Open STU-02 to start or STU-04 to resume | `STU-01-COMP-STATE-SUCCESS` | `STU-01-E2E-PRIMARY` |
| `STU-01-upcoming-v1` (upcoming) | success | upcoming label, server opening date/time/timezone, and disabled registration action with reason | enabled registration, client countdown as authority, or hidden opening time | Review date or open support | `STU-01-COMP-STATE-SUCCESS` | `STU-01-E2E-PRIMARY` |
| `STU-01-closed-v1` (closed) | stale | closed status, WINDOW_CLOSED or server reason, closing timestamp, and disabled action | enabled Start/Resume, stale open badge, or success | Refresh dashboard or open support | `STU-01-COMP-STATE-STALE` | `STU-01-E2E-FAILURE` |
| `STU-01-no-term-v1` (no term) | empty | no term heading, server-controlled explanation, and support/reference path | invented current term, enabled registration, or browser-date fallback | Refresh context or open support | `STU-01-COMP-STATE-EMPTY` | `STU-01-E2E-FAILURE` |
| `STU-01-hold-v1` (hold) | validation-error | REGISTRATION_HOLD, every safe hold reason, linked blocker text, and disabled Start/Resume | dismiss/override control, hidden reason, or registration navigation | Use supplied blocker/support action | `STU-01-COMP-STATE-VALIDATION-ERROR` | `STU-01-E2E-FAILURE` |
| `STU-01-incomplete-profile-v1` (incomplete profile) | validation-error | incomplete profile reason, required safe next step, and disabled Start/Resume | client-completed profile claim, enabled registration, or protected correction action | Use the server-provided profile/support action | `STU-01-COMP-STATE-VALIDATION-ERROR` | `STU-01-E2E-FAILURE` |

## Review notes

- Six annotated layouts are specific to STU-01 and preserve DOM, keyboard,
  action, state, and information order without hiding reasons or boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; commands
  name their endpoint and navigation-only actions never imply a missing write.
- Background stale/offline/service updates preserve focus; submitted validation
  focuses its summary, and route-specific restoration is explicit in focusOrder.
- Ahmed ELbamby must approve this exact draft before its design task can close.
  Approval does not promote the route beyond `design-only`.
