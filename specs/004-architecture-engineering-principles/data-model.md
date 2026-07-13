# Data Model: Architecture and Engineering Principles

## Owned Architecture Artifacts

- **ModuleBoundary**: Versioned project/module dependency declaration.
- **ArchitectureDecision**: Versioned, approved ADR record.
- **DependencyRule**: Executable architecture-conformance rule.
- **AuditEvent**: Append-only shared SQL audit record used by every business
  module without a downstream module dependency.
- **AuditWritePort**: Narrow transaction-aware contract implemented by
  Infrastructure.SqlServer as `IAuditEventWriter`.

The first three are repository governance artifacts. AuditEvent is a persisted
cross-cutting record; AuditWritePort is a contract, not a table.

## Detailed Model

| Schema | Owner |
|---|---|
| auth | IdentityAccess |
| academics | Academics |
| scheduling | Scheduling |
| registration | Registration |
| audit write foundation | Architecture/Infrastructure.SqlServer |
| audit query/export | StaffAdministration |

## Governance Rules

- Every business module has one canonical project and may expose only approved contracts.
- The project-reference graph must be acyclic and architecture tests reject forbidden references.
- Every boundary/deployment change requires an approved ADR and corresponding test update.
- Artifact versions and approvals are preserved in Git; SQL lifecycle rules do not apply.
- The demo environment contract pins SQL Server 2022 Developer compatibility
  160, Docker-provisioned Development persistence until guarded reset, and
  Testcontainers-provisioned per-run Testing disposal. It permits only
  synthetic records; local credential artifacts, logs, and exports are
  Git-ignored and removed within seven days. Production edition/topology and
  key-store authorities remain separate decisions.
- AuditEvent requires actor/subject references, action, reason, redacted
  before/after JSON, correlation ID, server timestamp, and append-only access.
