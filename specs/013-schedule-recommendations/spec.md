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
And the minimal/useful conflicting course set and manual actions are shown.
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

## Edge Cases

- EC-1: Group becomes full during search -> may be excluded from fresh query;
  final commit remains authority.
- EC-2: One selected course has zero viable groups -> immediate no-solution.
- EC-3: Equal scores -> stable tie-break by course/group identifiers.
- EC-4: Invalid preference weight -> reject configuration, use last approved.

## Requirements

### Functional Requirements

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

### Key Entities

- **SchedulePreferences**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ScheduleOption**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ScoreComponent**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **OptimizationDiagnostic**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Every returned option contains exactly one viable group per selected course and no hard conflict.
- **SC-2**: Up to three options are ranked deterministically with understandable score reasons.
- **SC-3**: A no-solution or time-budget outcome never presents an incomplete schedule as valid.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-012](../012-schedule-builder-conflicts/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Out of Scope

- OS-1: Machine learning/AI ranking.
- OS-2: Institution-wide timetable generation or changing published resources.
- OS-3: Guaranteed seat/reservation.
- OS-4: OR-Tools dependency until benchmark evidence requires it.
