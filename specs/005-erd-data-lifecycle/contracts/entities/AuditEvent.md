# AuditEvent ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### AuditEvent

- Canonical entity: AuditEvent
- Canonical owner: SPEC-004
- Canonical source path: `src/StudentRegistration.Infrastructure.SqlServer/Audit/AuditEvent.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/AuditEventModelConfiguration.cs` (`append-only-cross-module`)

#### Fields

- `uniqueidentifier Id PK`
- `string ActorReference`
- `string SubjectReference`
- `string Action`
- `string EntityType`
- `string EntityId`
- `string Reason`
- `string BeforeSummaryJson`
- `string AfterSummaryJson`
- `string CorrelationId`
- `datetime2 OccurredAtUtc`

#### Relationships and invariants

- AuditEvent has no relational foreign keys; privacy-safe actor, subject, entity, and correlation references connect it across modules.
- `audit foundation: AuditEvent is owned upstream by SPEC-004 infrastructure`.
- `Audit events are append-only for sensitive administrative actions`.
- The transaction-aware audit writer commits the business mutation and audit event together, or rolls both back.
- Owning features do not depend on SPEC-017; SPEC-017 provides only the governed read/export side.
- Summary JSON and logs must remain privacy-safe.

#### Ownership boundary

This is a design-time reference. The runtime source already belongs to SPEC-004; SPEC-005 neither recreates nor changes it, and SPEC-017 consumes only the read side.
