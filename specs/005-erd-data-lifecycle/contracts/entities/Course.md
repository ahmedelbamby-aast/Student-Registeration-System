# Course ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### Course

- Canonical entity: Course
- Canonical owner: SPEC-009
- Canonical source path: `src/StudentRegistration.Academics/Domain/Course.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/CatalogueModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier CatalogueVersionId FK`
- `string Code`
- `string Title`
- `decimal Credits`
- `bool IsActive`

#### Relationships and invariants

- Course belongs to one CatalogueVersion, participates in curriculum, and is offered through CourseOffering.
- `COURSE ||--o{ COURSE_PREREQUISITE : course`
- `COURSE ||--o{ COURSE_PREREQUISITE : required`
- `Course(CatalogueVersionId, Code)` is unique.
- Curriculum and prerequisite references stay inside one catalogue version.
- A `published catalogue version and all of its program/course/curriculum rows are immutable`; a later version supersedes it.

#### Ownership boundary

This is a design-time reference only. SPEC-009 alone may implement or change the runtime entity and its EF mapping.
