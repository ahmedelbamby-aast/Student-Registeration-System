# API Contract: Schedule Builder and Conflicts

## Contract Status

The three endpoint sections below are finalized for the approved Gate A demo
core. They pin authentication, owner/term scope, complete plan replacement,
optimistic concurrency, non-mutating validation, conflict and stale-selection
recovery, privacy-safe errors, and the no-seat-reservation boundary.

The credit-load response projection is approved as
`spec012-credit-load/1.0` by Ahmed ELbamby on 2026-07-16. Every plan response
returns server-authored `defaultTargetCredits=18`,
`maximumAllowedCredits=18`, and sourced `loadReasons`. There is no overload or
GPA-derived 12-credit path.

## Approved DTOs

```typescript
interface ResolutionActionDto {
  action: "change-group" | "remove-group";
  targetGroupId: string;
  label: string;
  route: string;
}

interface ScheduleConflictParticipantDto {
  groupId: string;
  groupCode: string;
  courseCode: string;
  subjectTitle: string;
  startLocal: string;
  endLocal: string;
}

interface ScheduleConflictDto {
  code: "MEETING_OVERLAP" | "TRAVEL_BUFFER";
  first: ScheduleConflictParticipantDto;
  second: ScheduleConflictParticipantDto;
  dayOfWeek: number;
  overlapStartLocal: string;
  overlapEndLocal: string;
  message: string;
  actions: ResolutionActionDto[];
}

interface PlanSelectionIssueDto {
  code: "GROUP_CHANGED" | "GROUP_FULL" | "GROUP_UNPUBLISHED" |
    "GROUP_CLOSED" | "GROUP_CANCELLED" | "REGISTRATION_PAUSED" |
    "GROUP_UNAVAILABLE";
  offeringId: string;
  groupId: string;
  groupCode: string;
  message: string;
  blocking: true;
  actions: ResolutionActionDto[];
}

interface ValidationSnapshotDto {
  evaluatedAtUtc: string;
  academicContextVersion: string;
  policyVersion: string;
  catalogueVersion: string;
  offeringVersions: Record<string, string>;
  groupVersions: Record<string, string>;
}

interface LoadPolicyReasonDto {
  code: string;
  blocking: boolean;
  message: string;
  requiredValue?: string;
  currentValue?: string;
  policySetId: string;
  policyVersion: string;
  sourceReference: string;
}

interface RegistrationPlanDto {
  id: string;
  termId: string;
  rowVersion: string;
  selectedGroups: GroupDto[];
  totalCredits: number;
  defaultTargetCredits: 18;
  maximumAllowedCredits: 18;
  loadReasons: LoadPolicyReasonDto[];
  selectionIssues: PlanSelectionIssueDto[];
  conflicts: ScheduleConflictDto[];
  validation: ValidationSnapshotDto;
  reviewBlocked: boolean;
}

interface RegistrationPlanMutationRequest {
  expectedPlanRowVersion: string;
  selectedGroupIds: string[];
}

interface StaleRegistrationPlanResponse {
  error: ApiError & {
    code: "STALE_VERSION";
    currentVersion: string;
  };
  currentPlan: RegistrationPlanDto;
}
```

`GroupDto` is the approved bounded Scheduling projection owned by SPEC-010.
`ApiError` is the canonical privacy-safe error owned by SPEC-006. DTOs never
serialize an EF entity, navigation graph, raw claim set, or persistence detail.

`TRAVEL_BUFFER` remains reserved for a future approved policy and is never
emitted by this demo. `MEETING_OVERLAP` uses strict half-open intervals. Each
conflict identifies both groups and original local intervals, the exact
intersection, a stable safe message, and change/remove actions for both groups.

`selectionIssues` carries changed or unavailable selected-group state that is
not a meeting conflict. Each issue identifies only the affected selection and
provides server-authored change/remove actions. Any conflict or selection issue
sets `reviewBlocked=true`; the server never silently replaces a group.

## Common Authorization, Authority, and Privacy Rules

