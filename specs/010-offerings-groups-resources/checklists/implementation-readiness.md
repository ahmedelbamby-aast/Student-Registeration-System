# SPEC-010 Implementation Readiness Gate

**Result:** PASS  
**Scope:** Non-production demo implementation under the recorded Gate A
approval. Gate B-D, release, production, and official AASTMT authorization
remain separate.

## Decisions and ownership

- [x] **DEC-11 complete activity bundle is fixed.** A Published/Open selectable
  group has a Lecturer-staffed Lecture and at least one TA-staffed Tutorial or
  Laboratory; every present Tutorial/Laboratory has a TA. `Tutorial` is the
  canonical value and `Section` is display text only. Staff assignments target
  the existing meeting/activity, so no extra Activity entity is introduced.
  Evidence: [research](../research.md), [FR-2](../requirements.md),
  [data model](../data-model.md), [API contract](../contracts/api.md).
- [x] **DEC-12 availability boundary is fixed.** Staff mutate their own
  declarations through the SPEC-016 Scheduling contract. Admin may only view
  or import them as read-only planning inputs; no Admin create, replace, edit,
  correction, or override endpoint/control exists.
  Evidence: [research](../research.md), [FR-6 and AC-9](../requirements.md),
  [API contract](../contracts/api.md), [ADM-07 design](../../003-ux-storyboard-accessibility/design/pages/ADM-07.md).
- [x] **Canonical ownership is acyclic and singular.** SPEC-010 owns
  `CourseOffering`, `SectionGroup`, `MeetingSlot`, `Room`,
  `GroupStaffAssignment`, `StaffTermAvailability`, `StaffAvailability`, and
  `ScheduleImpactAlert`. `StaffAvailability` is a child range without an
  independent version; SPEC-016 consumes the parent aggregate. SPEC-009 retains
  Course/catalogue/policy ownership and SPEC-004 retains the shared DbContext
  and AuditEvent foundation.
  Evidence: [SPEC-010 data model](../data-model.md),
  [dependency baseline](../dependency-baseline.md),
  [entity ownership manifest](../../../.specify/entity-ownership.json),
  [persistence manifest](../../../.specify/persistence-manifest.json).

## Versions, locks, transactions, and alerts

- [x] Validation and publication bind expected `CourseOffering`,
  `SectionGroup`, `Room`, and `StaffTermAvailability` versions. Every group
  state, meeting, room assignment, or staff assignment mutation advances the
  owning `SectionGroup` rowversion; availability replacement advances the
  `StaffTermAvailability` rowversion.
  Evidence: [FR-8 and FR-10](../requirements.md),
  [data-model integrity rules](../data-model.md),
  [dependency baseline T002](../dependency-baseline.md).
- [x] Publication locks in stable order:
  `CourseOffering`, sorted `SectionGroup` IDs, sorted `Room` IDs, then sorted
  `StaffTermAvailability` IDs. It revalidates dependency versions, staffing,
  capacity, overlaps, and availability inside the same transaction. A
  deadlock retry reruns the complete idempotent transaction.
  Evidence: [FR-8 and EC-5](../requirements.md),
  [research](../research.md), [data model](../data-model.md).
- [x] Capacity edits and registration allocation serialize on the same
  `SectionGroup` row/version and preserve
  `0 <= EnrolledCount <= Capacity`; publication and audited mutations are
  all-or-nothing.
  Evidence: [FR-5, FR-7, and FR-9](../requirements.md),
  [dependency baseline T002](../dependency-baseline.md).
- [x] An availability/resource change affecting a published group creates or
  refreshes a durable versioned `ScheduleImpactAlert`; it never silently moves
  staff, room, or time. Alerts persist reason, affected versions, validation
  result, and Open/Revalidated/Resolved state. Resolve requires a passing
  revalidation and expected version.
  Evidence: [AC-9 and EC-3](../requirements.md),
  [data model](../data-model.md), [API contract](../contracts/api.md).

## API registry

All 14 positive APIs are declared in the feature contract and each has
contract-finalization, failing-contract-test, and handler-delivery tasks. The
Admin availability mutation route is deliberately not a fifteenth API.
Evidence: [API contract](../contracts/api.md), [tasks](../tasks.md).

| # | API | Task evidence |
|---:|---|---|
| 01 | `GET /api/offerings/{offeringId}` | T016-T017, T075 |
| 02 | `GET /api/groups/{groupId}` | T018-T019, T076 |
| 03 | `GET /api/admin/offerings` | T020-T021, T077 |
| 04 | `POST /api/admin/offerings` | T022-T023, T078 |
| 05 | `PUT /api/admin/groups/{groupId}` | T024-T025, T079 |
| 06 | `POST /api/admin/offerings/{offeringId}/validate` | T026-T027, T080 |
| 07 | `POST /api/admin/offerings/{offeringId}/publish` | T028-T029, T081 |
| 08 | `GET /api/admin/rooms` | T030-T031, T082 |
| 09 | `POST /api/admin/rooms` | T032-T033, T083 |
| 10 | `PUT /api/admin/rooms/{roomId}` | T034-T035, T084 |
| 11 | `GET /api/admin/staff-availability` | T036-T037, T085 |
| 12 | `GET /api/admin/schedule-impact-alerts` | T087-T089 |
| 13 | `POST /api/admin/schedule-impact-alerts/{alertId}/revalidate` | T090-T092 |
| 14 | `POST /api/admin/schedule-impact-alerts/{alertId}/resolve` | T093-T095 |

