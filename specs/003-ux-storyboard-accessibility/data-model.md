# Data Model: UX Storyboard and Accessibility

## Owned Entities

- **AppContext**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **UiStatus**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ConflictView**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| View model | Required data |
|---|---|
| AppContext | server time, timezone, teaching term, registration term/window, user/role |
| UiStatus | code, heading, message, severity, next actions, reference ID |
| ConflictView | subjects/groups, overlap slots, alternatives, resolution links |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
