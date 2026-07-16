# SPEC-011 Implementation Readiness Gate

**Result:** PASS
**Scope:** Non-production Gate A demo implementation. Gate B-D, release,
production policy use, and official AASTMT authorization remain separate.

The decisions below resolve the dependency and consistency findings without a
new service, writable eligibility model, duplicated plan aggregate, generic
repository, or client-authoritative calculation.

## Policy, curriculum, and evaluation

- [x] The runtime profile is exactly `DEMO-POC-2026.1` over the 19 curated
  curriculum rows. Normal target/maximum is 18, probation maximum is 12, and
  18/19 plus 12/13 boundaries are mandatory.
- [x] Eligibility evaluates the governed SPEC-002 registry in its stable
  category/reason order: window, standing, hold, prerequisite, normal load,
  probation load, repeat, capacity, and meeting conflict. The feature does not
  delegate eligibility to the current Admin policy-simulation service.
- [x] A passed course already present as a current completed transcript leaf
  triggers fail-closed `REPEAT_POLICY_UNAVAILABLE`; the demo has no approved
  repeat/advisor/exception workflow. No prior attempt yields
  `REPEAT_NOT_APPLICABLE`.
- [x] Per-rule reasons preserve the exact SPEC-002 stable code. The
  SPEC-011-owned `DECISION_DATA_UNAVAILABLE` code is only the aggregate
  fail-closed result when a complete category decision cannot be constructed;
  any known category also emits its governed `*_UNAVAILABLE` code.
- [x] Every reason contains pass/block flags, message, safe required/current
  values, PolicySet ID/version, source reference and access date, approval
  reference, effective start/end, `overridePossible=false` for a blocker, and
  optional support path. A bounded privacy-safe input summary and deterministic
  rule order are part of the offering decision.

Evidence:
[SPEC-002 baseline](../dependency-baseline.md),
[rule registry](../../002-aastmt-policy-rulebook/schemas/policy-rule-types.json),
[decision contract](../../002-aastmt-policy-rulebook/contracts/policy-decision.md),
and [SPEC-011 requirements](../requirements.md).

## Minimal cross-module read seams

- [x] Academics exposes one narrow provider port at
  `StudentRegistration.Academics/Application/Ports/IEligibilityAcademicReader.cs`.
  Its immutable snapshot contains the authenticated Student identity/scope,
  authoritative term/window instant and versions, complete profile/holds/
  current transcript leaves, the one published catalogue/course/prerequisite
  projection and version, and the one approved published PolicySet projection
  with typed rules/provenance/version. It returns values, not EF entities.
- [x] Scheduling exposes one narrow provider port at
  `StudentRegistration.Scheduling/Application/Ports/IEligibilityOfferingReader.cs`.
  It supplies bounded published offering/group snapshots with offering and
  group versions, lifecycle, pause/capacity/selectability inputs, and meetings
  with nested staff/room/local-time data.
- [x] Registration owns
  `StudentRegistration.Registration/Application/Ports/ICurrentPlanReader.cs`.
  The snapshot is only current credits, selected meeting intervals, and an
  opaque version. It does not own or persist a second `RegistrationPlan`.
- [x] Until SPEC-012 contributes its plan reader inside the same Registration
  module, the live SPEC-011 adapter returns a versioned empty initial plan
  (`initial-empty/1`, zero credits, no meetings). Direct EligibilityService
  tests inject 15-credit and exact-conflict snapshots through the same port,
  proving AC-1 and conflict behavior without a browser-authored plan.
- [x] Infrastructure.SqlServer implements the provider ports with the one
  shared DbContext, `AsNoTracking`, bounded predicates, deterministic ordering,
  and projection-only reads. No business module reaches another module's EF
  table or application/domain internals.

Task ownership: T028 tests the three port inputs and empty/non-empty plan
behavior; T034 delivers EligibilityService plus `ICurrentPlanReader`; T036
delivers the two provider port value contracts and group projection; T043-T044
verify and deliver the single read-only SQL adapter/configuration.

Evidence:
[module boundary](../../../docs/architecture/module-boundaries.md),
[persistence boundary](../../../docs/architecture/persistence-boundary.md),
[SPEC-011 plan](../plan.md), and [tasks](../tasks.md).

## Canonical projection contract

- [x] `OfferingEligibilityDto` contains offering/course identity and credits;
  current/projected/default/effective load; eligible flag; complete ordered
  reasons; bounded groups; evaluated UTC instant; academic-context/
  StudentTermAcademicState version; catalogue version; PolicySet ID/version;
  offering rowversion; and current-plan version.
- [x] `GroupSummaryDto.state` is lifecycle-only:
  `draft | published | closed | cancelled`. Capacity state is derived through
  `selectable`, `seatsRemaining`, and `nonSelectableReasons`; “full” is not a
  lifecycle value.
- [x] Each group contains ID/code, lifecycle, selectable flag, capacity,
  enrolled count, seats remaining, rowversion, non-selectable reasons, and
  meetings. Each meeting contains activity, day/local interval, room/location,
  and its nested staff records with role/name. No array-order inference links
  staff to activities.
- [x] Default eligible discovery returns only offerings with at least one
  selectable published group. The unavailable/all list modes and detail
  endpoint retain bounded non-selectable group summaries needed to explain
  full, closed, cancelled, paused, incomplete, or changed results.
- [x] Eligibility is advisory. Group/offering/plan versions support stale
  presentation and refresh; details and SPEC-014 submission re-resolve current
  state.

T009-T011 own exact model tests; T031-T033 own the three immutable projections;
T030/T036 own the complete group mapping. T043-T044 create no writable
eligibility table.

## Exact query and endpoint protocol

