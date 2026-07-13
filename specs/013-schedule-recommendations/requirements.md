# SPEC-013: Schedule Recommendations

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Technical Lead<br>
**Reviewers:** Product Owner, UX, QA, Performance owner<br>
**Target:** Sprint 5<br>
**Dependencies:** SPEC-003, SPEC-010, SPEC-011, SPEC-012, SPEC-018<br>

## Context

When selected groups conflict, the system should search other published groups
for the selected courses and propose the best feasible timetable. It must be
bounded, deterministic, explainable, and honest when no solution exists.

## Functional Requirements

- FR-1: The optimizer MUST choose exactly one published viable group per
  selected course.
- FR-2: It MUST enforce all hard meeting, availability, completeness,
  eligibility, and credit constraints. A travel-buffer constraint MUST remain
  disabled until a typed, sourced, approved policy defines its minutes/matrix.
- FR-3: It MUST order constrained courses first and prune invalid partial
  schedules.
- FR-4: It SHOULD return up to three distinct feasible schedules.
- FR-5: It MUST rank feasible results lexicographically using the versioned
  MVP factor order: fewer explicit student day/time preference violations,
  fewer total idle minutes, fewer teaching days, then the stable sorted group
  ID tuple. Missing preferences contribute zero violations. Preference inputs
  are limited to avoided weekdays plus optional earliest/latest local times;
  no hidden weight or institutional assumption is permitted. The Technical
  Lead and Product Owner MUST approve every factor/version change, and every
  response MUST name the configuration version and explain each component.
- FR-6: It MUST support cancellation and a configured computation time budget.
- FR-7: If no feasible result exists, it MUST return at least one
  **inclusion-minimal** blocking set: removing any member makes that reported
  hard conflict no longer hold. Every diagnostic MUST include the affected
  course/group IDs, stable hard-reason code, both involved meeting intervals
  where applicable, and a direct change-group or remove-course action for
  every member. Diagnostics MUST be ordered deterministically by set size and
  then stable course/group identifiers.
- FR-8: Final submission MUST revalidate all results; a recommendation does not
  reserve seats.
- FR-9: A recommendation request MUST include the expected plan rowversion and
  request correlation ID; each result MUST identify the captured plan,
  catalogue/group, policy, and optimizer-configuration versions. Every option
  MUST carry an authenticated, encrypted, expiring option token binding the
  authenticated student, plan, complete group selection, captured versions,
  correlation ID, issued time, and expiry so any stateless replica can
  validate it without server memory or a durable option table.
- FR-10: Applying an option MUST submit that option token and the current
  expected plan rowversion, validate signature/expiry/owner/plan/payload and
  dependency versions, and perform one atomic versioned plan mutation. A
  tampered, cross-owner, expired, stale-plan, or stale-dependency token MUST
  fail without mutation using a stable safe code.

## Non-Functional Requirements

- NFR-1: p95 optimization MUST be <= 500 ms for 8 courses with up to 10 groups
  each under approved hardware/load.
- NFR-2: Same inputs/configuration MUST produce the same ordering.
- NFR-3: Time-budget expiry MUST return a safe status, not an unbounded task.
- NFR-4: Unit coverage for optimizer branches MUST be at least 90%.

## Acceptance Criteria

### AC-1: Alternative found (FR-1, FR-2, FR-4)
Given two selected groups overlap and a non-overlapping alternate group exists<br>
When recommendations are requested<br>
Then at least one complete conflict-free schedule is returned<br>
And no subject is omitted or duplicated.

### AC-2: Best score explanation (FR-5)
Given two feasible schedules and one has fewer explicit preference violations,
or ties with fewer idle minutes under the approved factor version<br>
When results are ranked<br>
Then that schedule ranks first<br>
And the gap and other score components are shown.

### AC-3: No solution (FR-7)
Given every combination has a hard overlap<br>
When search completes<br>
Then no fake solution is returned<br>
And a minimal blocking set, hard reason codes/intervals, and change/remove
actions for every member are shown.

### AC-4: Time budget (FR-6, NFR-3)
Given search exceeds its configured budget<br>
When cancellation occurs<br>
Then the response indicates budget expiry with any verified results<br>
And final submission remains blocked unless a complete valid plan is chosen.

### AC-5: Recommendation revalidation (FR-8)
Given a recommended schedule was valid when calculated<br>
And a group fills before submission<br>
When the student submits that option<br>
Then final registration revalidates and rejects the stale group<br>
And no recommendation is treated as a reservation.

