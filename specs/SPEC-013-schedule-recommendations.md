# SPEC-013: Schedule Recommendations

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Technical Lead<br>
**Reviewers:** Product Owner, UX, QA, Performance owner<br>
**Target:** Sprint 5<br>

## Context

When selected groups conflict, the system should search other published groups
for the selected courses and propose the best feasible timetable. It must be
bounded, deterministic, explainable, and honest when no solution exists.

## Functional Requirements

- FR-1: The optimizer MUST choose exactly one published viable group per
  selected course.
- FR-2: It MUST enforce all hard meeting, availability, completeness,
  eligibility, credit, and configured travel-buffer constraints.
- FR-3: It MUST order constrained courses first and prune invalid partial
  schedules.
- FR-4: It SHOULD return up to three distinct feasible schedules.
- FR-5: It MUST score results with approved soft preferences and explain score
  components.
- FR-6: It MUST support cancellation and a configured computation time budget.
- FR-7: If no feasible result exists, it MUST return a useful conflict set and
  manual-resolution path.
- FR-8: Final submission MUST revalidate all results; a recommendation does not
  reserve seats.

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
Given two feasible schedules and one has fewer gaps under approved preferences<br>
When results are ranked<br>
Then that schedule ranks first<br>
And the gap and other score components are shown.

### AC-3: No solution (FR-7)
Given every combination has a hard overlap<br>
When search completes<br>
Then no fake solution is returned<br>
And the minimal/useful conflicting course set and manual actions are shown.

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

## Edge Cases

- EC-1: Group becomes full during search -> may be excluded from fresh query;
  final commit remains authority.
- EC-2: One selected course has zero viable groups -> immediate no-solution.
- EC-3: Equal scores -> stable tie-break by course/group identifiers.
- EC-4: Invalid preference weight -> reject configuration, use last approved.

## API Contracts

```typescript
interface ScheduleOptionDto {
  rank: number;
  groups: GroupDto[];
  score: number;
  scoreExplanation: Array<{ factor: string; value: number; message: string }>;
}
interface OptimizationResultDto {
  status: "complete" | "no-solution" | "time-budget";
  options: ScheduleOptionDto[];
  conflicts: ScheduleConflictDto[];
  evaluatedAtUtc: string;
}
```

Endpoint: POST /api/student/registration-plans/{id}/recommendations.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| SchedulePreferences | value object | approved bounded weights/ranges |
| ScheduleOption | transient result | complete one-group-per-course solution |
| ScoreComponent | value | factor, numeric contribution, explanation |
| OptimizationDiagnostic | transient | bounded/no-solution reason; no seat claim |

## Out of Scope

- OS-1: Machine learning/AI ranking.
- OS-2: Institution-wide timetable generation or changing published resources.
- OS-3: Guaranteed seat/reservation.
- OS-4: OR-Tools dependency until benchmark evidence requires it.
