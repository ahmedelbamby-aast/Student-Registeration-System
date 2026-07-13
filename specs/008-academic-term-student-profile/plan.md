# Implementation Plan: Academic Term and Student Profile

**Branch**: 008-academic-term-student-profile | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Design complete; human approval and institutional data-source approval remain pending. Implementation is not authorized.

## Summary

Deliver Academic Term and Student Profile inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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

- [SPEC-002](../002-aastmt-policy-rulebook/spec.md)
- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Project Structure

Academics domain, application, and owner endpoints belong in
`src/StudentRegistration.Academics/{Domain,Application,Endpoints}`. The API
composition root is `src/StudentRegistration.Api`, cross-module DTO conventions
are in `src/StudentRegistration.Contracts`, pages are in
`src/StudentRegistration.Client`, and mappings are in
`src/StudentRegistration.Infrastructure.SqlServer`. No generic Server/Domain
business project is introduced.

## Feature Design

1. Resolve server time, teaching term, registration term, and the single
   matching published window in one Academics query boundary.
2. Compose that academic portion with SPEC-007 session data for `/api/context`.
3. Persist explicit term/window lifecycle plus rowversions; compute
   upcoming/open/closed at read time.
4. Use the SPEC-008-owned `StudentTermAcademicState` for hold/profile versus
   registration serialization.
5. Keep term and student Admin list/mutation handlers in Academics; publish
   bounded audit facts for SPEC-017 rather than duplicating writes.

## Execution and Gate Order

Dependency baselines and consistency analysis precede Ahmed ELbamby's final
planning approval. After approval: create failing model/contract, acceptance,
race, and workstream tests; deliver domain/application behavior; add handlers;
then add route E2E/pages and release evidence. Every handler and page is
preceded by its contract and behavior tests.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)



## Non-Functional Requirements

- NFR-1: Time-dependent behavior MUST use TimeProvider and boundary tests.
- NFR-2: Dashboard context SHOULD load within 300 ms p95 at the SPEC-018
  300-read-requests-per-second target.
- NFR-3: Instants MUST be stored in UTC datetime2; recurring class times use
  DayOfWeek/TimeOnly and term timezone.
- NFR-4: Student academic data MUST be restricted to self and approved staff
  scopes.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
