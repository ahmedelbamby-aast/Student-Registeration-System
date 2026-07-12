# API Contract: Admin Operations Audit and Reporting

## Feature Contract

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

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
