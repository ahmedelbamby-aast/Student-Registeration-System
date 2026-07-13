# Research: Admin Operations, Audit, and Reporting

## Decisions

### No enrollment correction in MVP

**Decision**: Registration Administration provides inspection, monitoring,
reconciliation visibility, and links to owner-spec master-data commands only.
No enrollment correction, drop, withdrawal, or seat-decrement command exists.

**Rationale**: SPEC-014/015 explicitly exclude these policy-sensitive
workflows. A future workflow needs its own approved policy, authorization,
capacity, finance, audit, and student-notification design.

### Atomic audit

**Decision**: Consume the SPEC-004 AuditEvent/write foundation and SPEC-007
SecurityEvent read stream. SPEC-017 merges them for scoped query/export and
verifies that feature mutations use the upstream same-transaction writer.

**Rationale**: Best-effort logging can report success without evidence.
Redacted summaries preserve accountability while minimizing PII.

### Durable export lifecycle

**Decision**: POST creates an idempotent durable ExportJob; GET exposes status;
GET /download authorizes and audits access. Workers use a 60-second renewable
conditional SQL lease and at most three attempts. Only the lease owner can
publish one artifact.

**Rationale**: A database lease is sufficient for two or more replicas, process
failure, and retry without a broker. It prevents duplicate output and avoids
in-memory ownership.

### Final-Admin serialization

**Decision**: Delegate role changes to SPEC-007, which locks its singleton
AdminSecurityGuard before active-Admin recount, role mutation, and audit.

**Rationale**: Rowversion on separate RoleAssignment rows does not prevent two
concurrent transactions from each seeing two Admins and revoking different
ones. One shared database guard prevents write skew.

### Feature delegation

**Decision**: Admin pages call canonical owner endpoints directly. SPEC-017
owns cross-feature conformance checks, audit query/export, and metrics, but no
generic command or confirmation facade; term,
identity, academics, policy, and scheduling mutations to their public module
ports and preserves their preview/version invariants.

## Open Research

Export retention and institutional audit retention remain approved
configuration owned by the appropriate authority. Unknown values fail closed;
no institutional number is invented here.
