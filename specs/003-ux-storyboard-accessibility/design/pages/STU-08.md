# STU-08 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-007<br>
**Implementation task:** SPEC-007/T096<br>
**Design task:** SPEC-003/T182

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-08",
  "routeTemplate": "/student/account",
  "pageName": "StudentAccountPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-007",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Show read-only institutional identity and controlled password and session actions.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Institutional identity",
    "Current session",
    "Password-change form",
    "Sign-out actions",
    "Confirmation and result"
  ],
  "responsiveWireframes": {
    "320": "Collapsed navigation and heading precede read-only institutional identity, current session/status, authorized context control when supplied, password form, Revoke all sessions, Sign out, recovery, and result. Confirmation is modal and secrets never leave fields.",
    "375": "The 320 order is retained with 44 CSS px context, password, revoke, sign-out, and recovery targets. Password requirements and errors wrap below fields; destructive actions remain separated.",
    "768": "Compact navigation precedes read-only identity/session cards and a bounded password form. Revoke/sign-out actions follow the form; the confirmation dialog overlays without changing DOM reading order.",
    "1024": "Persistent navigation, identity/session summary sidebar, and main security-actions column are allowed. Validation remains before password fields; conditional context choices contain only server-returned roles.",
    "1280": "The account view uses bounded columns with session state beside read-only identity. Revoke confirmation remains separated from Sign out and cancel restores the originating trigger.",
    "1920": "A centered maximum-width shell preserves the 1280 hierarchy. Surplus width adds gutters only; no password, role choice, session identifier, validation, or destructive action is duplicated."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "EntityCard",
    "StatusBadge",
    "FormField",
    "ValidationSummary",
    "Button",
    "ConfirmationDialog",
    "Alert",
    "AppLink",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/auth/session",
    "POST /api/auth/password/change",
    "POST /api/auth/sessions/revoke-all",
    "POST /api/auth/logout"
  ],
  "actions": [
    "Refresh GET /api/auth/session",
    "Submit POST /api/auth/password/change",
    "Open and confirm POST /api/auth/sessions/revoke-all",
    "Submit POST /api/auth/logout",
    "Navigate to AUTH-05 recovery"
  ],
  "navigationTransitions": [
    "Recovery -> AUTH-05",
    "Sign out -> AUTH-02",
    "Dashboard -> STU-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STU-08-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Student account heading with stable landmarks",
        "Named pending status and disabled duplicate command",
        "No duplicate request, partial outcome, or false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "STU-08-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "STU-08-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "empty is not applicable on STU-08: An authenticated account/session resource is required; missing context maps to unauthorized, session-expired, or service error."
      ],
      "expectedFocusTarget": "Student account heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "STU-08-COMP-STATE-EMPTY"
      ],
      "reason": "An authenticated account/session resource is required; missing context maps to unauthorized, session-expired, or service error."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STU-08-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Read-only institutional identity and current session",
        "Only server-authorized active-context choices",
        "Accepted password, revoke-all, logout, or context result presented once",
        "No plaintext password, role union, MFA/2FA prompt, or client-granted role",
        "Minimum journeys: read-only; validation; stale; password failure; sign-out-all confirmation",
        "Success only when serverAccepted is true"
      ],
      "expectedFocusTarget": "Student account heading on load; cancel restores the revoke trigger; accepted logout follows the safe login destination",
      "liveRegion": "polite",
      "nextActions": [
        "Change password",
        "Switch an authorized context",
        "Revoke all sessions",
        "Sign out"
      ],
      "testIds": [
        "STU-08-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STU-08-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary linked to password or context control",
        "Password-policy/failure text without secret echo",
        "Only server-returned role context may be selected",
        "No success while serverAccepted is false",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field, group, or blocker",
      "liveRegion": "assertive",
      "nextActions": [
        "Correct the linked field",
        "Retry the named endpoint explicitly"
      ],
      "testIds": [
        "STU-08-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STU-08-service-error-v1",
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
        "Open safe support path"
      ],
      "testIds": [
        "STU-08-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STU-08-unauthorized-v1",
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
        "STU-08-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STU-08-session-expired-v1",
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
        "STU-08-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STU-08-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change or uncertain-result heading",
        "STALE_VERSION, SESSION_EXPIRED, or changed session version",
        "Authoritative refresh/review action",
        "Cached authorization, plan, result, or success is not reused",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "Refresh GET /api/auth/session",
        "Review context/session changes",
        "Refresh session and review"
      ],
      "testIds": [
        "STU-08-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STU-08-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline status for Student account heading",
        "Browser connectivity state and safe retry guidance",
        "No queued command, cached result, partial outcome, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry GET session when online; never queue security commands"
      ],
      "testIds": [
        "STU-08-COMP-STATE-OFFLINE"
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
    "Student account heading",
    "Read-only institutional identity",
    "Current session status",
    "Authorized active-context control only when returned by server",
    "Validation summary after a submitted validation failure",
    "Current password field",
    "New password field",
    "Confirm new password field",
    "Show or hide password buttons",
    "Change password button",
    "Revoke all sessions trigger",
    "Revoke confirmation heading",
    "Cancel revoke button",
    "Confirm revoke button",
    "On Cancel or Escape restore Revoke all sessions trigger",
    "Sign out button",
    "Recovery link",
    "Safe support/reference link"
  ],
  "testIds": [
    "STU-08-CONTRACT-T183",
    "STU-08-COMP-T184",
    "STU-08-E2E-PRIMARY",
    "STU-08-E2E-FAILURE",
    "STU-08-A11Y-T185",
    "STU-08-VIS-T186"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-007": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Collapsed navigation and heading precede read-only institutional identity, current session/status, authorized context control when supplied, password form, Revoke all sessions, Sign out, recovery, and result. Confirmation is modal and secrets never leave fields. |
| 375 | The 320 order is retained with 44 CSS px context, password, revoke, sign-out, and recovery targets. Password requirements and errors wrap below fields; destructive actions remain separated. |
| 768 | Compact navigation precedes read-only identity/session cards and a bounded password form. Revoke/sign-out actions follow the form; the confirmation dialog overlays without changing DOM reading order. |
| 1024 | Persistent navigation, identity/session summary sidebar, and main security-actions column are allowed. Validation remains before password fields; conditional context choices contain only server-returned roles. |
| 1280 | The account view uses bounded columns with session state beside read-only identity. Revoke confirmation remains separated from Sign out and cancel restores the originating trigger. |
| 1920 | A centered maximum-width shell preserves the 1280 hierarchy. Surplus width adds gutters only; no password, role choice, session identifier, validation, or destructive action is duplicated. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-007/T095. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STU-08-read-only-v1` (read-only) | success | institutional identity and session shown read-only with server-authorized actions | editable university ID, secret/token, unreturned role, or client authority | Use an authorized security action | `STU-08-COMP-STATE-SUCCESS` | `STU-08-E2E-PRIMARY` |
| `STU-08-validation-v1` (validation) | validation-error | summary linked to the invalid password/context field and externalized rule text | inline secret echo, hidden error, false success, or arbitrary role | Correct the field and retry | `STU-08-COMP-STATE-VALIDATION-ERROR` | `STU-08-E2E-FAILURE` |
| `STU-08-stale-v1` (stale) | stale | STALE_VERSION or changed session, refresh/review action, and disabled authority-sensitive command | cached role/session success, silent overwrite, or focus theft | Refresh session and review | `STU-08-COMP-STATE-STALE` | `STU-08-E2E-FAILURE` |
| `STU-08-password-failure-v1` (password failure) | validation-error | generic password failure, summary focus, cleared secrets, and recovery action | which credential failed, retained password, MFA/2FA prompt, or changed password success | Retry or open recovery | `STU-08-COMP-STATE-VALIDATION-ERROR` | `STU-08-E2E-FAILURE` |
| `STU-08-sign-out-all-confirmation-v1` (sign-out-all confirmation) | success | labelled confirmation, consequence text, Cancel and Confirm, focus trap, and trigger restoration on cancel | immediate revoke without confirmation, focus escape/loss, duplicate POST, or secret/session IDs | Cancel and restore trigger or confirm once | `STU-08-COMP-STATE-SUCCESS` | `STU-08-E2E-PRIMARY` |

## Review notes

- Six annotated layouts are specific to STU-08 and preserve DOM, keyboard,
  action, state, and information order without hiding reasons or boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; commands
  name their endpoint and navigation/browser-only actions imply no missing write.
- Background stale/offline/service updates preserve focus; submitted validation
  and modal/navigation focus/restoration follow focusOrder and the reason map.
 Approval does not promote the route beyond `design-only`.
- Ahmed ELbamby approved immutable record version 1.0 on 2026-07-13, so its
  design task may close. Approval does not promote the route beyond `design-only`.
