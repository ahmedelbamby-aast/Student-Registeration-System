# SPEC-006 Dependency Baseline

**Recorded:** 2026-07-13<br>
**Feature:** SPEC-006 Domain Classes and API Contracts<br>
**Result:** PASS

## SPEC-004 accepted contracts

- Completion commit: `6b087936c0d8b6341c656ade698247561caa3996`.
- Human approval: Ahmed ELbamby, 2026-07-13, including the corrected Gate A
  executable baseline for the non-production demo.
- Commit-pinned inputs: `spec.md`, `requirements.md`, `plan.md`,
  `data-model.md`, and `contracts/api.md` under
  `specs/004-architecture-engineering-principles/`.
- Accepted versions: module-boundary schema `1.0`, architecture-decision
  schema `1.0`, persistence manifest `2.1.0`, entity-ownership manifest
  `2.0.0`, and persistence approval boundary `Gate-A-2026-07-13`.
- Consumed boundary: the exact nine-project modular monolith, composition-only
  API, infrastructure-independent business modules, one
  `StudentRegistrationDbContext`, Infrastructure.SqlServer-owned composition
  and migrations, module-owned model/mapping contributions, EF Core/LINQ, and
  projected DTOs that never expose EF entities.
- SPEC-006 owns shared transport contracts and domain value shapes only. It
  does not acquire entity, EF mapping, DbContext, migration, or AuditEvent
  persistence ownership from SPEC-004.

## SPEC-005 accepted design-contract baseline

- Accepted design-contract commit:
  `d88892f97e2164fc3ee60b530187c1f4a658ceb4`. This is not a SPEC-005 runtime,
  release-evidence, or completion commit.
- Human approval: Ahmed ELbamby, 2026-07-13, Gate A demo implementation scope;
  later release and production gates remain required.
- Commit-pinned inputs: `spec.md`, `requirements.md`, `plan.md`,
  `data-model.md`, and `contracts/api.md` under
  `specs/005-erd-data-lifecycle/`.
- Accepted manifest versions: persistence manifest `2.1.0`, entity-ownership
  manifest `2.0.0`, workstream manifest `2.0.0`, and specification manifest
  `2.0.1`.
- Accepted design contracts: `code-first-ownership-map/1.0`,
  `unique-invariants/1.0`, `check-constraints/1.0`,
  `concurrency-tokens/1.0`, `immutable-history/1.0`,
  `relational-invariants/1.0`, `controlled-migrations/1.0`,
  `import-provenance-contract/1.0`, and
  `registration-transaction-schema/1.0`. The entity ERD reference set under
  `contracts/entities/` has no independent semantic version and is therefore
  pinned exactly by the accepted commit above.
- Consumed boundary: SPEC-005 owns the cross-module ERD, relational invariant,
  provenance, controlled-migration, and data-lifecycle contracts. Canonical
  owner specs implement their runtime entities and mappings, while
  Infrastructure.SqlServer composes the model. SPEC-006 may reflect approved
  fields and protocols in DTOs but must not serialize persistence internals or
  re-own the relational model.
- Deferred-runtime boundary: planned source, mapping, migration, bootstrap,
  seed, and evidence paths are declarations only and do not assert that those
  runtime artifacts exist or are activated. Runtime mappings, migration
  artifacts, SQL execution evidence, performance/restore evidence, and
  production edition, topology, window, retention, backup, and release
  authority remain deferred to their canonical owner specs and fail closed.
- `RegistrationSubmission` remains the sole persistence record for its
  owner/scope/key idempotency protocol; a separate `IdempotencyRecord` is
  prohibited. SPEC-006 may publish the shared request/result and
  `expectedRowVersion` transport rules without implementing that persistence.

## SPEC-003 targeted design-governance baseline

- Approved design-governance commit:
  `8ee8f724af3b60bbbc464532bc68de6f173247e5`.
- Human approval: Ahmed ELbamby, 2026-07-13, design review only for the
  non-production demo.
- Accepted versions consumed by the SPEC-006 SYS-01 contribution are
  `page-design-record/1.1`, route manifest `2.1.0`, and page/API manifest
  `1.1.0`.
- The immutable SYS-01 Page Design Record is record version `1.0`, governed by
  `page-design-record/1.1`, and is approved in the `design-only` readiness
  state. SPEC-003 remains its design and implementation owner; SPEC-006 owns
  only its safe-error/status contract contribution.
- SYS-01 contributor contract versions for SPEC-006, SPEC-007, SPEC-008, and
  SPEC-018 remain `not-pinned`. That value is a blocking state, not a wildcard:
  it grants no route source, API-binding, component, browser, accessibility,
  visual, or end-to-end runtime approval.
- This is a targeted design-governance input for SPEC-006 T052/T053. It does
  not promote SPEC-003 into a runtime dependency or change the formal
  SPEC-006 feature dependencies on SPEC-004 and SPEC-005.

## SPEC-006 governed manifest intent

- Specification manifest intent: version `2.0.2`, with SPEC-006 entries
  `ApiError`, `Page`, `AppContext`, `TermSummaryDto`, and `PublicContextDto`.
- Entity-ownership manifest intent: version `2.0.1`, with canonical artifact
  paths for those same concrete shared contracts under
  `StudentRegistration.Contracts`.
- Workstream manifest intent: version `2.0.1`; the safe-error workstream's
  bounded delivery is the API composition error pipeline, while the sole
  canonical `ApiError` source remains owned by its earlier test-first task.
- These version increments record the corrected SPEC-006 planning and delivery
  baseline. They do not assert that any future DTO source, test, handler, or
  OpenAPI artifact already exists. The Ahmed-authored commit that contains
  this remediation becomes the immutable downstream pin; no pre-commit hash is
  invented here.
- Former generic `CommandResult` and `DomainValue` placeholders are not
  canonical SPEC-006 manifest owners or planned shared runtime types. Feature
  specs own their command payload/results; SPEC-006 governs only the explicit
  shared protocol metadata and concrete contract types listed above.

## Dependency conclusion

The direct consumed edges are `SPEC-004 -> SPEC-006` and
`SPEC-005 -> SPEC-006`; the scoped `SPEC-003 -> SPEC-006` SYS-01 design input
does not authorize runtime delivery. The accepted transitive graph includes
`SPEC-001 -> SPEC-002`, `SPEC-001 -> SPEC-003`, `SPEC-002 -> SPEC-003`,
`SPEC-001 -> SPEC-004`, `SPEC-003 -> SPEC-004`, `SPEC-002 -> SPEC-005`, and
`SPEC-004 -> SPEC-005`; it is acyclic. The pinned architecture baseline,
approved SPEC-005 design contracts, and targeted SPEC-003 design record are
sufficient for dependency-ordered SPEC-006 contract work after SPEC-006's own
gates. No route/component activation, migration readiness, production
authority, or release approval is inherited through this baseline.
