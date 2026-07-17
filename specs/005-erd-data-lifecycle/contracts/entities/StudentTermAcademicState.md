# StudentTermAcademicState ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.8`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.2`

### StudentTermAcademicState

- Canonical entity: StudentTermAcademicState
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/StudentTermAcademicState.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier StudentId FK`
- `uniqueidentifier TermId FK`
- `decimal GpaAtStart`
- `decimal EarnedCreditsAtStart`
- `string StandingAtStart`
- `string Source`
- `string SourceReference`
- `string DataVersion`
- `datetime2 DataAsOfUtc`
- `rowversion Version`

#### Relationships and invariants

- `STUDENT ||--o{ STUDENT_TERM_ACADEMIC_STATE : has`
- `ACADEMIC_TERM ||--o{ STUDENT_TERM_ACADEMIC_STATE : scopes`
- `StudentTermAcademicState(StudentId, TermId)` is unique.
- Hold/profile mutations and registration submission lock this row through
  `ExecuteRegistrationBoundaryAsync` before reading decision inputs and
  advance its version in the same transaction.

#### Ownership boundary

This is a design-time reference only. SPEC-008 alone may implement or change
the runtime entity and its EF mapping; later registration features consume
this boundary without redefining it.
