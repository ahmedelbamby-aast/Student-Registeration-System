# AcademicTerm ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### AcademicTerm

- Canonical entity: AcademicTerm
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/AcademicTerm.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `string Code UK`
- `date TeachingStarts`
- `date TeachingEnds`
- `string TimeZoneId`
- `string State`
- `rowversion Version`

#### Relationships and invariants

- `ACADEMIC_TERM ||--o{ REGISTRATION_WINDOW : exposes`
- `ACADEMIC_TERM ||--o{ COURSE_OFFERING : contains`
- The term also scopes academic state, transcripts, policies, availability, plans, submissions, and registration guards.
- `AcademicTerm.Code` is unique.
- `registration/term end > start` is required.
- Time zone and lifecycle state are server-controlled; rowversion protects mutable term state.

#### Ownership boundary

This is a design-time reference only. SPEC-008 alone may implement or change the runtime entity and its EF mapping.
