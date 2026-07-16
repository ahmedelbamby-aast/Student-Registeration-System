# SPEC-012 Implementation Readiness Gate

**Result:** PASS for the approved core demo baseline  
**Scope:** T001-T005 only; no implementation or release evidence is completed

Ahmed ELbamby's 2026-07-13 Gate A approval remains current. The approved
baseline is ready for dependency-ordered test-first work. The credit-load
amendment was separately approved on 2026-07-16. Gate B-D, release, production,
and official AASTMT authorization remain separate.

## Approved decisions and ownership

- [x] **Travel behavior is fixed.** Meeting conflicts use strict half-open
  overlap (`startA < endB && startB < endA`), so adjacent meetings do not
  overlap. `TRAVEL_BUFFER` remains a reserved future code and is not emitted by
  the demo. No campus matrix or duration is guessed.
- [x] **Ownership is singular.** SPEC-012 owns `RegistrationPlan`,
  `RegistrationPlanItem`, `ScheduleConflict`, and `ValidationSnapshot` in the
  Registration module. SPEC-010 retains scheduling aggregates, SPEC-011 owns
  immutable eligibility projections, SPEC-004 remains the sole shared
  DbContext writer, and SPEC-014 revalidates at submission.
- [x] **Persistence is simple.** One active plan exists per authenticated
  student and term. The plan root owns item replacement and rowversion;
  `RegistrationPlanItem` is unique by plan/offering and carries selected group
  plus captured offering/group versions. No independently patched item,
  duplicate plan aggregate, repository abstraction, service, or database is
  introduced.

Evidence: [research](../research.md), [data model](../data-model.md),
[dependency baseline](../dependency-baseline.md),
[entity ownership manifest](../../../.specify/entity-ownership.json), and
[persistence manifest](../../../.specify/persistence-manifest.json).

## Plan routes, versions, and authority

- [x] The SPEC-012 endpoint registry contains exactly:
  `GET /api/student/terms/{termId}/registration-plan`,
  `PUT /api/student/terms/{termId}/registration-plan`, and
  `POST /api/student/terms/{termId}/registration-plan/validate`.
- [x] The server derives the student from authentication and authorizes the
  requested term. Routes do not accept an arbitrary client-owned plan ID;
  another student's object returns no protected data or version.
- [x] PUT is a complete atomic replacement of `selectedGroupIds`, requires
  `expectedPlanRowVersion`, enforces at most one group per offering,
  recalculates plan state, and advances one root version. An authorized stale
  write returns `409 STALE_VERSION` with the current authorized plan and does
  not lose the winning update.
- [x] Validate is non-mutating. GET, PUT, and Validate never reserve a seat;
  displayed capacity remains advisory until SPEC-014 revalidates submission.

Evidence: [requirements FR-1/FR-6/FR-7/FR-8](../requirements.md),
[data model](../data-model.md), [API contract](../contracts/api.md),
[endpoint manifest](../../../.specify/endpoint-manifest.json), and
[workstream manifest](../../../.specify/workstream-manifest.json).

## Conflict details, actions, snapshots, and blocking

- [x] Every `MEETING_OVERLAP` identifies both group IDs/codes, course codes,
  subject titles, original local intervals, day, exact overlap start/end,
  stable code/message, and actions. Duplicate slots are de-duplicated by stable
  identity before deterministic pair evaluation.
- [x] Resolution actions allow changing or removing either affected selection
  and carry only action kind, target group ID, accessible label, and a
  server-authored authorized route target. They are identifiers, not HTML. The
  client emits the action and revalidates; no action implies an override or a
  guaranteed alternative.
- [x] `ValidationSnapshot` records evaluated UTC time plus academic context,
  policy, catalogue, every selected offering, and every selected group version.
  It is advisory and timestamped, not authority to skip a current recheck.
- [x] Any hard conflict blocks Review/submission. A selected group that is
  changed, full, closed, cancelled, or unpublished also returns a safe
  change/remove path and blocks Review. No stale or invalid selection is
  silently replaced.

