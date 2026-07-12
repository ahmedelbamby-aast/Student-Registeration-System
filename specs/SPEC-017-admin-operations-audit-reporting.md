# SPEC-017: Admin Operations, Audit, and Reporting

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Product Owner<br>
**Reviewers:** Admin/Registrar, Security, Data, DevOps, QA<br>
**Target:** Sprint 2-S7<br>

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

## Edge Cases

- EC-1: Metrics backend unavailable -> show stale timestamp/degraded state, not
  fabricated zero.
- EC-2: Export fails/expires -> safe status and authorized retry.
- EC-3: Concurrent admin edit -> 409 current version, no lost update.
- EC-4: Bulk import partially invalid -> preview errors; publish all-or-nothing.
- EC-5: Admin disables own final Admin role -> require safeguard/second actor
  according to security approval.

## API Contracts

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
