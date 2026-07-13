# SPEC-017: Admin Operations, Audit, and Reporting

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Admin/Registrar, Security, Data, DevOps, QA<br>
**Target:** Sprint 2-S7<br>
**Dependencies:** SPEC-003, SPEC-004, SPEC-007, SPEC-008, SPEC-009, SPEC-010, SPEC-014, SPEC-015, SPEC-016, SPEC-018<br>

## Context

Admins need safe master-data operations, registration-record inspection, peak
monitoring, audit evidence, and operational exports. Broad Admin access still
uses least privilege, reasons, optimistic concurrency, immutable audit, and
durable cross-replica work claims. Enrollment correction, drop, and withdrawal
are not part of this MVP.

## Functional Requirements

- FR-1: Authorized Admin MUST manage terms/windows, users/roles, student
  records/holds, catalogue/policies, resources, offerings/groups, and imports
  only through feature-owner endpoints/commands selected by each Admin page.
  SPEC-017 MUST NOT add a generic AdminCommandService facade.
- FR-2: Admin audit search MUST merge the shared SPEC-004 `AuditEvent` stream
  with SPEC-007 `SecurityEvent` facts into one scoped chronological projection.
  Sensitive changes MUST expose reason, actor, timestamp, redacted before/after
  summary, correlation ID, source stream, and action code where applicable.
- FR-3: The system MUST provide registration-window metrics for traffic,
  success, expected rejections, server failures, fill rates, lock waits, and
  data-quality alerts.
- FR-4: Admin orchestration MUST NOT expose enrollment correction, drop,
  withdrawal, or seat-decrement commands in MVP. Delegated master-data
  commands MUST preserve capacity and timetable invariants and cannot bypass
  the owning feature module.
- FR-5: Exports MUST enforce the same row/data scope and PII minimization as
  UI and use an explicit request/status/download lifecycle. ExportJob MUST be
  durable, owner/scope/request-bound, expiring, and claimed by workers through
  a conditional SQL lease; only the current lease owner may publish one
  artifact, and request/download actions MUST be audited.
- FR-6: Admin list/search endpoints MUST be paged, filtered, and safely
  parameterized.
- FR-7: SPEC-017 MUST treat shared AuditEvent and SecurityEvent as append-only
  consumed records; normal users cannot update/delete either stream.
- FR-8: Import, publication, and other approved sensitive feature-spec
  mutations MUST use preview and explicit confirmation; this does not
  authorize enrollment correction.
- FR-9: Break-glass behavior MUST NOT exist without a separate approved spec.
- FR-10: Update/delete commands MUST require expected rowversion; retryable
  creates, imports, exports, and confirmed feature-spec mutations MUST require
  an idempotency key.
- FR-11: Preview tokens MUST bind actor, permission scope, canonical payload,
  dependency versions, and expiry; confirmation MUST reject any changed input,
  scope, permission, dependency, or expired token. These rules are conformance
  checks on owner endpoints; SPEC-017 MUST NOT add a generic
  AdminConfirmationService.
- FR-12: SPEC-017 conformance tests MUST prove that sensitive feature commands
  use the upstream SPEC-004 transaction-aware audit writer so mutation and
  AuditEvent commit or roll back together; SPEC-017 MUST NOT own a second
  transaction writer.
- FR-13: Every Admin-role grant/revocation MUST delegate to the SPEC-007
  IdentityAccess command, which owns and locks AdminSecurityGuard before
  recount/mutation/audit. SPEC-017 MUST NOT mutate RoleAssignment or the guard.
  Concurrent revocations MUST leave one active Admin and return
  FINAL_ADMIN_REQUIRED from the Identity owner when necessary.

## Non-Functional Requirements

- NFR-1: Operational metrics SHOULD be no more than 60 seconds stale and show
  observation timestamp.
- NFR-2: Audit search SHOULD return first page within 1 second p95 at approved
  retention volume.
- NFR-3: Export generation MUST be asynchronous and bounded, use a 60-second
  renewable SQL lease with at most three attempts, publish at most one
  artifact, and expire the artifact after the configured approved retention
  interval.
- NFR-4: Admin actions MUST have authorization, audit, concurrency, and
  validation tests.

## Acceptance Criteria

### AC-1: Reasoned sensitive mutation (FR-2, FR-4)
Given Admin has the owning feature permission and provides a valid reason<br>
When a safe feature-spec master-data mutation is committed<br>
Then the invariant remains valid<br>
And an append-only event records actor, reason, time and before/after summary.

