# SPEC-004: Architecture and Engineering Principles

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Technical Lead/Architect<br>
**Reviewers:** Backend, Data, Security, DevOps, QA<br>
**Target:** Sprint 0<br>
**Dependencies:** SPEC-001, SPEC-003<br>

## Context

The system needs strong transactional consistency and future feature seams
without the deployment and failure complexity of an early distributed system.
docs/ARCHITECTURE.md and ADR-001 define the proposed modular monolith.

## Functional Requirements

- FR-1: The solution MUST use .NET 10 LTS, ASP.NET Core, Blazor WebAssembly,
  EF Core/LINQ, and SQL Server.
- FR-2: The solution MUST contain IdentityAccess, Academics, Scheduling,
  Registration, and StaffAdministration business modules.
- FR-3: Module internals MUST NOT be referenced across boundaries; interaction
  uses approved interfaces/contracts.
- FR-4: One DbContext/database MUST support atomic registration initially.
- FR-5: API DTOs MUST NOT expose EF entities.
- FR-6: Reads SHOULD use LINQ projection/AsNoTracking; commands use focused
  application services.
- FR-7: Generic repository, microservices, broker, event sourcing, dynamic rule
  DSL, and institution-wide solver MUST NOT be introduced in MVP.
- FR-8: Architectural changes MUST include an ADR and architecture-test update.

## Non-Functional Requirements

- NFR-1: Architecture tests MUST fail on forbidden module references/cycles.
- NFR-2: Application instances MUST be stateless except for shared database
  and approved key/config stores.
- NFR-3: The architecture MUST support at least two application replicas.
- NFR-4: Domain projects MUST have no dependency on ASP.NET, Blazor, EF, or SQL.

## Acceptance Criteria

### AC-1: Forbidden dependency (FR-2, FR-3, NFR-1)
Given Academics has an internal implementation type<br>
When Registration directly references that type<br>
Then the architecture test fails the build.

### AC-2: Horizontal instance (NFR-2, NFR-3)
Given two application instances share SQL and Data Protection keys<br>
When an authenticated user sends consecutive requests to different instances<br>
Then authorization and plan state remain correct.

### AC-3: Complexity gate (FR-7)
Given a proposal adds a message broker before a durable external consumer
exists<br>
When architecture review occurs<br>
Then the proposal is rejected or moved to a separately approved ADR/spec.

### AC-4: Persistence and DTO boundary (FR-4, FR-5, FR-6)
Given an offering read and an atomic registration command<br>
When architecture/code review runs<br>
Then both use the single approved DbContext transaction boundary where needed<br>
And the read uses a projected DTO rather than exposing an EF entity.

### AC-5: Architecture change governance (FR-8)
Given a pull request changes a module dependency or deployment decision<br>
When CI and review run<br>
Then an approved ADR and updated architecture test are required.

### AC-6: Required stack and domain purity (FR-1, NFR-4)
Given the solution manifest and compiled dependency graph<br>
When architecture conformance tests execute<br>
Then the solution uses the approved .NET/ASP.NET Core/Blazor/EF Core/SQL Server
stack<br>
And Domain projects reference none of ASP.NET, Blazor, EF Core, or SQL Server.

## Edge Cases

- EC-1: A module needs an additional read -> add a narrow query interface, not
  direct table ownership leakage.
- EC-2: Cross-module transaction emerges -> keep it in the single DbContext or
  stop for an architectural review.
- EC-3: A read cache is stale -> final registration never trusts it.

## API Contracts

This spec establishes dependency/deployment constraints. Its minimal
composition boundary includes GET /api/health; public feature shapes belong to
SPEC-006 onward.

## Data Models

| Schema | Owner |
|---|---|
| auth | IdentityAccess |
| academics | Academics |
| scheduling | Scheduling |
| registration | Registration |
| audit | StaffAdministration / audit service |

## Out of Scope

- OS-1: Independent module deployments in MVP.
- OS-2: Kubernetes and service mesh.
- OS-3: Separate read/write databases.
- OS-4: Distributed transaction protocol.
