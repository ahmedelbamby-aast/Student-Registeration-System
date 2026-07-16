# SPEC-009 Implementation Readiness

**Baseline state:** FROZEN AND APPROVED FOR DEPENDENCY-ORDERED DEMO WORK  
**Frozen:** 2026-07-16 under Ahmed Elbamby's 2026-07-13 Gate A approval

- [x] SPEC-002, SPEC-003, SPEC-005, SPEC-006, SPEC-008, and SPEC-018
  consumed contracts and deferred boundaries are recorded in
  `dependency-baseline.md`.
- [x] `docs/DEMO_CURRICULUM.md` contains exactly 19 normalized unique course
  codes, records the official Data Science source URL and 2026-07-13 access
  date, and explicitly classifies every three-credit value as synthetic demo
  data rather than an official AASTMT fact.
- [x] The curated prerequisite graph resolves inside the snapshot and is
  acyclic; missing/inconsistent references remain invalid-import fixtures and
  are never silently corrected.
- [x] The approved simple demo policy boundaries are exact: normal target and
  maximum 18, GPA-below-2.0 maximum 12, DS413 GPA 2.0 and 96 earned credits,
  hard prerequisites/capacity/overlap, and no waitlist, override, waiver,
  advisor, add/drop, withdrawal, or executable policy scripting.
- [x] Program, Course, CurriculumCourse, CoursePrerequisite, PolicySet,
  PolicyRule, ImportBatch, CatalogueDraft, and CatalogueVersion have one
  canonical SPEC-009 owner and delivery path.
- [x] Draft and import lifecycle states, immutable published/superseded
  versions, field-level provenance, row errors, content hashes, and rowversion
  boundaries are explicit and testable.
- [x] Preview binding includes actor, normalized scope, canonical content,
  dependency versions, import/content hash where applicable, and expiry.
  Draft/import/dependency changes invalidate the preview.
- [x] Publication uses one normalized-scope lock, in-transaction graph and
  conflict revalidation, payload-bound idempotency, immutable version
  creation, activation/supersession, and append-only audit in one local SQL
  transaction.
- [x] Every endpoint has one Academics owner, bounded DTO/page/error behavior,
  explicit Admin permission, expected-version or idempotency metadata where
  applicable, and no generic property or executable-expression mutation path.
- [x] ADM-05 retains the approved state matrix, responsive widths, keyboard,
  focus, live-region, stale/retry, provenance, simulation, and publish
  confirmation behavior. SPEC-009 will pin the contributor after executable
  route evidence exists.
- [x] The persistence contribution is
  `CatalogueModelConfiguration.cs`; SPEC-004 remains the sole DbContext
  writer, and SPEC-010 remains the owner of the combined
  `S2CatalogueScheduling` migration.
- [x] Required evidence covers all AC-1 through AC-7, EC-1 through EC-5,
  SC-1 through SC-3, fourteen endpoints, ADM-05, real SQL mapping, 10,000-row
  validation, deterministic simulation, transactional publication, audit,
  scope review, traceability, and release perspectives.
- [x] Production catalogue/policy authority, official AASTMT completeness,
  live scraping, exception workflows, historical deletion, Gate B-D, and
  release remain excluded and fail closed.
- [x] The requirements and gate checklists pass with no unresolved item, and
  the human Gate A approval is present.

## Readiness evidence

- `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks
  -IncludeTasks`: resolves SPEC-009 and its required artifacts.
- Requirements checklist: 8/8 complete; gate checklist: 10/10 complete.
- Curriculum inspection: 19 rows, 19 unique normalized codes, zero duplicates.
- Dependency and owner review: PASS with no cycle or writer collision.
- `git diff --check`: required before the readiness commit.

T009 and later may proceed test-first only after T001-T007 are checked in
`tasks.md`. Production and release authority remain separate.

