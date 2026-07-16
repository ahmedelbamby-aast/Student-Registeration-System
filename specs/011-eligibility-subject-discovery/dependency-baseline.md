# SPEC-011 Dependency Baseline

This record freezes the upstream contracts consumed by SPEC-011. It does not
copy their ownership into Registration, widen Gate A approval, or treat a
documented contract as proof that every required provider port or runtime
behavior already exists.

## T001 - SPEC-002 demo policy, reasons, provenance, and exclusions

- `DEMO-POC-2026.1` uses a regular submitted-plan range of 9 through 18
  credits, displays 18 as the default target, applies 18 as the hard normal
  maximum, and applies a 12-credit hard maximum when GPA is below 2.0. The
  approved boundaries are 18/19 and 12/13; GPA exactly 2.0 uses the normal
  limit.
- The required ordered categories are `RegistrationWindow`,
  `AcademicStanding`, `BlockingHold`, `Prerequisite`, `CreditLoad`,
  `ProbationLoad`, `RepeatEligibility`, `Capacity`, and `MeetingConflict`.
  Their stable codes include the pass/fail pairs recorded in
  `schemas/policy-rule-types.json`, including
  `MINIMUM_EARNED_CREDITS_NOT_MET`, `GROUP_FULL`, `MEETING_CONFLICT`, and
  `REPEAT_POLICY_UNAVAILABLE`.
- The approved curriculum is exactly the 19 rows in
  `docs/DEMO_CURRICULUM.md`. Code, title, sequence, and displayed
  prerequisites/conditions reference `SRC-DATA-SCIENCE`; every local
  `Credits=3` value is `SyntheticDemo` and references `DEMO-CREDITS-3`.
- Every decision is deterministic for the same complete input, immutable
  policy version, and authoritative instant. Reasons retain stable ordering,
  source reference/access date, policy approval/effective metadata, safe input
  summary, and `overridePossible=false` for blocking demo outcomes. Missing,
  ambiguous, expired, unapproved, or provenance-incomplete policy data fails
  closed.
- Repeat interpretation, advisor selection/approval, overload/underload
  exceptions, prerequisite exceptions, waitlists, reservations, overrides,
  add/drop, withdrawal, correction, and travel-buffer rules remain disabled or
  unresolved outside the demo. They cannot fall through to eligibility.

Evidence:
[rule coverage](../002-aastmt-policy-rulebook/contracts/policy-rule-coverage.md),
[reason registry](../002-aastmt-policy-rulebook/schemas/policy-rule-types.json),
[boundary examples](../002-aastmt-policy-rulebook/policy-boundary-examples.md),
[source register](../002-aastmt-policy-rulebook/policy-sources.md),
[decision contract](../002-aastmt-policy-rulebook/contracts/policy-decision.md),
and [approval/fail-closed gate](../002-aastmt-policy-rulebook/contracts/policy-approval-gate.md).

## T002 - SPEC-003 STU-02/STU-03 design and test obligations

- `STU-02` is `/student/subjects`, implemented by SPEC-011. It covers eligible
  and unavailable results, search/filter/page behavior, no-results/reset,
  stale capacity, service failure, normal-18 and probation-12 presentation,
  complete reason text/codes, and server-authored group details.
- `STU-03` is `/student/subjects/{offeringId}`, implemented by SPEC-011 with
  SPEC-010 as a data contributor. It covers open, nearly-full, full, changed,
  unpublished, selected-intent, normal-18, and probation-12 journeys.
  Selection is navigation intent to STU-04; STU-03 performs no plan write.
- Both records require loading, empty, success, validation-error,
  service-error, unauthorized, session-expired, stale, and offline states,
  safe retry/recovery, stable reason preservation, keyboard/focus/live-region
  behavior, text and icon independent of color, 320-1920 CSS-pixel reflow, and
  400% zoom behavior.
- SPEC-011 owns feature-functional E2E and non-color evidence through
  T039-T041/T048. SPEC-003 separately governs the product-wide route contract,
  component, accessibility, and visual families in T153-T161. Their current
  `not-pinned` state is recorded honestly but does not block the bounded
  SPEC-011 feature task/release evidence.

Evidence:
[page matrix](../003-ux-storyboard-accessibility/page-matrix.md),
[STU-02 design record](../003-ux-storyboard-accessibility/design/pages/STU-02.md),
[STU-03 design record](../003-ux-storyboard-accessibility/design/pages/STU-03.md),
and [SPEC-003 tasks](../003-ux-storyboard-accessibility/tasks.md).

## T003 - SPEC-006 pagination and SPEC-008 academic context

- The canonical response is `StudentRegistration.Contracts.Page<T>` with
  serialized `items`, `page`, `pageSize`, `totalCount`, and `sort`. Omitted
  page values mean 1/20, the maximum page size is 100, invalid values return
  `400 PAGE_SIZE_INVALID` without silent capping, and `sort` echoes the applied
  canonical sort ending in a unique-ID tie-break.
- The server derives the authenticated student. SPEC-008 supplies authoritative
  UTC time, term/window context, program, cohort, GPA, earned credits,
  standing, transcript attempts, the complete active-hold set, academic
  provenance, and opaque data/row versions. A request at a window close instant
  is closed; missing or ambiguous context fails safely.
- `StudentTermAcademicState` is the shared student/term serialization boundary
  and has its own SQL rowversion. The current self DTO exposes `dataVersion`,
  while the provider store snapshot also carries the exact Student and
  StudentTermAcademicState rowversions. SPEC-011 needs a narrow, read-only
  Academics provider-port projection rather than consuming EF state or another
  student's endpoint DTO.
