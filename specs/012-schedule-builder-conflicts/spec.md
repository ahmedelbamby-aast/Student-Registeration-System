# Feature Specification: Schedule Builder and Conflicts

**Feature Branch**: 012-schedule-builder-conflicts
**Created**: 2026-07-12
**Status**: APPROVED
**Owner**: Technical Lead
**Normative detail**: [requirements.md](requirements.md)
**Credit-load amendment**: `spec012-credit-load/1.0`, approved 2026-07-16

## Context

A student may select groups with overlapping meeting intervals. The builder
must identify every hard conflict, support manual changes, and block review or
submission until the plan is valid.

## User Scenarios and Testing

### User Story 1 - Overlap detection (FR-2, FR-3, FR-4) (P1)

As a Student, I need the Overlap detection (FR-2, FR-3, FR-4) behavior so that Schedule Builder and Conflicts produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given Group A meets Monday 10:00-11:30 and Group B 11:00-12:00<br>
When both are selected<br>
Then a conflict is returned and displayed with red X, Conflict text, both
subjects, and 11:00-11:30 overlap.
### User Story 2 - Adjacent meetings (FR-2) (P1)

As a Student, I need the Adjacent meetings (FR-2) behavior so that Schedule Builder and Conflicts produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given Group A ends Monday 11:00 and Group B starts Monday 11:00<br>
When both are selected<br>
Then no overlap exists<br>
And the demo's disabled travel-buffer rule adds no conflict.
### User Story 3 - Submission blocked (FR-5) (P2)

As a Student, I need the Submission blocked (FR-5) behavior so that Schedule Builder and Conflicts produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given an unresolved hard conflict<br>
When the student opens Review<br>
Then submission is disabled with a visible reason and resolution links.
### User Story 4 - Versioned plan editing (FR-1, FR-6, FR-7, FR-8) (P2)

As a Student, I need the Versioned plan editing (FR-1, FR-6, FR-7, FR-8) behavior so that Schedule Builder and Conflicts produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given a current plan and advisory group capacity<br>
When the student selects a second group for one offering or saves with a stale
rowversion<br>
Then the invalid/stale update is rejected<br>
And the student can load the current plan and change/remove a group<br>
And every returned plan reports default target 18 and maximum 18.
### User Story 5 - Plan quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Student, I need the Plan quality gate (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Schedule Builder and Conflicts produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

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

## Requirements

### Functional Requirements

- FR-1: The student MUST add at most one group per course offering to a plan.
- FR-2: The server MUST detect overlap for every meeting slot using strict
  half-open interval logic; adjacent meetings do not overlap. The approved
  simple demo policy disables travel-buffer conflicts and MUST NOT guess a
  room/campus travel duration or matrix.
- FR-3: Each conflict MUST identify both groups, subjects, day, times, and
  exact overlap interval plus accessible change/remove actions.
- FR-4: The UI MUST render a red X plus text/icon-accessible conflict state.
- FR-5: Review/submission MUST be blocked while any hard conflict exists.
- FR-6: Students MUST be able to change/remove groups and see recalculated
  credits/conflicts. Every plan response MUST include server-composed
  `defaultTargetCredits=18` and `maximumAllowedCredits=18`. SPEC-012 has no
  overload path or GPA-derived 12-credit branch. Server-authored `loadReasons`
  MUST retain safe policy/source provenance.
- FR-7: One plan per authenticated student/term MUST persist server-side with
  rowversion and owner/term routes; direct-object access returns no data.
- FR-8: The client MUST treat capacity displayed in a plan as advisory until
  final submission revalidates it; a timestamped dependency-version snapshot
  identifies stale/full/closed/cancelled/unpublished selections and blocks review.

### Non-Functional Requirements

- NFR-1: Conflict recalculation SHOULD complete within 200 ms p95 for 8 courses
  with 10 meeting slots each.
- NFR-2: Conflict results MUST be deterministic.
- NFR-3: Calendar and chronological list MUST contain equivalent content.
- NFR-4: Plan editing MUST reject lost updates with 409.

### Key Entities

- **RegistrationPlan**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationPlanItem**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ScheduleConflict**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ValidationSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Every overlapping meeting interval is identified before review.
- **SC-2**: Unresolved hard conflicts always block submission with accessible resolution guidance.
- **SC-3**: Equivalent calendar and chronological views contain the same schedule information.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-011](../011-eligibility-subject-discovery/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| STU-04 | /student/schedule | ScheduleBuilderPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-012 |
| STU-05 | /student/review | RegistrationReviewPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-014 |

## Out of Scope

- OS-1: Creating/moving official staff or room schedules.
- OS-2: Automatic academic conflict override.
- OS-3: Final seat reservation while editing.
- OS-4: Optimized alternatives, covered by SPEC-013.
