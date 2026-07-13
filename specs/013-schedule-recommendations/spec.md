# Feature Specification: Schedule Recommendations

**Feature Branch**: 013-schedule-recommendations
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Technical Lead
**Normative detail**: [requirements.md](requirements.md)

## Context

When selected groups conflict, the system should search other published groups
for the selected courses and propose the best feasible timetable. It must be
bounded, deterministic, explainable, and honest when no solution exists.

## User Scenarios and Testing

### User Story 1 - Alternative found (FR-1, FR-2, FR-4) (P1)

As a Student, I need the Alternative found (FR-1, FR-2, FR-4) behavior so that Schedule Recommendations produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given two selected groups overlap and a non-overlapping alternate group exists<br>
When recommendations are requested<br>
Then at least one complete conflict-free schedule is returned<br>
And no subject is omitted or duplicated.
### User Story 2 - Best score explanation (FR-5) (P1)

As a Student, I need the Best score explanation (FR-5) behavior so that Schedule Recommendations produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given two feasible schedules and one has fewer gaps under approved preferences<br>
When results are ranked<br>
Then that schedule ranks first<br>
And the gap and other score components are shown.
### User Story 3 - No solution (FR-7) (P2)

As a Student, I need the No solution (FR-7) behavior so that Schedule Recommendations produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given every combination has a hard overlap<br>
When search completes<br>
Then no fake solution is returned<br>
And a minimal blocking set, hard reason codes/intervals, and change/remove
actions for every member are shown.
### User Story 4 - Time budget (FR-6, NFR-3) (P2)

As a Student, I need the Time budget (FR-6, NFR-3) behavior so that Schedule Recommendations produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given search exceeds its configured budget<br>
When cancellation occurs<br>
Then the response indicates budget expiry with any verified results<br>
And final submission remains blocked unless a complete valid plan is chosen.
### User Story 5 - Recommendation revalidation (FR-8) (P3)

As a Student, I need the Recommendation revalidation (FR-8) behavior so that Schedule Recommendations produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given a recommended schedule was valid when calculated<br>
And a group fills before submission<br>
When the student submits that option<br>
Then final registration revalidates and rejects the stale group<br>
And no recommendation is treated as a reservation.
### User Story 6 - Plan changes during optimization (FR-9, FR-10) (P3)

As a Student, I need the Plan changes during optimization (FR-9, FR-10) behavior so that Schedule Recommendations produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given optimization starts for plan rowversion 5<br>
When the student edits the plan to rowversion 6 before the result is applied<br>
Then applying the old option returns 409 PLAN_CHANGED<br>
And rowversion 6 remains unchanged.
### User Story 7 - Out-of-order responses (FR-9, FR-10, NFR-2) (P3)

As a Student, I need the Out-of-order responses (FR-9, FR-10, NFR-2) behavior so that Schedule Recommendations produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

Given request B for the current plan starts after request A<br>
When B completes first and A completes later<br>
Then the client renders only the response matching the current request
correlation ID and plan version<br>
And the server rejects any stale apply attempt.
### User Story 8 - Optimizer pruning, performance, and coverage (FR-3, NFR-1, NFR-4) (P3)

As a Student, I need the Optimizer pruning, performance, and coverage (FR-3, NFR-1, NFR-4) behavior so that Schedule Recommendations produces a verifiable outcome.

**Independent Test**: Execute AC-8 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-8)**

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
  reject it and retain the last approved version.
- EC-5: Catalogue/group/policy changes during search -> the result uses one
  coherent captured version set or returns STALE_INPUT; mixed-version options
  MUST NOT be returned.

## Requirements

### Functional Requirements

- FR-1: The optimizer MUST choose exactly one published viable group per
  selected course.
- FR-2: It MUST enforce all hard meeting, availability, completeness,
  eligibility, and credit constraints. A travel-buffer constraint MUST remain
  disabled until a typed, sourced, approved policy defines its minutes/matrix.
- FR-3: It MUST order constrained courses first and prune invalid partial
  schedules.
- FR-4: It SHOULD return up to three distinct feasible schedules.
- FR-5: It MUST use the approved versioned lexicographic factor order
  (preference violations, idle minutes, teaching days, stable group tuple),
  disclose the configuration version, and explain every component; factors
  MUST NOT be silently reweighted.
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

### Non-Functional Requirements

- NFR-1: p95 optimization MUST be <= 500 ms for 8 courses with up to 10 groups
  each under approved hardware/load.
- NFR-2: Same inputs/configuration MUST produce the same ordering.
- NFR-3: Time-budget expiry MUST return a safe status, not an unbounded task.
- NFR-4: Unit coverage for optimizer branches MUST be at least 90%.

### Key Entities

- **SchedulePreferences**: Registration-module owned transient value object.
- **ScheduleOption**: Registration-module transient value; it is not a durable entity and is transported through a protected option token.
- **ScoreComponent**: Registration-module owned transient scoring value.
- **OptimizerConfiguration**: Registration-module owned immutable factor-order/version value.
- **OptimizationDiagnostic**: Registration-module transient value containing a deterministic inclusion-minimal blocking set, reasons, intervals, and actions.

## Success Criteria

- **SC-1**: Every returned option contains exactly one viable group per selected course and no hard conflict.
- **SC-2**: Up to three options are ranked deterministically with understandable score reasons.
- **SC-3**: A no-solution or time-budget outcome never presents an incomplete schedule as valid.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-011](../011-eligibility-subject-discovery/spec.md)
- [SPEC-012](../012-schedule-builder-conflicts/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| STU-04 | /student/schedule | ScheduleBuilderPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-012 |

## Out of Scope

- OS-1: Machine learning/AI ranking.
- OS-2: Institution-wide timetable generation or changing published resources.
- OS-3: Guaranteed seat/reservation.
- OS-4: OR-Tools dependency until benchmark evidence requires it.
