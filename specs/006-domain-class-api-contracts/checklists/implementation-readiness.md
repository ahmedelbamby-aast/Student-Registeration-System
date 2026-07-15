# SPEC-006 Implementation Readiness

**Baseline state:** FROZEN AND APPROVED FOR DEPENDENCY-ORDERED DEMO CONTRACT WORK<br>
**Frozen:** 2026-07-13 by Ahmed ELbamby

- [x] SPEC-004's completion commit and SPEC-005's accepted design-contract
  commit, exact contract versions, ownership limits, and deferred-runtime
  boundaries are recorded in `dependency-baseline.md`.
- [x] The targeted SPEC-003 SYS-01 design baseline is pinned to commit
  `8ee8f724af3b60bbbc464532bc68de6f173247e5`, PDR schema `1.1`, route
  manifest `2.1.0`, and page/API manifest `1.1.0`.
- [x] SYS-01 is approved only as immutable Page Design Record version `1.0` in
  `design-only` state; route source, API binding, component, browser,
  accessibility, visual, and end-to-end contributor pins remain `not-pinned`.
- [x] Specification manifest `2.0.2`, entity-ownership manifest `2.0.1`, and
  workstream manifest `2.0.1` define the corrected SPEC-006 ownership and
  delivery intent without asserting that future sources or handlers already
  exist.
- [x] The 10 FR, 4 NFR, 9 AC, 4 EC, 3 SC, four exclusions, endpoint/route
  boundaries, and T001-T063 dependency order are internally consistent.
- [x] SPEC-006 owns shared DTO/value schemas and protocol rules only. It does
  not own EF entities, the DbContext, migrations, context handlers, feature
  endpoints, browser authority, or downstream domain decisions.
- [x] ApiError safety, pagination, stable sorting, concurrency, idempotency,
  cancellation, TimeProvider, public/authenticated context, serialization,
  and API-version governance rules are frozen for test-first delivery.
- [x] SPEC-005 planned runtime paths remain declarations. Relevant canonical
  models/mappings and controlled SQL activation must exist before any
  persistence-backed SPEC-006 integration claims passing evidence.
- [x] The deterministic OpenAPI baseline/CI gate, exact SPEC-007/SPEC-008
  context contributor pins, SYS-01 runtime/component pins, NFR evidence,
  scope review, traceability, and release approvals remain later tasks.
- [x] Gates B-D, production SQL/topology and operational authority, migration
  execution, and production release approval remain fail closed.

Dependency-ordered demo work may start only after the workflow records T001-T005
as complete. Each source-delivery task still depends on its designated failing
test for the expected reason. Any requirement, ownership, manifest,
dependency, endpoint/route, or production-authority change requires renewed
analysis and Ahmed ELbamby's approval.

## 2026-07-14 amendment revalidation

**State:** APPROVED FOR DEPENDENCY-ORDERED NON-PRODUCTION CONTRACT WORK<br>
**Approved by:** Ahmed ELbamby

- [x] Specification manifest `2.0.4`, entity-ownership manifest `2.0.5`, and
  workstream manifest `2.0.3` include the canonical
  RegistrationWindowSummaryDto inventory, path, and test focus.
- [x] T010/T011 govern the single test-first shared source delivery, while
  T047/T048 govern composition validation/publication without a writer
  collision; SPEC-008 remains the only context-handler owner.
- [x] Authoritative-null, matched-window state consistency/privacy, and the
  unchanged six-field public DTO are synchronized across planning artifacts.
- [x] The 63-task baseline and current 46/63 completion truth are preserved;
  no additional checked task or runtime success claim is inferred.
- [x] Runtime completion, downstream exact pins, OpenAPI/NFR evidence, Gates
  B-D, production configuration, and release approval remain fail closed.
- [x] The canonical ApiError schema and C# source enforce at most 20 field
  keys, 5 messages per key, and 256 characters per message through the
  existing T006/T007 and T012/T013 sole-writer pairs.
