# ADM-08 Page Design Record

**Record version:** `draft/1.0`<br>
**Approval status:** Pending Ahmed ELbamby review<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-017<br>
**Implementation task:** SPEC-017/T088<br>
**Design task:** SPEC-003/T222

This complete design draft is governed by `page-design-record/1.1-draft`. It may be
reviewed as design evidence, but it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-08",
  "routeTemplate": "/admin/registrations",
  "pageName": "RegistrationAdministrationPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-017",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-014",
    "SPEC-015",
    "SPEC-017"
  ],
  "actors": [
    "Admin"
  ],
  "purpose": "Provide read-only monitoring of submissions, receipts, fill, failures, collisions, and reconciliation alerts.",
  "informationHierarchy": [
    "Authenticated AppShell context and timestamp",
    "Operations metrics",
    "Filters",
    "Submission and fill table",
    "Receipt detail",
    "Collision and degraded alerts",
    "Safe support reference"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Timestamp/filters, stacked submission summaries, selected receipt, collision/degraded alerts, then support reference and no-repair notice.",
    "375": "At 375 CSS px, Same order with filter controls before results and every alert adjacent to its Student-safe detail link.",
    "768": "At 768 CSS px, Compact navigation; paged submission table above receipt detail. Alerts and support reference span the content width.",
    "1024": "At 1024 CSS px, Persistent navigation with result table left and read-only receipt/alert detail right; no command toolbar is rendered.",
    "1280": "At 1280 CSS px, Bounded monitoring and detail columns with metrics timestamp always visible before values.",
    "1920": "At 1920 CSS px, Centered workspace; wide space never implies repair controls or separates paused-group alerts from support guidance."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "SearchFilter",
    "DataTable",
    "Pagination",
    "ReceiptSummary",
    "StatusBadge",
    "Alert",
    "StatePanel",
    "AppLink"
  ],
  "dataContracts": [
    "GET /api/admin/operations/metrics",
    "GET /api/admin/students/{studentId}/terms/{termId}/registrations",
    "GET /api/admin/students/{studentId}/terms/{termId}/registrations/{submissionId}"
  ],
  "actions": [
    "Filter registrations",
    "Clear registration filters",
    "Open submission detail",
    "Open Student profile",
    "Open safe support reference"
  ],
  "navigationTransitions": [
    "Actions stay on ADM-08",
    "Student profile -> ADM-04",
    "Dashboard -> ADM-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-08-loading-v1",
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
        "ADM-08-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "ADM-08-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: user action",
        "No matching registrations heading",
        "Current filters and plain-language no-results reason",
        "Clear or change registration filters action",
        "Empty is not a successful command result"
      ],
      "expectedFocusTarget": "Preserve the active registration filter while the no-results heading is announced",
      "liveRegion": "polite",
      "nextActions": [
        "Clear or change registration filters"
      ],
      "testIds": [
        "ADM-08-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-08-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
      "Metrics timestamp, filtered submissions, receipts, fill, failure, and collision status",
      "Read-only detail plus accepted metrics/reconciliationAlerts, paused-group support reference, and no-repair notice",
      "Minimum journeys: live; stale; degraded; collision; paused-group support reference; no repair/correction action"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Inspect a submission",
        "Open safe support reference",
        "Follow the support runbook reference",
        "Return to monitoring"
      ],
      "testIds": [
        "ADM-08-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "not-applicable",
      "fixture": "ADM-08-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "State is not applicable; collision observations arrive in a successful metrics response as reconciliation alerts."
      ],
      "expectedFocusTarget": "Page heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "ADM-08-COMP-STATE-VALIDATION-ERROR"
      ],
      "reason": "This read-only route has no form validation command; operational collisions are observation-only reconciliationAlerts in a successful owner response."
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-08-service-error-v1",
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
        "Retry or open support"
      ],
      "testIds": [
        "ADM-08-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-08-unauthorized-v1",
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
        "ADM-08-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-08-session-expired-v1",
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
        "ADM-08-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-08-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "availabilityState stale exactly as returned",
        "observedAtUtc and refresh action",
        "No claim that stale metrics are live"
      ],
      "expectedFocusTarget": "Preserve current focus; move to the concurrent-change heading only after failed navigation or a submitted command",
      "liveRegion": "polite",
      "nextActions": [
        "Refresh monitoring data"
      ],
      "testIds": [
        "ADM-08-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-08-offline-v1",
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
        "ADM-08-COMP-STATE-OFFLINE"
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
    "Registrations heading",
    "Timestamp and filters",
    "Submission and fill table",
    "Pagination",
    "Selected receipt summary",
    "Collision, degraded, or reconciliation alerts",
    "Student-profile and safe-support links"
  ],
  "testIds": [
    "ADM-08-CONTRACT-T223",
    "ADM-08-COMP-T224",
    "ADM-08-E2E-PRIMARY",
    "ADM-08-E2E-FAILURE",
    "ADM-08-A11Y-T225",
    "ADM-08-VIS-T226"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1-draft",
    "SPEC-014": "not-pinned",
    "SPEC-015": "not-pinned",
    "SPEC-017": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "pending-Ahmed-review"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Timestamp/filters, stacked submission summaries, selected receipt, collision/degraded alerts, then support reference and no-repair notice. |
| 375 | Same order with filter controls before results and every alert adjacent to its Student-safe detail link. |
| 768 | Compact navigation; paged submission table above receipt detail. Alerts and support reference span the content width. |
| 1024 | Persistent navigation with result table left and read-only receipt/alert detail right; no command toolbar is rendered. |
| 1280 | Bounded monitoring and detail columns with metrics timestamp always visible before values. |
| 1920 | Centered workspace; wide space never implies repair controls or separates paused-group alerts from support guidance. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-08-live-v1` (live) | success | live timestamp, submission/fill metrics, receipt status, and reconciliation state | repair, correction, drop, or withdrawal controls | Inspect a submission | `ADM-08-COMP-STATE-SUCCESS` | `ADM-08-E2E-PRIMARY` |
| `ADM-08-no-results-v1` (no results) | empty | active registration filters, no-results heading, and clear/change action | unrelated Student records or a false monitoring failure | Clear or change registration filters | `ADM-08-COMP-STATE-EMPTY` | `ADM-08-E2E-PRIMARY` |
| `ADM-08-stale-v1` (stale) | stale | stale timestamp/reason and refresh action | claiming metrics are live | Refresh monitoring data | `ADM-08-COMP-STATE-STALE` | `ADM-08-E2E-FAILURE` |
| `ADM-08-degraded-v1` (degraded) | service-error | degraded warning, reference ID, last safe timestamp, and retry/support | raw operations detail or healthy claim | Retry or open support | `ADM-08-COMP-STATE-SERVICE-ERROR` | `ADM-08-E2E-FAILURE` |
| `ADM-08-collision-v1` (collision) | success | accepted reconciliation Alert with affected group reference, paused/repaired state, detected timestamp, invariant-safe observation, and support path | invented collision reason code or manual seat repair | Open safe support reference | `ADM-08-COMP-STATE-SUCCESS` | `ADM-08-E2E-PRIMARY` |
| `ADM-08-paused-group-support-reference-v1` (paused-group support reference) | success | paused-group warning, support reference, and read-only affected submissions | repairing or resuming the group from this page | Follow the support runbook reference | `ADM-08-COMP-STATE-SUCCESS` | `ADM-08-E2E-PRIMARY` |
| `ADM-08-no-repair-correction-action-v1` (no repair/correction action) | success | explicit no-repair/correction notice and read-only semantics | any mutation control | Return to monitoring | `ADM-08-COMP-STATE-SUCCESS` | `ADM-08-E2E-PRIMARY` |


## Review notes

- The six responsive descriptions preserve one information and focus order,
  wrap all reasons, and provide semantic table alternatives where applicable.
- API entries are copied verbatim from `.specify/page-api-manifest.json`;
  server authorization and decisions remain with the named owner specifications.
- Every UI state has a deterministic fixture, content, focus, live-region,
  next-action, and planned test identifier.
- Ahmed ELbamby must approve this exact draft version before its PDR task can
  close. Approval does not promote the route beyond `design-only`.
