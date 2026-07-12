# Data Model: Registration Capacity and Concurrency

## Owned Entities

- **RegistrationSubmission**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Enrollment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SectionGroup**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **DecisionSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StudentTermRegistrationGuard**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/entity | Type | Constraints |
|---|---|---|
| StudentTermRegistrationGuard | aggregate row | unique student + term; database serialization/version boundary |
| RegistrationSubmission.ClientRequestId | UUID | unique with student + term |
| RegistrationSubmission.PayloadHash | fixed hash | canonical server-computed payload; immutable |
| RegistrationSubmission.State | enum | Processing, Accepted, Rejected; deterministic final result stored |
| Enrollment | entity | unique active student + offering; group belongs to offering |
| SectionGroup.EnrolledCount | integer | 0 <= count <= Capacity |
| SectionGroup.Version | rowversion | shared capacity/publication concurrency token |
| SectionGroup.RegistrationPaused | boolean | true blocks conditional seat allocation after reconciliation mismatch |
| DecisionSnapshot | JSON/value | exact policy/profile/plan/group input and result versions |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
