# API Contract: Admin Operations, Audit, and Reporting

## Shared contract

All five endpoints are same-origin JSON APIs, use the authenticated server
identity and current permission scope, and return the shared bounded `ApiError`
shape for errors. The server creates `correlationId`; callers cannot supply or
override actor, owner, permission scope, or correlation identity.

```typescript
interface ApiError {
  code: string;
  message: string;
  correlationId: string;
  fieldErrors?: Record<string, string[]>;
  currentVersion?: string;
}
interface AdminCommandMetadata {
  reason: string;
  expectedRowVersion: string;
  clientRequestId: string;
  previewToken?: string;
}
interface RedactedChangeFieldDto {
  name: string;
  displayValue: string;
}
interface RedactedChangeSummaryDto {
  fields: RedactedChangeFieldDto[];
  redactionVersion: string;
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
interface AuditEventPageDto {
  items: AuditEventDto[];
  page: number;
  pageSize: number;
  totalCount: number;
}
interface OperationalMetricDto {
  name: string;
  value: number;
  dimensions: Record<string, string>;
  observedAtUtc: string;
}
interface RegistrationReconciliationAlertDto {
  groupId: string;
  state: "paused" | "repaired";
  detectedAtUtc: string;
  metric: string;
  threshold: number;
  observedValue: number;
  supportReferencePath: string;
}
interface AdminOperationsMetricsDto {
  observedAtUtc: string;
  availabilityState: "live" | "stale" | "degraded";
  metrics: OperationalMetricDto[];
  reconciliationAlerts: RegistrationReconciliationAlertDto[];
}
interface AuditExportFilterDto {
  occurredFromUtc?: string;
  occurredToUtc?: string;
  actorId?: string;
  action?: string;
  sourceStream?: "audit" | "identity-security";
}
interface CreateExportRequest {
  clientRequestId: string;
  exportType: "audit";
  filters: AuditExportFilterDto;
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

Lists default to page 1 and page size 20, reject page sizes above 100, and use
stable unique tie-breakers. Query values are passed as parameters to bounded
LINQ/SQL queries; they are never concatenated into SQL. Error messages do not
expose SQL, stack traces, lease owners, artifact paths, authorization scope
hashes, request hashes, or unredacted event metadata.

Authentication failure returns `401 UNAUTHORIZED`. An authenticated caller
without the named permission returns `403 FORBIDDEN`. A missing resource and a
resource outside the caller's current row scope are indistinguishable and
return `404 RESOURCE_NOT_FOUND`. Named rate-limit policies return
`429 RATE_LIMITED` with `Retry-After`. Unexpected failures return
`500 INTERNAL_ERROR` with a safe message and correlation ID. An unavailable
required backend returns `503 ADMIN_OPERATIONS_UNAVAILABLE`; it never produces
a fabricated successful response.

## GET /api/admin/operations/metrics

Requires authenticated Admin permission `AdminOperations.Metrics.Read` and the
named `AdminOperationsRead` rate-limit policy. It accepts no request body.
Optional query fields are `termId` and `registrationWindowId`; both are UUIDs
and must be within the caller's current administrative scope. Metrics are
bounded to the requested registration window and contain traffic, success,
expected rejection, server failure, fill-rate, lock-wait, capacity-conflict,
and data-quality signals. Observation-only reconciliation alerts contain a
safe support reference and never a repair command or URL.

| Outcome | HTTP | Body / headers |
|---|---:|---|
| Current or explicitly stale/degraded observation | 200 | `AdminOperationsMetricsDto`; `observedAtUtc` and `availabilityState` are required |
| Invalid UUID or unsupported query combination | 400 | `ApiError` with `METRICS_FILTER_INVALID` and bounded `fieldErrors` |
| Missing authentication | 401 | `ApiError` with `UNAUTHORIZED` |
| Missing `AdminOperations.Metrics.Read` | 403 | `ApiError` with `FORBIDDEN` |
| Window absent or outside row scope | 404 | `ApiError` with `RESOURCE_NOT_FOUND` |
| Named limit exceeded | 429 | `ApiError` with `RATE_LIMITED`; `Retry-After` |
| Metrics source unavailable and no last observation exists | 503 | `ApiError` with `METRICS_UNAVAILABLE` |
| Unexpected failure | 500 | `ApiError` with `INTERNAL_ERROR` |

No 409 conflict is defined for this read endpoint. If the metrics source is
unavailable but a bounded last observation exists, the endpoint returns 200
with its original `observedAtUtc` and `availabilityState = "degraded"`; it
MUST NOT replace missing values with zero.

## GET /api/admin/audit

Requires authenticated Admin permission `AdminAudit.Read` and the named
`AdminOperationsRead` rate-limit policy. It accepts no request body. Supported
query fields are `page`, `pageSize`, `occurredFromUtc`, `occurredToUtc`,
`actorId`, `action`, and `sourceStream`. Results merge shared SPEC-004
AuditEvent and SPEC-007 SecurityEvent records within the caller's scope, apply
PII minimization before
materialization, and sort by `occurredAtUtc` descending then `id` descending.
All filters are safely parameterized.

| Outcome | HTTP | Body / headers |
|---|---:|---|
| Scoped page | 200 | `AuditEventPageDto`; default page 1/page size 20, maximum page size 100 |
| Invalid paging, time range, UUID, action, or source stream | 400 | `ApiError` with `AUDIT_FILTER_INVALID` or `PAGE_SIZE_INVALID` |
| Missing authentication | 401 | `ApiError` with `UNAUTHORIZED` |
| Missing `AdminAudit.Read` | 403 | `ApiError` with `FORBIDDEN` |
| Named limit exceeded | 429 | `ApiError` with `RATE_LIMITED`; `Retry-After` |
| Audit sources unavailable | 503 | `ApiError` with `AUDIT_UNAVAILABLE` |
| Unexpected failure | 500 | `ApiError` with `INTERNAL_ERROR` |

An empty in-scope result is `200` with an empty `items` array. No 404 or 409
outcome is defined for this collection read. Restricted rows and fields are
omitted before paging/counting; the response never reveals that an omitted
security event exists. Both consumed streams remain append-only and this
endpoint exposes no mutation affordance. Responses never expose secret/unbounded
metadata.

## POST /api/admin/exports

Requires authenticated Admin permission `AdminAudit.Export`, the named
`AdminExportCreate` rate-limit policy, JSON content type, and a valid
same-origin antiforgery token. The request body is `CreateExportRequest`.
`clientRequestId` is a non-empty UUID. The caller cannot provide `ownerId`,
`scopeHash`, `requestHash`, lease fields, artifact identity, or expiry.

The server canonicalizes the permitted export type and filters, binds the
current actor and permission/row scope, and durably stores OwnerId, ScopeHash,
RequestHash and Pending state. Creating the job and its request audit event is
atomic. The response has `Location: /api/admin/exports/{jobId}` and
`Retry-After`.

| Outcome | HTTP | Body / headers |
|---|---:|---|
| New durable Pending job | 202 | `ExportJobDto`; `Location`; `Retry-After` |
| Same owner/scope/key/canonical payload replay | 202 | The same `ExportJobDto` and `jobId`; no duplicate job or audit event |
| Malformed body, invalid UUID/filter, or unsupported export type | 400 | `ApiError` with `EXPORT_REQUEST_INVALID` and bounded `fieldErrors` |
| Invalid or missing antiforgery token | 400 | `ApiError` with `ANTIFORGERY_INVALID` |
| Missing authentication | 401 | `ApiError` with `UNAUTHORIZED` |
| Missing `AdminAudit.Export` | 403 | `ApiError` with `FORBIDDEN` |
| Same scoped key with a different canonical payload | 409 | `ApiError` with `IDEMPOTENCY_KEY_REUSED`; neither payload is newly executed |
| Named limit exceeded | 429 | `ApiError` with `RATE_LIMITED`; `Retry-After` |
| Durable job/audit persistence unavailable | 503 | `ApiError` with `EXPORT_UNAVAILABLE`; no success is reported |
| Unexpected failure | 500 | `ApiError` with `INTERNAL_ERROR` |

A Failed or Expired job is terminal and is never reset or re-queued. After
reauthorization, the owner may retry the same export type and filters by POSTing
them with a new `clientRequestId`; this creates and audits a new Pending job
under the caller's current scope. Reusing the old key returns the original job
in its current terminal state and never generates another artifact. Reusing the
old key with changed input remains `409 IDEMPOTENCY_KEY_REUSED`. This bounded
new-key rule is the only retry flow; there is no retry endpoint or mutable
reset command.

## GET /api/admin/exports/{jobId}

Requires an authenticated original owner with the still-current bound scope,
or explicit scoped Admin permission `AdminAudit.Export.ReadAll`, plus the
named `AdminOperationsRead` rate-limit policy. `jobId` must be a UUID. The
endpoint accepts no request body and reauthorizes on every request.

| Outcome | HTTP | Body / headers |
|---|---:|---|
| Pending or Running | 202 | `ExportJobDto`; `Retry-After` and `retryAfterSeconds` |
| Complete or safely Failed | 200 | `ExportJobDto`; Complete may expose only the canonical `downloadUrl` |
| Invalid `jobId` | 400 | `ApiError` with `EXPORT_JOB_ID_INVALID` |
| Missing authentication | 401 | `ApiError` with `UNAUTHORIZED` |
| Missing resource or current owner/row scope | 404 | `ApiError` with `RESOURCE_NOT_FOUND` |
| Expired job or artifact | 410 | `ApiError` with `EXPORT_EXPIRED` |
| Named limit exceeded | 429 | `ApiError` with `RATE_LIMITED`; `Retry-After` |
| Status store unavailable | 503 | `ApiError` with `EXPORT_UNAVAILABLE` |
| Unexpected failure | 500 | `ApiError` with `INTERNAL_ERROR` |

A caller that is authenticated but lacks both ownership scope and
`AdminAudit.Export.ReadAll` receives the privacy-preserving 404 above, not a
403 existence oracle. No 409 conflict is defined for status reads.

## GET /api/admin/exports/{jobId}/download

Uses the same owner/current-scope or `AdminAudit.Export.ReadAll`
reauthorization as status and the named `AdminExportDownload` rate-limit
policy. `jobId` must be a UUID. Only a Complete, unexpired job with the single
published artifact is downloadable. A successful download is audited before
the short-lived server response is released.

| Outcome | HTTP | Body / headers |
|---|---:|---|
| Authorized Complete artifact | 200 | Bounded file stream; `Content-Type`; safe `Content-Disposition`; `Cache-Control: no-store`; `X-Content-Type-Options: nosniff` |
| Invalid `jobId` | 400 | `ApiError` with `EXPORT_JOB_ID_INVALID` |
| Missing authentication | 401 | `ApiError` with `UNAUTHORIZED` |
| Missing resource or current owner/row scope | 404 | `ApiError` with `RESOURCE_NOT_FOUND` |
| Pending, Running, Failed, or artifact not published | 409 | `ApiError` with `EXPORT_NOT_READY`; no artifact bytes |
| Expired job or artifact | 410 | `ApiError` with `EXPORT_EXPIRED`; no artifact bytes |
| Named limit exceeded | 429 | `ApiError` with `RATE_LIMITED`; `Retry-After` |
| Artifact/audit persistence unavailable | 503 | `ApiError` with `EXPORT_UNAVAILABLE`; no artifact bytes |
| Unexpected failure | 500 | `ApiError` with `INTERNAL_ERROR` |

Download never exposes a filesystem/blob path, lease owner, request hash, or
scope hash. Failed authorization produces no download audit event presented as
a successful download; security denial telemetry remains owned by SPEC-007.

## Worker lease contract

POST only creates durable work; generation is asynchronous. A worker uses one
conditional SQL update to claim Pending work or Running work with an expired
lease, setting its unique LeaseOwnerId and a 60-second LeaseExpiresAtUtc. Only
the current lease owner may renew or publish Complete. AttemptCount is capped
at three. Publication conditionally records exactly one ArtifactId and the
configured ExpiresAtUtc. Two replicas racing for one job publish at most one
artifact; the loser observes the winning lease and performs no generation.
Process death permits reclaim only after lease expiry. Expiry makes status and
download return 410 and makes artifact bytes inaccessible.

## Delegated command boundary

`AdminCommandMetadata` is a conformance shape for canonical feature-owner
commands, not a request accepted by a generic SPEC-017 endpoint. Admin pages
call SPEC-007 through SPEC-010/014 owner endpoints directly. Role changes use
SPEC-007's `AdminSecurityGuard`; SPEC-017 exposes no RoleAssignment writer.
Delegated owner conflicts remain HTTP 409 with their canonical stable result:
`STALE_PREVIEW`, `STALE_VERSION`, `FINAL_ADMIN_REQUIRED`, or
`IDEMPOTENCY_KEY_REUSED`. SPEC-017 does not translate these results.
Enrollment correction, drop, withdrawal, seat decrement, break-glass, and
Admin availability mutation/correction/override endpoints do not exist.
