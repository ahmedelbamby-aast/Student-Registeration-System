# TranscriptAttempt ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.5`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.1`

### TranscriptAttempt

- Canonical entity: TranscriptAttempt
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/TranscriptAttempt.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AcademicContextModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier StudentId FK`
- `uniqueidentifier TermId FK`
- `uniqueidentifier SupersedesAttemptId FK` (nullable self-reference)
- `string CourseCode`
- `decimal Credits`
- `string GradeCode` (nullable)
- `string Status` (`in-progress`, `passed`, `failed`, or `withdrawn`)
- `string Source`
- `string SourceReference`
- `datetime2 ImportedAtUtc`

#### Relationships and invariants

- `STUDENT ||--o{ TRANSCRIPT_ATTEMPT : has`
- `ACADEMIC_TERM ||--o{ TRANSCRIPT_ATTEMPT : attempted_in`
- CourseCode is a stable imported source code and has no FK to the downstream
  SPEC-009 Course definition.
- Rows are immutable. A governed correction appends a sourced row whose
  `SupersedesAttemptId` references the prior attempt; the prior row remains
  unchanged and queryable.
- `SupersedesAttemptId` has a filtered unique index when non-null, so one
  attempt has at most one direct successor.
- A correction may supersede only the current leaf and must retain the same
  `StudentId`, `TermId`, and `CourseCode` as its predecessor. The FK targets an
  already-existing immutable row; together with the no-update/no-delete rule,
  this prevents branching and cycles.

#### Ownership boundary

This is a design-time reference only. SPEC-008 alone may implement or change
the runtime entity and its EF mapping.