### AC-6: Plan changes during optimization (FR-9, FR-10)
Given optimization starts for plan rowversion 5<br>
When the student edits the plan to rowversion 6 before the result is applied<br>
Then applying the old option returns 409 PLAN_CHANGED<br>
And rowversion 6 remains unchanged.

### AC-7: Out-of-order responses (FR-9, FR-10, NFR-2)
Given request B for the current plan starts after request A<br>
When B completes first and A completes later<br>
Then the client renders only the response matching the current request
correlation ID and plan version<br>
And the server rejects any stale apply attempt.

### AC-8: Optimizer pruning, performance, and coverage (FR-3, NFR-1, NFR-4)
Given eight courses with ten groups each and fixtures exercising every
constraint/pruning branch<br>
When the optimizer benchmark and branch-coverage suite executes<br>
Then constrained courses are processed first and invalid partial schedules are
pruned<br>
And p95 completion is at most 500 ms<br>
And optimizer branch coverage is at least 90%.

## Edge Cases

- EC-1: Group becomes full during search -> may be excluded from fresh query;
  final commit remains authority.
- EC-2: One selected course has zero viable groups -> immediate no-solution.
- EC-3: Equal scores -> stable tie-break by course/group identifiers.
- EC-4: Invalid preference bound, factor order, or configuration version ->
  reject it and retain the last Product Owner/Technical Lead-approved version.
- EC-5: Catalogue/group/policy changes during search -> the result uses one
  coherent captured version set or returns STALE_INPUT; mixed-version options
  MUST NOT be returned.

## API Contracts

```typescript
interface MeetingIntervalDto {
  dayOfWeek: number;
  startLocal: string;
  endLocal: string;
}
interface SchedulePreferencesDto {
  avoidedWeekdays: number[];
  earliestPreferredStartLocal?: string;
  latestPreferredEndLocal?: string;
}
interface ScheduleOptionDto {
  optionToken: string;
  rank: number;
  groups: GroupDto[];
  score: number;
  scoreExplanation: Array<{
    factor: "preference-violations" | "idle-minutes" | "teaching-days" | "stable-group-tuple";
    value: number | string;
    message: string;
  }>;
}
interface OptimizationDiagnosticDto {
  diagnosticId: string;
  reasonCode: string;
  members: Array<{
    courseId: string;
    groupId?: string;
    interval?: MeetingIntervalDto;
    action: "change-group" | "remove-course";
  }>;
  conflictingInterval?: MeetingIntervalDto;
  minimality: "inclusion-minimal";
}
interface OptimizationResultDto {
  requestCorrelationId: string;
  planRowVersion: string;
  catalogueVersion: string;
  groupVersionSetHash: string;
  policyVersion: string;
  optimizerConfigurationVersion: string;
  status: "complete" | "no-solution" | "time-budget";
  options: ScheduleOptionDto[];
  diagnostics: OptimizationDiagnosticDto[];
  evaluatedAtUtc: string;
}
interface RecommendScheduleRequest {
  expectedPlanRowVersion: string;
  requestCorrelationId: string;
  preferences: SchedulePreferencesDto;
}
interface ApplyScheduleOptionRequest {
  optionToken: string;
  expectedPlanRowVersion: string;
  requestCorrelationId: string;
}
```

Endpoints: POST /api/student/terms/{termId}/registration-plan/recommendations and PUT
/api/student/terms/{termId}/registration-plan/recommended-option. Applying an option is
an atomic versioned plan update. The server derives the student and the one
student/term plan; no plan/student identifier is accepted from the client.
Stale input returns 409 PLAN_CHANGED or STALE_INPUT.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| SchedulePreferences | value object | unique avoided weekdays; optional earliest/latest local times within the term timetable; empty defaults |
| ScheduleOption | transient result | complete solution protected in a replica-safe signed token; never persisted as an option row |
| ScoreComponent | value | factor, numeric contribution, explanation |
| OptimizerConfiguration | immutable value | semantic version, fixed factor order, Product Owner/Technical Lead approval reference |
| OptimizationDiagnostic | transient | deterministic inclusion-minimal set, hard reason, intervals and per-member actions; no seat claim |

## Out of Scope

- OS-1: Machine learning/AI ranking.
- OS-2: Institution-wide timetable generation or changing published resources.
- OS-3: Guaranteed seat/reservation.
- OS-4: OR-Tools dependency until benchmark evidence requires it.
