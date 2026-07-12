# Data Model: Identity and Account Lifecycle

## Owned Entities

- **ApplicationUser**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Staff**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StudentActivation**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RoleAssignment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SecurityAudit**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Entity | Key fields |
|---|---|
| ApplicationUser | Identity fields, enabled state, student/staff link |
| StudentActivation | hashed one-time token, expiry, used timestamp |
| RoleAssignment | user, role, effective dates, assigning actor |
| SecurityAudit | event code, actor/subject, time, safe metadata |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
