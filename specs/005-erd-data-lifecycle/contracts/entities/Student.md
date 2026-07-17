# Student ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.5`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.1`

### Student

- Canonical entity: Student
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/Student.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier ApplicationUserId FK,UK`
- `string ProgramCode`
- `string Cohort`
- `decimal CurrentGpa`
- `decimal EarnedCredits`
- `string Standing`
- `bool IsActive`
- `string Source`
- `string SourceReference`
- `string DataVersion`
- `datetime2 DataAsOfUtc`
- `datetime2 ImportedAtUtc`
- `rowversion Version`

#### Relationships and invariants

- `STUDENT ||--o{ TRANSCRIPT_ATTEMPT : has`
- `STUDENT ||--o{ REGISTRATION_PLAN : prepares`
- Student also relates to the shared term academic state, holds, submissions,
  and enrollments; registration consumes that one academic-state boundary.
- ApplicationUserId is unique, allowing at most one academic profile per identity.
- ProgramCode is a stable imported source code and has no FK to the downstream
  SPEC-009 Program definition.
- Source, source reference, data version, as-of time, and import time are
  required; an incomplete profile fails closed.
- `Unique StudentTermAcademicState(StudentId, TermId)`.
- `No transcript attempt is overwritten`; historical attempts are retained.
- The rowversion protects mutable student-profile state.

#### Ownership boundary

This is a design-time reference only. SPEC-008 alone may implement or change the runtime entity and its EF mapping.
