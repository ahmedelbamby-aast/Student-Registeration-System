# Data Model: Catalogue Prerequisites and Policy Administration

## Owned Entities

- **Program**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Course**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **CurriculumCourse**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **CoursePrerequisite**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicySet**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicyRule**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ImportBatch**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| Course.Code | string | normalized unique, not null |
| Course.Credits | decimal | positive approved range |
| CoursePrerequisite | composite key | course != required course; acyclic graph |
| PolicySet.Version | string | unique in scope; published immutable |
| ImportRowError.SourceRow | integer | required when input row is known |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
