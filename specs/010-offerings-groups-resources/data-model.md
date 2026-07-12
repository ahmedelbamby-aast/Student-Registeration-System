# Data Model: Offerings, Groups, and Resources

## Owned Entities

- **CourseOffering**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SectionGroup**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **MeetingSlot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Room**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **GroupStaffAssignment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StaffAvailability**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| CourseOffering | entity | unique term + course |
| SectionGroup.Capacity | integer | >= EnrolledCount; nonnegative |
| MeetingSlot | value/entity | EndLocal > StartLocal |
| GroupStaffAssignment | bridge | unique group + staff + teaching role |
| StaffAvailability | range | valid term/day/start/end; rowversion |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
