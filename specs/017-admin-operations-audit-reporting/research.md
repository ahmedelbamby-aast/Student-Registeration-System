# Research: Admin Operations Audit and Reporting

## Decisions

### Modular boundary
**Decision**: Own this capability in the StaffAdministration module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface AdminCommandMetadata {
  reason: string;
  expectedRowVersion?: string;
}
interface AuditEventDto {
  id: string;
  occurredAtUtc: string;
  actorDisplay: string;
  action: string;
  entityType: string;
  entityId: string;
  reason: string;
  correlationId: string;
}
```

Endpoints include GET /api/admin/operations/metrics, GET /api/admin/audit,
POST /api/admin/exports, and approved feature commands under /api/admin.

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
