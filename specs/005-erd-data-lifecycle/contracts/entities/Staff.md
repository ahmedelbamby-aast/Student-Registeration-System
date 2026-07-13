# Staff ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### Staff

- Canonical entity: Staff
- Canonical owner: SPEC-007
- Canonical source path: `src/StudentRegistration.IdentityAccess/Domain/Staff.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/IdentityAccessModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier ApplicationUserId FK,UK`
- `string StaffNumber UK`
- `string DisplayName`
- `bool IsActive`

#### Relationships and invariants

- `STAFF ||--o{ GROUP_STAFF_ASSIGNMENT : assigned`
- `STAFF ||--o{ STAFF_TERM_AVAILABILITY : declares`
- ApplicationUserId permits at most one staff profile per identity.
- `unique Staff.StaffNumber` is required.
- Identity owns staff activation; `Scheduling owns StaffTermAvailability and StaffAvailability` and their assignment rules.

#### Ownership boundary

This is a design-time reference only. SPEC-007 alone may implement or change the runtime entity and its EF mapping; Scheduling retains ownership of availability and assignment children.
