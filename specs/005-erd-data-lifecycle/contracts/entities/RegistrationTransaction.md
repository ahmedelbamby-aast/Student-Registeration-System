# Registration Transaction ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.8`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.2`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationModelConfiguration.cs` (`writable-transactional`)

### RegistrationSubmission

- Canonical entity: RegistrationSubmission
- Canonical owner: SPEC-014
- Canonical source path: `src/StudentRegistration.Registration/Domain/RegistrationSubmission.cs`

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier StudentId FK`
- `uniqueidentifier TermId FK`
- `uniqueidentifier ClientRequestId`
- `string PayloadHash`
- `string ProcessingState`
- `string ResultCode`
- `string Reference UK`
- `string ReceiptSnapshotJson`
- `string DecisionSnapshotJson`
- `datetime2 ReceivedAtUtc`
- `datetime2 UpdatedAtUtc`
- `datetime2 CompletedAtUtc` nullable

#### Relationships and invariants

- RegistrationSubmission belongs to Student and AcademicTerm and creates Enrollment rows.
- `Unique RegistrationSubmission(StudentId, TermId, ClientRequestId)` establishes the owner/scope/key idempotency boundary.
- RegistrationSubmission.ReceivedAtUtc is its immutable creation instant; UpdatedAtUtc records progress, and CompletedAtUtc is nullable until a final outcome.
- PayloadHash binds a replay to the original payload; the deterministic final result remains durable across process restart.
- `Unique non-null RegistrationSubmission.Reference`; Reference and ReceiptSnapshotJson are created only for an accepted final result.
- `RegistrationSubmission idempotency claim, final result, decision snapshot, audit event, and any successful counters/enrollments commit in one SQL transaction`.
- A rejected allocation rolls seat and enrollment mutations back to a savepoint before its final rejected result commits.
- A Processing claim cannot be committed as a standalone durable row; HTTP 202 is transport-only and persists no SubmissionId.
- `A separate IdempotencyRecord table and a seventh SPEC-008 idempotency entity are prohibited`.

### Consumed StudentTermAcademicState boundary

- Canonical entity: StudentTermAcademicState
- Canonical owner: SPEC-008
- Canonical source path: `src/StudentRegistration.Academics/Domain/StudentTermAcademicState.cs`
- Canonical reference: `specs/005-erd-data-lifecycle/contracts/entities/StudentTermAcademicState.md`
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

This is a design-time reference only. SPEC-014 alone may implement or change
RegistrationSubmission and its mapping. SPEC-008 alone owns
StudentTermAcademicState; SPEC-014 consumes that public boundary without a
second entity or mapping.
