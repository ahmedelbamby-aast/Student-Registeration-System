# SPEC-013 STU-04 Recommendation Contributor Contract

**Contract version:** `spec013-stu04/1.0`  
**Route:** `STU-04 /student/schedule`  
**Canonical page owner:** SPEC-012  
**Contributor:** SPEC-013

SPEC-013 contributes only the bounded recommendation request, result, and
apply-option behavior described here. It does not own or redesign
`ScheduleBuilderPage.razor`, duplicate the canonical registration plan, or
change SPEC-012's calendar, chronological list, conflict, credit, validation,
or Continue behavior. The smallest approved integration is one
`ScheduleRecommendationsPanel` plus recommendation methods on the existing
`RegistrationApiClient`.

## Server APIs and authority

- `POST /api/student/terms/{termId}/registration-plan/recommendations`
  accepts `RecommendScheduleRequest` with the current opaque
  `expectedPlanRowVersion`, a new `requestCorrelationId`, and bounded
  `SchedulePreferencesDto`.
- `PUT /api/student/terms/{termId}/registration-plan/recommended-option`
  accepts only the opaque `optionToken`, the current
  `expectedPlanRowVersion`, and the option's `requestCorrelationId`.
- Both endpoints require the authenticated `Student` policy. The server
  derives the student, route term, and canonical plan. No student ID or
  client-owned plan ID is sent by STU-04.
- A recommendation is advisory and never reserves capacity. Applying an
  option performs the canonical complete-plan replacement; final registration
  remains SPEC-014 owned and revalidates every rule and seat.
- The browser treats the option token as opaque. It is never decoded, logged,
  persisted in local storage, copied into a URL, or presented as a seat claim.

## Request lifecycle and stale-response suppression

1. The Student chooses **Find schedule alternatives**. STU-04 snapshots the
   currently rendered plan rowversion, creates a new request correlation ID,
   cancels the preceding recommendation request, and sends one POST.
2. While pending, the panel announces **Finding schedule alternatives** in a
   polite status region. The existing plan, conflicts, manual actions, and
   Continue blocker remain visible. A duplicate request and Apply buttons are
   disabled.
3. STU-04 renders a response only when both
   `OptimizationResultDto.requestCorrelationId` equals the active request and
   `OptimizationResultDto.planRowVersion` equals the currently rendered plan
   rowversion. A late response, cancelled response, or response for an older
   plan is silently discarded as display data; it never overwrites current
   options, errors, focus, or plan state.
4. Any local plan add/change/remove, successful option apply, term change,
   logout, or route disposal cancels the pending request and clears its
   options. Client suppression is a usability safeguard only; the server still
   returns `PLAN_CHANGED` or `STALE_INPUT` for a stale request or apply.
5. Apply submits the option token with the current plan rowversion and the
   option response correlation ID. A 200 `RegistrationPlanDto` replaces the
   page's canonical plan view, clears old recommendations, remaps the existing
   calendar/list/conflict state, and restores focus to the first changed group
   heading or the recommendation success heading when no group changed.

## Complete options

For `status=complete`, the panel displays up to three options in ascending
`rank`. Every option shows the complete one-group-per-selected-course set and
all four ordered score explanations:

1. `preference-violations`
2. `idle-minutes`
3. `teaching-days`
4. `stable-group-tuple`

Score values are server text, not client weights or arithmetic. The panel also
names the `optimizerConfigurationVersion` and labels each action
**Apply option {rank}**. Capacity is described as advisory until final
submission. The client does not reorder, merge, complete, or invent an option.

## No-solution diagnostics

For `status=no-solution`, STU-04 displays **No complete schedule alternative
was found** and no Apply button. It renders every server diagnostic in its
received deterministic order, including:

- the stable hard `reasonCode`;
- every member course ID and optional group ID;
- both involved day/start/end intervals where supplied;
- the `inclusion-minimal` label; and
- a keyboard-operable `change-group` or `remove-course` action for every
  member.

`remove-course` is presented as **Remove group** on STU-04 and invokes the
existing canonical complete-plan replacement. Diagnostics never imply an
override, partial valid schedule, guaranteed group, or reservation.

## Time budget

For `status=time-budget`, the panel announces **Recommendation time budget
reached**. It may show only complete verified options returned by the server;
an incomplete partial assignment is never rendered as an option. If no
verified option exists, manual change/remove actions remain available and
Continue remains governed solely by the current SPEC-012 plan. The browser
does not extend the server budget or continue optimization locally.

## Errors and recovery

| HTTP/code | STU-04 behavior |
|---|---|
| `400 VALIDATION_ERROR` | Show the safe validation message; preserve the current plan and manual actions. |
| `400 INVALID_OPTION_TOKEN` | Clear options, announce that alternatives must be requested again, and do not mutate the plan. |
| `401` / session expired | Use the existing session-expired state and clear protected recommendation data. |
| `403` | Use the existing unauthorized state and expose no plan or token data. |
| `404 REGISTRATION_CONTEXT_NOT_FOUND` | Clear options and use the existing private missing-context recovery. |
| `409 OPTION_EXPIRED` | Clear the expired option and offer **Find schedule alternatives** again. |
| `409 PLAN_CHANGED` | Clear all options, refresh the authorized current plan, announce **Plan changed**, and require review. |
| `409 STALE_INPUT` | Clear all options, announce that schedule data changed, and require a fresh request. |
| `429 RATE_LIMITED` | Preserve the plan, announce a safe retry message, and do not auto-retry. |
| `503 RECOMMENDATIONS_UNAVAILABLE` | Preserve the plan and manual actions; expose explicit retry and safe support paths. |
| `500 INTERNAL_ERROR` or malformed success | Preserve the plan, show the safe fallback/reference ID, and never claim success. |
| Network/offline/cancelled | Never queue an apply or recommendation; preserve plan/manual actions and allow an explicit retry when online. |

Every failure preserves the stable server code where one exists and never
shows a stack trace, token, protected payload, SQL detail, another owner, or a
false successful apply.

## Accessibility and interaction

- The panel is headed **Schedule alternatives** and is referenced by the
  conflict area's **Find schedule alternatives** control.
- Pending and background results use a polite live region; submitted apply
  errors use an assertive alert without repeated announcements.
- Options are an ordered list. Rank, group/course text, every score factor and
  explanation, advisory-capacity text, and Apply action are available without
  color. Diagnostics are a semantic list with their reason, intervals,
  minimality, and actions in text.
- Find, retry, manual-resolution, and Apply controls use native buttons/links,
  remain keyboard operable, and meet the existing 44-by-44 CSS-pixel target
  rule. Disabled controls retain an accessible reason.
- A completed background request does not steal focus. Successful Apply moves
  focus to the changed group/success heading; an apply error moves focus to
  the panel alert. Removing stale results does not move focus.
- At 320 CSS pixels and 400% zoom, the panel stays in the existing STU-04 DOM
  order after conflicts and before Continue; no option, diagnostic, score, or
  action requires horizontal scrolling.

## Scope exclusions

This contribution adds no machine-learning ranking, institution-wide solver,
published-resource mutation, travel-buffer guess, seat reservation, conflict
override, durable option store, sticky-session dependency, or client-side
academic decision. SPEC-012 remains the canonical STU-04 implementation owner.
