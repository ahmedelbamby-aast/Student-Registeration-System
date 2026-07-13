# Student ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### Student

- Canonical entity: Student
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/Student.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier ApplicationUserId FK,UK`
- `string ProgramCode`
- `decimal CurrentGpa`
- `decimal EarnedCredits`
- `string Standing`
- `rowversion Version`

#### Relationships and invariants

- `STUDENT ||--o{ TRANSCRIPT_ATTEMPT : has`
- `STUDENT ||--o{ REGISTRATION_PLAN : prepares`
- Student also relates to term academic state, holds, submissions, the student-term guard, and enrollments.
- ApplicationUserId is unique, allowing at most one academic profile per identity.
- `Unique StudentTermAcademicState(StudentId, TermId)`.
- `No transcript attempt is overwritten`; historical attempts are retained.
- The rowversion protects mutable student-profile state.

#### Ownership boundary

This is a design-time reference only. SPEC-008 alone may implement or change the runtime entity and its EF mapping.
