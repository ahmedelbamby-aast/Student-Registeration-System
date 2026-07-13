# STU-07 Page Design Record

**Record version:** `draft/1.0`<br>
**Approval status:** Pending Ahmed ELbamby review<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-015<br>
**Implementation task:** SPEC-015/T053<br>
**Design task:** SPEC-003/T177

This complete design draft is governed by `page-design-record/1.1-draft`. It may be
reviewed as design evidence, but it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-07",
  "routeTemplate": "/student/registrations",
  "pageName": "RegistrationHistoryPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-015",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-015"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Show current timetable and historical registration records with equivalent printable representations.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Current and history selector",
    "Term and status summary",
    "Calendar and chronological list",
    "Registration table",
    "Pagination, print, and empty recovery"
  ],
  "responsiveWireframes": {
    "320": "Collapsed navigation and heading precede current/history selector, term/status summary, chronological current timetable, optional equivalent calendar, stacked registration rows, pagination, Print, and dashboard link. No table-only information is lost.",
    "375": "The 320 order is retained with 44 CSS px selector, view, paging, Print, and link targets. Registration rows expose term, status, reference, and Open action without horizontal page scroll.",
    "768": "Compact navigation precedes selector/status and a schedule region; semantic table may replace stacked rows only when all columns fit. The chronological list remains available and pagination follows results.",
    "1024": "Persistent navigation, a main calendar/list/history table, and bounded term summary are allowed. Table uses caption/headers; its labelled scroll region has a non-scrolling stacked alternative.",
    "1280": "The full priority table and pagination fit within a bounded main region; Print stays attached to the selected registration. Calendar and list contain identical staff, room, day, and time data.",
    "1920": "A centered maximum-width shell preserves the 1280 data and focus order. Surplus width adds whitespace only; no column, archived status, print control, or empty recovery moves off-screen."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "FormField",
    "ScheduleCalendar",
    "ScheduleList",
    "DataTable",
    "Pagination",
    "StatusBadge",
    "Button",
    "StatePanel",
    "AppLink"
  ],
  "dataContracts": [
    "GET /api/student/registrations",
    "GET /api/student/registrations/{submissionId}",
    "GET /api/student/registrations/current/timetable"
  ],
  "actions": [
    "Select period and page through GET /api/student/registrations",
    "Open owned submission through GET /api/student/registrations/{submissionId}",
    "Refresh current timetable through GET /api/student/registrations/current/timetable",
    "Switch equivalent Calendar/List view",
    "Print the current browser-rendered registration view",
    "Open STU-01 dashboard"
  ],
  "navigationTransitions": [
    "Selection stays on STU-07",
    "Submission -> STU-06",
    "Dashboard -> STU-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STU-07-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Registrations heading with stable landmarks",
        "Named pending status and disabled duplicate command",
        "No duplicate request, partial outcome, or false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "STU-07-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STU-07-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: user action",
        "No registrations for selected period",
        "Selected criteria and empty explanation",
        "Dashboard or clear/change selection action",
        "No invented current registration"
      ],
      "expectedFocusTarget": "Preserve the active period selector while the empty registrations heading is announced",
      "liveRegion": "polite",
      "nextActions": [
        "Change the period selector",
        "Return to STU-01",
        "Change period or return dashboard"
      ],
      "testIds": [
        "STU-07-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STU-07-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Current or history selection and result count",
        "Owned registration status, reference, term, and submission link",
        "Equivalent calendar and chronological timetable for current data",
        "Semantic table/stacked alternative and pagination",
        "Minimum journeys: empty; current; history; archived; service error",
        "Registration list and timetable are rendered only from the authoritative server-returned records"
      ],
      "expectedFocusTarget": "Registrations heading on load; view switching restores the matching meeting; period/page changes focus the updated result heading or table caption",
      "liveRegion": "polite",
      "nextActions": [
        "Open an owned submission",
        "Change period/page",
        "Print the rendered view"
      ],
      "testIds": [
        "STU-07-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STU-07-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary linked to invalid period/page selection",
        "Stable server reason and unchanged safe criteria",
        "No arbitrary query sent as success",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field, group, or blocker",
      "liveRegion": "assertive",
      "nextActions": [
        "Correct selector",
        "Rerun GET /api/student/registrations"
      ],
      "testIds": [
        "STU-07-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STU-07-service-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Safe service-error heading",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown code preserved exactly",
        "Reference ID and retry/support action",
        "No stack trace, SQL text, credential, arbitrary payload, partial outcome, or success",
        "SERVICE_UNAVAILABLE, MAINTENANCE, or unknown-code safe fallback"
      ],
      "expectedFocusTarget": "Keep current focus for a background failure; move to the service-error heading only after a navigation failure",
      "liveRegion": "polite",
      "nextActions": [
        "Retry the failed API explicitly",
        "Open safe support path",
        "Retry selected GET resources"
      ],
      "testIds": [
        "STU-07-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STU-07-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Access-denied heading",
        "UNAUTHORIZED or FORBIDDEN preserved exactly",
        "No protected identifier, receipt, account, cached content, or unauthorized action"
      ],
      "expectedFocusTarget": "Access-denied heading after navigation",
      "liveRegion": "assertive",
      "nextActions": [
        "Return to authorized home or sign in"
      ],
      "testIds": [
        "STU-07-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STU-07-session-expired-v1",
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
        "Refetch and revalidate before acting"
      ],
      "testIds": [
        "STU-07-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STU-07-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change or uncertain-result heading",
        "STALE_VERSION or changed current timetable version",
        "Authoritative refresh/review action",
        "Cached authorization, plan, result, or success is not reused",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "Refresh registrations and current timetable",
        "Review changed status"
      ],
      "testIds": [
        "STU-07-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STU-07-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline status for Registrations heading",
        "Browser connectivity state and safe retry guidance",
        "No queued command, cached result, partial outcome, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry the selected GET resources when online"
      ],
      "testIds": [
        "STU-07-COMP-STATE-OFFLINE"
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
    "Registrations heading",
    "Current/history period selector",
    "Term and registration status summary",
    "Calendar/List view controls",
    "Chronological schedule items and corresponding calendar entries",
    "Registration table caption and owned submission links",
    "Pagination controls",
    "Print registration button",
    "Dashboard link",
    "Safe support/reference link"
  ],
  "testIds": [
    "STU-07-CONTRACT-T178",
    "STU-07-COMP-T179",
    "STU-07-E2E-PRIMARY",
    "STU-07-E2E-FAILURE",
    "STU-07-A11Y-T180",
    "STU-07-VIS-T181"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1-draft",
    "SPEC-015": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "pending-Ahmed-review"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Collapsed navigation and heading precede current/history selector, term/status summary, chronological current timetable, optional equivalent calendar, stacked registration rows, pagination, Print, and dashboard link. No table-only information is lost. |
| 375 | The 320 order is retained with 44 CSS px selector, view, paging, Print, and link targets. Registration rows expose term, status, reference, and Open action without horizontal page scroll. |
| 768 | Compact navigation precedes selector/status and a schedule region; semantic table may replace stacked rows only when all columns fit. The chronological list remains available and pagination follows results. |
| 1024 | Persistent navigation, a main calendar/list/history table, and bounded term summary are allowed. Table uses caption/headers; its labelled scroll region has a non-scrolling stacked alternative. |
| 1280 | The full priority table and pagination fit within a bounded main region; Print stays attached to the selected registration. Calendar and list contain identical staff, room, day, and time data. |
| 1920 | A centered maximum-width shell preserves the 1280 data and focus order. Surplus width adds whitespace only; no column, archived status, print control, or empty recovery moves off-screen. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-015/T052. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STU-07-empty-v1` (empty) | empty | empty heading, selected period, no-results explanation, and dashboard/change-period action | blank table, invented registration, or success count | Change period or return dashboard | `STU-07-COMP-STATE-EMPTY` | `STU-07-E2E-FAILURE` |
| `STU-07-current-v1` (current) | success | current status/reference, complete meeting collection, equivalent calendar/list, and Print | calendar-only data, missing staff/room/time, or archived item shown current | Open submission or print | `STU-07-COMP-STATE-SUCCESS` | `STU-07-E2E-PRIMARY` |
| `STU-07-history-v1` (history) | success | history rows with term/status/reference, paging, and owned Open links | unowned record, missing caption/headers, or client-altered status | Change page or open owned submission | `STU-07-COMP-STATE-SUCCESS` | `STU-07-E2E-PRIMARY` |
| `STU-07-archived-v1` (archived) | success | archived label, immutable receipt/decision snapshot, and print-safe content | editable archived record, active/current claim, or hidden archive status | Open or print archive | `STU-07-COMP-STATE-SUCCESS` | `STU-07-E2E-PRIMARY` |
| `STU-07-service-error-v1` (service error) | service-error | safe service error, reference ID, Retry, and support | stack trace, SQL, raw payload, empty-as-success, or cached current data | Retry selected GET resources | `STU-07-COMP-STATE-SERVICE-ERROR` | `STU-07-E2E-FAILURE` |

## Review notes

- Six annotated layouts are specific to STU-07 and preserve DOM, keyboard,
  action, state, and information order without hiding reasons or boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; commands
  name their endpoint and navigation/browser-only actions imply no missing write.
- Background stale/offline/service updates preserve focus; submitted validation
  and modal/navigation focus/restoration follow focusOrder and the reason map.
- Ahmed ELbamby must approve this exact draft before its design task can close.
  Approval does not promote the route beyond `design-only`.
