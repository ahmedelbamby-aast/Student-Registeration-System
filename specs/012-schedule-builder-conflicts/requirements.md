# SPEC-012: Schedule Builder and Conflicts

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Technical Lead<br>
**Reviewers:** UX, Registrar/Policy SME, Backend, QA<br>
**Target:** Sprint 4<br>
**Dependencies:** SPEC-003, SPEC-010, SPEC-011, SPEC-018<br>

## Context

A student may select groups with overlapping meeting intervals. The builder
must identify every hard conflict, support manual changes, and block review or
submission until the plan is valid.

## Functional Requirements

- FR-1: The student MUST add at most one group per course offering to a plan.
- FR-2: The server MUST detect overlap for every meeting slot using strict
  interval logic.
- FR-3: Each conflict MUST identify both groups, subjects, day, times, and
  resolution links.
- FR-4: The UI MUST render a red X plus text/icon-accessible conflict state.
- FR-5: Review/submission MUST be blocked while any hard conflict exists.
- FR-6: Students MUST be able to change/remove groups and see recalculated
  credits/conflicts.
- FR-7: Plans MUST persist server-side and use rowversion.
- FR-8: Capacity displayed in a plan is advisory until final submission.

## Non-Functional Requirements

- NFR-1: Conflict recalculation SHOULD complete within 200 ms p95 for 8 courses
  with 10 meeting slots each.
- NFR-2: Conflict results MUST be deterministic.
- NFR-3: Calendar and chronological list MUST contain equivalent content.
- NFR-4: Plan editing MUST reject lost updates with 409.

## Acceptance Criteria

### AC-1: Overlap detection (FR-2, FR-3, FR-4)
Given Group A meets Monday 10:00-11:30 and Group B 11:00-12:00<br>
When both are selected<br>
Then a conflict is returned and displayed with red X, Conflict text, both
subjects, and 11:00-11:30 overlap.

### AC-2: Adjacent meetings (FR-2)
Given Group A ends Monday 11:00 and Group B starts Monday 11:00<br>
When both are selected<br>
Then no overlap exists unless an approved travel-buffer rule applies.

### AC-3: Submission blocked (FR-5)
Given an unresolved hard conflict<br>
When the student opens Review<br>
Then submission is disabled with a visible reason and resolution links.

### AC-4: Versioned plan editing (FR-1, FR-6, FR-7, FR-8)
Given a current plan and advisory group capacity<br>
When the student selects a second group for one offering or saves with a stale
rowversion<br>
Then the invalid/stale update is rejected<br>
And the student can load the current plan and change/remove a group.

## Edge Cases

- EC-1: Multi-slot group conflicts on only one day -> group is still hard
  conflict.
- EC-2: Stale plan version -> reject update and return current plan.
- EC-3: Selected group unpublished/full -> mark stale/unavailable and require
  change; no silent replacement.
- EC-4: Same meeting slot duplicate data -> publication validation prevents it;
  builder de-duplicates defensively.

## API Contracts

```typescript
interface ScheduleConflictDto {
  code: "MEETING_OVERLAP" | "TRAVEL_BUFFER";
  firstGroupId: string;
  secondGroupId: string;
  dayOfWeek: number;
  overlapStartLocal: string;
  overlapEndLocal: string;
  message: string;
}
interface RegistrationPlanDto {
  id: string;
  termId: string;
  rowVersion: string;
  selectedGroups: GroupDto[];
  totalCredits: number;
  conflicts: ScheduleConflictDto[];
}
```

Endpoints: GET/PUT /api/student/registration-plans/{id}; POST validate.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| RegistrationPlan | aggregate | unique active student + term; rowversion |
| RegistrationPlanItem | entity | unique plan + offering |
| PreferredGroupId | identifier | must belong to selected offering |
| ValidationSnapshot | JSON/value | advisory only; timestamped |

## Out of Scope

- OS-1: Creating/moving official staff or room schedules.
- OS-2: Automatic academic conflict override.
- OS-3: Final seat reservation while editing.
- OS-4: Optimized alternatives, covered by SPEC-013.
