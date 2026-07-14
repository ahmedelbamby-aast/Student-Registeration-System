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

## GET /api/health

This public liveness/readiness summary accepts no request body or query
parameters and returns no component, host, replica, SQL, certificate, key-ring,
or exporter detail.

| Outcome | HTTP | Body |
|---|---:|---|
| Healthy or degraded | 200 | `HealthSummary` |
| Unhealthy dependency | 503 | `HealthSummary` with `status = "unhealthy"` |
| Unexpected failure | 500 | safe `ApiError` |

Validation, authentication, authorization, conflict, and rate-limit outcomes
are not applicable because this is a bounded, bodyless, privacy-safe public
probe. Infrastructure throttling may terminate abusive traffic without
changing the endpoint contract. `version` is the application version and
`timestampUtc` comes from server `TimeProvider`; neither field reveals
topology. The response carries the safe correlation header and is marked
`Cache-Control: no-store` by the API pipeline.

## GET /api/operations/metrics

This endpoint is restricted to an authenticated server-derived `Admin` role.
It accepts optional `page` and `pageSize` query values using the SPEC-006
default `1`/`20` and maximum `100`. The response is
`Page<OperationalMetric>`, ordered by the canonical unique metric-series key
(`name` plus sorted allow-listed dimensions). Only allow-listed aggregate
dimensions are returned.

| Outcome | HTTP | Body |
|---|---:|---|
| Authorized bounded read | 200 | `Page<OperationalMetric>` |
| Invalid page/pageSize | 400 | `ApiError` with `PAGE_SIZE_INVALID` |
| Unauthenticated | 401 | `ApiError` with `AUTHENTICATION_REQUIRED` |
| Authenticated non-Admin | 403 | `ApiError` with `ACCESS_DENIED` |
| Unexpected failure | 500 | safe `ApiError` |

Conflict and domain rate-limit outcomes are not applicable to this read.
Platform traffic protection may still reject abusive traffic without exposing
metrics or topology. A degraded source does not invent values: the endpoint
returns the bounded metrics currently observed and their individual server
timestamps.

Every metrics response is marked `Cache-Control: no-store` and carries only
the safe correlation reference established by the API pipeline.

Health detail and metrics MUST expose no secret, credential, identity,
student/profile value, connection data, host/replica name, or internal
topology. Unknown metric or dimension names are rejected by the telemetry
collector rather than reflected from request or log input.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
