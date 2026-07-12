# SPEC-017: Admin Operations, Audit, and Reporting

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Admin/Registrar, Security, Data, DevOps, QA<br>
**Target:** Sprint 2-S7<br>
**Dependencies:** SPEC-003, SPEC-007, SPEC-008, SPEC-009, SPEC-010, SPEC-014, SPEC-015, SPEC-016, SPEC-018<br>

## Context

Admins need safe master-data operations, peak monitoring, authorized
corrections, audit evidence, and operational exports. Broad Admin access must
still use least privilege, reasons, optimistic concurrency, and immutable
audit.

## Functional Requirements

- FR-1: Authorized Admin MUST manage terms/windows, users/roles, student
  records/holds, catalogue/policies, resources, offerings/groups, and imports
  only through feature-spec commands.
- FR-2: Sensitive changes MUST require reason, actor, timestamp, before/after
  summary, correlation ID, and audit event.
- FR-3: The system MUST provide registration-window metrics for traffic,
  success, expected rejections, server failures, fill rates, lock waits, and
  data-quality alerts.
- FR-4: Normal corrections MUST NOT exceed capacity or create timetable
  conflicts.
- FR-5: Exports MUST enforce the same row/data scope and PII minimization as UI.
- FR-6: Admin list/search endpoints MUST be paged, filtered, and safely
  parameterized.
- FR-7: Audit events for sensitive actions MUST be append-only to normal users.
- FR-8: Import/publish/correction MUST use preview and explicit confirmation.
- FR-9: Break-glass behavior MUST NOT exist without a separate approved spec.
- FR-10: Update/delete commands MUST require expected rowversion; retryable
  creates, imports, corrections, exports, and confirmations MUST require an
  idempotency key.
- FR-11: Preview tokens MUST bind actor, permission scope, canonical payload,
  dependency versions, and expiry; confirmation MUST reject any changed input,
  scope, permission, dependency, or expired token.
- FR-12: A sensitive business mutation and its audit event MUST commit in the
  same local SQL transaction; audit failure MUST roll back the mutation.
- FR-13: The final-active-Admin role MUST NOT be removed by an ordinary
  command; removal requires a separately approved two-actor recovery procedure.

## Non-Functional Requirements

- NFR-1: Operational metrics SHOULD be no more than 60 seconds stale and show
  observation timestamp.
- NFR-2: Audit search SHOULD return first page within 1 second p95 at approved
  retention volume.
- NFR-3: Export generation MUST be asynchronous/bounded for large data and
  expire securely.
- NFR-4: Admin actions MUST have authorization, audit, concurrency, and
  validation tests.

## Acceptance Criteria

### AC-1: Reasoned correction (FR-2, FR-4)
Given Admin has correction permission and provides a valid reason<br>
When a safe correction is committed<br>
Then the invariant remains valid<br>
And an append-only event records actor, reason, time and before/after summary.

### AC-2: Capacity bypass rejected (FR-4, FR-9)
Given a group is full<br>
When Admin attempts a normal correction that adds another active enrollment<br>
Then the command is rejected<br>
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
Given an admin previews a correction and its dependency version changes<br>
When the admin confirms the old token twice with the same idempotency key<br>
Then both responses report 409 STALE_PREVIEW<br>
And no correction or duplicate audit event commits.

### AC-7: Audit failure rolls back mutation (FR-2, FR-12)
Given a sensitive change passes validation<br>
When audit-event persistence is fault-injected to fail<br>
Then the business mutation rolls back<br>
And the API returns a generic correlated failure without reporting success.

### AC-8: Final Admin safeguard (FR-13)
Given one active Admin role assignment remains<br>
When an ordinary admin command attempts to revoke it<br>
Then the command returns 409 FINAL_ADMIN_REQUIRED<br>
And the assignment remains active.

### AC-9: Admin operations quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given target operational metrics, a production-size audit dataset, large export,
and positive/negative/concurrent admin command matrix<br>
When admin quality tests execute<br>
Then metrics are no more than 60 seconds stale and show observation time<br>
And audit first page returns within 1 second p95<br>
And export executes asynchronously with bounded resources, expiry, and audit<br>
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
interface AdminCommandMetadata {
  reason: string;
  expectedRowVersion: string;
  clientRequestId: string;
  previewToken?: string;
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
Update/delete commands require expectedRowVersion. Retryable create, export,
import, correction, and confirmation commands require clientRequestId.
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
- OS-2: Break-glass capacity/conflict override.
- OS-3: Business-intelligence warehouse.
- OS-4: Long-term report replica until primary impact is measured.
