# SectionGroup ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### SectionGroup

- Canonical entity: SectionGroup
- Canonical owner: SPEC-010
- Canonical source path: `src/StudentRegistration.Scheduling/Domain/SectionGroup.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/SchedulingModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier OfferingId FK`
- `string GroupCode`
- `int Capacity`
- `int EnrolledCount`
- `string State`
- `bool RegistrationPaused`
- `rowversion Version`

#### Relationships and invariants

- `COURSE_OFFERING ||--o{ SECTION_GROUP : has`
- SectionGroup owns meeting slots, staff assignments, and schedule-impact alerts; plan items may prefer it and enrollments allocate it.
- The group code is unique within an offering.
- `Alternate key SectionGroup(Id, OfferingId), referenced by Enrollment` prevents cross-offering group allocation.
- `Check Capacity >= 0 and 0 <= EnrolledCount <= Capacity`.
- Allocation also requires RegistrationPaused to be false.
- `Every group-state, MeetingSlot, room, and GroupStaffAssignment mutation locks and advances its owning SectionGroup.Version`.
- Publication requires a lecturer for lecture activity and a TA for each lab or tutorial activity.

#### Ownership boundary

This is a design-time reference only. SPEC-010 alone may implement or change the runtime entity and its EF mapping.
