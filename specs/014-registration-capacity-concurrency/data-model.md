# Data Model: Registration Capacity and Concurrency

## Owned Entities

- **RegistrationSubmission**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Enrollment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SectionGroup**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **DecisionSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| RegistrationSubmission.ClientRequestId | UUID | unique with student + term |
| Enrollment | entity | unique student + offering |
| SectionGroup.EnrolledCount | integer | 0 <= count <= capacity |
| SectionGroup.Version | rowversion | concurrency token |
| DecisionSnapshot | JSON/value | policy/input/result version retained |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
