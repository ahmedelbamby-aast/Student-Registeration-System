# AUTH-02 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-007<br>
**Implementation task:** SPEC-007/T088<br>
**Design task:** SPEC-003/T127

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "AUTH-02",
  "routeTemplate": "/student/login",
  "pageName": "StudentLoginPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-007",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007",
    "SPEC-008"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Authenticate a Student with University ID and password without disclosing account state.",
  "informationHierarchy": [
    "AASTMT identity and login heading",
    "University ID and password fields",
    "Validation summary",
    "Pending or failure status",
    "Activation, recovery, and back links"
  ],
  "responsiveWireframes": {
    "320": "One labelled Student login form follows the heading and privacy-safe public status. University ID, Password, show/hide, and Sign in stack in DOM order; activation, recovery, gateway, and support links follow. Labels remain visible during autofill.",
    "375": "The 320 order is retained with 44 CSS px controls. Sign in remains full width, show/hide stays attached to Password, and validation text wraps without shifting field labels.",
    "768": "A bounded login panel is the main column; the public status may occupy a labelled complementary panel. Keyboard order stays heading, status, summary, fields, command, then links.",
    "1024": "The form and non-sensitive help may form two columns, but validation remains immediately before the fields. No role-selection or second-factor region appears.",
    "1280": "The login panel remains at a readable maximum width; activation and recovery links align below Sign in only when labels fit. Public context never displaces errors.",
    "1920": "The centered 1280 composition gains whitespace only. Credentials, validation, pending state, and recovery remain grouped; secrets are not mirrored in side content."
  },
  "components": [
    "FormField",
    "ValidationSummary",
    "Button",
    "AppLink",
    "StatusBadge",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/public/context",
    "POST /api/auth/student/login"
  ],
  "actions": [
    "Submit POST /api/auth/student/login",
    "Toggle password visibility",
    "Open activation",
    "Open recovery",
    "Return to gateway",
    "Retry GET /api/public/context"
  ],
  "navigationTransitions": [
    "Accepted login -> STU-01",
    "Activation -> AUTH-03",
    "Recovery -> AUTH-05",
    "Gateway -> AUTH-01",
    "Safe status -> SYS-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "AUTH-02-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Student login heading without replacing landmarks",
        "Named pending status and disabled command",
        "No duplicate request or false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "AUTH-02-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "AUTH-02-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "empty is not applicable on AUTH-02: A credential form is always rendered; account lookup never produces an empty collection."
      ],
      "expectedFocusTarget": "Student login heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-02-COMP-STATE-EMPTY"
      ],
      "reason": "A credential form is always rendered; account lookup never produces an empty collection."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "AUTH-02-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Server-accepted Student authentication",
        "Server-authorized destination, normally STU-01",
        "Password-only login with no MFA or 2FA prompt",
        "Password is not echoed, logged, or retained",
        "No role picker or client-selected role",
        "Minimum journeys: valid; invalid; locked; rate-limited; keyboard/error focus",
        "Success only when serverAccepted is true",
        "Password-only demo login with no second factor"
      ],
      "expectedFocusTarget": "Server-authorized destination heading after navigation; keep focus on Sign in while pending",
      "liveRegion": "polite",
      "nextActions": [
        "Follow the server-authorized Student destination",
        "Submit login once and follow the server destination"
      ],
      "testIds": [
        "AUTH-02-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "AUTH-02-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary linked to University ID or Password",
        "Generic invalid, locked, or rate-limited outcome without account enumeration",
        "Password cleared after a rejected credential attempt",
        "No success while serverAccepted is false",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field",
      "liveRegion": "assertive",
      "nextActions": [
        "Focus the linked invalid field",
        "Open recovery/support or resubmit POST /api/auth/student/login"
      ],
      "testIds": [
        "AUTH-02-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "AUTH-02-service-error-v1",
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
        "AUTH-02-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "not-applicable",
      "fixture": "AUTH-02-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "unauthorized is not applicable on AUTH-02: Invalid credentials are a validation outcome; this public form exposes no protected resource."
      ],
      "expectedFocusTarget": "Student login heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-02-COMP-STATE-UNAUTHORIZED"
      ],
      "reason": "Invalid credentials are a validation outcome; this public form exposes no protected resource."
    },
    {
      "state": "session-expired",
      "applicability": "not-applicable",
      "fixture": "AUTH-02-session-expired-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "session-expired is not applicable on AUTH-02: No authenticated session is required to open Student login."
      ],
      "expectedFocusTarget": "Student login heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-02-COMP-STATE-SESSION-EXPIRED"
      ],
      "reason": "No authenticated session is required to open Student login."
    },
    {
      "state": "stale",
      "applicability": "not-applicable",
      "fixture": "AUTH-02-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "stale is not applicable on AUTH-02: The login command has no editable version token; public context refresh is independent."
      ],
      "expectedFocusTarget": "Student login heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-02-COMP-STATE-STALE"
      ],
      "reason": "The login command has no editable version token; public context refresh is independent."
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "AUTH-02-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline state for Student login heading",
        "Browser connectivity status and retry guidance",
        "No queued, cached, duplicate, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry POST /api/auth/student/login only when online"
      ],
      "testIds": [
        "AUTH-02-COMP-STATE-OFFLINE"
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
    "Student login heading",
    "Public service and registration status",
    "Validation summary after a submitted validation failure",
    "University ID field",
    "Password field",
    "Show or hide password button",
    "Sign in button",
    "Student activation link",
    "Account recovery link",
    "Return to gateway link",
    "Safe support/reference link supplied by the server"
  ],
  "testIds": [
    "AUTH-02-CONTRACT-T128",
    "AUTH-02-COMP-T129",
    "AUTH-02-E2E-PRIMARY",
    "AUTH-02-E2E-FAILURE",
    "AUTH-02-A11Y-T130",
    "AUTH-02-VIS-T131"
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
| 320 | One labelled Student login form follows the heading and privacy-safe public status. University ID, Password, show/hide, and Sign in stack in DOM order; activation, recovery, gateway, and support links follow. Labels remain visible during autofill. |
| 375 | The 320 order is retained with 44 CSS px controls. Sign in remains full width, show/hide stays attached to Password, and validation text wraps without shifting field labels. |
| 768 | A bounded login panel is the main column; the public status may occupy a labelled complementary panel. Keyboard order stays heading, status, summary, fields, command, then links. |
| 1024 | The form and non-sensitive help may form two columns, but validation remains immediately before the fields. No role-selection or second-factor region appears. |
| 1280 | The login panel remains at a readable maximum width; activation and recovery links align below Sign in only when labels fit. Public context never displaces errors. |
| 1920 | The centered 1280 composition gains whitespace only. Credentials, validation, pending state, and recovery remain grouped; secrets are not mirrored in side content. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-007/T087. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `AUTH-02-valid-v1` (valid) | success | visible University ID and Password labels, password-only authentication with no MFA/2FA, one pending request, accepted Student context, and server destination | password echo, staff role picker, client-granted role, OTP, authenticator, or second-factor prompt | Submit login once and follow the server destination | `AUTH-02-COMP-STATE-SUCCESS` | `AUTH-02-E2E-PRIMARY` |
| `AUTH-02-invalid-v1` (invalid) | validation-error | generic invalid-credential message, validation-summary focus, linked fields, and cleared password | account-existence confirmation, retained password, dashboard navigation, or success | Review both credentials and resubmit | `AUTH-02-COMP-STATE-VALIDATION-ERROR` | `AUTH-02-E2E-FAILURE` |
| `AUTH-02-locked-v1` (locked) | validation-error | locked safe reason plus recovery or support action | remaining-attempt enumeration, protected data, or successful navigation | Open recovery or support | `AUTH-02-COMP-STATE-VALIDATION-ERROR` | `AUTH-02-E2E-FAILURE` |
| `AUTH-02-rate-limited-v1` (rate-limited) | validation-error | rate-limited safe reason and server retry guidance | automatic repeated POSTs, success, or account disclosure | Wait, then explicitly resubmit | `AUTH-02-COMP-STATE-VALIDATION-ERROR` | `AUTH-02-E2E-FAILURE` |
| `AUTH-02-keyboard-error-focus-v1` (keyboard/error focus) | validation-error | autofill-safe visible labels, logical tab order, summary focus, and links to invalid fields | focus loss, placeholder-only labels, hidden errors, or pointer-only secret control | Tab through, submit, then follow the summary link | `AUTH-02-COMP-STATE-VALIDATION-ERROR` | `AUTH-02-E2E-FAILURE` |

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
