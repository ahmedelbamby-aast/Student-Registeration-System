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
| Non-production seed profile | SPEC-005 contract; canonical owners write their rows | configuration, not an entity | SQL Server 2022 Developer compatibility 160; Docker Development persists until guarded reset; Testcontainers Testing is disposed per run; wholly synthetic versioned deterministic fixture; idempotent seed; no real data |
| ApplicationUser password credential | SPEC-007 | password hash | ASP.NET Identity hash only; no plaintext PIN/password persistence |
| StudentTermRegistrationGuard | SPEC-014 | entity | unique student + term; serialization boundary |
| RegistrationSubmission | SPEC-014 | entity and idempotency record | unique owner + scope + key; payload hash, state, result, timestamps |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
- Development and per-run Testing databases use the same composed Code First
  model at SQL Server compatibility level 160. Docker-provisioned Development
  persists until guarded reset; each Testcontainers Testing database is
  disposed after its run. Migrations complete before seeding; seed data is not
  migration data.
- A seed-profile version plus stable fixture ordinals determine logical IDs and
  academic values. Cryptographic password-hash salt may vary, so tests verify
  the password through ASP.NET Identity rather than comparing hash bytes.
- Reapplying one seed version is idempotent. Reset is separately invoked,
  verifies Development/Testing before any destructive action, and rejects all
  other environment names or connection targets.
- Development and Testing contain only synthetic records. Local credential
  artifacts, logs, and exports are Git-ignored and removed within seven days;
  no cleanup rule alters immutable repository history or production data.
- A separate `IdempotencyRecord` table is prohibited; RegistrationSubmission
  is the canonical claim, processing-state, and deterministic-result record.
