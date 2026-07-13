# SPEC-004 complete traceability evidence

- Feature: Architecture and Engineering Principles
- Evidence date: 2026-07-13
- Release scope: approved non-production demo architecture
- Automated gate:
  `tests/StudentRegistration.QualityTests/Specs/Spec004/TraceabilityEvidenceTests.cs`

## Admission rule

Every SPEC-004 FR, NFR, AC, EC, SC, and frontend-route boundary must appear
exactly once below with executable evidence and status `PASS`.
Missing, duplicate, non-PASS, or evidence-free row rejects release. The
automated gate also verifies the critical exact project shape, atomic audit,
stateless key ring, local artifact lifecycle, and single-writer DbContext proof
files.

Production SQL topology, key-store/certificate custody, and deployment
authority are outside this demo release and remain fail-closed; a `PASS` here
does not represent institutional Production approval.

## Functional requirements

| ID | Requirement | Executable evidence | Status |
|---|---|---|---|
| FR-1 | Approved .NET/ASP.NET Core/Blazor WASM/EF Core/LINQ/SQL Server demo stack and pinned SQL 2022 runtime | `tests/StudentRegistration.ArchitectureTests/ApprovedStackTests.cs`; `tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerContainerFixture.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-6Tests.cs` | PASS |
| FR-2 | Exact nine-project business-module shape and composition-only API | `tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-1Tests.cs` | PASS |
| FR-3 | No cross-module internal references; only approved contracts/ports | `tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-1Tests.cs` | PASS |
| FR-4 | One Infrastructure.SqlServer DbContext and one SQL transaction boundary | `tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-4Tests.cs` | PASS |
| FR-5 | DTO boundary never exposes EF entities/navigation graphs | `tests/StudentRegistration.ArchitectureTests/DtoIsolationTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-4Tests.cs` | PASS |
| FR-6 | Projected AsNoTracking reads and focused command transactions | `tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-2Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-3Tests.cs` | PASS |
| FR-7 | Prohibited generic repository, microservice, broker, event-sourcing, DSL, and institution-wide solver complexity stays absent | `tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-3Tests.cs` | PASS |
| FR-8 | Boundary changes require an ADR plus architecture-test update | `tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-5Tests.cs` | PASS |
| FR-9 | Shared append-only writer joins caller transaction; business and atomic audit rows commit or roll back together without SPEC-017 | `tests/StudentRegistration.ContractTests/Shared/AuditWritePortTests.cs`; `tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs`; `tests/StudentRegistration.IntegrationTests/Persistence/AuditEventPersistenceTests.cs` | PASS |

## Non-functional requirements

| ID | Requirement | Executable evidence | Status |
|---|---|---|---|
| NFR-1 | Forbidden references and dependency cycles fail the architecture gate | `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-1EvidenceTests.cs`; `tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs` | PASS |
| NFR-2 | Shared encrypted keys, stateless cross-instance state, bounded demo databases/artifacts, and fail-closed Production authority | `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-2EvidenceTests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec004/DataProtectionRegistrationTests.cs`; `tests/StudentRegistration.QualityTests/Specs/Spec004/LocalArtifactRetentionTests.cs` | PASS |
| NFR-3 | Architecture supports at least two stateless application replicas | `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-3EvidenceTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-2Tests.cs` | PASS |
| NFR-4 | Business-module Domain code has no ASP.NET, Blazor, EF Core, or SQL Server dependency | `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-4EvidenceTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-6Tests.cs` | PASS |

## Acceptance scenarios

| ID | Scenario | Executable evidence | Status |
|---|---|---|---|
| AC-1 | Forbidden direct module dependency fails the build | `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-1Tests.cs`; `tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs` | PASS |
| AC-2 | Consecutive authenticated requests can cross replicas without losing authorization or plan state | `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-2Tests.cs`; `tests/StudentRegistration.IntegrationTests/Specs/Spec004/DataProtectionRegistrationTests.cs` | PASS |
| AC-3 | Premature architectural complexity is rejected or requires a separate approved decision | `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-3Tests.cs`; `tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs` | PASS |
| AC-4 | Reads use projected DTOs and commands use the one approved persistence boundary | `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-4Tests.cs`; `tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs`; `tests/StudentRegistration.ArchitectureTests/DtoIsolationTests.cs` | PASS |
| AC-5 | Architecture change includes accepted ADR and updated tests | `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-5Tests.cs`; `tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs` | PASS |
| AC-6 | Required stack is present and business Domain code remains plain C# | `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-6Tests.cs`; `tests/StudentRegistration.ArchitectureTests/ApprovedStackTests.cs`; `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-4EvidenceTests.cs` | PASS |
| AC-7 | Business mutation and append-only audit commit together or both roll back under success, injected fault, and caller rollback | `tests/StudentRegistration.IntegrationTests/Audit/AuditAtomicityTests.cs`; `tests/StudentRegistration.IntegrationTests/Persistence/AuditEventPersistenceTests.cs` | PASS |

## Edge cases

| ID | Boundary | Executable evidence | Status |
|---|---|---|---|
| EC-1 | Additional module read uses a narrow interface, never direct table/internal access | `tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-1Tests.cs`; `tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs` | PASS |
| EC-2 | Cross-module transaction remains in the single DbContext or triggers architectural review | `tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-2Tests.cs`; `tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs` | PASS |
| EC-3 | Stale read cache never authorizes final registration | `tests/StudentRegistration.IntegrationTests/Specs/Spec004/EdgeCases/EC-3Tests.cs`; `tests/StudentRegistration.ArchitectureTests/PersistenceBoundaryTests.cs` | PASS |

## Success criteria

| ID | Criterion | Executable evidence | Status |
|---|---|---|---|
| SC-1 | Every module dependency is explicit and acyclic | `tests/StudentRegistration.ArchitectureTests/ModuleDependencyTests.cs`; `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-1EvidenceTests.cs` | PASS |
| SC-2 | At least two stateless instances preserve user-visible behavior | `tests/StudentRegistration.QualityTests/Specs/Spec004/NFR-3EvidenceTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-2Tests.cs` | PASS |
| SC-3 | Architecture exceptions have an approved decision and simpler alternatives | `tests/StudentRegistration.ArchitectureTests/ProhibitedComplexityTests.cs`; `tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-5Tests.cs` | PASS |

## Frontend route-to-test boundary

| ID | Boundary | Executable evidence | Status |
|---|---|---|---|
| ROUTE-NONE | SPEC-004 owns no frontend route; later UI requires a SPEC-003 route-manifest amendment | `tests/StudentRegistration.QualityTests/Specs/Spec004/TraceabilityEvidenceTests.cs`; `specs/004-architecture-engineering-principles/spec.md` | PASS |

## Critical workstream proof

- Exact project shape and dependency-cycle enforcement:
  `ModuleDependencyTests.cs`.
- Shared stateless key ring and cross-replica configuration:
  `DataProtectionRegistrationTests.cs` and `NFR-2EvidenceTests.cs`.
- Repository-bounded seven-day artifact lifecycle:
  `LocalArtifactRetentionTests.cs`.
- Single-writer DbContext, projected reads, and focused local transactions:
  `PersistenceBoundaryTests.cs`.
- Caller-owned transaction with append-only atomic audit persistence:
  `AuditAtomicityTests.cs` and `AuditEventPersistenceTests.cs`.

The final repository gate reruns all suites before commit. This matrix is
machine-checked; editing a row cannot turn absent or failing evidence into a
passing release.
