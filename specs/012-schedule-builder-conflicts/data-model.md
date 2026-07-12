# Data Model: Schedule Builder and Conflicts

## Owned Entities

- **RegistrationPlan**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationPlanItem**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ScheduleConflict**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ValidationSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| RegistrationPlan | aggregate | unique active student + term; rowversion |
| RegistrationPlanItem | entity | unique plan + offering |
| PreferredGroupId | identifier | must belong to selected offering |
| ValidationSnapshot | JSON/value | advisory only; timestamped |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
