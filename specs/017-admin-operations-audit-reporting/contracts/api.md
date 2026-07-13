# API Contract: Admin Operations, Audit, and Reporting

## Feature Contract

```typescript
interface RedactedChangeSummaryDto {
  fields: Array<{ name: string; displayValue: string }>;
  redactionVersion: string;
}
interface AdminCommandMetadata {
  reason: string;
  expectedRowVersion: string;
  clientRequestId: string;
  previewToken?: string;
}
interface AuditEventDto {
  id: string;
  occurredAtUtc: string;
  actorId: string;
  actorDisplay: string;
  action: string;
  entityType: string;
  entityId: string;
  reason: string;
  beforeSummary: RedactedChangeSummaryDto;
  afterSummary: RedactedChangeSummaryDto;
  correlationId: string;
  sourceStream: "audit" | "identity-security";
}
interface RegistrationReconciliationAlertDto {
  groupId: string;
  state: "paused" | "repaired";
  detectedAtUtc: string;
  supportReferencePath: string;
}
interface AdminOperationsMetricsDto {
  observedAtUtc: string;
  availabilityState: "live" | "stale" | "degraded";
  metrics: OperationalMetric[];
  reconciliationAlerts: RegistrationReconciliationAlertDto[];
}
interface ExportJobDto {
  jobId: string;
  state: "pending" | "running" | "complete" | "failed" | "expired";
  createdAtUtc: string;
  completedAtUtc?: string;
  expiresAtUtc?: string;
  retryAfterSeconds?: number;
  downloadUrl?: string;
  failureCode?: string;
}
```

`AdminCommandMetadata` is a conformance shape used to audit owner contracts;
it is not a request accepted by a generic Admin endpoint/service. Each Admin
page calls the canonical SPEC-007 through SPEC-010/014 owner endpoint directly.

## Canonical Endpoints

- `GET /api/admin/operations/metrics`
- `GET /api/admin/audit?page=1&pageSize=20`
- `POST /api/admin/exports`
- `GET /api/admin/exports/{jobId}`
- `GET /api/admin/exports/{jobId}/download`

POST requires `clientRequestId`, export type/filters, and the caller's current
permission scope. It stores OwnerId, ScopeHash, RequestHash, and Pending state,
returns 202 plus status URL, and replays the same job for the same scoped key
and payload. Different payload returns 409 `IDEMPOTENCY_KEY_REUSED`.

Status returns 200/202 while authorized and not expired. Download is available
only for Complete, verifies the original owner/scope again, audits the
download, and uses a short-lived server-generated response; expired output
returns 410 `EXPORT_EXPIRED`.

## Worker Lease Contract

A worker uses one conditional SQL update to claim Pending or expired-lease
Running jobs, setting a unique LeaseOwnerId and 60-second LeaseExpiresAtUtc.
Only that owner may renew or publish Complete. AttemptCount is capped at three.
A two-replica race must create exactly one published artifact; loser observes
the winning lease and does no generation. Process death permits reclaim after
lease expiry.

## Audit and Admin-Role Contract

Audit DTOs merge shared SPEC-004 AuditEvent and SPEC-007 SecurityEvent records,
include redacted before/after summaries and correlation ID, and never expose
secret/unbounded metadata. Feature mutations and shared audit commit atomically
through SPEC-004; this specification owns query/export only.

Admin-role grant/revoke commands delegate to SPEC-007 IdentityAccess. That
owner locks AdminSecurityGuard before recount and mutation. Two concurrent
revocations when two Admins remain may commit at most one; the loser receives
409 `FINAL_ADMIN_REQUIRED`. SPEC-017 has no RoleAssignment write endpoint.

Reconciliation alerts are observation-only. Repair is an internal SPEC-014
operations-service action protected by `Registration.Reconcile`; Admin metrics
provide only a safe support reference and no repair URL/action.

Enrollment correction, drop, withdrawal, and seat-decrement endpoints do not
exist in MVP. Registration Administration is read/monitor only and delegates
all approved master-data mutations to owner-spec commands.

## Shared Responses

Authentication is 401; authorization is 403; privacy-sensitive absent/out-of-
scope resources are 404. Stale preview/version and idempotency conflicts are
409 with stable codes `STALE_PREVIEW`, `STALE_VERSION`,
`FINAL_ADMIN_REQUIRED`, and `IDEMPOTENCY_KEY_REUSED`. Lists default to 20,
max 100, and use stable unique tie-breakers.