- All three operations require an authenticated Student. The server derives
  the Student identifier from the validated authentication context and checks
  the route `termId` against that Student's current authorized academic and
  registration context before resolving a plan or version.
- No route or request accepts a Student identifier or client-owned plan
  identifier. Authorization runs before protected existence, rowversion, group,
  conflict, or snapshot data is disclosed. Another Student's object is never
  returned and cannot be distinguished through a version oracle.
- Successful responses are full server-authored `RegistrationPlanDto`
  projections. Meeting conflicts, group state, credits, review blocking, and
  `ValidationSnapshotDto` are recalculated from current server dependencies;
  browser calculations are advisory only.
- Every empty, current, replaced, validated, and authorized stale-current plan
  projection has `defaultTargetCredits=18`, `maximumAllowedCredits=18`, and
  safe server-authored `loadReasons` with policy/source provenance. No browser
  GPA calculation or overload path changes those values.
- Capacity shown in `GroupDto` is advisory. GET, PUT, and validate never
  reserve, decrement, allocate, or promise a seat. SPEC-014 revalidates
  capacity and every academic rule during submission.
- All error bodies use canonical `ApiError`, except the explicitly declared
  authorized stale-write envelope that pairs `ApiError` with the current plan.
  Messages and correlation IDs are safe; no exception, SQL, credential, raw
  claim, or unauthorized identifier/version is returned.
- Dates use ISO 8601, UTC evaluation timestamps, and the server-configured
  academic-term timezone for local meeting values. Arrays are bounded to the
  plan and deterministically ordered.

## Endpoint 01 - Current Registration Plan (T011)

### `GET /api/student/terms/{termId}/registration-plan`

**Contract status:** FINALIZED, including `spec012-credit-load/1.0`.

The request has no body. Success is `200 RegistrationPlanDto` for the
authenticated Student and authorized route term. A current empty plan is a
normal success with `selectedGroups`, `selectionIssues`, and `conflicts` empty,
`totalCredits=0`, and a server-owned plan rowversion; it is not `404` and does
not reserve a seat. It still returns the fixed 18/18 credit fields and sourced
load reasons. A non-empty response contains the current complete plan, fresh
conflict/selection state, and a timestamped validation snapshot.

| Outcome | Status and body |
|---|---|
| Current empty or non-empty plan | `200 RegistrationPlanDto` |
| Unauthenticated | `401 ApiError` |
| Authenticated without Student permission | `403 ApiError`, no protected data or version |
| Route term absent or outside the Student's authorized registration context | `404 REGISTRATION_CONTEXT_NOT_FOUND` as `ApiError` |
| Required academic, policy, catalogue, Scheduling, or plan storage unavailable | `503 REGISTRATION_PLAN_UNAVAILABLE` as `ApiError` |
| Unexpected failure | `500 INTERNAL_ERROR` as `ApiError` |
| Concurrency conflict | Not applicable to this read |

The read has no durable mutation and no seat-allocation side effect.

## Endpoint 02 - Replace Registration Plan (T013)

### `PUT /api/student/terms/{termId}/registration-plan`

**Contract status:** FINALIZED, including `spec012-credit-load/1.0`.

The body is exactly `RegistrationPlanMutationRequest`. `selectedGroupIds` is
the complete desired selection, not an item patch; an empty array clears the
plan. `expectedPlanRowVersion` is required and opaque. `If-Match` and HTTP 412
are outside the MVP protocol.

The server authorizes owner/term scope, resolves every selected group and its
offering, rejects duplicate group IDs or more than one group for an offering,
and conditionally updates the one plan root using the expected rowversion. The
item replacement, total-credit calculation, conflicts, selection issues,
validation snapshot, review state, and one root-version advance are one atomic
operation. A rejection changes neither the root nor its items; no partial
replacement is observable.

Success is `200 RegistrationPlanDto` with the new rowversion, fixed 18/18
credit fields, and sourced load reasons. A group that
became changed, full, unpublished, closed, cancelled, paused, or otherwise
unavailable is returned as a blocking `selectionIssues` entry with safe
change/remove actions; it is never silently substituted. Capacity remains
advisory and the update allocates no seat.

