# Data Model: Catalogue, Prerequisites, and Policy Administration

## Canonical Ownership and Consumption

- **Program**, **Course**, **CurriculumCourse**, **CoursePrerequisite**, **PolicySet**, **PolicyRule**, **ImportBatch**, **CatalogueDraft**, and **CatalogueVersion** are canonical entities owned by SPEC-009.
- Eligibility and registration features consume immutable published catalogue/policy versions and MUST NOT redefine them.
- SPEC-002 supplies approved policy-rulebook evidence; SPEC-009 owns the runtime policy aggregates and publication lifecycle.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| Course.Code | string | normalized unique, not null |
| Course.Credits | decimal | positive approved range |
| CoursePrerequisite | composite key | course != required course; acyclic graph |
| Catalogue field provenance | owned value | source URL/reference, access date, official-source or synthetic-demo classification, explicit synthetic field names |
| PolicySet.Version | string | unique in scope; published immutable |
| ImportRowError.SourceRow | integer | required when input row is known |
| CatalogueDraft | aggregate | scope, based-on version, Editing/Validated/Published/Abandoned state, canonical content hash, rowversion |
| CatalogueVersion | immutable aggregate | scope, version label, Published/Superseded state, source, publisher, published time |
| ImportBatch | aggregate | target draft, source/access time, content hash, synthetic-field count, Uploaded/Validating/Invalid/Validated/Publishing/Published/Failed state, rowversion, result version |
| ImportRowError | child | import, source row/field, stable code, safe message; immutable after validation result |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
- Programs, courses, curricula, and prerequisite edges belong to one draft or
  immutable catalogue version; published children cannot be updated in place.
- Every imported or locally supplied catalogue field carries provenance. A
  source-backed record can still list specific synthetic gap fields; a fully
  local record is classified synthetic-demo and cannot be displayed as an
  official AASTMT fact.
- The initial 19-course demo snapshot uses the official College-of-AI URL and
  2026-07-13 access date recorded in `docs/DEMO_CURRICULUM.md`. Its locally assigned
  three-credit values and fixture-only flags/identifiers are synthetic-demo-
  only; the snapshot is a subset, not the complete curriculum.
- Editing a draft or import advances its rowversion/content hash and invalidates
  all earlier preview tokens.
- Publication locks the normalized catalogue or policy scope, revalidates the
  complete graph in-transaction, writes the immutable version plus audit fact,
  and changes activation state atomically.
