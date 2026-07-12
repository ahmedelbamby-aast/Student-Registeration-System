# Data Model: ERD and Data Lifecycle

## Owned Entities

- **ApplicationUser**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Student**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Staff**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Program**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Course**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **AcademicTerm**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicySet**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **CourseOffering**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SectionGroup**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationPlan**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationSubmission**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Enrollment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **AuditEvent**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

The entities, fields, relationships, keys, checks, indexes, schemas, and
lifecycle are normative in docs/diagrams/ERD.md after approval.

| Field/example | Type | Constraints |
|---|---|---|
| Student.UniversityId | string | normalized, unique, not null |
| SectionGroup.Version | rowversion | concurrency token |
| Enrollment offering/group | composite FK | group must belong to offering |
| ImportedRecord.Source | string | required provenance |
| StudentTermRegistrationGuard | entity | unique student + term; rowversion/serialization boundary |
| IdempotencyRecord | entity | unique owner + scope + key; payload hash, state, result, timestamps |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
