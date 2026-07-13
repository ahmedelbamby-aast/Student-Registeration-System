# Program ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### Program

- Canonical entity: Program
- Canonical owner: SPEC-009
- Canonical source path: `src/StudentRegistration.Academics/Domain/Program.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/CatalogueModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier CatalogueVersionId FK`
- `string Code`
- `string Name`

#### Relationships and invariants

- Program belongs to one CatalogueVersion.
- `PROGRAM ||--o{ CURRICULUM_COURSE : defines`
- `PROGRAM ||--o{ POLICY_SET : specializes`
- `Unique Program(CatalogueVersionId, Code)`.
- Same-version curriculum foreign keys prevent cross-version mixing.
- A `published catalogue version and all of its program/course/curriculum rows are immutable`; a later version supersedes it.

#### Ownership boundary

This is a design-time reference only. SPEC-009 alone may implement or change the runtime entity and its EF mapping.
