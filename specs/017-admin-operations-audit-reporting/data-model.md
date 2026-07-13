# Data Model: Admin Operations, Audit, and Reporting

## Owned Models

- **ExportJob**: Durable scoped asynchronous export lifecycle.

## Consumed Models

- **ImportBatch** is owned by SPEC-009.
- **OperationalMetric** is a contract owned by SPEC-018.
- **AuditEvent** and **AuditWritePort** are owned by upstream SPEC-004.
- **SecurityEvent** and **AdminSecurityGuard** are owned by SPEC-007;
  SPEC-017 consumes them and never mutates their identity state.
- Registration records are consumed read-only from SPEC-015.
- Master data and ScheduleImpactAlert actions delegate to owning feature
  modules; SPEC-017 does not redefine them.
- Staff availability is a consumed bounded read-only projection. Import copies
  ranges into offering-planning input; no Admin mutation, correction,
  permission, notification, or correction-audit model is owned here.

## Detailed Model

| Model/field | Type | Constraints |
|---|---|---|
| AuditEvent | append-only entity | ActorId/display, action, entity type/ID, reason, BeforeSummary, AfterSummary, OccurredAtUtc, CorrelationId |
| Audit summaries | redacted JSON/value | bounded field allow-list; no credentials or unnecessary student record |
| ExportJob | entity | JobId, OwnerId, ScopeHash, RequestHash, State, ArtifactId?, ExpiresAtUtc, rowversion |
| Export lease | job fields | LeaseOwnerId?, LeaseExpiresAtUtc?, AttemptCount 0..3; 60-second renewable lease |
| ExportJob.State | enum | Pending, Running, Complete, Failed, Expired |
| AdminSecurityGuard | consumed singleton | SPEC-007 locks it before active-Admin recount/mutation |
| OperationalMetric | consumed projection | name, value, dimensions, observed time, availability state |

## Integrity Rules

- Sensitive business mutation and AuditEvent commit through the SPEC-004
  writer in one local SQL transaction; SPEC-017 verifies but does not own it.
- Normal application roles cannot update/delete AuditEvent.
- A worker conditionally claims Pending work or Running work with an expired
  lease. Only the current lease owner can renew or publish Complete, and one
  JobId has at most one active artifact.
- Status/download verify authenticated owner or explicit scoped Admin
  permission; expired artifacts return 410 and are inaccessible.
- Every Admin-role mutation is delegated to SPEC-007, whose command locks
  AdminSecurityGuard and applies the final-Admin invariant. Admin pages call
  canonical owner endpoints directly; no generic command facade exists.
