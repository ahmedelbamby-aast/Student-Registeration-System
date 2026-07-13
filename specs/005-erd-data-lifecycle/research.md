# Research: ERD and Data Lifecycle

## Decisions

### Modular boundary
**Decision**: Own this capability in the Infrastructure.SqlServer module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
Database design is exposed only through approved feature endpoints. SPEC-015
owns `GET /api/student/registrations`; shared DTO rules are defined by SPEC-006.
SPEC-005 owns no endpoint.

### Non-production database profiles
**Decision**: Use SQL Server 2022 Developer at compatibility level 160. Docker
provisions one isolated Development database that persists until explicit
guarded reset; Testcontainers creates and disposes a fresh isolated Testing
database per run. Apply approved Code First migrations before canonical-owner
seed contributors. The seed profile is wholly synthetic, versioned, logically
deterministic, and idempotent; real institutional/student data is prohibited.
**Rationale**: Development remains reproducible, parallel test runs cannot
contaminate one another, and fixture data cannot become schema history or reach
production accidentally.
**Alternatives rejected**: Seed rows in migrations, a shared mutable test
database, startup seeding in every environment, and a production-capable reset
switch.

### Local artifact lifecycle
**Decision**: Keep every generated local credential artifact, log, and export
out of Git and remove it no later than seven days after creation.
**Rationale**: The demo remains usable and diagnosable without allowing local
copies of sensitive or identifying data to accumulate indefinitely.
**Alternatives rejected**: Checked-in artifacts, indefinite retention, and
cleanup that relies only on a manual reminder.

### Credential handling in seeded data
**Decision**: The identity owner receives generated PIN/password plaintext only
in transient process memory and persists an ASP.NET Identity password hash.
Database rows, source fixtures, snapshots, telemetry, and evidence never store
plaintext credentials or full student profiles.
**Rationale**: A demo credential must remain usable without weakening the
repository and database handling rules.
**Alternatives rejected**: Plaintext PIN columns, reversible encryption,
checked-in credential sheets, and logging generated credentials.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
