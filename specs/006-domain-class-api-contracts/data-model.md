# Data Model: Domain Classes and API Contracts

## Owned Entities

- **ApiError**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Page**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **AppContext**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **CommandResult**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **DomainValue**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Type | Purpose |
|---|---|
| Strong ID/value object | Prevent accidental entity/primitive mixing |
| Command/result | One application use case |
| API request/response DTO | Versioned client contract |
| Domain entity/aggregate | Invariant behavior; infrastructure independent |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
