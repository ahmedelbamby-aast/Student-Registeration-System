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
composition boundary includes GET /api/health; public feature shapes belong to
SPEC-006 onward.

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
