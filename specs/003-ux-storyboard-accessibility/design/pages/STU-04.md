# STU-04 Page Design Record

**Record version:** `draft/1.0`<br>
**Approval status:** Pending Ahmed ELbamby review<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-012<br>
**Implementation task:** SPEC-012/T043<br>
**Design task:** SPEC-003/T162

This complete design draft is governed by `page-design-record/1.1-draft`. It may be
reviewed as design evidence, but it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-04",
  "routeTemplate": "/student/schedule",
  "pageName": "ScheduleBuilderPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-012",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-012",
    "SPEC-013"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Edit the registration plan with server-sourced effective credit limits, expose calendar/list equivalence, show conflicts, and present ranked alternatives.",
  "informationHierarchy": [
    "Authenticated AppShell and total/default/effective credit summary with policy reasons",
    "Selected groups",
    "Calendar and chronological list",
    "Conflict and blocker panel",
    "Recommendations",
    "Manual resolution actions",
    "Continue action"
  ],
  "responsiveWireframes": {
    "320": "Collapsed navigation and heading precede the server total, default-target-18, and effective-maximum-12-or-18 credit summary with safe policy/source reasons, selected groups, chronological ScheduleList, labelled calendar region, ConflictPanel, ranked alternatives, manual actions, and Continue last.",
    "375": "The 320 order is retained with 44 CSS px view, change, remove, recommendation, and Continue controls. The effective maximum and load reason remain visible; Red X plus Conflict, named groups, exact overlap intervals, and every blocker wrap in full.",
    "768": "Compact navigation precedes the effective credit summary, a selected-group column, and schedule column; the chronological list remains primary/readable and calendar is equivalent. ConflictPanel spans both columns before ranked alternatives and Continue.",
    "1024": "Persistent navigation, main calendar/list, and bounded plan/load summary are allowed without DOM reordering. Every load/conflict/manual action precedes Continue, and no sticky control covers blockers or focus.",
    "1280": "The schedule and plan summary use bounded columns; default 18, effective 12/18, policy reasons, and ranked alternatives remain explicit. Continue stays disabled and described while any load or conflict blocker remains.",
    "1920": "A centered maximum-width shell preserves equivalent calendar/list data and the 1280 action order. Surplus width adds gutters only; no effective limit, load reason, overlap detail, blocker, recommendation, or manual action is detached."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "ValidationSummary",
    "ScheduleCalendar",
    "ScheduleList",
    "ConflictPanel",
    "GroupCard",
    "CapacityIndicator",
    "StatusBadge",
    "Alert",
    "Button",
    "AppLink",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/student/terms/{termId}/registration-plan",
    "PUT /api/student/terms/{termId}/registration-plan",
    "POST /api/student/terms/{termId}/registration-plan/validate",
    "POST /api/student/terms/{termId}/registration-plan/recommendations",
    "PUT /api/student/terms/{termId}/registration-plan/recommended-option"
  ],
  "actions": [
    "Remove or change a group through PUT /api/student/terms/{termId}/registration-plan",
    "Validate through POST /api/student/terms/{termId}/registration-plan/validate",
    "Request ranked alternatives through POST /api/student/terms/{termId}/registration-plan/recommendations",
    "Apply a ranked option through PUT /api/student/terms/{termId}/registration-plan/recommended-option",
    "Continue to STU-05 only after server validation accepts every blocker"
  ],
  "navigationTransitions": [
    "Add or change -> STU-02 or STU-03",
    "Plan edits stay on STU-04",
    "Valid plan -> STU-05",
    "Dashboard -> STU-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STU-04-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Schedule builder heading with stable landmarks",
        "Named progress status and disabled duplicate command",
        "No false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [
        "Wait for the authoritative validation or recommendation result",
        "Wait or use only contract-supported cancel"
      ],
      "testIds": [
        "STU-04-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STU-04-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "Empty registration plan",
      "0 total credits, default target 18, and server-returned effective maximum 12 or 18 with safe load reason",
        "Add subject action to STU-02",
        "No false validated or submitted state"
      ],
      "expectedFocusTarget": "Schedule builder heading",
      "liveRegion": "none",
      "nextActions": [
        "Open STU-02 to add a subject"
      ],
      "testIds": [
        "STU-04-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STU-04-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
      "Server plan version and selected groups",
      "Credit total, default target 18, effective maximum 12 or 18, and safe policy/source load reasons",
        "Equivalent calendar and chronological list",
        "Ranked alternatives only from server results",
      "Continue enabled only after server validation accepts every load and schedule blocker",
      "Minimum journeys: valid; recalculating; warning; conflict; stale; full; no solution; normal 18; probation 12",
        "Success only when serverAccepted is true"
      ],
      "expectedFocusTarget": "Schedule builder heading on initial load; view switching restores the corresponding meeting; applied alternatives restore the changed group heading",
      "liveRegion": "polite",
      "nextActions": [
        "Edit groups through the plan PUT",
        "Request/apply ranked recommendations",
        "Continue to STU-05 after accepted validation"
      ],
      "testIds": [
        "STU-04-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STU-04-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: submitted form failure",
        "Validation summary lists every blocker",
        "Red X icon plus visible word Conflict",
        "Named subjects/groups and each exact overlap day, start, and end",
        "Manual Change group and Remove action",
        "Continue and submission remain disabled while any blocking conflict exists",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Validation summary; its first link targets the first invalid field, group, or blocker",
      "liveRegion": "assertive",
      "nextActions": [
        "Change a group",
        "Remove a subject/group",
        "Request ranked alternatives",
        "Revalidate after edits"
      ],
      "testIds": [
        "STU-04-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STU-04-service-error-v1",
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
        "STU-04-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STU-04-unauthorized-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Access-denied heading",
        "UNAUTHORIZED or FORBIDDEN preserved exactly",
        "No Student protected data, identifier, cached content, or unauthorized action"
      ],
      "expectedFocusTarget": "Access-denied heading after navigation",
      "liveRegion": "assertive",
      "nextActions": [
        "Return to authorized home or sign in"
      ],
      "testIds": [
        "STU-04-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STU-04-session-expired-v1",
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
        "Refetch and revalidate before editing"
      ],
      "testIds": [
        "STU-04-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STU-04-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change heading",
        "PLAN_CHANGED, GROUP_FULL, POLICY_CHANGED, WINDOW_CHANGED, or STALE_VERSION",
        "Authoritative refresh and review action",
        "Cached availability, plan, capacity, or success is not reused",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "Refresh GET registration-plan",
        "Review changes and revalidate"
      ],
      "testIds": [
        "STU-04-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STU-04-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline status for Schedule builder heading",
        "Browser connectivity state and retry guidance",
        "No queued, cached, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry the plan GET when online; never queue a plan write"
      ],
      "testIds": [
        "STU-04-COMP-STATE-OFFLINE"
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
    "Schedule builder heading",
    "Server-validated total credits, default target 18, effective maximum 12 or 18, and policy/source load reasons",
    "Validation summary when a submitted transition fails",
    "Calendar/List view controls",
    "Chronological schedule items and equivalent calendar entries",
    "Selected GroupCards",
    "ConflictPanel Red X and Conflict heading with named group intervals",
    "Change group and Remove subject/group manual-resolution actions",
    "Request ranked alternatives button",
    "Ranked alternatives and Apply option buttons",
    "Continue to review button with all blockers in aria-describedby",
    "Safe support/reference link"
  ],
  "testIds": [
    "STU-04-CONTRACT-T163",
    "STU-04-COMP-T164",
    "STU-04-E2E-PRIMARY",
    "STU-04-E2E-FAILURE",
    "STU-04-A11Y-T165",
    "STU-04-VIS-T166"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1-draft",
    "SPEC-012": "not-pinned",
    "SPEC-013": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "pending-Ahmed-review"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Collapsed navigation and heading precede the server total, default-target-18, and effective-maximum-12-or-18 credit summary with safe policy/source reasons, selected groups, chronological ScheduleList, labelled calendar region, ConflictPanel, ranked alternatives, manual actions, and Continue last. |
| 375 | The 320 order is retained with 44 CSS px view, change, remove, recommendation, and Continue controls. The effective maximum and load reason remain visible; Red X plus Conflict, named groups, exact overlap intervals, and every blocker wrap in full. |
| 768 | Compact navigation precedes the effective credit summary, a selected-group column, and schedule column; the chronological list remains primary/readable and calendar is equivalent. ConflictPanel spans both columns before ranked alternatives and Continue. |
| 1024 | Persistent navigation, main calendar/list, and bounded plan/load summary are allowed without DOM reordering. Every load/conflict/manual action precedes Continue, and no sticky control covers blockers or focus. |
| 1280 | The schedule and plan summary use bounded columns; default 18, effective 12/18, policy reasons, and ranked alternatives remain explicit. Continue stays disabled and described while any load or conflict blocker remains. |
| 1920 | A centered maximum-width shell preserves equivalent calendar/list data and the 1280 action order. Surplus width adds gutters only; no effective limit, load reason, overlap detail, blocker, recommendation, or manual action is detached. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-012/T042. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STU-04-valid-v1` (valid) | success | valid server plan, total/default/effective credits and sourced load reasons, equivalent calendar/list, no blockers, and enabled Continue | client load/validation as authority, missing list equivalent, or enabled submission without server validation | Continue to STU-05 | `STU-04-COMP-STATE-SUCCESS` | `STU-04-E2E-PRIMARY` |
| `STU-04-recalculating-v1` (recalculating) | loading | pending validation/recommendation, disabled duplicate commands, stable schedule, and polite progress | second request, disappearing plan, focus theft, or early success | Wait or use only contract-supported cancel | `STU-04-COMP-STATE-LOADING` | `STU-04-E2E-PRIMARY` |
| `STU-04-warning-v1` (warning) | success | nonblocking warning text, affected group, server reason, and explicit effect on Continue | colour-only warning, hidden reason, or warning converted into hard conflict | Review warning or continue if server allows | `STU-04-COMP-STATE-SUCCESS` | `STU-04-E2E-PRIMARY` |
| `STU-04-conflict-v1` (conflict) | validation-error | Red X and word Conflict, named subjects/groups, every overlap day/start/end, ranked alternatives, manual Change/Remove, and disabled Continue/submission | colour-only X, vague interval, hidden blocker, override, enabled Continue, or success | Change/remove, apply a ranked option, then revalidate | `STU-04-COMP-STATE-VALIDATION-ERROR` | `STU-04-E2E-FAILURE` |
| `STU-04-stale-v1` (stale) | stale | PLAN_CHANGED or stable stale code, refreshed version, differences, and disabled Continue | cached plan success, automatic overwrite, or focus stolen during background refresh | Refresh, review, and revalidate | `STU-04-COMP-STATE-STALE` | `STU-04-E2E-FAILURE` |
| `STU-04-full-v1` (full) | stale | GROUP_FULL, affected subject/group, capacity text, alternatives/manual action, and disabled Continue | client seat claim, Available status, or submitted success | Change/remove group and revalidate | `STU-04-COMP-STATE-STALE` | `STU-04-E2E-FAILURE` |
| `STU-04-no-solution-v1` (no solution) | validation-error | honest no solution message, evaluated constraints summary, manual Change/Remove actions, and disabled Continue | invented recommendation, guaranteed feasibility, hidden conflict, or submission | Resolve manually and revalidate | `STU-04-COMP-STATE-VALIDATION-ERROR` | `STU-04-E2E-FAILURE` |
| `STU-04-normal-18-v1` (normal 18) | success | default target 18, effective maximum 18, current total, safe sourced load context, and Continue governed by server validation | GPA-derived browser limit, 12-credit claim, or missing policy provenance | Edit or validate the plan | `STU-04-COMP-STATE-SUCCESS` | `STU-04-E2E-PRIMARY` |
| `STU-04-probation-12-v1` (probation 12) | validation-error | default target 18, effective maximum 12, safe GPA-below-2.0 policy/source reason, affected credit total, and disabled Continue when over 12 | presenting 18 as applicable, hiding the 12-credit reason, exposing unrelated profile data, or client override | Remove or change groups and revalidate | `STU-04-COMP-STATE-VALIDATION-ERROR` | `STU-04-E2E-FAILURE` |

## Review notes

- Six annotated layouts are specific to STU-04 and preserve DOM, keyboard,
  action, state, and information order without hiding reasons or boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; commands
  name their endpoint and navigation-only actions never imply a missing write.
- The SPEC-012 plan response credit-load projection is a pending draft
  amendment and remains `not-pinned` until Ahmed approves and versions it.
- Background stale/offline/service updates preserve focus; submitted validation
  focuses its summary, and route-specific restoration is explicit in focusOrder.
- Ahmed ELbamby must approve this exact draft before its design task can close.
  Approval does not promote the route beyond `design-only`.
