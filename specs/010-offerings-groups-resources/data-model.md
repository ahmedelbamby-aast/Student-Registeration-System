# Data Model: Offerings, Groups, and Resources

## Canonical Ownership and Consumption

- **CourseOffering**, **SectionGroup**, **MeetingSlot**, **Room**, and **GroupStaffAssignment** are canonical entities owned by SPEC-010.
- **StaffTermAvailability**: SPEC-010-owned aggregate root; SPEC-016 consumes it for staff editing.
- **StaffAvailability**: SPEC-010-owned child range; it has no independent concurrency version.
- **ScheduleImpactAlert**: SPEC-010-owned durable revalidation state consumed by Admin/staff workspaces.

## Detailed Model

| Field/example | Type | Constraints |
|---|---|---|
| CourseOffering | entity | unique term + course; lifecycle and rowversion |
| SectionGroup | aggregate | offering, code, capacity, enrolled count, Draft/Published/Closed/Cancelled state, rowversion |
| MeetingSlot | child/value | group, Lecture/Tutorial/Laboratory activity, day/start/end, room; EndLocal > StartLocal |
| Room | aggregate | code/location/capacity/availability state/rowversion |
| GroupStaffAssignment | bridge | unique group + staff + teaching role |
| StaffTermAvailability | aggregate root | unique staff + term; deadline; complete range set; rowversion |
| StaffAvailability | child range | parent root, day/start/end/available-or-unavailable; no rowversion |
| ScheduleImpactAlert | entity | group, resource, detected resource/group versions, reason, Open/Revalidated/Resolved state, timestamps |

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
- Every child schedule mutation locks and advances its SectionGroup root.
- Availability replacement locks and advances StaffTermAvailability; children
  are never independently inserted or updated.
- Publication lock order is CourseOffering, sorted SectionGroups, sorted Rooms,
  then sorted StaffTermAvailability roots. Validation and dependency-version
  comparison occur after those locks and before commit.
- An availability/resource mutation affecting a published group creates or
  refreshes a durable ScheduleImpactAlert; it never silently reassigns staff,
  room, or meeting time.
