# Registration Transaction ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`
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

### StudentTermRegistrationGuard

- Canonical entity: StudentTermRegistrationGuard
- Canonical owner: SPEC-014
- Canonical source path: `src/StudentRegistration.Registration/Domain/StudentTermRegistrationGuard.cs`

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier StudentId FK`
- `uniqueidentifier TermId FK`
- `rowversion Version`

#### Relationships and invariants

- `STUDENT ||--o{ STUDENT_TERM_REGISTRATION_GUARD : serializes`
- `ACADEMIC_TERM ||--o{ STUDENT_TERM_REGISTRATION_GUARD : serializes`
- `Unique StudentTermRegistrationGuard(StudentId, TermId)`.
- The guard is the database serialization boundary for one student's term registration; it is part of the same transaction as the submission outcome.

#### Ownership boundary

This is a design-time reference only. SPEC-014 alone may implement or change either runtime entity or its EF mapping.
