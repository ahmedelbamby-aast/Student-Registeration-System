# Implementation Plan: Architecture and Engineering Principles

**Branch**: 004-architecture-engineering-principles | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Gate A requirements and the post-Gate-A consistency correction to
the executable task baseline were approved by Ahmed ELbamby on 2026-07-13.
T006 and later implementation tasks may run in dependency order; Gates B-D and
production release approval remain required.

## Summary

Deliver Architecture and Engineering Principles inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: ASP.NET Core, Blazor WebAssembly, Entity Framework Core, LINQ
**Storage**: Demo SQL Server 2022 Developer, compatibility level 160, through
Docker for Development and Testcontainers for per-run Testing; Code First
migrations; no production edition/topology decision
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
- Pin the demo database runtime to SQL Server 2022 Developer compatibility 160:
  Docker provisions Development and Testcontainers provisions isolated Testing
  databases. Dispose Testing after each run; retain Development until an
  explicit guarded reset. No real institutional data is permitted.
- Keep local credential artifacts, logs, and exports Git-ignored and remove
  them no later than seven days after creation.
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

1. Validate the immutable SPEC-001 and SPEC-003 inputs, then reconcile
   architecture documents, module contracts, and planned paths through
   consistency analysis.
2. Obtain Ahmed ELbamby's explicit approval of the corrected executable task
   baseline before T006 or any later implementation task; retain later release
   and production architecture approvals.
3. Establish the missing ContractTests shell and run T010 red checks; T011
   creates the ArchitectureTests shell plus only the Contracts and
   Infrastructure.SqlServer runtime shells and canonical shared types. T023
   then proves the still-incomplete full module shape fails, and T024 completes
   the six remaining source projects and composition.
4. After the single DbContext configuration seam exists, run failing
   stateless-replica/key-lifecycle checks, then deliver the SQL-backed,
   certificate-protected key ring, fail-closed production configuration,
   rotation runbook, and bounded seven-day local cleanup.
5. Write failing dependency, DTO-isolation, persistence, atomic-audit, SQL
   provisioner/lifecycle, synthetic-only, and Git-ignore checks before their
   bounded delivery.
6. Require an accepted ADR plus updated tests for every later boundary or
   deployment change.

## Post-Gate-A Consistency Correction

The approved task baseline adds no requirement or architecture decision. It
restores the already-approved AC-2 and AC-7 traceability, makes audit ownership
and ADR status literal, moves project creation ahead of tests that need those
projects, removes a shared-file writer collision, and adds the missing bounded
NFR-2 delivery task before release evidence. It also completes one truncated
activation sentence from DEC-01/DEC-08 and corrects SPEC-005's single stale
AuditEvent owner line. Ahmed ELbamby approved the exact correction on
2026-07-13, completing T005 and allowing dependency-ordered implementation.



## Non-Functional Requirements

- NFR-1: Architecture tests MUST fail on forbidden module references/cycles.
- NFR-2: Application instances MUST be stateless except for shared database
  and approved key/config stores. Demo Testing databases are disposed per run,
  Development persists until guarded reset, all data is synthetic, and local
  credential/log/export artifacts are Git-ignored and retained at most seven
  days; production key-store decisions remain fail closed.
- NFR-3: The architecture MUST support at least two application replicas.
- NFR-4: Domain code inside each business-module project MUST reference no
  ASP.NET, Blazor, EF Core, or SQL Server type or namespace.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
