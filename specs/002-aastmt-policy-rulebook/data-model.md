# Data Model: AASTMT Policy Rulebook

## Owned Entities

- **PolicySet**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicyRule**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicyDecisionSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Entity | Required data |
|---|---|
| PolicySet | version, scope, priority, effective dates, approval state/actor |
| PolicyRule | typed rule, reason code, validated config, source URL/access date |
| PolicyDecisionSnapshot | version, input summary, result list, evaluated time |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