| Outcome | Status and body |
|---|---|
| Complete replacement committed | `200 RegistrationPlanDto` |
| Malformed body, missing/invalid rowversion, duplicate group ID, or unbounded input | `400 VALIDATION_ERROR` as `ApiError` |
| Two selected groups resolve to the same offering | `409 DUPLICATE_OFFERING_SELECTION` as `ApiError`; no plan change |
| Authorized stale plan rowversion | `409 StaleRegistrationPlanResponse` containing only the owner's current plan |
| Unauthenticated | `401 ApiError` |
| Authenticated without Student permission | `403 ApiError`, no protected data or version |
| Route term absent or outside the Student's authorized registration context | `404 REGISTRATION_CONTEXT_NOT_FOUND` as `ApiError` |
| Required dependency/storage unavailable before commit | `503 REGISTRATION_PLAN_UNAVAILABLE` as `ApiError`; no plan change |
| Unexpected failure before commit | `500 INTERNAL_ERROR` as `ApiError`; no partial plan change |

## Endpoint 03 - Revalidate Current Plan (T015)

### `POST /api/student/terms/{termId}/registration-plan/validate`

**Contract status:** FINALIZED, including `spec012-credit-load/1.0`.

The request has no body and validates the authenticated Student's current
server-stored plan for the authorized route term. Success is
`200 RegistrationPlanDto` containing fresh deterministic conflicts, selection
issues, actions, credits, fixed 18/18 credit fields, sourced load reasons,
validation versions, and review-blocked state. The response rowversion
identifies the plan that was evaluated so the client can discard an
out-of-order result.

Validation is side-effect-free: it does not replace selections, advance the
plan rowversion, persist an advisory result, reserve capacity, or allocate a
seat. A changed, full, unpublished, closed, cancelled, paused, or otherwise
unavailable selected group is returned as a blocking issue with change/remove
actions. Every meeting overlap is returned with complete actions; no blocker
is silently removed or overridden.

| Outcome | Status and body |
|---|---|
| Current empty or non-empty plan evaluated | `200 RegistrationPlanDto` |
| Unauthenticated | `401 ApiError` |
| Authenticated without Student permission | `403 ApiError`, no protected data or version |
| Route term absent or outside the Student's authorized registration context | `404 REGISTRATION_CONTEXT_NOT_FOUND` as `ApiError` |
| Required academic, policy, catalogue, Scheduling, or plan storage unavailable | `503 REGISTRATION_PLAN_UNAVAILABLE` as `ApiError` |
| Unexpected failure | `500 INTERNAL_ERROR` as `ApiError` |
| Concurrency conflict | Not applicable; the returned rowversion names the evaluated current plan |

## Finalized Contract Evidence

| Task | Evidence pinned by this document |
|---|---|
| T011 / API-Endpoint01 | Exact GET route; Student/term ownership; empty/current `200` projection; privacy-safe outcomes; fresh server validation; read-only and no-seat-reservation behavior |
| T013 / API-Endpoint02 | Exact PUT route and request; complete atomic replacement; duplicate-offering rejection; expected-rowversion/authorized stale-current-plan response; group recovery actions; no partial write or seat reservation |
| T015 / API-Endpoint03 | Exact validate route; current-plan server evaluation; complete conflicts/issues/actions/snapshot; explicit no-body and non-mutating behavior; no rowversion advance or seat reservation |

This evidence includes the approved `spec012-credit-load/1.0` response shape.
It authorizes test-first and runtime work but does not itself complete T012,
T014, T016, any handler, or runtime delivery.

## Approved Credit-Load Response Amendment

Ahmed ELbamby approved `spec012-credit-load/1.0` on 2026-07-16. The extension
is part of `RegistrationPlanDto`, including the `currentPlan` returned only to
the authorized owner after `STALE_VERSION`.

- `defaultTargetCredits` is always 18.
- `maximumAllowedCredits` is always 18.
- `loadReasons` remains server-authored and retains safe policy/source
  provenance.
- No overload path or GPA-derived 12-credit branch exists.
- The browser never derives either value or invents a reason.
