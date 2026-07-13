# Implementation Plan: Architecture and Engineering Principles

**Branch**: 004-architecture-engineering-principles | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Deliver Architecture and Engineering Principles inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: ASP.NET Core, Blazor WebAssembly, Entity Framework Core, LINQ
**Storage**: SQL Server with Code First migrations
**Testing**: xUnit plus API, integration, concurrency, accessibility, and browser tests as applicable
**Project Type**: Web application with hosted WebAssembly client and server API
**Performance Goals**: Governed by SPEC-018 and feature NFRs
**Constraints**: Atomic writes, WCAG 2.2 AA, stateless APIs, no client-authoritative decisions
**Scale/Scope**: Registration-peak horizontal scaling; bounded and paginated queries

## Constitution Check

- PASS: Git ownership is reserved for Ahmed ELbamby.
- PASS: Requirements, acceptance scenarios, and tasks use stable traceability identifiers.
- PASS: The design remains a simple modular monolith.
- PASS: Security, policy, schedule, capacity, and term decisions remain server-authoritative.
- PASS: Accessibility, scalability, concurrency, and observability requirements are retained.
- PASS: No application source code or migration is created by this planning phase.

## Dependency Check

- [SPEC-001](../001-product-charter-rbac/spec.md)
- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)

## Project Structure

The canonical solution shape is exactly the project-per-business-module modular
monolith in `docs/ARCHITECTURE.md`: Client, Api, Contracts, IdentityAccess,
Academics, Scheduling, Registration, StaffAdministration, and
Infrastructure.SqlServer projects plus `tests/`. The API project is composition
only; each business project contains its own Domain, Application, and Endpoints
folders. A generic Server, Domain, Application, or Infrastructure project is a
forbidden alternative shape.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Feature Design

- Encode allowed project references and absence of cycles as executable
  architecture rules.
- Keep one deployable API and one SQL Server database/DbContext while allowing
  each module to own its model/configuration contribution through narrow ports.
- Require encrypted, least-privilege shared Data Protection keys for all API
  replicas without sticky sessions; production repository/encryption authority
  remain fail-closed until Security/DevOps approval.
- Own architecture records and conformance tests only; runtime module
  registration belongs to the API composition root, and DbContext source and
  migrations belong exclusively to Infrastructure.SqlServer.
- Own the narrow transaction-aware audit write port, append-only AuditEvent
  mapping, and SQL writer upstream so every feature can audit atomically
  without depending on SPEC-017's Admin query/export module.

## Execution Strategy

1. Validate SPEC-001 and SPEC-003, then reconcile architecture documents,
   module contracts, and planned paths through consistency analysis.
2. Record Architect and Ahmed ELbamby approval as the final planning gate.
3. Write failing solution-shape, dependency, DTO-isolation, persistence, and
   atomic-audit boundary tests before creating the solution, composition,
   persistence, and audit seams.
4. Require an approved ADR plus updated tests for every later boundary or
   deployment change.



## Non-Functional Requirements

- NFR-1: Architecture tests MUST fail on forbidden module references/cycles.
- NFR-2: Application instances MUST be stateless except for shared database
  and approved key/config stores.
- NFR-3: The architecture MUST support at least two application replicas.
- NFR-4: Domain code inside each business-module project MUST reference no
  ASP.NET, Blazor, EF Core, or SQL Server type or namespace.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
