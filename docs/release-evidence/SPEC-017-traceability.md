# SPEC-017 Complete Traceability Matrix

Date: 2026-07-17  
Evidence baseline: Phase 6 ledger HEAD `937e8c7`  
Release rule: every declared FR, NFR, SC, AC, EC, frontend route, and API
endpoint must have named passing executable evidence. Any missing or non-PASS
row rejects release.

## Functional requirements

| ID | Requirement outcome | Passing evidence | Status |
|---|---|---|---|
| FR-1 | Admin pages delegate to feature owners; no generic facade | `tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs`; `docs/release-evidence/SPEC-017-FR-1-delegation.md` | PASS |
| FR-2 | Scoped AuditEvent and SecurityEvent merge with redacted context | `tests/StudentRegistration.IntegrationTests/Audit/AuditAggregationConformanceTests.cs`; `tests/StudentRegistration.ContractTests/Specs/Spec017/AuditSourceMergeContractTests.cs` | PASS |
| FR-3 | Timestamped registration-window metrics and alerts | `tests/StudentRegistration.IntegrationTests/Admin/AdminMetricsTests.cs`; `tests/StudentRegistration.E2ETests/Specs/Spec017/AdminDashboardPageFeatureTests.cs` | PASS |
| FR-4 | No enrollment/availability repair or invariant bypass | `tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs`; `tests/StudentRegistration.ContractTests/Registration/RegistrationConflictTests.cs`; `docs/release-evidence/SPEC-017-FR-4-excluded-actions.md` | PASS |
| FR-5 | Scoped durable expiring export lifecycle with audited actions | `tests/StudentRegistration.AuthorizationTests/AuditExportScopeTests.cs`; `tests/StudentRegistration.IntegrationTests/Admin/AuditExportSqlConcurrencyTests.cs` | PASS |
| FR-6 | Bounded, paged, filtered, parameterized admin reads | `tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint01ContractTests.cs`; `tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint02ContractTests.cs` | PASS |
| FR-7 | Shared audit/security streams remain append-only | `tests/StudentRegistration.IntegrationTests/Specs/Spec017/AuditSourceConsumptionTests.cs`; `docs/release-evidence/SPEC-017-audit-append-only.md` | PASS |
| FR-8 | Owner preview/confirmation and read-only availability import | `tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs`; `docs/release-evidence/SPEC-017-FR-8-owner-preview.md` | PASS |
| FR-9 | No break-glass behavior | `tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs`; `docs/release-evidence/SPEC-017-FR-9-no-break-glass.md` | PASS |
| FR-10 | Expected rowversion and idempotency metadata | `tests/StudentRegistration.ContractTests/Shared/CommandMetadataTests.cs`; `docs/release-evidence/SPEC-017-FR-10-command-metadata.md` | PASS |
| FR-11 | Preview binds actor, scope, payload, versions, and expiry | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-6Tests.cs`; `tests/StudentRegistration.IntegrationTests/Admin/AdminConfirmationConcurrencyTests.cs` | PASS |
| FR-12 | Sensitive mutation and audit commit/rollback atomically | `tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-7Tests.cs` | PASS |
| FR-13 | Identity owner serializes final-Admin safeguards | `tests/StudentRegistration.IntegrationTests/Specs/Spec017/IdentityAdminDelegationTests.cs`; `tests/StudentRegistration.IntegrationTests/Admin/AdminConfirmationConcurrencyTests.cs` | PASS |

## Non-functional requirements

| ID | Requirement outcome | Passing evidence | Status |
|---|---|---|---|
| NFR-1 | Metrics live through 60 seconds, stale after, timestamp retained | `tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-1EvidenceTests.cs`; `docs/release-evidence/SPEC-017-NFR-1.md` | PASS |
| NFR-2 | 100,000-row real-SQL first-page p95 below one second | `tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-2EvidenceTests.cs`; `docs/release-evidence/SPEC-017-NFR-2.md` | PASS |
| NFR-3 | Bounded async export, renewable lease, one artifact, secure expiry | `tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-3EvidenceTests.cs`; `docs/release-evidence/SPEC-017-NFR-3.md` | PASS |
| NFR-4 | Authorization, audit, concurrency, anti-forgery, validation matrix | `tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-4EvidenceTests.cs`; `docs/release-evidence/SPEC-017-NFR-4.md` | PASS |

## Success criteria

| ID | Measurable outcome | Passing evidence | Status |
|---|---|---|---|
| SC-1 | Every sensitive mutation records actor, reason, time, before/after | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-1Tests.cs`; `tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs` | PASS |
| SC-2 | Normal admin actions cannot bypass capacity/timetable invariants | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-2Tests.cs`; `tests/StudentRegistration.AuthorizationTests/AdminCommandInvariantTests.cs` | PASS |
| SC-3 | Operational data is timestamped, scoped, and visibly stale/degraded | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-4Tests.cs`; `tests/StudentRegistration.QualityTests/Specs/Spec017/NFR-1EvidenceTests.cs` | PASS |

## Acceptance criteria

| ID | Scenario | Passing evidence | Status |
|---|---|---|---|
| AC-1 | Reasoned sensitive mutation | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-1Tests.cs` | PASS |
| AC-2 | Capacity bypass rejected | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-2Tests.cs` | PASS |
| AC-3 | Audit export scope | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-3Tests.cs` | PASS |
| AC-4 | Monitor degradation | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-4Tests.cs` | PASS |
| AC-5 | Governed bounded master-data command | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-5Tests.cs` | PASS |
| AC-6 | Stale preview confirmation | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-6Tests.cs` | PASS |
| AC-7 | Audit failure rolls back mutation | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-7Tests.cs` | PASS |
| AC-8 | Final Admin safeguard | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-8Tests.cs` | PASS |
| AC-9 | Admin operations quality gate | `tests/StudentRegistration.AcceptanceTests/Specs/Spec017/AC-9Tests.cs`; `docs/release-evidence/SPEC-017-phase-6.md` | PASS |

