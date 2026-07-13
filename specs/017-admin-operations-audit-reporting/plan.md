# Implementation Plan: Admin Operations, Audit, and Reporting

**Branch**: 017-admin-operations-audit-reporting | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Implement least-privilege Admin orchestration, append-only audit, operational
metrics, and durable asynchronous exports in
`StudentRegistration.StaffAdministration`. Enrollment correction, drop, and
withdrawal are excluded from MVP; Admin registration views are inspection and
monitoring only.

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
   and APIs; obtain approval last.
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

## Constitution and Approval Gate

Every operation is server-authorized and audited, and implementation is
forbidden until all dependency baselines and consistency analysis pass and
Ahmed ELbamby's approval is recorded last.

## Artifacts

- [Requirements](requirements.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Tasks](tasks.md)

## Complexity Tracking

One SQL-backed export lease and the consumed Identity guard solve the identified cross-replica races;
no broker, distributed scheduler, super-admin, or reporting replica is added.
