# Feature Specification: Architecture and Engineering Principles

**Feature Branch**: 004-architecture-engineering-principles
**Created**: 2026-07-12
**Status**: Approved (Gate A demo implementation, 2026-07-13)
**Owner**: Technical Lead/Architect
**Normative detail**: [requirements.md](requirements.md)

## Context

The system needs strong transactional consistency and future feature seams
without the deployment and failure complexity of an early distributed system.
docs/ARCHITECTURE.md and ADR-001 define the proposed modular monolith.

## User Scenarios and Testing

### User Story 1 - Forbidden dependency (FR-2, FR-3, NFR-1) (P1)

As a Technical Lead, I need the Forbidden dependency (FR-2, FR-3, NFR-1) behavior so that Architecture and Engineering Principles produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given Academics has an internal implementation type<br>
When Registration directly references that type<br>
Then the architecture test fails the build.
### User Story 2 - Horizontal instance (NFR-2, NFR-3) (P1)

As a Technical Lead, I need the Horizontal instance (NFR-2, NFR-3) behavior so that Architecture and Engineering Principles produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given two application instances share SQL and Data Protection keys<br>
When an authenticated user sends consecutive requests to different instances<br>
Then authorization and plan state remain correct<br>
And evidence proves encrypted shared-key persistence without sticky sessions.
### User Story 3 - Complexity gate (FR-7) (P2)

As a Technical Lead, I need the Complexity gate (FR-7) behavior so that Architecture and Engineering Principles produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a proposal adds a message broker before a durable external consumer
exists<br>
When architecture review occurs<br>
Then the proposal is rejected or moved to a separately approved ADR/spec.
### User Story 4 - Persistence and DTO boundary (FR-4, FR-5, FR-6) (P2)

As a Technical Lead, I need the Persistence and DTO boundary (FR-4, FR-5, FR-6) behavior so that Architecture and Engineering Principles produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given an offering read and an atomic registration command<br>
When architecture/code review runs<br>
Then both use the single approved DbContext transaction boundary where needed<br>
And the read uses a projected DTO rather than exposing an EF entity.
### User Story 5 - Architecture change governance (FR-8) (P3)

As a Technical Lead, I need the Architecture change governance (FR-8) behavior so that Architecture and Engineering Principles produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given a pull request changes a module dependency or deployment decision<br>
When CI and review run<br>
Then an approved ADR and updated architecture test are required.
### User Story 6 - Required stack and domain purity (FR-1, NFR-4) (P3)

As a Technical Lead, I need the Required stack and domain purity (FR-1, NFR-4) behavior so that Architecture and Engineering Principles produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given the solution manifest and compiled dependency graph<br>
When architecture conformance tests execute<br>
Then the solution uses the approved .NET/ASP.NET Core/Blazor/EF Core/SQL Server
stack and exact project-per-business-module shape<br>
And business-module Domain code references none of ASP.NET, Blazor, EF Core,
or SQL Server.

## Edge Cases

- EC-1: A module needs an additional read -> add a narrow query interface, not
  direct table ownership leakage.
- EC-2: Cross-module transaction emerges -> keep it in the single DbContext or
  stop for an architectural review.
- EC-3: A read cache is stale -> final registration never trusts it.

## Requirements

### Functional Requirements

- FR-1: The solution MUST use .NET 10 LTS, ASP.NET Core, Blazor WebAssembly,
  EF Core/LINQ, and SQL Server. The demo database runtime MUST use SQL Server
  2022 Developer at compatibility level 160, provisioned through Docker for
  Development and Testcontainers for Testing. This demo choice MUST NOT be
  represented as approval of a production SQL Server edition or topology.
- FR-2: The solution MUST use the exact project-per-business-module shape from
  `docs/ARCHITECTURE.md`: Client, Api, Contracts, IdentityAccess, Academics,
  Scheduling, Registration, StaffAdministration, and Infrastructure.SqlServer.
  The API project is composition-only; generic Server, Domain, Application, or
  Infrastructure projects MUST NOT replace these module projects.
- FR-3: Module internals MUST NOT be referenced across boundaries; interaction
  uses approved interfaces/contracts.
- FR-4: Infrastructure.SqlServer MUST own the single DbContext, migrations, and
  configuration composition for one SQL Server database so registration can be
  atomic initially; canonical feature specs own their model/mapping requirements.
- FR-5: API DTOs MUST NOT expose EF entities.
- FR-6: Reads SHOULD use LINQ projection/AsNoTracking; commands use focused
  application services.
- FR-7: Generic repository, microservices, broker, event sourcing, dynamic rule
  DSL, and institution-wide solver MUST NOT be introduced in MVP.
- FR-8: Architectural changes MUST include an ADR and architecture-test update.
- FR-9: The architecture MUST provide the upstream transaction-aware
  AuditWritePort, append-only AuditEvent model/mapping, and SQL writer so
  sensitive business state and audit commit or roll back together without a
  dependency on downstream SPEC-017.

### Non-Functional Requirements

- NFR-1: Architecture tests MUST fail on forbidden module references/cycles.
- NFR-2: Application instances MUST be stateless except for shared database
  and approved key/config stores. Data Protection keys MUST be shared across
  replicas, encrypted at rest, rotated under an approved runbook, and readable
  by only the application identity. The production repository and key-encryption
  authority remain an explicit Security/DevOps institutional decision; release
  readiness MUST fail closed until approved. In the demo profile, every
  per-run Testing database MUST be disposed after its run, the Development
  database MUST persist until an explicit guarded reset, and all records MUST
  be synthetic. Local credential artifacts, logs, and exports MUST be
  Git-ignored and removed no later than seven days after creation.
- NFR-3: The architecture MUST support at least two application replicas.
- NFR-4: Domain code inside each business-module project MUST reference no
  ASP.NET, Blazor, EF Core, or SQL Server type or namespace.

### Key Entities

- **ModuleBoundary**: Governed architecture artifact owned by SPEC-004; not a runtime or SQL entity.
- **ArchitectureDecision**: Governed ADR record owned by SPEC-004; not a runtime or SQL entity.
- **DependencyRule**: Executable architecture-test rule owned by SPEC-004; not a persistence entity.
- **AuditEvent**: Shared append-only persistence record owned by SPEC-004.
- **AuditWritePort**: Shared transaction-aware contract owned by SPEC-004.

## Success Criteria

- **SC-1**: Every module dependency is explicit and free of cycles.
- **SC-2**: The system can run on at least two stateless application instances without changing user-visible behavior.
- **SC-3**: Every architecture exception is recorded with an approved decision and simpler alternatives considered.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-001](../001-product-charter-rbac/spec.md)
- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)

## Frontend Route Ownership

No route is directly owned. Any later UI exposure requires a SPEC-003 route-manifest amendment before implementation.

## Out of Scope

- OS-1: Independent module deployments in MVP.
- OS-2: Kubernetes and service mesh.
- OS-3: Separate read/write databases.
- OS-4: Distributed transaction protocol.
