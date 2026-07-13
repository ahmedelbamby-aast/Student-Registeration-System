# SPEC-012: Schedule Builder and Conflicts

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** APPROVED<br>
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
  half-open interval logic (`startA < endB && startB < endA`). Adjacent
  meetings do not conflict. The approved simple demo policy disables the
  travel-buffer rule, which MUST NOT create a conflict or guess a room/campus
  duration or matrix.
- FR-3: Each conflict MUST identify both groups, subjects, day, times, and
  exact overlap interval. It MUST include accessible resolution actions to
  change either group or remove either selection; actions are identifiers and
  authorized route targets, not untrusted HTML.
- FR-4: The UI MUST render a red X plus text/icon-accessible conflict state.
- FR-5: Review/submission MUST be blocked while any hard conflict exists.
- FR-6: Students MUST be able to change/remove groups and see recalculated
  credits/conflicts through an explicit GET, versioned PUT, and non-mutating
  validate contract.
- FR-7: One active RegistrationPlan per authenticated student and term MUST
  persist server-side and use rowversion. Owner routes use the authorized
  term, not an arbitrary client-owned plan identifier; direct-object access to
  another student's plan returns no data.
- FR-8: The client MUST treat capacity displayed in a plan as advisory until
  final submission revalidates it. Every plan response MUST include a
  timestamped ValidationSnapshot with the academic context, policy,
  catalogue, offering, and selected SectionGroup versions used; any changed,
  full, closed, cancelled, or unpublished group is returned as stale/invalid
  with change/remove actions and blocks review.

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
Then no overlap exists<br>
And the demo's disabled travel-buffer rule adds no conflict.

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

### AC-5: Plan quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given an eight-course/ten-groups-per-course plan, fixed meeting data, equivalent
calendar/list fixtures, and two concurrent editors<br>
When schedule performance, determinism, equivalence, and rowversion tests run<br>
Then recalculation is at most 200 ms p95<br>
And conflict output is deterministic<br>
And calendar/list content is identical<br>
And one stale editor receives 409 without a lost update.

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
  first: { groupId: string; groupCode: string; courseCode: string; subjectTitle: string; startLocal: string; endLocal: string };
  second: { groupId: string; groupCode: string; courseCode: string; subjectTitle: string; startLocal: string; endLocal: string };
  dayOfWeek: number;
  overlapStartLocal: string;
  overlapEndLocal: string;
  message: string;
  actions: Array<{ action: "change-group" | "remove-group"; targetGroupId: string; label: string; route: string }>;
}
interface ValidationSnapshotDto {
  evaluatedAtUtc: string;
  academicContextVersion: string;
  policyVersion: string;
  catalogueVersion: string;
  offeringVersions: Record<string, string>;
  groupVersions: Record<string, string>;
}
interface RegistrationPlanDto {
  id: string;
  termId: string;
  rowVersion: string;
  selectedGroups: GroupDto[];
  totalCredits: number;
  conflicts: ScheduleConflictDto[];
  validation: ValidationSnapshotDto;
  reviewBlocked: boolean;
}
interface RegistrationPlanMutationRequest { expectedPlanRowVersion: string; selectedGroupIds: string[]; }
```

Endpoints: GET and PUT /api/student/terms/{termId}/registration-plan, and POST
/api/student/terms/{termId}/registration-plan/validate. PUT atomically replaces
the selected group set, recalculates credits/conflicts/validation, and returns
the new plan. A stale version returns 409 STALE_VERSION with the current plan
only to its authorized owner. Validate never reserves a seat or mutates the
plan.

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