### AC-2: Capacity bypass rejected (FR-4, FR-9)
Given a group has active enrollments at its current capacity<br>
When Admin attempts a SPEC-010 capacity reduction below EnrolledCount<br>
Then the owning feature command rejects the change<br>
And no capacity/enrollment state changes.

### AC-3: Audit export scope (FR-5, FR-7)
Given Admin lacks permission for restricted security events<br>
When an audit export is requested<br>
Then restricted rows/fields are omitted or request denied<br>
And the export action itself is audited.

### AC-4: Monitor degradation (FR-3)
Given server failures or capacity conflicts spike above configured threshold<br>
When the admin dashboard refreshes<br>
Then a timestamped alert identifies metric, threshold and investigation link.

### AC-5: Governed bounded master-data command (FR-1, FR-6, FR-8)
Given authorized Admin filters a large master-data list and previews a change<br>
When the bounded request and confirmed mutation execute<br>
Then only a paged parameterized result is returned<br>
And the confirmed feature-spec command is validated/audited.

### AC-6: Stale preview confirmation (FR-8, FR-10, FR-11)
Given an Admin previews an offering publication and its dependency version
changes<br>
When the Admin confirms the old token twice with the same idempotency key<br>
Then both responses report 409 STALE_PREVIEW<br>
And no publication or duplicate audit event commits.

### AC-7: Audit failure rolls back mutation (FR-2, FR-12)
Given a sensitive change passes validation<br>
When audit-event persistence is fault-injected to fail<br>
Then the business mutation rolls back<br>
And the API returns a generic correlated failure without reporting success.

### AC-8: Final Admin safeguard (FR-13)
Given exactly two active Admin role assignments remain<br>
When two replicas concurrently revoke different assignments<br>
Then both transactions serialize through the shared AdminSecurityGuard<br>
And at most one revocation commits, the loser returns 409
FINAL_ADMIN_REQUIRED, and at least one active Admin remains.

### AC-9: Admin operations quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given target operational metrics, a production-size audit dataset, large export,
and positive/negative/concurrent admin command matrix<br>
When admin quality tests execute<br>
Then metrics are no more than 60 seconds stale and show observation time<br>
And audit first page returns within 1 second p95<br>
And two replicas competing for one export publish exactly one artifact through
a durable lease, while authorized status/download and secure expiry are
enforced<br>
And every admin action passes authorization, audit, concurrency, and
anti-forgery checks.

## Edge Cases

- EC-1: Metrics backend unavailable -> show stale timestamp/degraded state, not
  fabricated zero.
- EC-2: Export fails/expires -> safe status and authorized retry.
- EC-3: Concurrent admin edit -> 409 current version, no lost update.
- EC-4: Bulk import partially invalid -> preview errors; publish all-or-nothing.
- EC-5: Admin disables own final Admin role -> require safeguard/second actor
  according to security approval.
- EC-6: The same idempotency key is reused with a different admin command
  payload -> return 409 IDEMPOTENCY_KEY_REUSED and execute neither new payload.

## API Contracts

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

Endpoints include GET /api/admin/operations/metrics, GET /api/admin/audit,
POST /api/admin/exports, GET /api/admin/exports/{jobId}, GET
/api/admin/exports/{jobId}/download, and approved feature commands under
/api/admin.
Update/delete commands require expectedRowVersion. Retryable create, export,
import, export, and confirmation commands require clientRequestId.
Confirmation requires a previewToken bound to actor/scope/payload/dependency
versions/expiry; conflicts return 409 STALE_PREVIEW, STALE_VERSION,
FINAL_ADMIN_REQUIRED, or IDEMPOTENCY_KEY_REUSED.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| AuditEvent | append-only entity | actor, action, entity, reason, time, correlation |
| ImportBatch | aggregate | preview/validated/published lifecycle |
| ExportJob | entity | authorized owner, state, secure expiry |
| OperationalMetric | projection | name/value/dimensions/observed time |

## Out of Scope

- OS-1: Unrestricted super-admin and unaudited direct database edits.
- OS-2: Break-glass capacity/conflict override and any enrollment correction, drop, withdrawal, or seat-decrement workflow.
- OS-3: Business-intelligence warehouse.
- OS-4: Long-term report replica until primary impact is measured.
