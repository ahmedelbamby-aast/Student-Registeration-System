# Research: Quality, Security, Scalability, and Operations

## Decisions

### Modular boundary
**Decision**: Own this capability in the Operations module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface HealthSummary {
  status: "healthy" | "degraded" | "unhealthy";
  version: string;
  timestampUtc: string;
}
interface OperationalMetric {
  name: string;
  value: number;
  observedAtUtc: string;
  dimensions: Record<string, string>;
}
```

GET /api/health exposes only the safe HealthSummary. GET /api/operations/metrics
is restricted. Health detail and metrics MUST expose no secrets/topology.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