Only these list query keys are accepted:
`q`, `eligibility`, `credits`, `day`, `availability`, `sort`, `page`, and
`pageSize`. Unknown or duplicate scalar keys return `400 VALIDATION_ERROR`.

- `q`: Unicode NFKC-normalized, trimmed, literal parameterized code/title
  contains search, 1-100 characters when supplied.
- `eligibility`: `eligible | unavailable | all`; default `eligible`.
- `credits`: one positive decimal value from 0.5 through 30 with at most two
  decimal places; exact numeric match.
- `day`: integer 0-6 using the server term-timezone meeting day.
- `availability`: `available | full | unavailable | all`; default `all`.
  `available` means at least one selectable group; `full` means every otherwise
  published candidate group has zero seats; `unavailable` means no selectable
  group for any capacity/lifecycle/staffing reason.
- `sort`: exact allow-list `courseCode,id` (default),
  `courseCode-desc,id`, `title,id`, `title-desc,id`, `credits,id`, or
  `credits-desc,id`.
- `page/pageSize`: canonical 1/20 defaults, maximum 100, no silent capping,
  offering ID final tie-break, and applied sort echoed by canonical `Page<T>`.

Eligibility is evaluated first against the complete candidate set; filters,
sort, and page are applied to the resulting server decisions. The client cannot
supply an eligibility result, Student ID, policy version, plan credits, or
selectability override.

### Endpoint 01

`GET /api/student/terms/{termId}/offerings`

- Authorization: Student plus `Catalogue.ReadAvailable`; authenticated user is
  resolved before context/resource lookup.
- Success: `200 Page<OfferingEligibilityDto>`.
- Errors: `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403` without
  protected data; authorized `404 REGISTRATION_CONTEXT_NOT_FOUND`; `503
  DISCOVERY_UNAVAILABLE`; `500 INTERNAL_ERROR`.
- Conflict is not applicable to this committed-state GET.

### Endpoint 02

`GET /api/student/offerings/{offeringId}/eligibility`

- Authorization: Student plus `Catalogue.ReadAvailable`; the offering must
  belong to the authenticated student's authorized registration context.
- Success: `200 OfferingEligibilityDto`, including unavailable/full detail and
  current advisory versions.
- Errors: `401`; `403` without an existence oracle; authorized `404
  OFFERING_NOT_FOUND_OR_OUTSIDE_CONTEXT`; `503 DISCOVERY_UNAVAILABLE`; `500
  INTERNAL_ERROR`. Validation and conflict are not applicable to a valid route
  identifier GET.
- EC-2 means this detail GET is refreshed after a group becomes full. It
  returns the new unavailable/full decision and version; it is not a
  submission endpoint. Final commit revalidation remains SPEC-014-owned.

T012 and T014 transcribe these frozen matrices into `contracts/api.md` before
T013/T015 can pass. T037-T038 remain thin handlers delegating to application
services.

## Ownership, persistence, and routes

- [x] SPEC-011 owns `OfferingEligibility`, `EligibilityReason`, and
  `GroupSummary`; the entity-ownership and persistence manifests name all
  three. They are immutable/keyless read projections, not writable sources of
  truth.
- [x] The endpoint manifest contains exactly Endpoint01/02 with owner 011.
- [x] STU-02 and STU-03 have design owner SPEC-003 and implementation owner
  SPEC-011. STU-03 consumes the SPEC-010 contribution but only SPEC-011 writes
  the Razor page.
- [x] STU-03 sends selected-group intent to STU-04 and performs no plan write.
- [x] T039/T041 are the SPEC-011 feature E2E suites and T048 owns its
  text/icon/non-color evidence. SPEC-003 T153-T161 remain separate product-wide
  visual/accessibility governance and do not block completion of the bounded
  SPEC-011 task package.

Evidence:
[entity ownership](../../../.specify/entity-ownership.json),
[persistence manifest](../../../.specify/persistence-manifest.json),
[endpoint manifest](../../../.specify/endpoint-manifest.json),
[route manifest](../../../.specify/route-manifest.json), and
[SPEC-003 page matrix](../../003-ux-storyboard-accessibility/page-matrix.md).

## Complete task trace and ordering

- [x] FR-1/2/4/6/7: T016-T019, T021, T025/T027, T028, T031-T034,
  T037-T042. T028 explicitly includes repeat, holds, all load boundaries,
  versioned current-plan inputs, and fail-closed reasons.
- [x] FR-3/8: T019, T023, T029, T035, T037, T039-T040.
- [x] FR-5: T016, T030, T036, T039-T042.
- [x] NFR-1 through NFR-4: T020 and T045-T048. T020 and SC-2/T026 may be
  created earlier as failing fixtures but must remain unchecked until the
  measured T045-T048 evidence passes.
- [x] AC-1 through AC-5: T016-T020; EC-1 through EC-4: T021-T024; SC-1
  through SC-3: T025-T027; OS-1 through OS-4: T049.
- [x] Endpoint contract finalization T012/T014 precedes endpoint contract tests
  T013/T015; behavior tests T028-T030 precede T034-T036; all those tests
  precede handlers T037-T038 and pages T040/T042.
- [x] T043-T044 prove a keyless/read-only query contribution and required
  source indexes without a writable eligibility table or a new DbContext.
- [x] T050 produces the final matrix and T051 remains the human release gate
  for SPEC-011 and its feature-applicable SPEC-018 evidence. Pending SPEC-003
  product-wide visual governance is recorded but is not represented as
  completed by this feature gate.

## Gate conclusion

All material ambiguity is frozen with one simple, acyclic design. T001-T007
may close. T009 may begin test-first; this readiness result does not complete
any implementation, test, quality, route, or release task.
