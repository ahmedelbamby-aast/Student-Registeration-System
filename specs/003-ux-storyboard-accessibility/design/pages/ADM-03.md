# ADM-03 Page Design Record

**Record version:** `draft/1.0`<br>
**Approval status:** Pending Ahmed ELbamby review<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-007<br>
**Implementation task:** SPEC-007/T124<br>
**Design task:** SPEC-003/T197

This complete design draft is governed by `page-design-record/1.1-draft`. It may be
reviewed as design evidence, but it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "ADM-03",
  "routeTemplate": "/admin/users",
  "pageName": "UserAdministrationPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-007",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007",
    "SPEC-017"
  ],
  "actors": [
    "Admin"
  ],
  "purpose": "Import and provision accounts and manage Staff roles and account status through governed identity commands.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "User search and list",
    "Import preview and row errors",
    "User detail",
    "Role and status editor",
    "Safeguard confirmation and result"
  ],
  "responsiveWireframes": {
    "320": "At 320 CSS px, Search/import controls, preview summary, stacked row errors, selected user detail, role/status controls, then confirmations.",
    "375": "At 375 CSS px, Single column with compact error rows and role controls grouped by user; destructive status actions remain after safeguards.",
    "768": "At 768 CSS px, Compact navigation; user list and preview table above selected-user detail. Authorized inline row errors and paging follow their tables; no download action is implied.",
    "1024": "At 1024 CSS px, Persistent navigation with list/preview left and selected user role editor right; dialogs restore the invoking action.",
    "1280": "At 1280 CSS px, Bounded list and detail columns with source-row errors visible before publish; final-Admin warning stays beside confirmation.",
    "1920": "At 1920 CSS px, Centered workspace; wide tables remain labelled with a stacked alternative and never push role controls off screen."
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
    "GET /api/admin/users",
    "POST /api/admin/users/imports",
    "GET /api/admin/users/imports/{importId}",
    "POST /api/admin/users/imports/{importId}/publish",
    "PATCH /api/admin/users/{userId}/status",
    "PUT /api/admin/users/{userId}/roles"
  ],
  "actions": [
    "Preview user import",
    "Publish user import",
    "Enable or disable account",
    "Assign or revoke role"
  ],
  "navigationTransitions": [
    "Actions stay on ADM-03",
    "Student details -> ADM-04",
    "Dashboard -> ADM-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "ADM-03-loading-v1",
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
        "ADM-03-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "ADM-03-empty-v1",
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
        "ADM-03-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "ADM-03-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "User/import status, preview totals, row results, account status, and server-authorized roles",
        "Final-Admin safeguard and source-row references when applicable",
        "Success only when serverAccepted is true",
        "Minimum journeys: preview; duplicates; partial-invalid; disable; concurrent role change"
      ],
      "expectedFocusTarget": "Page heading unless a user action set focus",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with an authorized route action"
      ],
      "testIds": [
        "ADM-03-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "ADM-03-validation-error-v1",
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
        "ADM-03-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "ADM-03-service-error-v1",
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
        "ADM-03-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "ADM-03-unauthorized-v1",
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
        "ADM-03-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "ADM-03-session-expired-v1",
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
        "ADM-03-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "ADM-03-stale-v1",
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
        "Refresh and review roles"
      ],
      "testIds": [
        "ADM-03-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "ADM-03-offline-v1",
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
        "ADM-03-COMP-STATE-OFFLINE"
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
    "Users heading",
    "Search and import controls",
    "Preview summary and row-error table",
    "Selected user identity and status",
    "Role editor controls",
    "Validation summary after failure",
    "Confirmation dialog and return to its trigger",
    "Pagination and safe support link"
  ],
  "testIds": [
    "ADM-03-CONTRACT-T198",
    "ADM-03-COMP-T199",
    "ADM-03-E2E-PRIMARY",
    "ADM-03-E2E-FAILURE",
    "ADM-03-A11Y-T200",
    "ADM-03-VIS-T201"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1-draft",
    "SPEC-007": "not-pinned",
    "SPEC-017": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "pending-Ahmed-review"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Search/import controls, preview summary, stacked row errors, selected user detail, role/status controls, then confirmations. |
| 375 | Single column with compact error rows and role controls grouped by user; destructive status actions remain after safeguards. |
| 768 | Compact navigation; user list and preview table above selected-user detail. Authorized inline row errors and paging follow their tables; no download action is implied. |
| 1024 | Persistent navigation with list/preview left and selected user role editor right; dialogs restore the invoking action. |
| 1280 | Bounded list and detail columns with source-row errors visible before publish; final-Admin warning stays beside confirmation. |
| 1920 | Centered workspace; wide tables remain labelled with a stacked alternative and never push role controls off screen. |

## Minimum journey and test traceability

All fixtures use `frontend-fixture/1.0`. Required and forbidden content is
normative; a forbidden item fails the planned route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `ADM-03-preview-v1` (preview) | success | preview totals, synthetic source rows, proposed roles, and zero committed changes | claiming import is published | Publish or correct the source | `ADM-03-COMP-STATE-SUCCESS` | `ADM-03-E2E-PRIMARY` |
| `ADM-03-duplicates-v1` (duplicates) | validation-error | duplicate source-row references and non-disclosing account reasons | creating duplicate identities | Correct duplicates and re-preview | `ADM-03-COMP-STATE-VALIDATION-ERROR` | `ADM-03-E2E-FAILURE` |
| `ADM-03-partial-invalid-v1` (partial-invalid) | validation-error | valid and invalid row counts plus authorized inline `IdentityImportBatchDto.errors` row/code/message references | silent partial publication or an uncontracted error-download action | Correct invalid rows | `ADM-03-COMP-STATE-VALIDATION-ERROR` | `ADM-03-E2E-FAILURE` |
| `ADM-03-disable-v1` (disable) | success | target user, disabled status, reason, actor, and server timestamp | revealing credentials or disabling the final Admin | Return to user detail | `ADM-03-COMP-STATE-SUCCESS` | `ADM-03-E2E-PRIMARY` |
| `ADM-03-concurrent-role-change-v1` (concurrent role change) | stale | changed role set, expected/current version, and refresh action | overwriting concurrent roles | Refresh and review roles | `ADM-03-COMP-STATE-STALE` | `ADM-03-E2E-FAILURE` |


## Review notes

- The six responsive descriptions preserve one information and focus order,
  wrap all reasons, and provide semantic table alternatives where applicable.
- API entries are copied verbatim from `.specify/page-api-manifest.json`;
  server authorization and decisions remain with the named owner specifications.
- Every UI state has a deterministic fixture, content, focus, live-region,
  next-action, and planned test identifier.
- Ahmed ELbamby must approve this exact draft version before its PDR task can
  close. Approval does not promote the route beyond `design-only`.
