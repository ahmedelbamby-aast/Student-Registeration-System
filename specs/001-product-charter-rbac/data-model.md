# Data Model: Product Charter and RBAC

## Owned Entities

- **Role**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Permission**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RoleAssignment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Concept | Required values |
|---|---|
| Role | Student, Admin, Lecturer, TeachingAssistant |
| Permission | Stable server policy name and allowed operations |
| RoleAssignment | User, role, effective dates, assigning actor |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
