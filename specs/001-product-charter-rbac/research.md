# Research: Product Charter and RBAC

## Decisions

### Modular boundary
**Decision**: Own this capability in the Governance module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
Detailed contracts belong to SPEC-006 and feature specs. The charter's
role/context boundary is observed through `GET /api/context`, whose canonical
handler owner is SPEC-008; SPEC-001 owns no endpoint.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
