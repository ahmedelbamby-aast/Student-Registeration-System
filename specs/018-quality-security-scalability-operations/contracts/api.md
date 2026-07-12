# API Contract: Quality, Security, Scalability, and Operations

## Feature Contract

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

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