Evidence: [requirements FR-2/FR-3/FR-5/FR-8](../requirements.md),
[data model](../data-model.md), [API contract](../contracts/api.md),
[SPEC-003 conflict panel](../../003-ux-storyboard-accessibility/design/components/conflict-panel.md),
and [SPEC-010 baseline](../dependency-baseline.md).

## Frontend contribution and accessibility

- [x] `STU-04 /student/schedule` is designed by SPEC-003 and implemented by
  SPEC-012 at `ScheduleBuilderPage.razor`. SPEC-012 renders selected groups,
  equivalent calendar/list data, conflicts, manual actions, and Continue state;
  SPEC-013 remains the recommendation contributor.
- [x] `STU-05 /student/review` is implemented by SPEC-014. SPEC-012 contributes
  validated plan/conflict/snapshot data and resolution links only; T044 must not
  edit its Razor page.
- [x] Conflict presentation uses red X plus visible/announced **Conflict**,
  complete subject/group/interval text, all blockers in text, keyboard-native
  change/remove links, declared focus/live-region behavior, and equivalent
  chronological content. `STU-04` remains `design-only` until its exact
  contributor pins and executable evidence satisfy SPEC-003 promotion rules.

Evidence: [route manifest](../../../.specify/route-manifest.json),
[page API manifest](../../../.specify/page-api-manifest.json),
[STU-04 design](../../003-ux-storyboard-accessibility/design/pages/STU-04.md),
[STU-05 design](../../003-ux-storyboard-accessibility/design/pages/STU-05.md),
and [dependency baseline T001](../dependency-baseline.md).

## Complete task trace

- [x] **Models and contracts:** T007-T010 create failing ownership/invariant
  checks; T032-T035 deliver the four canonical models. Endpoint contract
  finalization/tests are T011-T016, followed by handlers T039-T041.
- [x] **Functional requirements:** FR-1 is covered by T007/T008/T020/T029/
  T032/T033/T036/T042/T043; FR-2 by T017/T018/T030/T037/T042/T043; FR-3 by
  T009/T017/T030/T034/T037/T042/T043; FR-4 by T009/T017/T031/T034/T038/
  T042/T043; FR-5 by T019/T027/T031/T038/T044/T045; FR-6/FR-7/FR-8 by
  T007-T010/T012/T014/T016/T020/T023/T024/T029/T032-T036/T039-T045 as their
  task tags declare.
- [x] **Acceptance and edge behavior:** AC-1 through AC-5 map one-to-one to
  T017-T021. EC-1 through EC-4 map one-to-one to T022-T025. SC-1 through SC-3
  map one-to-one to T026-T028.
- [x] **Consolidated workstreams:** T029-T031 create failing versioned-plan,
  interval-detector, and accessible-blocking-state suites before T036-T038
  deliver them. Endpoint handlers and page work retain their declared failing
  test prerequisites.
- [x] **Quality and persistence:** T046 fails before T047 writes the SPEC-012
  mapping/migration contribution. NFR-1 through NFR-4 map to T048-T051; scope
  exclusions map to T052; the final full trace and release approvals remain
  T053-T054.

Evidence: [tasks](../tasks.md), [requirements](../requirements.md),
[plan](../plan.md), [workstream manifest](../../../.specify/workstream-manifest.json),
and [persistence manifest](../../../.specify/persistence-manifest.json).

## Approved amendment

- [x] `spec012-credit-load/1.0` was approved by Ahmed ELbamby on 2026-07-16.
  Every plan response carries fixed `defaultTargetCredits=18`, fixed
  `maximumAllowedCredits=18`, and safe server-authored `loadReasons` with
  policy/source provenance.
- [x] The prior 12/18 GPA branch is superseded. No overload path is introduced,
  and the browser derives neither values nor reasons. T012, T014, T016, T020,
  T029, and T042 may proceed in their existing dependency order; approval does
  not itself complete any task or runtime behavior.

Evidence: [approval amendment](approval.md),
[requirements](../requirements.md), and [API contract](../contracts/api.md).

## Gate conclusion

T001-T005 have direct evidence and pass. T006 remains the immutable recorded
Gate A approval. Test-first work may proceed only in task dependency order.
The amendment approval removes its former block but completes no later
implementation, test, migration, frontend, quality, or release task.
