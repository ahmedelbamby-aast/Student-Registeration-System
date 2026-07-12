# Feature Specification: Admin Operations Audit and Reporting

**Feature Branch**: 017-admin-operations-audit-reporting
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Product Owner
**Normative detail**: [requirements.md](requirements.md)

## Context

Admins need safe master-data operations, peak monitoring, authorized
corrections, audit evidence, and operational exports. Broad Admin access must
still use least privilege, reasons, optimistic concurrency, and immutable
audit.

## User Scenarios and Testing

### User Story 1 - Reasoned correction (FR-2, FR-4) (P1)

As a Authorized administrator, I need the Reasoned correction (FR-2, FR-4) behavior so that Admin Operations Audit and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given Admin has correction permission and provides a valid reason<br>
When a safe correction is committed<br>
Then the invariant remains valid<br>
And an append-only event records actor, reason, time and before/after summary.
### User Story 2 - Capacity bypass rejected (FR-4, FR-9) (P1)

As a Authorized administrator, I need the Capacity bypass rejected (FR-4, FR-9) behavior so that Admin Operations Audit and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given a group is full<br>
When Admin attempts a normal correction that adds another active enrollment<br>
Then the command is rejected<br>
And no capacity/enrollment state changes.
### User Story 3 - Audit export scope (FR-5, FR-7) (P2)

As a Authorized administrator, I need the Audit export scope (FR-5, FR-7) behavior so that Admin Operations Audit and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given Admin lacks permission for restricted security events<br>
When an audit export is requested<br>
Then restricted rows/fields are omitted or request denied<br>
And the export action itself is audited.
### User Story 4 - Monitor degradation (FR-3) (P2)

As a Authorized administrator, I need the Monitor degradation (FR-3) behavior so that Admin Operations Audit and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given server failures or capacity conflicts spike above configured threshold<br>
When the admin dashboard refreshes<br>
Then a timestamped alert identifies metric, threshold and investigation link.
### User Story 5 - Governed bounded master-data command (FR-1, FR-6, FR-8) (P3)

As a Authorized administrator, I need the Governed bounded master-data command (FR-1, FR-6, FR-8) behavior so that Admin Operations Audit and Reporting produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

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

## Requirements

### Functional Requirements

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

### Key Entities

- **AuditEvent**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ImportBatch**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ExportJob**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **OperationalMetric**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Every sensitive administrative mutation records actor, reason, time, and before/after context.
- **SC-2**: Normal administrative actions cannot bypass capacity or timetable invariants.
- **SC-3**: Operational information is timestamped, scoped, and distinguishable from stale or unavailable data.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-014](../014-registration-capacity-concurrency/spec.md)
- [SPEC-015](../015-student-registration-records/spec.md)
- [SPEC-016](../016-lecturer-ta-workspace/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Out of Scope

- OS-1: Unrestricted super-admin and unaudited direct database edits.
- OS-2: Break-glass capacity/conflict override.
- OS-3: Business-intelligence warehouse.
- OS-4: Long-term report replica until primary impact is measured.
