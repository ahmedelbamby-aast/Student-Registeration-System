# STU-06 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-015<br>
**Implementation task:** SPEC-015/T051<br>
**Design task:** SPEC-003/T172

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-06",
  "routeTemplate": "/student/registration/result/{id}",
  "pageName": "RegistrationResultPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-015",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-014",
    "SPEC-015"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Present the atomic accepted or rejected result, no-partial semantics, receipt, schedule, and reference.",
  "informationHierarchy": [
    "Atomic result heading",
    "Reference and term",
    "Receipt and registered-group details",
    "Calendar and chronological schedule",
    "Decision snapshot",
    "Recovery and next actions"
  ],
  "responsiveWireframes": {
    "320": "Collapsed navigation and atomic-result heading precede status/reference, ReceiptSummary or rejection, chronological schedule, optional equivalent calendar, decision snapshot, and recovery/next links. No partial outcome is visually split.",
    "375": "The 320 order is retained with 44 CSS px Retry lookup and navigation controls. Receipt values, reason codes, reference IDs, and no-partial text wrap without horizontal scroll.",
    "768": "Compact navigation precedes a receipt/rejection main column and bounded decision/reference summary. Chronological list remains primary and calendar equivalence follows the same canonical items.",
    "1024": "Persistent navigation, main result content, and decision snapshot sidebar are allowed. Retry lost-response remains adjacent to uncertain status and no protected identifier enters denied state.",
    "1280": "Accepted receipt and schedule may use bounded columns; rejected/no-partial reason remains one atomic region. History, edit, and dashboard links follow the authoritative outcome.",
    "1920": "A centered maximum-width shell preserves the 1280 hierarchy. Surplus width adds gutters only; receipt, schedule equivalence, decision snapshot, and recovery stay in one reading order."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "ReceiptSummary",
    "ScheduleCalendar",
    "ScheduleList",
    "StatusBadge",
    "Alert",
    "Button",
    "AppLink",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
    "GET /api/student/registrations/{submissionId}"
  ],
  "actions": [
    "Recover uncertain delivery through GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
    "Load owned receipt through GET /api/student/registrations/{submissionId}",
    "Open STU-07 registration history",
    "Navigate to STU-04 to edit a rejected plan",
    "Return to STU-01 dashboard"
  ],
  "navigationTransitions": [
    "Retry stays on STU-06",
    "History -> STU-07",
    "Edit plan -> STU-04",
    "Dashboard -> STU-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STU-06-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Atomic registration result heading with stable landmarks",
        "Named pending status and disabled duplicate command",
        "No duplicate request, partial outcome, or false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "STU-06-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "not-applicable",
      "fixture": "STU-06-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "empty is not applicable on STU-06: A result route resolves to accepted, rejected, uncertain, denied, or service error; an owned result is never a legitimate empty collection."
      ],
      "expectedFocusTarget": "Atomic registration result heading",
      "liveRegion": "none",
      "nextActions": [],
      "testIds": [
        "STU-06-COMP-STATE-EMPTY"
      ],
      "reason": "A result route resolves to accepted, rejected, uncertain, denied, or service error; an owned result is never a legitimate empty collection."
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STU-06-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Authoritative server-returned atomic registration outcome",
        "Receipt reference, term, registered groups, and decision snapshot",
        "Equivalent calendar and chronological list",
        "No partial registration and no success unless the server-returned result is accepted",
        "Minimum journeys: accepted; rejected/no partial result; lost-response recovery; denied"
      ],
      "expectedFocusTarget": "Atomic result heading after navigation; view switching restores the corresponding meeting item",
      "liveRegion": "polite",
      "nextActions": [
        "Open STU-07 history",
        "Return to STU-01",
        "Open history or dashboard"
      ],
      "testIds": [
        "STU-06-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STU-06-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "Rejected outcome and stable reason codes",
        "Explicit no partial result",
        "No receipt presented as accepted",
        "Edit plan and safe retry/support actions",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Atomic registration result heading",
      "liveRegion": "none",
      "nextActions": [
        "Navigate to STU-04 to edit",
        "Open safe support/reference path",
        "Open STU-04 or support"
      ],
      "testIds": [
        "STU-06-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STU-06-service-error-v1",
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
        "STU-06-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STU-06-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Access-denied heading",
        "UNAUTHORIZED or FORBIDDEN preserved exactly",
        "No protected identifier, receipt, account, cached content, or unauthorized action"
      ],
      "expectedFocusTarget": "Access-denied heading after navigation",
      "liveRegion": "assertive",
      "nextActions": [
        "Return to authorized home or sign in",
        "Return to authorized home"
      ],
      "testIds": [
        "STU-06-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STU-06-session-expired-v1",
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
        "STU-06-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STU-06-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change or uncertain-result heading",
        "STALE_VERSION or uncertain delivery requiring idempotent result lookup",
        "Authoritative refresh/review action",
        "Cached authorization, plan, result, or success is not reused",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "GET result by clientRequestId",
        "Then GET the owned submission when identified",
        "GET by request ID, then load owned submission"
      ],
      "testIds": [
        "STU-06-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STU-06-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline status for Atomic registration result heading",
        "Browser connectivity state and safe retry guidance",
        "No queued command, cached result, partial outcome, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry idempotent result lookup when online; never claim queued success"
      ],
      "testIds": [
        "STU-06-COMP-STATE-OFFLINE"
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
    "Atomic registration result heading",
    "Accepted, rejected, uncertain, or denied status and reference",
    "ReceiptSummary when accepted or validation summary when rejected",
    "Calendar/List view controls and chronological meeting items",
    "Decision snapshot",
    "Retry lost-response lookup button when applicable",
    "Registration history link",
    "Edit rejected plan link when applicable",
    "Dashboard link",
    "Safe support/reference link"
  ],
  "testIds": [
    "STU-06-CONTRACT-T173",
    "STU-06-COMP-T174",
    "STU-06-E2E-PRIMARY",
    "STU-06-E2E-FAILURE",
    "STU-06-A11Y-T175",
    "STU-06-VIS-T176"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-014": "not-pinned",
    "SPEC-015": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Collapsed navigation and atomic-result heading precede status/reference, ReceiptSummary or rejection, chronological schedule, optional equivalent calendar, decision snapshot, and recovery/next links. No partial outcome is visually split. |
| 375 | The 320 order is retained with 44 CSS px Retry lookup and navigation controls. Receipt values, reason codes, reference IDs, and no-partial text wrap without horizontal scroll. |
| 768 | Compact navigation precedes a receipt/rejection main column and bounded decision/reference summary. Chronological list remains primary and calendar equivalence follows the same canonical items. |
| 1024 | Persistent navigation, main result content, and decision snapshot sidebar are allowed. Retry lost-response remains adjacent to uncertain status and no protected identifier enters denied state. |
| 1280 | Accepted receipt and schedule may use bounded columns; rejected/no-partial reason remains one atomic region. History, edit, and dashboard links follow the authoritative outcome. |
| 1920 | A centered maximum-width shell preserves the 1280 hierarchy. Surplus width adds gutters only; receipt, schedule equivalence, decision snapshot, and recovery stay in one reading order. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-015/T050. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STU-06-accepted-v1` (accepted) | success | accepted atomic outcome, receipt/reference, groups, decision snapshot, and equivalent calendar/list | rejected reason, partial result, missing reference, or client-invented success | Open history or dashboard | `STU-06-COMP-STATE-SUCCESS` | `STU-06-E2E-PRIMARY` |
| `STU-06-rejected-no-partial-result-v1` (rejected/no partial result) | validation-error | rejected stable reason, explicit no partial result, no accepted receipt, and Edit plan | partial enrollment, accepted badge, hidden reason, or resubmit from result page | Open STU-04 or support | `STU-06-COMP-STATE-VALIDATION-ERROR` | `STU-06-E2E-FAILURE` |
| `STU-06-lost-response-recovery-v1` (lost-response recovery) | stale | uncertain delivery text, clientRequestId-safe lookup, one recovered authoritative result, and reference | assumed failure/success, duplicate registration POST, or endless blind retry | GET by request ID, then load owned submission | `STU-06-COMP-STATE-STALE` | `STU-06-E2E-FAILURE` |
| `STU-06-denied-v1` (denied) | unauthorized | safe access-denied heading and authorized-home action | submission ID, receipt, student/group data, or ownership oracle | Return to authorized home | `STU-06-COMP-STATE-UNAUTHORIZED` | `STU-06-E2E-FAILURE` |

## Review notes

- Six annotated layouts are specific to STU-06 and preserve DOM, keyboard,
  action, state, and information order without hiding reasons or boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; commands
  name their endpoint and navigation/browser-only actions imply no missing write.
- Background stale/offline/service updates preserve focus; submitted validation
  and modal/navigation focus/restoration follow focusOrder and the reason map.
 Approval does not promote the route beyond `design-only`.
- Ahmed ELbamby approved immutable record version 1.0 on 2026-07-13, so its
  design task may close. Approval does not promote the route beyond `design-only`.
