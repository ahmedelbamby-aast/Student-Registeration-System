# AUTH-05 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-007<br>
**Implementation task:** SPEC-007/T094<br>
**Design task:** SPEC-003/T142

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "AUTH-05",
  "routeTemplate": "/account/recovery",
  "pageName": "AccountRecoveryPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-007",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-007"
  ],
  "actors": [
    "Student",
    "Admin",
    "Lecturer",
    "Teaching Assistant"
  ],
  "purpose": "Request or complete account recovery with enumeration-resistant confirmation.",
  "informationHierarchy": [
    "Recovery heading and instructions",
    "Identifier or token and new-password fields",
    "Validation summary",
    "Generic confirmation or result",
    "Login and back links"
  ],
  "responsiveWireframes": {
    "320": "The recovery heading and enumeration-safe instructions precede the active step. Request fields or code/new-password fields stack with their command; generic result, login, gateway, and support links follow.",
    "375": "The 320 sequence remains with full-width request/complete commands and 44 CSS px targets. Requirements and expired/used messages wrap below the active step.",
    "768": "A bounded two-step recovery panel is the main column; non-sensitive help may follow as complementary content. Hidden steps are not focusable.",
    "1024": "The active form and guidance may use two columns while validation stays before active fields. Sent confirmation never reveals account existence.",
    "1280": "The active step keeps a readable maximum width with Student/Staff login actions below the result. Only the active endpoint command is primary.",
    "1920": "The centered 1280 composition gains whitespace only. Codes and passwords remain in labelled fields and are not duplicated in side content."
  },
  "components": [
    "FormField",
    "ValidationSummary",
    "Button",
    "AppLink",
    "Alert",
    "StatePanel"
  ],
  "dataContracts": [
    "POST /api/auth/recovery/request",
    "POST /api/auth/recovery/complete"
  ],
  "actions": [
    "Submit POST /api/auth/recovery/request",
    "Submit POST /api/auth/recovery/complete",
    "Toggle new-password visibility",
    "Open Student login",
    "Open Staff login",
    "Return to gateway"
  ],
  "navigationTransitions": [
    "Recovery stays on AUTH-05",
    "Student login -> AUTH-02",
    "Staff login -> AUTH-04",
    "Gateway -> AUTH-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "AUTH-05-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Account recovery heading without replacing landmarks",
        "Named pending status and disabled command",
        "No duplicate request or false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "AUTH-05-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "AUTH-05-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "empty is not applicable on AUTH-05: Recovery is a form workflow; sent, expired, and used outcomes are not empty collections."
      ],
      "expectedFocusTarget": "Account recovery heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-05-COMP-STATE-EMPTY"
      ],
      "reason": "Recovery is a form workflow; sent, expired, and used outcomes are not empty collections."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "AUTH-05-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Generic request confirmation or server-accepted password reset",
        "Same request disclosure regardless of account existence",
        "Student login and Staff login next actions",
        "Recovery code and new password are not echoed",
        "Minimum journeys: sent; expired; used; rate-limited; success; enumeration resistance",
        "Success only when serverAccepted is true"
      ],
      "expectedFocusTarget": "Generic confirmation heading after request or recovery-complete heading after accepted completion",
      "liveRegion": "polite",
      "nextActions": [
        "Open Student login",
        "Open Staff login"
      ],
      "testIds": [
        "AUTH-05-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "AUTH-05-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary linked to identifier, recovery code, or new-password field",
        "Expired, used, or rate-limited safe reason without enumeration",
        "Password mismatch/policy text adjacent to fields",
        "No success while serverAccepted is false",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field",
      "liveRegion": "assertive",
      "nextActions": [
        "Request a new recovery flow when permitted",
        "Correct the linked field and resubmit the active endpoint"
      ],
      "testIds": [
        "AUTH-05-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "AUTH-05-service-error-v1",
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
        "AUTH-05-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "not-applicable",
      "fixture": "AUTH-05-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "unauthorized is not applicable on AUTH-05: Recovery responses are enumeration-safe validation/service outcomes and expose no protected resource."
      ],
      "expectedFocusTarget": "Account recovery heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-05-COMP-STATE-UNAUTHORIZED"
      ],
      "reason": "Recovery responses are enumeration-safe validation/service outcomes and expose no protected resource."
    },
    {
      "state": "session-expired",
      "applicability": "not-applicable",
      "fixture": "AUTH-05-session-expired-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "session-expired is not applicable on AUTH-05: Recovery is available without an authenticated session."
      ],
      "expectedFocusTarget": "Account recovery heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-05-COMP-STATE-SESSION-EXPIRED"
      ],
      "reason": "Recovery is available without an authenticated session."
    },
    {
      "state": "stale",
      "applicability": "not-applicable",
      "fixture": "AUTH-05-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "stale is not applicable on AUTH-05: Expired or used proofs are validation failures and must be replaced, not refreshed as stale data."
      ],
      "expectedFocusTarget": "Account recovery heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "AUTH-05-COMP-STATE-STALE"
      ],
      "reason": "Expired or used proofs are validation failures and must be replaced, not refreshed as stale data."
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "AUTH-05-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline state for Account recovery heading",
        "Browser connectivity status and retry guidance",
        "No queued, cached, duplicate, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry only the active recovery endpoint when online"
      ],
      "testIds": [
        "AUTH-05-COMP-STATE-OFFLINE"
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
    "Account recovery heading",
    "Privacy and recovery instructions",
    "Validation summary after a submitted validation failure",
    "Recovery identifier field in request step",
    "Request recovery button",
    "Recovery proof or code field in completion step",
    "New password field",
    "Confirm new password field",
    "Show or hide password buttons adjacent to their fields",
    "Complete recovery button",
    "Student login link",
    "Staff login link",
    "Return to gateway link",
    "Safe support/reference link supplied by the server"
  ],
  "testIds": [
    "AUTH-05-CONTRACT-T143",
    "AUTH-05-COMP-T144",
    "AUTH-05-E2E-PRIMARY",
    "AUTH-05-E2E-FAILURE",
    "AUTH-05-A11Y-T145",
    "AUTH-05-VIS-T146"
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
| 320 | The recovery heading and enumeration-safe instructions precede the active step. Request fields or code/new-password fields stack with their command; generic result, login, gateway, and support links follow. |
| 375 | The 320 sequence remains with full-width request/complete commands and 44 CSS px targets. Requirements and expired/used messages wrap below the active step. |
| 768 | A bounded two-step recovery panel is the main column; non-sensitive help may follow as complementary content. Hidden steps are not focusable. |
| 1024 | The active form and guidance may use two columns while validation stays before active fields. Sent confirmation never reveals account existence. |
| 1280 | The active step keeps a readable maximum width with Student/Staff login actions below the result. Only the active endpoint command is primary. |
| 1920 | The centered 1280 composition gains whitespace only. Codes and passwords remain in labelled fields and are not duplicated in side content. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-007/T093. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `AUTH-05-sent-v1` (sent) | success | generic request confirmation and safe next steps after one request | account existence, unapproved delivery details, or plaintext code | Continue only with a received proof | `AUTH-05-COMP-STATE-SUCCESS` | `AUTH-05-E2E-PRIMARY` |
| `AUTH-05-expired-v1` (expired) | validation-error | expired-proof reason, summary focus, and request-new action | password reset success, automatic proof reuse, or protected detail | Request a new recovery flow | `AUTH-05-COMP-STATE-VALIDATION-ERROR` | `AUTH-05-E2E-FAILURE` |
| `AUTH-05-used-v1` (used) | validation-error | used-proof reason and request-new action | second reset, code echo, or authenticated session | Request a new recovery flow | `AUTH-05-COMP-STATE-VALIDATION-ERROR` | `AUTH-05-E2E-FAILURE` |
| `AUTH-05-rate-limited-v1` (rate-limited) | validation-error | rate-limited safe reason and server retry/support guidance | automatic repeated requests, existence disclosure, or false sent success | Wait, then explicitly request again | `AUTH-05-COMP-STATE-VALIDATION-ERROR` | `AUTH-05-E2E-FAILURE` |
| `AUTH-05-success-v1` (success) | success | one accepted reset result and named Student/Staff login actions | echoed password/code, automatic role selection, second factor, or duplicate completion | Open the correct login | `AUTH-05-COMP-STATE-SUCCESS` | `AUTH-05-E2E-PRIMARY` |
| `AUTH-05-enumeration-resistance-v1` (enumeration resistance) | success | equivalent request confirmation content and UI shape for known and unknown identifiers | known/unknown wording, distinct navigation, identifiers, or account-state disclosure | Return to login/gateway or generic support | `AUTH-05-COMP-STATE-SUCCESS` | `AUTH-05-E2E-PRIMARY` |

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
