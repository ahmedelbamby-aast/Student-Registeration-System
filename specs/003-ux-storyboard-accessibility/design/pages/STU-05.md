# STU-05 Page Design Record

**Record version:** `1.0`<br>
**Approval status:** Approved by Ahmed ELbamby on 2026-07-13<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-014<br>
**Implementation task:** SPEC-014/T105<br>
**Design task:** SPEC-003/T167

This immutable Page Design Record version 1.0 is governed by
`page-design-record/1.1` and was approved by Ahmed ELbamby on 2026-07-13.
Approval authorizes design review only; it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-05",
  "routeTemplate": "/student/review",
  "pageName": "RegistrationReviewPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-014",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-012",
    "SPEC-014"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Review groups, credits, timetable, policy checks, and every blocker before atomic submission.",
  "informationHierarchy": [
    "Authenticated AppShell context",
    "Selected-group summary",
    "Chronological schedule",
    "Credit and policy checks",
    "Conflict and blocker summary",
    "Confirmation",
    "Submit result"
  ],
  "responsiveWireframes": {
    "320": "Collapsed navigation and heading precede selected groups, chronological schedule, credits/policy checks, every blocker, Edit plan, then Review and submit. The confirmation dialog appears only after accepted validation; disabled submit reasons wrap in full.",
    "375": "The 320 order is retained with 44 CSS px Edit, Review, Cancel, and Confirm controls. Conflict intervals and policy/window reasons remain before the disabled command and are never collapsed.",
    "768": "Compact navigation precedes a two-column selected-group and check summary; the schedule list spans both columns. Validation/conflict panels and Edit precede Review and submit in DOM order.",
    "1024": "Persistent navigation, main review summary, and bounded credit/policy sidebar are allowed. The confirmation dialog is modal, labelled, traps focus, and does not obscure validation.",
    "1280": "The review uses bounded columns with the chronological schedule and policy checks visible together. Destructive/submit confirmation remains separated; every blocker describes the disabled trigger.",
    "1920": "A centered maximum-width shell preserves the 1280 hierarchy. Surplus width adds gutters only; confirmation, result, blockers, and recovery actions stay attached to the owning region."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "EntityCard",
    "ScheduleList",
    "ConflictPanel",
    "StatusBadge",
    "Alert",
    "ValidationSummary",
    "ConfirmationDialog",
    "Button",
    "AppLink",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/student/terms/{termId}/registration-plan",
    "POST /api/student/terms/{termId}/registration-plan/validate",
    "POST /api/student/terms/{termId}/registrations"
  ],
  "actions": [
    "Refresh GET /api/student/terms/{termId}/registration-plan",
    "Validate through POST /api/student/terms/{termId}/registration-plan/validate",
    "Open the client confirmation dialog only after accepted validation",
    "Confirm one POST /api/student/terms/{termId}/registrations",
    "Navigate to STU-04 to edit the plan",
    "Return to STU-01 dashboard"
  ],
  "navigationTransitions": [
    "Edit plan -> STU-04",
    "Submission -> STU-06",
    "Rejected stale plan -> STU-04",
    "Dashboard -> STU-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STU-05-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Registration review heading with stable landmarks",
        "Named pending status and disabled duplicate command",
        "No duplicate request, partial outcome, or false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [
        "Wait for or safely recover the one authoritative registration result",
        "Wait for or safely recover the one result"
      ],
      "testIds": [
        "STU-05-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STU-05-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "No reviewable registration plan",
        "Add/edit plan action to STU-04",
        "Submit absent or disabled",
        "No empty plan accepted as success"
      ],
      "expectedFocusTarget": "Registration review heading",
      "liveRegion": "none",
      "nextActions": [
        "Open STU-04"
      ],
      "testIds": [
        "STU-05-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STU-05-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Latest server plan version and selected groups",
        "Chronological schedule plus credit and policy checks",
        "Accepted validation before confirmation",
        "Atomic accepted result only when serverAccepted is true",
        "Minimum journeys: valid; hard conflict; policy changed; window closed; double submit"
      ],
      "expectedFocusTarget": "Registration review heading on load; confirmation focuses its heading; cancel restores the trigger; accepted submit moves to STU-06 result heading",
      "liveRegion": "polite",
      "nextActions": [
        "Open confirmation after accepted validation",
        "Confirm one registration POST",
        "Navigate to STU-04 to edit",
        "Open confirmation and confirm one registration POST"
      ],
      "testIds": [
        "STU-05-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STU-05-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary lists every hard conflict or blocker",
        "Red X and Conflict with named groups and exact overlap interval when present",
        "Disabled Review/submit control describes every blocker",
        "No override, partial submission, or success",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field, group, or blocker",
      "liveRegion": "assertive",
      "nextActions": [
        "Return to STU-04 and resolve blockers",
        "Revalidate after changes",
        "Return to STU-04 and resolve"
      ],
      "testIds": [
        "STU-05-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STU-05-service-error-v1",
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
        "STU-05-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STU-05-unauthorized-v1",
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
        "STU-05-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STU-05-session-expired-v1",
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
        "STU-05-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STU-05-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change or uncertain-result heading",
        "POLICY_CHANGED, WINDOW_CLOSED, PLAN_CHANGED, GROUP_FULL, or STALE_VERSION",
        "Authoritative refresh/review action",
        "Cached authorization, plan, result, or success is not reused",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "Refresh GET registration-plan",
        "Review differences and revalidate"
      ],
      "testIds": [
        "STU-05-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STU-05-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline status for Registration review heading",
        "Browser connectivity state and safe retry guidance",
        "No queued command, cached result, partial outcome, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry result lookup or plan GET when online; never claim queued submission"
      ],
      "testIds": [
        "STU-05-COMP-STATE-OFFLINE"
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
    "Registration review heading",
    "Selected-group summary",
    "Chronological schedule list",
    "Credit and policy checks",
    "Validation summary and ConflictPanel blockers",
    "Edit plan link",
    "Review and submit trigger with aria-describedby blockers",
    "Confirmation dialog heading after it opens",
    "Cancel confirmation button",
    "Confirm registration button",
    "On Cancel or Escape restore Review and submit trigger",
    "On rejection focus validation summary; on acceptance focus STU-06 result heading",
    "Safe support/reference link"
  ],
  "testIds": [
    "STU-05-CONTRACT-T168",
    "STU-05-COMP-T169",
    "STU-05-E2E-PRIMARY",
    "STU-05-E2E-FAILURE",
    "STU-05-A11Y-T170",
    "STU-05-VIS-T171"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1",
    "SPEC-012": "not-pinned",
    "SPEC-014": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "1.0"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Collapsed navigation and heading precede selected groups, chronological schedule, credits/policy checks, every blocker, Edit plan, then Review and submit. The confirmation dialog appears only after accepted validation; disabled submit reasons wrap in full. |
| 375 | The 320 order is retained with 44 CSS px Edit, Review, Cancel, and Confirm controls. Conflict intervals and policy/window reasons remain before the disabled command and are never collapsed. |
| 768 | Compact navigation precedes a two-column selected-group and check summary; the schedule list spans both columns. Validation/conflict panels and Edit precede Review and submit in DOM order. |
| 1024 | Persistent navigation, main review summary, and bounded credit/policy sidebar are allowed. The confirmation dialog is modal, labelled, traps focus, and does not obscure validation. |
| 1280 | The review uses bounded columns with the chronological schedule and policy checks visible together. Destructive/submit confirmation remains separated; every blocker describes the disabled trigger. |
| 1920 | A centered maximum-width shell preserves the 1280 hierarchy. Surplus width adds gutters only; confirmation, result, blockers, and recovery actions stay attached to the owning region. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-014/T104. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STU-05-valid-v1` (valid) | success | latest valid plan, selected groups, schedule, credits/policies, accepted validation, confirmation, and one submit | confirmation before validation, partial write, duplicate POST, or success before server acceptance | Open confirmation and confirm one registration POST | `STU-05-COMP-STATE-SUCCESS` | `STU-05-E2E-PRIMARY` |
| `STU-05-hard-conflict-v1` (hard conflict) | validation-error | Red X plus Conflict, named subjects/groups, exact interval, manual-resolution link, all blockers, and disabled submit | override, hidden blocker, enabled confirmation/submit, or partial success | Return to STU-04 and resolve | `STU-05-COMP-STATE-VALIDATION-ERROR` | `STU-05-E2E-FAILURE` |
| `STU-05-policy-changed-v1` (policy changed) | stale | POLICY_CHANGED, changed policy summary, refreshed plan requirement, and disabled submit | cached policy acceptance, automatic submit, or success | Refresh, review, and revalidate | `STU-05-COMP-STATE-STALE` | `STU-05-E2E-FAILURE` |
| `STU-05-window-closed-v1` (window closed) | stale | WINDOW_CLOSED, server close time/timezone, support path, and disabled submit | browser clock authority, enabled submit, queued registration, or success | Return to dashboard or support | `STU-05-COMP-STATE-STALE` | `STU-05-E2E-FAILURE` |
| `STU-05-double-submit-v1` (double submit) | loading | pending and disabled Confirm registration, one idempotent request, and one authoritative result | two POSTs, two results, re-enabled command while pending, or partial outcome | Wait for or safely recover the one result | `STU-05-COMP-STATE-LOADING` | `STU-05-E2E-FAILURE` |

## Review notes

- Six annotated layouts are specific to STU-05 and preserve DOM, keyboard,
  action, state, and information order without hiding reasons or boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; commands
  name their endpoint and navigation/browser-only actions imply no missing write.
- Background stale/offline/service updates preserve focus; submitted validation
  and modal/navigation focus/restoration follow focusOrder and the reason map.
 Approval does not promote the route beyond `design-only`.
- Ahmed ELbamby approved immutable record version 1.0 on 2026-07-13, so its
  design task may close. Approval does not promote the route beyond `design-only`.