- [x] The API surface uses DTO projection, safe stable errors, bounded
  pagination, request-body expected versions, bound validation previews, and
  payload-bound idempotency. Protected data/version disclosure follows
  authorization.
  Evidence: [dependency baseline T003](../dependency-baseline.md),
  [API contract](../contracts/api.md).
- [x] `PUT /api/admin/staff/{staffId}/terms/{termId}/availability` is absent;
  T038-T039 verify the read-only contract and route absence, and T086 delivers
  endpoint registration with only the Admin GET.
  Evidence: [FR-6 and AC-9](../requirements.md),
  [tasks T038-T039/T086](../tasks.md), [API contract](../contracts/api.md).

## Route ownership and contributions

- [x] `STU-03 /student/subjects/{offeringId}` is implemented by SPEC-011;
  SPEC-010 contributes complete per-activity type/staff/room/location/day/time,
  capacity/state/version, and Tutorial display-label data without editing the
  Razor page. T096-T097 own this contribution and verification.
  Evidence: [route manifest](../../../.specify/route-manifest.json),
  [STU-03 design](../../003-ux-storyboard-accessibility/design/pages/STU-03.md),
  [tasks](../tasks.md).
- [x] `ADM-06 /admin/offerings` is implemented by SPEC-010 with SPEC-017 as a
  contributor. T098-T099 cover edit, validate, publish, conflict, capacity,
  audit, stale, and concurrency journeys.
  Evidence: [route manifest](../../../.specify/route-manifest.json),
  [ADM-06 design](../../003-ux-storyboard-accessibility/design/pages/ADM-06.md),
  [tasks](../tasks.md).
- [x] `ADM-07 /admin/resources` is implemented by SPEC-010 with SPEC-017 as a
  contributor. T100-T101 cover room operations, read-only availability,
  explicit no-override behavior, and alert list/revalidate/resolve journeys.
  Evidence: [route manifest](../../../.specify/route-manifest.json),
  [ADM-07 design](../../003-ux-storyboard-accessibility/design/pages/ADM-07.md),
  [tasks](../tasks.md).
- [x] Route states and accessibility remain governed by SPEC-003: deterministic
  loading/empty/success/error/denied/expired/stale/offline states, keyboard and
  focus behavior, WCAG 2.2 AA, 320-1920 CSS-pixel reflow, 400% zoom, and
  component/contract/E2E/accessibility/visual evidence.
  Evidence: [dependency baseline T001](../dependency-baseline.md).

## Acceptance and race coverage

- [x] AC-1 through AC-9 each have a dedicated acceptance task T040-T048,
  covering valid bundle publication, room conflict, full group, transactional
  audit/capacity, concurrent publish, capacity/allocation race, read quality,
  child-version propagation, and staff-owned availability/alert behavior.
  Evidence: [acceptance criteria](../requirements.md), [tasks](../tasks.md).
- [x] EC-1 through EC-5 each have a dedicated integration task T049-T053,
  including invalid multi-slot rejection, capacity/enrollment contention,
  post-publication unavailability alert, overnight rejection, and stable
  multi-resource locking with whole-transaction deadlock retry.
  Evidence: [edge cases](../requirements.md), [tasks](../tasks.md).
- [x] Consolidated race/behavior suites T057-T061 precede services T070-T074;
  entity tests T008-T015 precede entities T062-T069; endpoint contract tests
  precede handlers T075-T095. This preserves the required test-first order.
  Evidence: [tasks](../tasks.md), [plan](../plan.md).

## Complete task trace

- [x] **FR coverage:** FR-1/2 use T040, T057, T070; FR-3 uses T041/T044,
  T058/T071; FR-4/5/9 use T042/T045, T059/T072; FR-6 uses T039/T043/T048,
  T060/T073/T086; FR-7 uses T043/T044, T061/T074; FR-8/10 use T044/T047/T048,
  T058/T071. Route tasks T096-T101 add the applicable UI evidence.
  Evidence: [requirements](../requirements.md), [tasks](../tasks.md).
- [x] **NFR coverage:** AC-7/T046 exercises NFR-1 through NFR-4; T104-T107
  produce separate load, reason-code, timezone, and bounded-list release
  evidence under the SPEC-018 baseline.
  Evidence: [requirements](../requirements.md),
  [dependency baseline T005](../dependency-baseline.md), [tasks](../tasks.md).
- [x] **Outcome and scope coverage:** SC-1 through SC-3 map to T054-T056;
  OS-1 through OS-4 map to T108; the final complete trace matrix is T109 and
  release approval is T110.
  Evidence: [requirements](../requirements.md), [tasks](../tasks.md).
- [x] **Entity/persistence coverage:** each of the eight owned entities maps
  to a model test T008-T015 and delivery T062-T069; T102-T103 cover the real
  SQL mapping, constraints, indexes, rowversions, alerts, migration upgrade,
  rollback, and snapshot parity for `S2CatalogueScheduling`.
  Evidence: [data model](../data-model.md),
  [persistence manifest](../../../.specify/persistence-manifest.json),
  [tasks](../tasks.md).
- [x] **Dependency and approval gates:** T001-T005 are evidenced in the
  dependency baseline and checked; T007 records Ahmed Elbamby's 2026-07-13
  Gate A approval. No unresolved clarification remains in this readiness gate.
  Evidence: [dependency baseline](../dependency-baseline.md),
  [approval](approval.md), [gate checklist](gates.md).

## Gate conclusion

All required decisions, ownership boundaries, dependency versions, lock and
transaction rules, 14 positive APIs, forbidden Admin mutation, routes, durable
alerts, acceptance/edge races, and task traces have direct evidence. T006
passes. Implementation may proceed from T008 in the documented test-first
dependency order; this readiness result does not complete any later task.
