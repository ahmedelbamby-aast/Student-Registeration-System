# AUTH-04 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-007<br>
**Implementation task:** SPEC-007/T092<br>
**Design task:** SPEC-003/T137

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "AUTH-04",
  "routeTemplate": "/staff/login",
  "pageName": "StaffLoginPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-007",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007",
    "SPEC-008"
  ],
  "actors": [
    "Admin",
    "Lecturer",
    "Teaching Assistant"
  ],
  "purpose": "Provide one password-only Staff login while the server derives authorized role and scope without a role picker or second factor.",
  "informationHierarchy": [
    "AASTMT identity and Staff login heading",
    "Staff identifier and password fields",
    "Validation summary",
    "Pending or no-role status",
    "Recovery and back links"
  ],
  "responsiveWireframes": {
    "320": "The shared Staff login heading, password-only explanation, and public status precede one form. Staff identifier, Password, show/hide, and Sign in stack; recovery and gateway links follow. Only after role-selection-required, returned roles stack as 44 CSS px choices; no pre-auth picker or second-factor control exists.",
    "375": "The 320 order is retained with full-width Sign in. Disabled/no-role text wraps beside recovery/support; a post-auth role-selection-required panel shows only server-returned Admin, Lecturer, or Teaching Assistant choices.",
    "768": "A bounded staff form is the main column with privacy-safe guidance. No pre-auth selector exists; the conditional authenticated role panel follows the result and never appears beside credentials as a claimed role.",
    "1024": "Form and guidance may use two columns while validation stays before fields. A single role routes directly; conditional returned-role choices remain one labelled region followed by session rotation.",
    "1280": "The form remains bounded with recovery/support below the result. Conditional role choices stay attached to role-selection-required and route only after PUT /api/auth/session/context succeeds.",
    "1920": "The centered 1280 composition gains whitespace only. Additional space never creates pre-auth role cards, OTP/QR/MFA/2FA controls, unioned roles, or client-derived destinations."
  },
  "components": [
    "FormField",
    "ValidationSummary",
    "Button",
    "AppLink",
    "EntityCard",
    "StatusBadge",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/public/context",
    "POST /api/auth/staff/login",
    "PUT /api/auth/session/context"
  ],
  "actions": [
    "Submit POST /api/auth/staff/login",
    "Toggle password visibility",
    "When SessionDto is role-selection-required, select one server-returned role through PUT /api/auth/session/context",
    "Open recovery",
    "Return to gateway",
    "Retry GET /api/public/context"
  ],
  "navigationTransitions": [
    "Single server role routes directly: Admin -> ADM-01; Lecturer or Teaching Assistant -> STF-01",
    "role-selection-required -> choose exactly one returned role -> PUT /api/auth/session/context -> ADM-01 or STF-01",
    "Recovery -> AUTH-05",
    "Gateway -> AUTH-01",
    "Safe status -> SYS-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "AUTH-04-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Staff login heading without replacing landmarks",
        "Named pending status and disabled command",
        "No duplicate request or false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "AUTH-04-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "AUTH-04-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "empty is not applicable on AUTH-04: The shared credential form is always rendered; no-role is a validation outcome."
      ],
      "expectedFocusTarget": "Staff login heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-04-COMP-STATE-EMPTY"
      ],
      "reason": "The shared credential form is always rendered; no-role is a validation outcome."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "AUTH-04-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Server-accepted password-only Staff authentication",
        "Single server role routes directly, or SessionDto explicitly returns role-selection-required",
        "Conditional choice contains only server-returned Admin, Lecturer, or Teaching Assistant roles",
        "PUT /api/auth/session/context rotates the session for exactly one role before routing to ADM-01 or STF-01",
        "No pre-auth role picker, role union, client-granted role, MFA, or 2FA",
        "Success only when serverAccepted is true",
        "Password-only demo login with no second factor"
      ],
      "expectedFocusTarget": "Server-authorized destination heading for one role; role-selection heading only when explicitly returned; keep focus on Sign in while login is pending",
      "liveRegion": "polite",
      "nextActions": [
        "Follow the single server-authorized destination",
        "If role-selection-required, choose exactly one returned role and wait for PUT /api/auth/session/context before routing"
      ],
      "testIds": [
        "AUTH-04-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "AUTH-04-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary linked to Staff identifier or Password",
        "Invalid, disabled, or no-role safe reason",
        "No role claim through form, query, hidden field, or storage",
        "Password cleared after rejection",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field",
      "liveRegion": "assertive",
      "nextActions": [
        "Focus the linked invalid field",
        "Open recovery/support or resubmit POST /api/auth/staff/login"
      ],
      "testIds": [
        "AUTH-04-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "AUTH-04-service-error-v1",
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
        "AUTH-04-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "not-applicable",
      "fixture": "AUTH-04-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "unauthorized is not applicable on AUTH-04: Invalid, disabled, and no-role outcomes occur before authentication and expose no protected resource."
      ],
      "expectedFocusTarget": "Staff login heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-04-COMP-STATE-UNAUTHORIZED"
      ],
      "reason": "Invalid, disabled, and no-role outcomes occur before authentication and expose no protected resource."
    },
    {
      "state": "session-expired",
      "applicability": "not-applicable",
      "fixture": "AUTH-04-session-expired-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "session-expired is not applicable on AUTH-04: No authenticated session is required to open Staff login."
      ],
      "expectedFocusTarget": "Staff login heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-04-COMP-STATE-SESSION-EXPIRED"
      ],
      "reason": "No authenticated session is required to open Staff login."
    },
    {
      "state": "stale",
      "applicability": "not-applicable",
      "fixture": "AUTH-04-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "stale is not applicable on AUTH-04: The password-only command has no editable version token; public context refresh is independent."
      ],
      "expectedFocusTarget": "Staff login heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-04-COMP-STATE-STALE"
      ],
      "reason": "The password-only command has no editable version token; public context refresh is independent."
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "AUTH-04-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline state for Staff login heading",
        "Browser connectivity status and retry guidance",
        "No queued, cached, duplicate, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry POST /api/auth/staff/login only when online"
      ],
      "testIds": [
        "AUTH-04-COMP-STATE-OFFLINE"
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
    "Staff login heading",
    "Password-only explanation and public service status",
    "Validation summary after a submitted validation failure",
    "Staff identifier field",
    "Password field",
    "Show or hide password button",
    "Sign in button",
    "When and only when role-selection-required: authenticated context-selection heading",
    "Server-returned Admin, Lecturer, or Teaching Assistant role buttons in returned order",
    "Recovery link",
    "Return to gateway link",
    "Safe support/reference link supplied by the server"
  ],
  "testIds": [
    "AUTH-04-CONTRACT-T138",
    "AUTH-04-COMP-T139",
    "AUTH-04-E2E-PRIMARY",
    "AUTH-04-E2E-FAILURE",
    "AUTH-04-A11Y-T140",
    "AUTH-04-VIS-T141"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-007": "not-pinned",
    "SPEC-008": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | The shared Staff login heading, password-only explanation, and public status precede one form. Staff identifier, Password, show/hide, and Sign in stack; recovery and gateway links follow. Only after role-selection-required, returned roles stack as 44 CSS px choices; no pre-auth picker or second-factor control exists. |
| 375 | The 320 order is retained with full-width Sign in. Disabled/no-role text wraps beside recovery/support; a post-auth role-selection-required panel shows only server-returned Admin, Lecturer, or Teaching Assistant choices. |
| 768 | A bounded staff form is the main column with privacy-safe guidance. No pre-auth selector exists; the conditional authenticated role panel follows the result and never appears beside credentials as a claimed role. |
| 1024 | Form and guidance may use two columns while validation stays before fields. A single role routes directly; conditional returned-role choices remain one labelled region followed by session rotation. |
| 1280 | The form remains bounded with recovery/support below the result. Conditional role choices stay attached to role-selection-required and route only after PUT /api/auth/session/context succeeds. |
| 1920 | The centered 1280 composition gains whitespace only. Additional space never creates pre-auth role cards, OTP/QR/MFA/2FA controls, unioned roles, or client-derived destinations. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-007/T091. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `AUTH-04-admin-lecturer-ta-routing-v1` (Admin/Lecturer/TA routing) | success | a single server role routes directly; role-selection-required exposes only returned Admin, Lecturer, or Teaching Assistant roles, PUT rotates the session for exactly one choice, then routes ADM-01 or STF-01 | pre-auth role picker, client-derived role, role union, unreturned role, route before successful PUT, OTP, MFA, or 2FA | Follow the single role destination or select one returned role and wait for session rotation | `AUTH-04-COMP-STATE-SUCCESS` | `AUTH-04-E2E-PRIMARY` |
| `AUTH-04-invalid-v1` (invalid) | validation-error | generic invalid message, summary focus, cleared password, and recovery | account existence, password echo, protected data, or authenticated navigation | Review credentials and resubmit | `AUTH-04-COMP-STATE-VALIDATION-ERROR` | `AUTH-04-E2E-FAILURE` |
| `AUTH-04-disabled-v1` (disabled) | validation-error | disabled safe reason plus recovery or support | dashboard route, excess role detail, or automatic retry | Open recovery or safe support | `AUTH-04-COMP-STATE-VALIDATION-ERROR` | `AUTH-04-E2E-FAILURE` |
| `AUTH-04-no-role-v1` (no-role) | validation-error | no-authorized-role outcome and support/reference action | fallback role, client role chooser, or staff workspace access | Return to gateway or open support | `AUTH-04-COMP-STATE-VALIDATION-ERROR` | `AUTH-04-E2E-FAILURE` |
| `AUTH-04-password-only-no-second-factor-v1` (password-only/no-second-factor) | success | only Staff identifier, Password, show/hide, Sign in, recovery, and gateway controls | OTP, authenticator, SMS/email code, QR code, MFA/2FA, or role picker | Submit the password-only form once | `AUTH-04-COMP-STATE-SUCCESS` | `AUTH-04-E2E-PRIMARY` |

## Review notes

- The six annotated layouts are route-specific and preserve DOM, keyboard,
  action, and information order without hiding errors or privacy boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; every
  command names its endpoint and navigation or browser-only actions say so.
- Validation moves focus to the summary, while background offline/service
  changes preserve focus; successful navigation focuses the returned heading.
 Approval does not promote the route beyond `design-only`.
- Ahmed ELbamby approved immutable record version 1.0 on 2026-07-13, so its
  design task may close. Approval does not promote the route beyond `design-only`.
