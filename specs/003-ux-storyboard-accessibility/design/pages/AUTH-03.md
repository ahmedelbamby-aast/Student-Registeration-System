# AUTH-03 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-007<br>
**Implementation task:** SPEC-007/T090<br>
**Design task:** SPEC-003/T132

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "AUTH-03",
  "routeTemplate": "/student/activate",
  "pageName": "StudentActivationPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-007",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007",
    "SPEC-008"
  ],
  "actors": [
    "Pre-provisioned Student"
  ],
  "purpose": "Activate a matching generated University identity using University ID, PIN, and a new password.",
  "informationHierarchy": [
    "Activation heading and explanation",
    "University ID, PIN, and new-password fields",
    "Validation summary",
    "Activation result",
    "Login, recovery, and back links"
  ],
  "responsiveWireframes": {
    "320": "Activation explanation and public status precede one form. University ID, issued PIN, new password, confirmation, secret toggles, and Activate account stack; login, recovery, and gateway links follow.",
    "375": "The 320 order is retained with 44 CSS px targets and full-width Activate account. Password requirements and mismatch text wrap below their owning fields.",
    "768": "The bounded activation form is the main column; password guidance may follow in a complementary panel. Validation remains before the first invalid field in DOM order.",
    "1024": "Form and non-sensitive activation guidance may use two columns. PIN and password controls remain together, and no issued credential is copied into guidance.",
    "1280": "The form keeps a readable maximum width; the result appears directly after Activate account and login/recovery actions remain below that result.",
    "1920": "The centered 1280 composition gains whitespace only. Secret controls, requirements, pending state, and recovery stay in one sequence; no secret is duplicated."
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
    "POST /api/auth/student/activate"
  ],
  "actions": [
    "Submit POST /api/auth/student/activate",
    "Toggle PIN and password visibility",
    "Open Student login",
    "Open recovery",
    "Return to gateway",
    "Retry GET /api/public/context"
  ],
  "navigationTransitions": [
    "Successful activation -> AUTH-02",
    "Student login -> AUTH-02",
    "Recovery -> AUTH-05",
    "Gateway -> AUTH-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "AUTH-03-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Student activation heading without replacing landmarks",
        "Named pending status and disabled command",
        "No duplicate request or false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [
        "Wait for the single authoritative activation result",
        "Wait for the single authoritative result"
      ],
      "testIds": [
        "AUTH-03-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "AUTH-03-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "empty is not applicable on AUTH-03: The activation form is always rendered; account existence is never an empty result."
      ],
      "expectedFocusTarget": "Student activation heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-03-COMP-STATE-EMPTY"
      ],
      "reason": "The activation form is always rendered; account existence is never an empty result."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "AUTH-03-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Server-accepted first-use activation",
        "One authoritative activation result",
        "Server-provided Student login or authorized destination",
        "Issued PIN is a first-use activation credential, never an OTP, MFA, or 2FA factor",
        "Issued PIN and new password are not echoed",
        "Minimum journeys: valid; unknown; mismatch; activated; locked/rate-limited; double submit",
        "Success only when serverAccepted is true",
        "The generated PIN is a first-use activation credential and not a second factor"
      ],
      "expectedFocusTarget": "Activation result heading, then the server-provided login or destination action",
      "liveRegion": "polite",
      "nextActions": [
        "Open Student login or follow the server-authorized destination",
        "Submit activation once and follow the server action"
      ],
      "testIds": [
        "AUTH-03-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "AUTH-03-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary linked to University ID, PIN, new password, or confirmation",
        "Unknown, mismatch, activated, locked, or rate-limited safe reason",
        "No account enumeration and no false success",
        "Pending activation disables duplicate submission",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field",
      "liveRegion": "assertive",
      "nextActions": [
        "Focus the linked invalid field",
        "Open recovery or resubmit POST /api/auth/student/activate when permitted"
      ],
      "testIds": [
        "AUTH-03-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "AUTH-03-service-error-v1",
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
        "AUTH-03-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "not-applicable",
      "fixture": "AUTH-03-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "unauthorized is not applicable on AUTH-03: Activation rejection is a privacy-safe validation outcome, not protected-resource authorization."
      ],
      "expectedFocusTarget": "Student activation heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-03-COMP-STATE-UNAUTHORIZED"
      ],
      "reason": "Activation rejection is a privacy-safe validation outcome, not protected-resource authorization."
    },
    {
      "state": "session-expired",
      "applicability": "not-applicable",
      "fixture": "AUTH-03-session-expired-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "session-expired is not applicable on AUTH-03: No authenticated session is required for first-use activation."
      ],
      "expectedFocusTarget": "Student activation heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-03-COMP-STATE-SESSION-EXPIRED"
      ],
      "reason": "No authenticated session is required for first-use activation."
    },
    {
      "state": "stale",
      "applicability": "not-applicable",
      "fixture": "AUTH-03-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "stale is not applicable on AUTH-03: Used, already-activated, or changed PIN outcomes are validation failures and are never retried from cache."
      ],
      "expectedFocusTarget": "Student activation heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-03-COMP-STATE-STALE"
      ],
      "reason": "Used, already-activated, or changed PIN outcomes are validation failures and are never retried from cache."
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "AUTH-03-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline state for Student activation heading",
        "Browser connectivity status and retry guidance",
        "No queued, cached, duplicate, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry POST /api/auth/student/activate only when online"
      ],
      "testIds": [
        "AUTH-03-COMP-STATE-OFFLINE"
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
    "Student activation heading",
    "Activation explanation and public service status",
    "Validation summary after a submitted validation failure",
    "University ID field",
    "Issued one-time PIN field",
    "New password field",
    "Confirm new password field",
    "Show or hide secret buttons adjacent to their fields",
    "Activate account button",
    "Student login link",
    "Account recovery link",
    "Return to gateway link",
    "Safe support/reference link supplied by the server"
  ],
  "testIds": [
    "AUTH-03-CONTRACT-T133",
    "AUTH-03-COMP-T134",
    "AUTH-03-E2E-PRIMARY",
    "AUTH-03-E2E-FAILURE",
    "AUTH-03-A11Y-T135",
    "AUTH-03-VIS-T136"
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
| 320 | Activation explanation and public status precede one form. University ID, issued PIN, new password, confirmation, secret toggles, and Activate account stack; login, recovery, and gateway links follow. |
| 375 | The 320 order is retained with 44 CSS px targets and full-width Activate account. Password requirements and mismatch text wrap below their owning fields. |
| 768 | The bounded activation form is the main column; password guidance may follow in a complementary panel. Validation remains before the first invalid field in DOM order. |
| 1024 | Form and non-sensitive activation guidance may use two columns. PIN and password controls remain together, and no issued credential is copied into guidance. |
| 1280 | The form keeps a readable maximum width; the result appears directly after Activate account and login/recovery actions remain below that result. |
| 1920 | The centered 1280 composition gains whitespace only. Secret controls, requirements, pending state, and recovery stay in one sequence; no secret is duplicated. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-007/T089. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `AUTH-03-valid-v1` (valid) | success | all four visible field labels, the generated PIN identified only as a first-use activation credential, one accepted result, and server-provided login or destination | treating the PIN as OTP/2FA, PIN/password echo, plaintext storage, MFA flow, or client-created identity | Submit activation once and follow the server action | `AUTH-03-COMP-STATE-SUCCESS` | `AUTH-03-E2E-PRIMARY` |
| `AUTH-03-unknown-v1` (unknown) | validation-error | privacy-safe generic unknown outcome with recovery or support | confirmation that the University ID is absent or synthetic seed detail | Review identifier or open recovery | `AUTH-03-COMP-STATE-VALIDATION-ERROR` | `AUTH-03-E2E-FAILURE` |
| `AUTH-03-mismatch-v1` (mismatch) | validation-error | password mismatch in summary and beside confirmation with summary focus | accepted activation, success navigation, or cleared identifier and PIN before correction | Follow summary link, correct confirmation, then submit | `AUTH-03-COMP-STATE-VALIDATION-ERROR` | `AUTH-03-E2E-FAILURE` |
| `AUTH-03-activated-v1` (activated) | validation-error | already activated safe outcome with Student login and recovery | second activation, PIN disclosure, or authentication from a used PIN | Open Student login or recovery | `AUTH-03-COMP-STATE-VALIDATION-ERROR` | `AUTH-03-E2E-FAILURE` |
| `AUTH-03-locked-rate-limited-v1` (locked/rate-limited) | validation-error | locked/rate-limited stable reason and server retry or support guidance | automatic retries, remaining-attempt enumeration, or success | Wait or open recovery/support | `AUTH-03-COMP-STATE-VALIDATION-ERROR` | `AUTH-03-E2E-FAILURE` |
| `AUTH-03-double-submit-v1` (double submit) | loading | pending and disabled Activate account with exactly one result | two POST requests, duplicate results, or enabled command while pending | Wait for the single authoritative result | `AUTH-03-COMP-STATE-LOADING` | `AUTH-03-E2E-PRIMARY` |

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
