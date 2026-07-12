# Data Model: Architecture and Engineering Principles

## Owned Entities

- **ModuleBoundary**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ArchitectureDecision**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **DependencyRule**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Schema | Owner |
|---|---|
| auth | IdentityAccess |
| academics | Academics |
| scheduling | Scheduling |
| registration | Registration |
| audit | StaffAdministration / audit service |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
