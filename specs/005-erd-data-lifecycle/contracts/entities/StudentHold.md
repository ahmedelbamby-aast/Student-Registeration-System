# StudentHold ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.5`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.1`

### StudentHold

- Canonical entity: StudentHold
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/StudentHold.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier StudentId FK`
- `uniqueidentifier TermId FK`
- `string Code`
- `string Message`
- `bool BlocksRegistration`
- `datetime2 EffectiveFromUtc`
- `datetime2 EffectiveToUtc` (nullable)
- `string Source`
- `string SourceReference`
- `datetime2 ImportedAtUtc`

#### Relationships and invariants

- `STUDENT ||--o{ STUDENT_HOLD : may_have`
- `ACADEMIC_TERM ||--o{ STUDENT_HOLD : scopes`
- A non-null `EffectiveToUtc` must be later than `EffectiveFromUtc`.
- Every active blocking and non-blocking hold is returned with its source;
  registration re-resolves active blocking holds under the shared
  StudentTermAcademicState serialization boundary.

#### Ownership boundary

This is a design-time reference only. SPEC-008 alone may implement or change
the runtime entity and its EF mapping.
