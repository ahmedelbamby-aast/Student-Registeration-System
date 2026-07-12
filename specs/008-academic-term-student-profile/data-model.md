# Data Model: Academic Term and Student Profile

## Owned Entities

- **AcademicTerm**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationWindow**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Student**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **TranscriptAttempt**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StudentHold**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Entity | Key fields |
|---|---|
| AcademicTerm | code, teaching dates, timezone, state, rowversion |
| RegistrationWindow | term, scope, OpensUtc, ClosesUtc |
| Student | program/cohort, GPA, credits, standing, active, rowversion |
| TranscriptAttempt | course/term/grade/status/provenance |
| StudentHold | type, blocking flag, effective period, source |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