- Missing profile fields/provenance, more than 100 active holds, unavailable
  storage, or an unresolved context must not create an eligible result.

Evidence:
[SPEC-006 API contract](../006-domain-class-api-contracts/contracts/api.md),
[canonical Page source](../../src/StudentRegistration.Contracts/Page.cs),
[SPEC-008 API contract](../008-academic-term-student-profile/contracts/api.md),
[SPEC-008 data model](../008-academic-term-student-profile/data-model.md), and
[academic profile provider contract](../../src/StudentRegistration.Academics/Application/Ports/IStudentAcademicProfileStore.cs).

## T004 - SPEC-009 catalogue and PolicySet versions

- SPEC-009 alone owns `Program`, `Course`, `CurriculumCourse`,
  `CoursePrerequisite`, `CatalogueDraft`, immutable `CatalogueVersion`,
  `PolicySet`, and `PolicyRule`. SPEC-011 consumes one published catalogue and
  one approved, published, effective, scope-matching policy set; it does not
  redefine or edit those aggregates.
- Course discovery needs published course identity, normalized code, title,
  credits, active/program membership, prerequisite graph/conditions, field
  provenance, and the immutable catalogue version. Policy input needs PolicySet
  ID/version/effective scope plus typed rules, their stable reason codes, safe
  values, and source metadata.
- Published catalogue and policy versions are immutable. Missing, ambiguous,
  superseded-only, unapproved, or provenance-incomplete inputs fail closed.
- Current runtime caution: `PolicyAdministrationService` is an Admin
  simulation surface and does not include the upstream `BlockingHold` or
  `RepeatEligibility` categories. SPEC-011 therefore does not use it as the
  eligibility engine. The T007-frozen Academics reader supplies published
  policy/catalogue identity, typed values, provenance, and versions, while
  SPEC-011 evaluates the complete governed SPEC-002 registry and fails closed
  if the supplied PolicySet is incomplete.

Evidence:
[SPEC-009 data model](../009-catalog-prerequisites-policy-admin/data-model.md),
[SPEC-009 API contract](../009-catalog-prerequisites-policy-admin/contracts/api.md),
[SPEC-009 requirements](../009-catalog-prerequisites-policy-admin/requirements.md),
and [current policy service](../../src/StudentRegistration.Academics/Application/PolicyAdministrationService.cs).

## T005 - SPEC-010 offering and group projections

- SPEC-010 owns `CourseOffering`, `SectionGroup`, `MeetingSlot`, `Room`,
  `GroupStaffAssignment`, capacity/enrollment state, and their concurrency
  versions. Only published offerings are eligible for discovery.
- A student group projection preserves group ID/code, lifecycle state,
  `registrationPaused`, capacity, enrolled count, derived selectability,
  SectionGroup rowversion, and every meeting's activity type, day, local
  start/end, room code/location, plus meeting-bound Lecturer/TeachingAssistant
  names. `Tutorial` is canonical and `Section` is display text only.
- Full, unpublished, closed, cancelled, paused, or incomplete groups are not
  selectable. Capacity/version data is advisory for discovery; details and
  final submission re-read the authoritative group.
- The existing Scheduling student detail preserves the meeting-to-staff
  relationship. SPEC-011's planned flattened `GroupSummaryDto.staff` shape
  does not, which is a readiness issue recorded below rather than a reason to
  weaken the upstream baseline.

Evidence:
[SPEC-010 requirements](../010-offerings-groups-resources/requirements.md),
[SPEC-010 data model](../010-offerings-groups-resources/data-model.md),
[SPEC-010 API contract](../010-offerings-groups-resources/contracts/api.md),
[Scheduling DTOs](../../src/StudentRegistration.Contracts/Scheduling/SchedulingContracts.cs),
and [student offering projection](../../src/StudentRegistration.Scheduling/Application/OfferingService.cs).

## T006 - SPEC-018 quality, security, accessibility, and operations

- The blocking target is 10 minutes at 75 registration submissions/s plus 300
  reads/s across at least two stateless API replicas; the required spike is 60
  seconds at 200 submissions/s. The read mix is 50% offering discovery, 25%
  eligibility detail, 15% plan/timetable, and 10% registration records.
- Catalogue/discovery p95 is at most 300 ms and unexpected server failures are
  below 0.1%. Correctness invariants remain zero-defect; optional 2x, 5x, and
  120-minute soak profiles are diagnostic only.
- Protected reads require authentication, Student role,
  `Catalogue.ReadAvailable`, server-derived self/context scope, and negative
  role/ownership/direct-object tests. Safe errors, logs, metrics, traces,
  correlation IDs, and evidence contain no credential, full student profile,
  raw claim set, SQL detail, or unauthorized identifier/version.
- Critical routes require automated accessibility plus recorded keyboard and
  representative NVDA/Windows evidence. Status never relies on color alone.
  Release also requires current threat-model review, safe observability,
  migration/rollback checks, backup/restore rehearsal, RPO at most 5 minutes,
  and RTO at most 1 hour.
- SPEC-018's product-wide load, accessibility, recovery, trace, and release
  tasks remain pending. SPEC-011 may produce its feature evidence, but cannot
  claim those wider gates are complete.

Evidence:
[SPEC-018 requirements](../018-quality-security-scalability-operations/requirements.md),
[SPEC-018 plan](../018-quality-security-scalability-operations/plan.md),
[governed permission](../001-product-charter-rbac/contracts/permissions.md),
and [runtime authorization policy](../../src/StudentRegistration.IdentityAccess/Application/Authorization/RolePolicies.cs).
