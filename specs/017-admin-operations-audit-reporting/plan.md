# Implementation Plan: Admin Operations, Audit, and Reporting

**Branch**: 017-admin-operations-audit-reporting | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Approved for non-production demo implementation by Ahmed ELbamby on 2026-07-13; global line-approval and first-term monitoring amendment approved 2026-07-20 (Gate A).

## Summary

Implement least-privilege Admin orchestration, append-only audit, operational
metrics, and durable asynchronous exports in
`StudentRegistration.StaffAdministration`. Enrollment correction, drop, and
withdrawal are excluded from MVP; Admin registration views are inspection and
monitoring only. Staff own availability edits; Admin may view/import declared
ranges only as read-only offering-planning inputs.

The 2026-07-20 amendment adds global Admin pending-line decisions and bounded
first-term automatic-batch monitoring/retry through canonical SPEC-014
commands. It does not authorize correction of accepted Enrollment, capacity
override, policy bypass, or a second writer.

## Technical Context

- **Runtime**: C#/.NET 10, ASP.NET Core, Blazor WebAssembly, EF Core/LINQ.
- **Module**: `src/StudentRegistration.StaffAdministration/`; SQL audit/export
  adapters in `StudentRegistration.Infrastructure.SqlServer`.
- **Storage**: SQL Server Code First with rowversion, append-only permissions,
  idempotency claims, and durable export leases.
- **EF mapping contribution**:
  `AdministrationAuditModelConfiguration.cs` maps ExportJob only. AuditEvent
  mapping/writing is upstream SPEC-004; AdminSecurityGuard and SecurityEvent
  mapping/writing are SPEC-007. This slice contributes its incremental S7
  migration only after upstream slice migrations and its mapping pass.
- **Scale**: multiple stateless replicas; export workers compete through
  conditional SQL claims, not process memory.
- **Security**: reasoned actions, permission scope, anti-forgery, PII
  minimization, immutable before/after summaries and correlation IDs.

## Workstreams and Order

1. Baseline every contributing feature spec; complete consistency and ownership
   analysis.
2. Freeze Admin delegation, audit aggregation, preview/idempotency,
   Identity-owned final-Admin behavior, metric semantics, export lifecycle,
   read-only availability consumption, and APIs against Ahmed ELbamby's
   recorded 2026-07-13 Gate A approval.
3. Write failing model/contract, authorization, acceptance, atomic-audit,
   preview, two-replica export-worker, final-Admin write-skew, and E2E tests.
4. Implement audit/security-event aggregation; verify Admin pages call
   canonical owner endpoints directly and add no generic command facade.
5. Implement metrics and bounded audit queries.
6. Implement export request/status/download, durable lease/recovery, secure
   artifact expiry, and audit of request/download.
7. Verify page delegation to the serialized Identity final-Admin command, add the
   ExportJob mapping and S7 incremental migration, then map
   endpoint handlers and canonical pages after behavior tests fail.
8. Produce performance, security, scope, and trace evidence.
9. Add failing DecideAll permission, bounded/PII-minimized query, expected-
   version/idempotency/antiforgery/audit rollback, batch-monitor/retry,
   component, browser, accessibility, and two-replica tests.
10. Add Admin approval and first-term batch orchestration/UI only after amended
    tests fail; call SPEC-014 owner commands directly and refresh trace/release
    evidence without overwriting the historical baseline.

## Concurrency Boundaries

- Sensitive mutations use the SPEC-004 writer so their AuditEvent commits in
  the same local SQL transaction; SPEC-017 queries but does not write it.
- Final-Admin role changes delegate to SPEC-007, which locks its shared
  `AdminSecurityGuard` before recount/revocation.
- An export worker claims Pending or expired-Lease work using one conditional
  update. Only the current lease owner may publish completion; attempts are
  idempotent and two replicas cannot publish two artifacts.

## Design Decisions

### Ownership

SPEC-017 owns `ExportJob` plus Admin query/export projections and conformance
records. It consumes
SPEC-004 AuditEvent, SPEC-007 SecurityEvent/AdminSecurityGuard outcomes,
feature-owned master data, `ImportBatch`, registration records, schedule
impact alerts, and SPEC-018 operational metric contracts. It does not own role
mutation, audit transaction writing, a generic Admin command/confirmation
facade, or enrollment correction behavior.

Availability consumption is read-only: no Admin availability correction or
override command/permission, editable control, notification workflow, or
correction-audit flow is introduced. Schedule-impact alerts remain visible.

### Registration approval and automatic-batch monitoring

SPEC-014 owns pending lines, holds, decisions, finalization, expiry, and
FirstTermAutoEnrollmentBatch. SPEC-017 supplies global Admin query/action
orchestration and unified UI only. Every decision/retry uses an exact
permission, expected version, reason, ClientRequestId, antiforgery, and the
transaction-aware owner audit. General capacity shows enrolled/held/available
counts without holder identities. The same SPEC-003 roadmap, capacity,
approval status/timeline, decision, and route-state components are reused.

## Constitution and Approval Gate

Every operation is server-authorized and audited. Gate A authorizes
non-production demo implementation while all dependency baselines and
consistency gates pass. Gate B-D evidence and production/release approval
remain separate and mandatory for their respective milestones.

## Artifacts

- [Requirements](requirements.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Tasks](tasks.md)

## Complexity Tracking

One SQL-backed export lease and the consumed Identity guard solve the identified cross-replica races;
no broker, distributed scheduler, super-admin, or reporting replica is added.
