# Research: Architecture and Engineering Principles

## Decisions

### Modular boundary
**Decision**: Own this capability in the Architecture module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
This spec establishes dependency/deployment constraints. Its minimal
composition boundary references `GET /api/health`, whose canonical behavior and
handler are owned by SPEC-018; public feature shapes belong to SPEC-006 onward.
SPEC-004 owns no endpoint.

### Demo SQL runtime and local lifecycle
**Decision**: Use SQL Server 2022 Developer at compatibility level 160. Docker
provisions the persistent-until-guarded-reset Development database;
Testcontainers provisions and disposes an isolated Testing database for every
run. Demo records are wholly synthetic. Local credential artifacts, logs, and
exports are Git-ignored and removed within seven days.
**Rationale**: This supplies reproducible SQL Server behavior and realistic
concurrency for the POC while keeping environment cleanup and local-data
exposure bounded.
**Alternatives rejected**: LocalDB/in-memory substitutes for SQL integration,
a shared mutable Testing database, real institutional data, unbounded local
artifact retention, and treating Developer edition as a production choice.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
