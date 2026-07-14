# RegistrationWindow ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.5`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.1`

### RegistrationWindow

- Canonical entity: RegistrationWindow
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/RegistrationWindow.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier TermId FK`
- `string ScopeType` (`all-students`, `program`, or `cohort`)
- `string ScopeValue` (nullable only for `all-students`)
- `datetime2 OpensAtUtc`
- `datetime2 ClosesAtUtc`
- `string State` (`Draft`, `Published`, `EmergencyClosed`, or `Superseded`)
- `rowversion Version`

#### Relationships and invariants

- `ACADEMIC_TERM ||--o{ REGISTRATION_WINDOW : exposes`
- `ClosesAtUtc > OpensAtUtc` is required.
- Scope is normalized; program/cohort scope requires a value and all-students
  scope forbids one.
- Upcoming, Open, and Closed are computed response states from the persisted
  lifecycle plus authoritative server time. They are not persisted lifecycle
  values and browser time never determines them.
- Publication locks the AcademicTerm and affected windows in stable ID order,
  rechecks overlap, and permits at most one matching published context for a
  student at an instant.

#### Ownership boundary

This is a design-time reference only. SPEC-008 alone may implement or change
the runtime entity and its EF mapping.
