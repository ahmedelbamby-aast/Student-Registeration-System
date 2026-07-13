# ADM-05 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-009<br>
**Implementation task:** SPEC-009/T091<br>
**Design task:** SPEC-003/T207

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-05",
  "routeTemplate": "/admin/catalogue",
  "pageName": "CatalogueAdministrationPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-009",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-009",
    "SPEC-017"
  ],
  "actors": [
    "Admin"
  ],
  "purpose": "Manage versioned programs, curricula, courses, prerequisites, and typed policy values.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Version and status list",
    "Catalogue and policy editor",
    "Import and validation findings",
    "Simulation result",
    "Publish confirmation and result"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Version list, selected catalogue/policy editor, validation findings, simulation, then publish confirmation in one column.",
    "375": "At 375 CSS px, Same order with typed fields grouped by entity and all cycle/range reasons wrapping before actions.",
    "768": "At 768 CSS px, Compact navigation; version table above a two-column editor. Validation and simulation results span both columns.",
    "1024": "At 1024 CSS px, Persistent navigation with version list left and editor right; prerequisite graph has a semantic list and publish restores its trigger.",
    "1280": "At 1280 CSS px, Bounded catalogue and policy columns with simulation beside its inputs and findings immediately before publish.",
    "1920": "At 1920 CSS px, Centered workspace; surplus width never stretches forms or detaches provenance, validation, simulation, or publication state."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "SearchFilter",
    "DataTable",
    "Pagination",
    "FormField",
    "ValidationSummary",
    "StatusBadge",
    "ConfirmationDialog",
    "Button",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/admin/programs",
    "GET /api/admin/catalogue/versions",
    "GET /api/admin/catalogue/drafts/{draftId}",
    "PUT /api/admin/catalogue/drafts/{draftId}",
    "POST /api/admin/catalogue/imports",
    "GET /api/admin/catalogue/imports/{importId}",
    "POST /api/admin/catalogue/imports/{importId}/validate",
    "POST /api/admin/catalogue/imports/{importId}/publish",
    "GET /api/admin/policies",
    "POST /api/admin/policies",
    "PUT /api/admin/policies/{policySetId}",
    "POST /api/admin/policies/{policySetId}/validate",
    "POST /api/admin/policies/{policySetId}/simulate",
    "POST /api/admin/policies/{policySetId}/publish"
  ],
  "actions": [
    "Edit catalogue draft",
    "Import catalogue",
    "Validate catalogue or policy",
    "Simulate policy",
    "Publish approved version"
  ],
  "navigationTransitions": [
    "Actions stay on ADM-05",
    "Offerings -> ADM-06",
    "Dashboard -> ADM-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-05-loading-v1",
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
        "ADM-05-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "ADM-05-empty-v1",
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
        "ADM-05-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-05-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Catalogue or policy version, lifecycle, typed values, prerequisite graph, and provenance",
        "Validation, simulation, and publication results with stable reasons",
        "Success only when serverAccepted is true",
        "Minimum journeys: draft; invalid; cycle; simulate; publish; stale publish"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "ADM-05-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "ADM-05-validation-error-v1",
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
        "ADM-05-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-05-service-error-v1",
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
        "ADM-05-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-05-unauthorized-v1",
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
        "ADM-05-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-05-session-expired-v1",
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
        "ADM-05-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-05-stale-v1",
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
        "Refresh and revalidate"
      ],
      "testIds": [
        "ADM-05-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-05-offline-v1",
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
        "ADM-05-COMP-STATE-OFFLINE"
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
    "Catalogue heading",
    "Version and status controls",
    "Program, curriculum, course, prerequisite, or policy editor fields",
    "Import and validation findings",
    "Simulation inputs and result",
    "Validation summary after failure",
    "Publish dialog and return to its trigger",
    "Offerings and support links"
  ],
  "testIds": [
    "ADM-05-CONTRACT-T208",
    "ADM-05-COMP-T209",
    "ADM-05-E2E-PRIMARY",
    "ADM-05-E2E-FAILURE",
    "ADM-05-A11Y-T210",
    "ADM-05-VIS-T211"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-009": "not-pinned",
    "SPEC-017": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Version list, selected catalogue/policy editor, validation findings, simulation, then publish confirmation in one column. |
| 375 | Same order with typed fields grouped by entity and all cycle/range reasons wrapping before actions. |
| 768 | Compact navigation; version table above a two-column editor. Validation and simulation results span both columns. |
| 1024 | Persistent navigation with version list left and editor right; prerequisite graph has a semantic list and publish restores its trigger. |
| 1280 | Bounded catalogue and policy columns with simulation beside its inputs and findings immediately before publish. |
| 1920 | Centered workspace; surplus width never stretches forms or detaches provenance, validation, simulation, or publication state. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-05-draft-v1` (draft) | success | draft version, provenance, typed values, and unsaved-change status | labeling the draft published | Validate or continue editing | `ADM-05-COMP-STATE-SUCCESS` | `ADM-05-E2E-PRIMARY` |
| `ADM-05-invalid-v1` (invalid) | validation-error | invalid typed value, allowed range, and linked field | storing executable policy content | Correct the typed value | `ADM-05-COMP-STATE-VALIDATION-ERROR` | `ADM-05-E2E-FAILURE` |
| `ADM-05-cycle-v1` (cycle) | validation-error | every course in the prerequisite cycle and direct edit links | publishing the cycle | Edit prerequisites | `ADM-05-COMP-STATE-VALIDATION-ERROR` | `ADM-05-E2E-FAILURE` |
| `ADM-05-simulate-v1` (simulate) | success | simulation inputs, version, deterministic result, reasons, and no persisted change | presenting simulation as publication | Review or publish | `ADM-05-COMP-STATE-SUCCESS` | `ADM-05-E2E-PRIMARY` |
| `ADM-05-publish-v1` (publish) | success | published immutable version, actor, timestamp, and effective dates | success before server acceptance | Return to version list | `ADM-05-COMP-STATE-SUCCESS` | `ADM-05-E2E-PRIMARY` |
| `ADM-05-stale-publish-v1` (stale publish) | stale | current version, stale publish reason, and refresh action | overwriting the published successor | Refresh and revalidate | `ADM-05-COMP-STATE-STALE` | `ADM-05-E2E-FAILURE` |


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
