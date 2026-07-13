# STU-03 Page Design Record

**Record version:** `draft/1.0`<br>
**Approval status:** Pending Ahmed ELbamby review<br>
**Readiness:** `design-only`<br>
**Design owner:** SPEC-003<br>
**Implementation owner:** SPEC-011<br>
**Implementation task:** SPEC-011/T042<br>
**Design task:** SPEC-003/T157

This complete design draft is governed by `page-design-record/1.1-draft`. It may be
reviewed as design evidence, but it does not authorize route source, API
binding, route-specific executable tests, or visual-baseline approval. Exact
downstream contributor versions remain `not-pinned`.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-03",
  "routeTemplate": "/student/subjects/{offeringId}",
  "pageName": "SubjectDetailsPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-011",
  "ownerSpecs": [
    "SPEC-003",
    "SPEC-010",
    "SPEC-011"
  ],
  "actors": [
    "Student"
  ],
  "purpose": "Show subject eligibility, effective credit policy, and complete group capacity, Lecturer, TA, room, day, and time details.",
  "informationHierarchy": [
    "Breadcrumb and subject summary",
    "Eligibility and prerequisite result",
    "Current/projected credits, default target 18, effective 12/18 maximum, and sourced policy reasons",
    "Group cards",
    "Capacity, staff, location, and meeting details",
    "Stale or full reasons",
    "Selection action"
  ],
  "responsiveWireframes": {
    "320": "Collapsed navigation, breadcrumb, subject heading, eligibility result, current/projected/default-18/effective-12-or-18 credit summary, and one-column GroupCards appear in order. Every group shows occupied/total/remaining/full capacity, Lecturer, Tutorial/Lab TA, room, day, start, and end before its Continue action.",
    "375": "The 320 order is retained; the effective maximum and policy/source reason stay before groups. Capacity and staffing may use labelled pairs only when they fit; incomplete groups keep full text and no selectable control.",
    "768": "Compact navigation precedes subject/eligibility and effective-credit summaries plus a two-column GroupCard grid. Each card remains internally ordered capacity, complete staffing, location/time, status/reason, then action.",
    "1024": "Persistent navigation, a bounded subject/credit summary rail, and main group grid are allowed. No load reason, staffing, room, interval, capacity, or eligibility reason moves into a hover-only disclosure.",
    "1280": "The group grid may use three bounded columns. The default 18 and effective 12/18 maximum remain visible above it; CapacityIndicator and staff/activity rows stay readable and Continue stays attached to one complete selectable bundle.",
    "1920": "A centered maximum-width shell preserves the 1280 card sizing and DOM/focus order. Surplus width increases gutters only; cards never hide credit, policy, staffing, meeting, or capacity details or gain client-only selection state."
  },
  "components": [
    "AppShell",
    "RoleNavigation",
    "EntityCard",
    "GroupCard",
    "CapacityIndicator",
    "StatusBadge",
    "Alert",
    "Button",
    "AppLink",
    "StatePanel"
  ],
  "dataContracts": [
    "GET /api/student/offerings/{offeringId}/eligibility",
    "GET /api/offerings/{offeringId}",
    "GET /api/groups/{groupId}"
  ],
  "actions": [
    "Refresh GET /api/student/offerings/{offeringId}/eligibility, GET /api/offerings/{offeringId}, and GET /api/groups/{groupId}",
    "Continue with selected group intent to STU-04; only STU-04 may PUT the plan",
    "Open STU-04 current plan",
    "Return to STU-02 discovery"
  ],
  "navigationTransitions": [
    "Selected complete group intent -> STU-04; STU-04 owns plan PUT and revalidation",
    "Current plan -> STU-04",
    "Discovery -> STU-02",
    "Dashboard -> STU-01"
  ],
  "states": [
    {
      "state": "loading",
      "applicability": "required",
      "fixture": "STU-03-loading-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Loading Subject details heading with stable landmarks",
        "Named progress status and disabled duplicate command",
        "No false success"
      ],
      "expectedFocusTarget": "Preserve current focus during loading; use the route heading only for initial navigation",
      "liveRegion": "polite",
      "nextActions": [],
      "testIds": [
        "STU-03-COMP-STATE-LOADING"
      ]
    },
    {
      "state": "empty",
      "applicability": "required",
      "fixture": "STU-03-empty-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "No published complete group bundle",
        "Offering identity and safe reason",
        "Return to discovery action",
        "No selectable placeholder group"
      ],
      "expectedFocusTarget": "Subject details heading",
      "liveRegion": "none",
      "nextActions": [
        "Return to STU-02",
        "Refresh the three GET resources"
      ],
      "testIds": [
        "STU-03-COMP-STATE-EMPTY"
      ]
    },
    {
      "state": "success",
      "applicability": "required",
      "fixture": "STU-03-success-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
      "Subject code, title, credits, eligibility, and prerequisite result",
      "Current and projected plan credits, default target 18, effective maximum 12 or 18, and policy/source reason metadata",
        "For every group: occupied/total/remaining/full capacity",
        "Lecture Lecturer name; each Tutorial or Laboratory TA name",
        "Room/location, day, start time, and end time",
        "No incomplete staffing bundle is selectable",
      "Minimum journeys: open; nearly full; full; changed; unpublished; selected; normal 18; probation 12",
        "Every meeting identifies day and start/end time"
      ],
      "expectedFocusTarget": "Subject details heading on initial load; background capacity updates preserve the focused GroupCard; after returning from STU-04 restore the corresponding group heading",
      "liveRegion": "polite",
      "nextActions": [
        "Continue with a complete selectable group intent to STU-04",
        "Open current plan or return to discovery"
      ],
      "testIds": [
        "STU-03-COMP-STATE-SUCCESS"
      ]
    },
    {
      "state": "validation-error",
      "applicability": "required",
      "fixture": "STU-03-validation-error-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Trigger: initial navigation",
        "Stable eligibility or staffing reason",
        "Full group identity and missing/invalid field explanation",
        "Continue action absent or disabled for full, unpublished, ineligible, or incomplete staffing",
        "No client override or plan write",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Subject details heading",
      "liveRegion": "none",
      "nextActions": [
        "Choose another complete group",
        "Return to discovery",
        "Return to STU-02"
      ],
      "testIds": [
        "STU-03-COMP-STATE-VALIDATION-ERROR"
      ]
    },
    {
      "state": "service-error",
      "applicability": "required",
      "fixture": "STU-03-service-error-v1",
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
        "STU-03-COMP-STATE-SERVICE-ERROR"
      ]
    },
    {
      "state": "unauthorized",
      "applicability": "required",
      "fixture": "STU-03-unauthorized-v1",
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
        "STU-03-COMP-STATE-UNAUTHORIZED"
      ]
    },
    {
      "state": "session-expired",
      "applicability": "required",
      "fixture": "STU-03-session-expired-v1",
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
        "STU-03-COMP-STATE-SESSION-EXPIRED"
      ]
    },
    {
      "state": "stale",
      "applicability": "required",
      "fixture": "STU-03-stale-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Concurrent-change heading",
        "GROUP_FULL, PLAN_CHANGED, WINDOW_CHANGED, or STALE_VERSION",
        "Authoritative refresh and review action",
        "Cached availability, plan, capacity, or success is not reused",
        "Preserve the owner-contract reason code exactly"
      ],
      "expectedFocusTarget": "Preserve current focus during a background refresh; move to the concurrent-change heading only after a submitted stale command",
      "liveRegion": "polite",
      "nextActions": [
        "Refresh all eligibility/offering/group GET resources",
        "Review changed group details"
      ],
      "testIds": [
        "STU-03-COMP-STATE-STALE"
      ]
    },
    {
      "state": "offline",
      "applicability": "required",
      "fixture": "STU-03-offline-v1",
      "fixtureVersion": "frontend-fixture/1.0",
      "expectedContent": [
        "Offline status for Subject details heading",
        "Browser connectivity state and retry guidance",
        "No queued, cached, or false success",
        "Browser connectivity state; never claim queued success"
      ],
      "expectedFocusTarget": "Preserve current focus while the offline connection status is announced politely",
      "liveRegion": "polite",
      "nextActions": [
        "Retry all subject/group GET resources when online"
      ],
      "testIds": [
        "STU-03-COMP-STATE-OFFLINE"
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
    "Back to discovery breadcrumb",
    "Subject details heading",
    "Eligibility and prerequisite result",
    "Current/projected/default/effective credit summary and policy/source reason",
    "Group cards in server order",
    "Each group capacity indicator",
    "Each group Lecturer then Tutorial/Laboratory TA details",
    "Each group room, day, start, and end details",
    "Continue with this group link only for complete selectable bundles",
    "Current plan link",
    "Safe support/reference link"
  ],
  "testIds": [
    "STU-03-CONTRACT-T158",
    "STU-03-COMP-T159",
    "STU-03-E2E-PRIMARY",
    "STU-03-E2E-FAILURE",
    "STU-03-A11Y-T160",
    "STU-03-VIS-T161"
  ],
  "contributorContractVersions": {
    "SPEC-003": "frontend-design-index/1.1-draft",
    "SPEC-010": "not-pinned",
    "SPEC-011": "not-pinned"
  },
  "readinessState": "design-only",
  "approvalVersion": "pending-Ahmed-review"
}
```

## Annotated responsive layouts

| Width | Normative layout and action placement |
|---:|---|
| 320 | Collapsed navigation, breadcrumb, subject heading, eligibility result, current/projected/default-18/effective-12-or-18 credit summary, and one-column GroupCards appear in order. Every group shows occupied/total/remaining/full capacity, Lecturer, Tutorial/Lab TA, room, day, start, and end before its Continue action. |
| 375 | The 320 order is retained; the effective maximum and policy/source reason stay before groups. Capacity and staffing may use labelled pairs only when they fit; incomplete groups keep full text and no selectable control. |
| 768 | Compact navigation precedes subject/eligibility and effective-credit summaries plus a two-column GroupCard grid. Each card remains internally ordered capacity, complete staffing, location/time, status/reason, then action. |
| 1024 | Persistent navigation, a bounded subject/credit summary rail, and main group grid are allowed. No load reason, staffing, room, interval, capacity, or eligibility reason moves into a hover-only disclosure. |
| 1280 | The group grid may use three bounded columns. The default 18 and effective 12/18 maximum remain visible above it; CapacityIndicator and staff/activity rows stay readable and Continue stays attached to one complete selectable bundle. |
| 1920 | A centered maximum-width shell preserves the 1280 card sizing and DOM/focus order. Surplus width increases gutters only; cards never hide credit, policy, staffing, meeting, or capacity details or gain client-only selection state. |

## Minimum journey and test traceability

**Governed E2E task:** SPEC-011/T041. All fixtures use `frontend-fixture/1.0`.
Required and forbidden content is normative; a forbidden item fails the route test.

| Fixture | State | Required expected content | Forbidden content or action | Next action | Component test | E2E test |
|---|---|---|---|---|---|---|
| `STU-03-open-v1` (open) | success | open group with current/projected/default/effective credit context, exact occupied/total/remaining capacity, Lecturer, required TA, room, day, start/end, and Continue link | missing credit/staffing/location/time, client load/capacity decision, or direct plan PUT | Continue with group intent to STU-04 | `STU-03-COMP-STATE-SUCCESS` | `STU-03-E2E-PRIMARY` |
| `STU-03-nearly-full-v1` (nearly full) | success | nearly full text plus exact occupied/total/remaining capacity and complete staff/location/interval bundle | vague colour-only warning, hidden remaining seats, or guaranteed seat claim | Continue to STU-04 for server revalidation | `STU-03-COMP-STATE-SUCCESS` | `STU-03-E2E-PRIMARY` |
| `STU-03-full-v1` (full) | stale | GROUP_FULL, zero remaining, complete group details, and alternative/back action | enabled Continue, client reservation, or Available badge | Refresh or choose another group | `STU-03-COMP-STATE-STALE` | `STU-03-E2E-FAILURE` |
| `STU-03-changed-v1` (changed) | stale | changed capacity/staff/room/day/start/end fields, stable reason, and review action | silent replacement, stale selected state, or automatic plan mutation | Refresh and review the changed group | `STU-03-COMP-STATE-STALE` | `STU-03-E2E-FAILURE` |
| `STU-03-unpublished-v1` (unpublished) | validation-error | unpublished safe reason and Return to discovery | group identifiers beyond the safe contract, selectable action, or cached published status | Return to STU-02 | `STU-03-COMP-STATE-VALIDATION-ERROR` | `STU-03-E2E-FAILURE` |
| `STU-03-selected-v1` (selected) | success | selected intent summary names the complete group bundle and states STU-04 will perform server plan mutation | plan changed by this GET-only page, success before PUT/revalidation, or incomplete staffing selection | Navigate to STU-04 and let it PUT/revalidate | `STU-03-COMP-STATE-SUCCESS` | `STU-03-E2E-PRIMARY` |
| `STU-03-normal-18-v1` (normal 18) | success | current/projected credits, default target 18, effective maximum 18, and sourced nonblocking load context above complete group bundles | GPA-derived browser limit, 12-credit claim, or missing provenance | Continue with an eligible complete group | `STU-03-COMP-STATE-SUCCESS` | `STU-03-E2E-PRIMARY` |
| `STU-03-probation-12-v1` (probation 12) | success | current/projected credits, default target 18, effective maximum 12, and safe GPA-below-2.0 policy/source reason above complete group bundles | presenting 18 as applicable, exposing unrelated profile data, or client-calculated limit | Review the load reason before continuing | `STU-03-COMP-STATE-SUCCESS` | `STU-03-E2E-PRIMARY` |

## Review notes

- Six annotated layouts are specific to STU-03 and preserve DOM, keyboard,
  action, state, and information order without hiding reasons or boundaries.
- API entries remain verbatim from `.specify/page-api-manifest.json`; commands
  name their endpoint and navigation-only actions never imply a missing write.
- Background stale/offline/service updates preserve focus; submitted validation
  focuses its summary, and route-specific restoration is explicit in focusOrder.
- Ahmed ELbamby must approve this exact draft before its design task can close.
  Approval does not promote the route beyond `design-only`.
