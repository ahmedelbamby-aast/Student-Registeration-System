# Data Model: Schedule Recommendations

## Owned Entities

- **SchedulePreferences**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ScheduleOption**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ScoreComponent**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **OptimizationDiagnostic**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| SchedulePreferences | value object | approved bounded weights/ranges |
| ScheduleOption | transient result | complete one-group-per-course solution |
| ScoreComponent | value | factor, numeric contribution, explanation |
| OptimizationDiagnostic | transient | bounded/no-solution reason; no seat claim |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
