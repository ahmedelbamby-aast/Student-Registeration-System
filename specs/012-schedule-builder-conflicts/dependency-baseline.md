# SPEC-012 Dependency Baseline

**Recorded:** 2026-07-16  
**Feature:** SPEC-012 Schedule Builder and Conflicts  
**Result:** PASS for the approved non-production demo baseline

This baseline records only contracts consumed by SPEC-012. It does not promote
any SPEC-003 route from `design-only`, complete an upstream runtime task, or
authorize Gate B-D, release, production deployment, or official AASTMT use.

## T001 - SPEC-003 conflict, timetable, accessibility, and test contracts

- `STU-04 /student/schedule` has design owner SPEC-003 and implementation owner
  SPEC-012. Its immutable Page Design Record is version `1.0`, governed by the
  SPEC-003 design index `1.1`, and remains `design-only` with downstream
  contributor contracts `not-pinned`. SPEC-012 owns the page implementation;
  SPEC-013 contributes recommendations without changing that ownership.
- `STU-05 /student/review` is implemented by SPEC-014. SPEC-012 contributes the
  validated plan, hard-conflict blockers, and edit/resolution links, but MUST
  NOT edit `RegistrationReviewPage.razor`.
- A supplied conflict is presented with a red X and the visible word
  **Conflict**; color is never the only meaning. Both subject/group labels and
  every overlap day/start/end are shown, with a plain-language explanation and
  keyboard-operable change/remove actions. The panel presents server data and
  emits actions; it does not detect overlaps, invent missing details, rank
  alternatives, or permit an override.
- Calendar and chronological list render the same canonical meeting
  collection. Both expose subject, group, activity kind, supplied staff,
  room/location, day, local interval, timezone context, conflict text, and the
  owning-route action. The list order is day, start, end, subject code, group
  code, then stable meeting ID. Narrow layouts and 400% zoom keep the list
  available; view switching never recalculates or changes schedule meaning.
- Required UI states are loading, empty, success, validation error, recoverable
  service error, unauthorized, session expired, stale/concurrent change, and
  offline. Conflict/stale updates preserve focus and use the declared live
  region; submitted validation focuses the summary. Controls use native
  keyboard behavior and 44-by-44 CSS-pixel targets except a documented WCAG
  exception.
- Route evidence must cover component state, API contract, primary/failure
  browser journeys, automated accessibility, and approved visual baselines.
  SPEC-018 separately requires dated manual keyboard and NVDA/Windows evidence;
  automation alone does not satisfy the release gate.

Evidence: [SPEC-003 requirements](../003-ux-storyboard-accessibility/requirements.md),
[page matrix](../003-ux-storyboard-accessibility/page-matrix.md),
[STU-04 design](../003-ux-storyboard-accessibility/design/pages/STU-04.md),
[STU-05 design](../003-ux-storyboard-accessibility/design/pages/STU-05.md),
[conflict panel](../003-ux-storyboard-accessibility/design/components/conflict-panel.md),
[timetable equivalence](../003-ux-storyboard-accessibility/design/components/timetable-equivalence.md),
and [route contributor baseline](../003-ux-storyboard-accessibility/checklists/route-contributor-baseline.md).

## T002 - SPEC-010 scheduling data and version boundary

- SPEC-010 is the canonical owner of `CourseOffering`, `SectionGroup`,
  `MeetingSlot`, `Room`, and `GroupStaffAssignment`. SPEC-012 consumes these
  through a narrow Scheduling boundary and does not copy their aggregates.
- `GroupDto` supplies group and offering IDs, group code, capacity, enrolled
  count, registration-pause flag, lifecycle state, server-authored selectable
  flag/reasons, meeting-bound staff, meetings, and SectionGroup rowversion.
  Lifecycle state is `draft | published | closed | cancelled`; Full is derived
  from capacity and is not another lifecycle value.
- Each meeting supplies stable ID, canonical `Lecture | Tutorial | Laboratory`
  activity, day, local start/end, room ID/code/location, and its meeting-bound
  staff. `Tutorial` remains the stored/API value; `Section` is display text
  only. Array order never associates a staff member with an activity.
- Student non-selectable reasons are `GROUP_FULL`, `GROUP_UNPUBLISHED`,
  `GROUP_CLOSED`, `GROUP_CANCELLED`, or `REGISTRATION_PAUSED`.
  `GROUP_CHANGED` is a concurrency result, not an ordinary GET state.
- Every group-state, meeting-slot, room-assignment, or staff-assignment change
  locks and advances the owning SectionGroup rowversion. Capacity edits and
  final seat allocation serialize on that same boundary and preserve
  `0 <= EnrolledCount <= Capacity`.
- Plan capacity is advisory. SPEC-012 captures offering/group versions,
  re-resolves current Scheduling state for validation, and never reserves a
  seat while editing or validating.

