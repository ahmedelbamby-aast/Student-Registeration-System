# SPEC-013 Dependency Baseline

**Recorded:** 2026-07-17  
**Repository baseline:** `c76af079d9029ab70459bee7c0a176a2fa3bac58`  
**Result:** PASS after the SPEC-013-owned consistency reconciliations recorded
below.

## SPEC-003 frontend dependency

- Accepted normative design baseline:
  `8ee8f724af3b60bbbc464532bc68de6f173247e5`; accepted additive frontend
  shared-rules amendment:
  `4b37c176228475850a84b8351aba1fc91f3c2102`.
- Accepted versions: `page-design-record/1.1`,
  `frontend-design-index/1.1`, `route-inventory/1.1`,
  `route-contributors/1.1`, route manifest `2.1.0`, page/API manifest
  `1.1.0`, component manifest `1.1.0`, STU-04 record `1.0`, and
  `frontend-fixture/1.0`.
- SPEC-013 contributes to STU-04 `/student/schedule`;
  `ScheduleBuilderPage.razor` remains canonically owned by SPEC-012.
- The accepted UI boundary includes the two recommendation endpoints, ranked
  alternatives, honest no-solution behavior, server-authored stale/error
  states, equivalent calendar/list content, responsive and keyboard/focus
  behavior, duplicate-command prevention, and safe unknown-code fallback.
- STU-04 remains `design-only` with SPEC-013 `not-pinned`. T054 and T055 must
  pin and verify the exact recommendation contribution before route evidence
  can pass. SPEC-013 does not edit the canonical Razor page.

## SPEC-010 Scheduling dependency

- Accepted baseline:
  `99c25eee2fc1078605a426d19325351e69916140`.
- Gate A, implementation readiness, 110/110 evidence-backed tasks, and bounded
  non-production release evidence are complete.
- SPEC-013 consumes published group/offering identity, lifecycle,
  registration-pause state, capacity/count, selectability reasons, complete
  meeting-bound staff/room/time data, and exact offering/group versions.
- Only Published, selectable, complete, non-full, non-paused groups are
  optimizer candidates. Capacity is advisory and recommendations reserve no
  seat.
- Weekdays use the existing .NET `DayOfWeek` serialization `0..6`.

## SPEC-011 eligibility dependency

- Accepted baseline:
  `0182713780fa4cace24ea4ffea4065f764b8e6b7`.
- Gate A, implementation readiness, 51/51 evidence-backed tasks, and bounded
  non-production release evidence are complete.
- SPEC-013 consumes server-evaluated eligibility, complete ordered reasons,
  candidate groups, PolicySet identity/version, academic-context version,
  catalogue version, offering/group versions, and current-plan version.
- Missing decision data remains fail closed. Client-authored eligibility or
  selectability is never accepted.

## SPEC-012 registration-plan dependency

- Accepted baseline:
  `c76af079d9029ab70459bee7c0a176a2fa3bac58`.
- Gate A, implementation readiness, 54/54 evidence-backed tasks, and bounded
  non-production release evidence are complete.
- The accepted plan-response version is `spec012-credit-load/1.0`, approved
  2026-07-16, with fixed server-authored target 18 and maximum 18.
- SPEC-013 consumes the authenticated student/term `RegistrationPlanDto`, plan
  ID/rowversion, selected groups, conflicts, selection issues, and complete
  `ValidationSnapshotDto`.
- Applying an option uses the existing complete-plan replacement boundary and
  returns the full updated plan. The underlying stale-store outcome is
  translated to the SPEC-013 code `PLAN_CHANGED`.
- Recommendations swap groups only for the existing selected course set and
  do not add a GPA-derived plan limit or overload path.

## SPEC-018 quality and operations dependency

- Accepted runtime operations foundation:
  `4ac311825bf2c82ec382e488db1a4de145650c79`.
- Gate A is approved and was reverified 2026-07-14.
- SPEC-013 consumes the POC SQL-backed ASP.NET Core Data Protection key
  repository, generated external local certificate, common replica application
  name, stateless-replica requirement, privacy-safe telemetry/correlation
  contract, optimizer metric, and optimizer p95 ceiling of 500 ms.
- Production key custody/provider authority remains undecided and fail closed.
- Cross-replica option-token execution, measured optimizer p95, accessibility,
  traceability, and release approval are not inherited; SPEC-013 must create
  its own passing evidence.
- The current telemetry allow-list lacks `STALE_INPUT`,
  `INVALID_OPTION_TOKEN`, `OPTION_EXPIRED`, and a recommendation route value.
  The additive allow-list correction and its tests are required before
  SPEC-013 operational evidence passes.

## Reconciliations applied to the SPEC-013 baseline

- Adopted weekday encoding `0..6`.
- Replaced the undefined `groupVersionSetHash` with bounded explicit
  offering/group version maps.
- Bound plan ID/rowversion, academic context, catalogue, PolicySet,
  offering/group, and optimizer versions in every protected option.
- Removed the undefined numeric aggregate score; rank and ordered score
  components remain authoritative.
- Pinned apply success to the canonical full `RegistrationPlanDto`.
- Preserved the fixed SPEC-012 18/18 plan contract and documented
  `PLAN_CHANGED` translation.
- Corrected key-repository wording to the approved POC mechanism.

## Dependency result

The approved contracts are sufficient for dependency-ordered SPEC-013
implementation. They are not sufficient for feature completion until every
SPEC-013 task has its own passing evidence.
