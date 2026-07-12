# Data Model: Student Registration Records

## Owned Entities

- **RegistrationReceipt**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationSubmission**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Enrollment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **DecisionSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| RegistrationSubmission.Reference | string | unique human-safe reference |
| Enrollment.State | enum | controlled lifecycle |
| ReceiptSnapshot | JSON/value | immutable original display details |
| DecisionSnapshot.PolicyVersion | string | required historical version |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
