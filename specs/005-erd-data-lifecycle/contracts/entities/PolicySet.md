# PolicySet ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### PolicySet

- Canonical entity: PolicySet
- Canonical owner: SPEC-009
- Canonical source path: `src/StudentRegistration.Academics/Domain/PolicySet.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/CatalogueModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `string Version`
- `uniqueidentifier TermId FK`
- `uniqueidentifier ProgramId FK`
- `string ApprovalStatus`
- `datetime2 EffectiveFromUtc`
- `rowversion VersionToken`

#### Relationships and invariants

- `ACADEMIC_TERM ||--o{ POLICY_SET : governed_by`
- Program specializes PolicySet.
- `POLICY_SET ||--o{ POLICY_RULE : contains`
- Only an approved and effective set governs a decision; drafts use VersionToken for concurrency.
- `Published policy sets are immutable and superseded by new effective-dated versions`, preserving historical decision meaning.

#### Ownership boundary

This is a design-time reference only. SPEC-009 alone may implement or change the runtime entity and its EF mapping.
