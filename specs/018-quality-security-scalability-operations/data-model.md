# Data Model: Quality, Security, Scalability, and Operations

## Owned Entities

- **HealthSummary**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **OperationalMetric**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StructuredLog**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Trace**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **BackupEvidence**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ReleaseEvidence**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Operational data | Retention/handling |
|---|---|
| Structured log | Safe metadata; no credentials/full student records |
| Metric | Aggregated numerical/dimensional values |
| Trace | Correlation and timing with PII-minimized attributes |
| Backup | Encrypted, access-controlled, tested |
| Load/accessibility/security report | Versioned release evidence |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