Evidence: [SPEC-010 requirements](../010-offerings-groups-resources/requirements.md),
[data model](../010-offerings-groups-resources/data-model.md),
[API contract](../010-offerings-groups-resources/contracts/api.md), and
[implementation readiness](../010-offerings-groups-resources/checklists/implementation-readiness.md).

## T003 - SPEC-011 eligible offering, group, and explanation boundary

- SPEC-011 returns only server-evaluated eligibility. Default discovery lists
  eligible offerings with at least one published selectable group; unavailable
  detail retains the complete blocking explanation and advisory group state.
- `GroupSummaryDto` supplies group ID/code, lifecycle-only state, selectable
  flag, capacity, enrolled count, seats remaining, non-selectable reasons,
  meetings with nested staff/room/local-time data, and SectionGroup rowversion.
- `OfferingEligibilityDto` supplies offering/course identity, credits,
  eligibility, ordered reasons, bounded groups, evaluated UTC time, and
  academic/catalogue/policy/offering/group/current-plan dependency versions.
  Discovery is advisory; plan validation and SPEC-014 submission re-resolve
  authoritative state.
- Each `EligibilityReasonDto` preserves stable code, passed/blocking flags,
  safe message and values, PolicySet ID/version, source reference/access date,
  approval/effective metadata, no-override state for blockers, and an optional
  support path. Missing decision data fails closed rather than creating
  eligibility.
- Registration owns `ICurrentPlanReader`. SPEC-012 contributes the real
  versioned plan reader to replace SPEC-011's initial versioned-empty adapter;
  it does not create a second plan aggregate or let SPEC-011 write the plan.
- SPEC-011's discovery load fields remain valid on its own responses. SPEC-012
  independently approved `spec012-credit-load/1.0` on 2026-07-16: plan
  responses use fixed 18/18 values plus server-authored sourced load reasons,
  with no overload or GPA-derived 12-credit branch.

Evidence: [SPEC-011 requirements](../011-eligibility-subject-discovery/requirements.md),
[data model](../011-eligibility-subject-discovery/data-model.md),
[API contract](../011-eligibility-subject-discovery/contracts/api.md),
[dependency baseline](../011-eligibility-subject-discovery/dependency-baseline.md),
and [implementation readiness](../011-eligibility-subject-discovery/checklists/implementation-readiness.md).

## T004 - SPEC-018 quality, security, accessibility, concurrency, and operations

- Critical domain rules require boundary and concurrency tests before
  implementation acceptance. For SPEC-012 this includes two editors starting
  from one plan rowversion: one complete replacement succeeds, the stale write
  receives `409 STALE_VERSION` with only the authorized current plan, and no
  update is lost.
- SPEC-012's feature budget remains conflict recalculation at most 200 ms p95
  for eight courses with ten meeting slots each. Product-wide target traffic
  additionally includes 15% current-plan/timetable reads within 300 reads/s,
  runs for ten minutes with 75 submissions/s, and uses at least two stateless
  API replicas. The required spike is 200 submissions/s for 60 seconds; 2x,
  5x, and soak profiles are optional diagnostics.
- Protected plan routes require server-validated Student role, ownership, term
  scope, and negative direct-object tests. Authorization occurs before
  protected existence/version disclosure. Errors, logs, traces, metrics, and
  evidence expose no credentials, full student profile, SQL detail, secret,
  raw claim set, or unauthorized identifier/version.
- Critical routes must meet WCAG 2.2 AA with automated checks plus dated manual
  keyboard and representative NVDA/Windows evidence. Browser gates cover
  current stable Chrome, Edge, Firefox, and pinned Playwright WebKit; WebKit is
  not labeled Safari.
- Operations expose privacy-safe health, structured logs, metrics, traces, and
  correlation IDs. Monitoring includes latency, throughput, unexpected errors,
  business rejection codes, SQL latency, lock waits, deadlocks, capacity
  conflicts, and reconciliation. Conflict/eligibility/capacity code targets at
  least 90% branch coverage, but coverage never replaces behavior tests.
- Real-SQL evidence uses isolated per-run SQL Server 2022 Developer
  compatibility-160 databases, migrations before synthetic seed, and disposal
  after the run. Gate B-D security, accessibility, load, recovery, and release
  evidence remain separate and cannot be inferred from this baseline.

Evidence: [SPEC-018 requirements](../018-quality-security-scalability-operations/requirements.md),
[plan](../018-quality-security-scalability-operations/plan.md),
[research](../018-quality-security-scalability-operations/research.md),
[API contract](../018-quality-security-scalability-operations/contracts/api.md),
and [implementation readiness](../018-quality-security-scalability-operations/checklists/implementation-readiness.md).

## Reconciled boundary

The direct dependency graph `003/010/011/018 -> 012` is valid and acyclic.
SPEC-012 owns plan persistence and conflict calculation, consumes Scheduling
and eligibility through narrow contracts, and contributes only its declared
data to shared frontend routes. No dependency authorizes a client-side
academic decision, a seat reservation during editing, a conflict override, a
guessed travel rule, or a new service/database boundary.
