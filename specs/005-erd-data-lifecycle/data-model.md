# Data Model: ERD and Data Lifecycle

## Governed ERD References and Canonical Owners

| Canonical owner | Runtime entities referenced by the ERD |
|---|---|
| SPEC-007 | ApplicationUser, Staff |
| SPEC-008 | Student, AcademicTerm |
| SPEC-009 | Program, Course, PolicySet |
| SPEC-010 | CourseOffering, SectionGroup |
| SPEC-012 | RegistrationPlan |
| SPEC-014 | StudentTermRegistrationGuard, RegistrationSubmission, Enrollment |
| SPEC-004 | AuditEvent |

SPEC-005 owns the ERD, ownership matrix, invariant catalogue, and lifecycle
rules. Every runtime entity and EF configuration is delivered by its canonical
feature owner and composed by Infrastructure.SqlServer.

## Detailed Model

The entities, fields, relationships, keys, checks, indexes, schemas, and
lifecycle are normative in docs/diagrams/ERD.md after approval.

| Field/example | Canonical owner | Type | Constraints |
|---|---|---|---|
| ApplicationUser.UniversityId | SPEC-007 | string | normalized, filtered unique for pre-provisioned student identities |
| Student.ApplicationUserId | SPEC-008 | uniqueidentifier | unique FK to Identity-owned ApplicationUser |
| SectionGroup.Version | SPEC-010 | rowversion | concurrency token |
| Enrollment offering/group | SPEC-014 | composite FK | group must belong to offering |
| ImportedRecord.Source | owning import feature | string | required provenance |
| StudentTermRegistrationGuard | SPEC-014 | entity | unique student + term; serialization boundary |
| RegistrationSubmission | SPEC-014 | entity and idempotency record | unique owner + scope + key; payload hash, state, result, timestamps |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
- A separate `IdempotencyRecord` table is prohibited; RegistrationSubmission
  is the canonical claim, processing-state, and deterministic-result record.
