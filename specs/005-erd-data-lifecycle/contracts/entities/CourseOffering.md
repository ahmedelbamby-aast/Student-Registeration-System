# CourseOffering ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### CourseOffering

- Canonical entity: CourseOffering
- Canonical owner: SPEC-010
- Canonical source path: `src/StudentRegistration.Scheduling/Domain/CourseOffering.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/SchedulingModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier TermId FK`
- `uniqueidentifier CourseId FK`
- `string State`
- `rowversion Version`

#### Relationships and invariants

- CourseOffering belongs to AcademicTerm.
- `COURSE ||--o{ COURSE_OFFERING : offered_as`
- `COURSE_OFFERING ||--o{ SECTION_GROUP : has`
- RegistrationPlanItem and Enrollment reference the offering.
- `Unique CourseOffering(TermId, CourseId)`.
- `rowversion on mutable aggregate roots and admin records` protects offering publication and state.
- Publication requires valid group, meeting, room, lecturer, and TA assignments under Scheduling policy.

#### Ownership boundary

This is a design-time reference only. SPEC-010 alone may implement or change the runtime entity and its EF mapping.
