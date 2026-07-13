# ADM-04 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-008<br>
**Implementation task:** SPEC-008/T082<br>
**Design task:** SPEC-003/T202

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-04",
  "routeTemplate": "/admin/students",
  "pageName": "StudentAdministrationPage.razor",
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
  "purpose": "Inspect sourced Student academic records and submit reasoned concurrency-safe corrections.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Student search and list",
    "Academic summary and provenance",
    "Transcript and holds",
    "Correction dialog",
    "Validation or stale result"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Student search/results, sourced academic summary, transcript/holds, then a full-screen correction dialog with reason before confirm.",
    "375": "At 375 CSS px, Same order with compact provenance labels and correction trigger beside the exact field; validation stays above dialog fields.",
    "768": "At 768 CSS px, Compact navigation; results table above academic detail. Transcript and holds use semantic tables with stacked alternatives.",
    "1024": "At 1024 CSS px, Persistent navigation with student list left and academic/provenance detail right; correction dialog restores the field trigger.",
    "1280": "At 1280 CSS px, Bounded list/detail columns; transcript, holds, and correction history remain in DOM order before actions.",
    "1920": "At 1920 CSS px, Centered workspace with readable provenance and dialog widths; extra width never exposes unrelated student data."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "SearchFilter",
    "EntityCard",
    "DataTable",
    "Pagination",
    "FormField",
    "ValidationSummary",
    "ConfirmationDialog",
    "Button",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/admin/students",
    "GET /api/admin/students/{studentId}/academic-context",
    "PATCH /api/admin/students/{studentId}/academic-profile"
  ],
  "actions": [
    "Search Students",
    "Clear Student search",
    "Open Student detail",
    "Submit academic correction",
    "Open registrations"
  ],
  "navigationTransitions": [
    "Actions stay on ADM-04",
    "User account -> ADM-03",
    "Registrations -> ADM-08",
    "Dashboard -> ADM-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-04-loading-v1",
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
        "ADM-04-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "ADM-04-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: user action",
        "No matching Students heading",
        "Current search criteria and plain-language no-results reason",
        "Clear or change search criteria action",
        "Empty is not a successful command result"
      ],
      "expectedFocusTarget": "Preserve the active Student search control while the no-results heading is announced",
      "liveRegion": "polite",
      "nextActions": [
        "Clear or change search criteria"
      ],
      "testIds": [
        "ADM-04-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-04-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Sourced academic profile, GPA, transcript, holds, and provenance",
        "Current row version and authorized correction history",
        "Success only when serverAccepted is true",
        "Minimum journeys: read; invalid correction; stale; reason required; restricted"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Open an authorized correction"
      ],
      "testIds": [
        "ADM-04-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "ADM-04-validation-error-v1",
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
        "ADM-04-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-04-service-error-v1",
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
        "ADM-04-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-04-unauthorized-v1",
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
        "ADM-04-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-04-session-expired-v1",
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
        "ADM-04-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-04-stale-v1",
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
        "Refresh Student context"
      ],
      "testIds": [
        "ADM-04-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-04-offline-v1",
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
        "ADM-04-COMP-STATE-OFFLINE"
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
    "Students heading",
    "Student search and result list",
    "Academic summary and provenance",
    "Transcript and holds table",
    "Open-correction control",
    "Correction dialog reason and value fields",
    "Validation summary, cancel, then confirm",
    "Return focus to the correction trigger",
    "Registration and support links"
  ],
  "testIds": [
    "ADM-04-CONTRACT-T203",
    "ADM-04-COMP-T204",
    "ADM-04-E2E-PRIMARY",
    "ADM-04-E2E-FAILURE",
    "ADM-04-A11Y-T205",
    "ADM-04-VIS-T206"
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
| 320 | Student search/results, sourced academic summary, transcript/holds, then a full-screen correction dialog with reason before confirm. |
| 375 | Same order with compact provenance labels and correction trigger beside the exact field; validation stays above dialog fields. |
| 768 | Compact navigation; results table above academic detail. Transcript and holds use semantic tables with stacked alternatives. |
| 1024 | Persistent navigation with student list left and academic/provenance detail right; correction dialog restores the field trigger. |
| 1280 | Bounded list/detail columns; transcript, holds, and correction history remain in DOM order before actions. |
| 1920 | Centered workspace with readable provenance and dialog widths; extra width never exposes unrelated student data. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-04-read-v1` (read) | success | GPA, transcript, holds, provenance, source timestamp, and row version for the selected Student | editable values without an authorized correction command | Open an authorized correction | `ADM-04-COMP-STATE-SUCCESS` | `ADM-04-E2E-PRIMARY` |
| `ADM-04-no-results-v1` (no results) | empty | current Student search criteria, no-results heading, and clear/change action | unrelated Student data or implied authorization failure | Clear or change search criteria | `ADM-04-COMP-STATE-EMPTY` | `ADM-04-E2E-PRIMARY` |
| `ADM-04-invalid-correction-v1` (invalid correction) | validation-error | invalid value reason and linked correction field | committing the correction | Correct the value | `ADM-04-COMP-STATE-VALIDATION-ERROR` | `ADM-04-E2E-FAILURE` |
| `ADM-04-stale-v1` (stale) | stale | STALE_VERSION, changed academic data, and refresh/review action | overwriting current academic data | Refresh Student context | `ADM-04-COMP-STATE-STALE` | `ADM-04-E2E-FAILURE` |
| `ADM-04-reason-required-v1` (reason required) | validation-error | reason-required message linked to the reason field | submitting a blank reason | Enter a reason and retry | `ADM-04-COMP-STATE-VALIDATION-ERROR` | `ADM-04-E2E-FAILURE` |
| `ADM-04-restricted-v1` (restricted) | unauthorized | safe restricted heading and authorized-home action | academic data or Student identifier | Return to Admin home | `ADM-04-COMP-STATE-UNAUTHORIZED` | `ADM-04-E2E-FAILURE` |


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
