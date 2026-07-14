# AcademicTerm ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.5`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.1`

### AcademicTerm

- Canonical entity: AcademicTerm
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/AcademicTerm.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `string Code UK`
- `uniqueidentifier CreationClientRequestId UK`
- `string CreationPayloadHash`
- `string DisplayName`
- `date TeachingStartsOn`
- `date TeachingEndsOn`
- `string TimeZoneId`
- `string State`
- `rowversion Version`

#### Relationships and invariants

- `ACADEMIC_TERM ||--o{ REGISTRATION_WINDOW : exposes`
- `ACADEMIC_TERM ||--o{ COURSE_OFFERING : contains`
- The term also scopes academic state, transcripts, policies, availability, plans, submissions, and registration guards.
- `AcademicTerm.Code` is unique.
- `AcademicTerm.CreationClientRequestId` is globally unique and
  `CreationPayloadHash` binds a replay to the canonical POST create payload;
  same-key/different-payload replay is rejected.
- `TeachingEndsOn > TeachingStartsOn` is required.
- Time zone and lifecycle state are server-controlled; rowversion protects mutable term state.
- No separate term-create idempotency entity is added. Publication and later
  term/profile corrections use required expected versions instead of the
  creation request ID.

#### Ownership boundary

This is a design-time reference only. SPEC-008 alone may implement or change the runtime entity and its EF mapping.
