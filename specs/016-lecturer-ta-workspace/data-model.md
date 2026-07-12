# Data Model: Lecturer and Teaching Assistant Workspace

## Owned Entities

- **StaffAssignment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StaffTermAvailability**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StaffAvailability**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RosterRow**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **GroupSummary**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| GroupStaffAssignment | bridge | authorized staff + group + role |
| StaffTermAvailability | aggregate root | unique staff + term; deadline; rowversion; owns complete range set |
| StaffAvailability | child range | parent aggregate ID; day/start/end/type; no independent concurrency version |
| StaffAssignmentDto | projection | current assigned group details only |
| RosterRow | projection | minimal approved student fields |

Every create/update/delete of a StaffAvailability child MUST lock and advance
the owning StaffTermAvailability rowversion. Children MUST NOT be updated
through an endpoint or transaction that bypasses the aggregate root.

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