## Edge cases

| ID | Safe outcome | Passing evidence | Status |
|---|---|---|---|
| EC-1 | Missing metrics show degraded/stale timestamp, never zero | `tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-1Tests.cs` | PASS |
| EC-2 | Failed/expired export has safe status and authorized retry | `tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-2Tests.cs` | PASS |
| EC-3 | Concurrent edit returns current version without lost update | `tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-3Tests.cs` | PASS |
| EC-4 | Partially invalid bulk import previews errors and publishes nothing | `tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-4Tests.cs` | PASS |
| EC-5 | Self-removal of final Admin is safeguarded | `tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-5Tests.cs` | PASS |
| EC-6 | Reused key with changed payload returns IDEMPOTENCY_KEY_REUSED | `tests/StudentRegistration.IntegrationTests/Specs/Spec017/EdgeCases/EC-6Tests.cs` | PASS |

## Frontend route-to-test matrix

| ID | Route and ownership | Passing evidence | Status |
|---|---|---|---|
| ADM-01 | `/admin`; SPEC-017 canonical dashboard | `tests/StudentRegistration.Client.ContractTests/Routes/AdminDashboardPageContractTests.cs`; `tests/StudentRegistration.E2ETests/Specs/Spec017/AdminDashboardPageFeatureTests.cs` | PASS |
| ADM-02 | `/admin/terms`; SPEC-008 owner, SPEC-017 contributor | `tests/StudentRegistration.E2ETests/Specs/Spec017/TermAdministrationPageContributorTests.cs` | PASS |
| ADM-03 | `/admin/users`; SPEC-007 owner, SPEC-017 contributor | `tests/StudentRegistration.E2ETests/Specs/Spec017/UserAdministrationPageContributorTests.cs` | PASS |
| ADM-04 | `/admin/students`; SPEC-008 owner, SPEC-017 contributor | `tests/StudentRegistration.E2ETests/Specs/Spec017/StudentAdministrationPageContributorTests.cs` | PASS |
| ADM-05 | `/admin/catalogue`; SPEC-009 owner, SPEC-017 contributor | `tests/StudentRegistration.E2ETests/Specs/Spec017/CatalogueAdministrationPageContributorTests.cs` | PASS |
| ADM-06 | `/admin/offerings`; SPEC-010 owner, SPEC-017 contributor | `tests/StudentRegistration.E2ETests/Specs/Spec017/OfferingAdministrationPageContributorTests.cs` | PASS |
| ADM-07 | `/admin/resources`; SPEC-010 owner, SPEC-017 contributor | `tests/StudentRegistration.E2ETests/Specs/Spec017/ResourceAdministrationPageContributorTests.cs` | PASS |
| ADM-08 | `/admin/registrations`; SPEC-017 canonical read-only monitor | `tests/StudentRegistration.Client.ContractTests/Routes/RegistrationAdministrationPageContractTests.cs`; `tests/StudentRegistration.E2ETests/Specs/Spec017/RegistrationAdministrationPageFeatureTests.cs` | PASS |
| ADM-09 | `/admin/audit`; SPEC-017 canonical audit/export shell | `tests/StudentRegistration.Client.ContractTests/Routes/AuditAdministrationPageContractTests.cs`; `tests/StudentRegistration.E2ETests/Specs/Spec017/AuditAdministrationPageFeatureTests.cs` | PASS |

## API endpoint-to-test matrix

| ID | Endpoint | Passing evidence | Status |
|---|---|---|---|
| API-Endpoint01 | `GET /api/admin/operations/metrics` | `tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint01ContractTests.cs`; `tests/StudentRegistration.ApplicationTests/Specs/Spec017/Endpoint01BehaviorTests.cs` | PASS |
| API-Endpoint02 | `GET /api/admin/audit` | `tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint02ContractTests.cs`; `tests/StudentRegistration.ApplicationTests/Specs/Spec017/Endpoint02BehaviorTests.cs` | PASS |
| API-Endpoint03 | `POST /api/admin/exports` | `tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint03ContractTests.cs`; `tests/StudentRegistration.ApplicationTests/Specs/Spec017/ExportLifecycleBehaviorTests.cs` | PASS |
| API-Endpoint04 | `GET /api/admin/exports/{jobId}` | `tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint04ContractTests.cs`; `tests/StudentRegistration.ApplicationTests/Specs/Spec017/ExportLifecycleBehaviorTests.cs` | PASS |
| API-Endpoint05 | `GET /api/admin/exports/{jobId}/download` | `tests/StudentRegistration.ContractTests/Specs/Spec017/Endpoint05ContractTests.cs`; `tests/StudentRegistration.ApplicationTests/Specs/Spec017/ExportLifecycleBehaviorTests.cs` | PASS |

## Scope and release decision

The four exclusions are separately inspected in
`docs/release-evidence/SPEC-017-scope-review.md`. The executable schema and
completeness gate is
`tests/StudentRegistration.SpecificationTests/Spec017TraceabilityTests.cs`.
This matrix contains 49 unique PASS rows and no pending, waived, assumed, or
placeholder row. It supports demo release review only; Gate D, production,
official AASTMT, and institutional approval are not claimed.
