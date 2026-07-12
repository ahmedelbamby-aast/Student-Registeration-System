# Data Model: Admin Operations Audit and Reporting

## Owned Entities

- **AuditEvent**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ImportBatch**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ExportJob**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **OperationalMetric**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| AuditEvent | append-only entity | actor, action, entity, reason, time, correlation |
| ImportBatch | aggregate | preview/validated/published lifecycle |
| ExportJob | entity | authorized owner, state, secure expiry |
| OperationalMetric | projection | name/value/dimensions/observed time |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
