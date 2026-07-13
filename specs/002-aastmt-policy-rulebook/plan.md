# Implementation Plan: AASTMT Policy Rulebook

**Branch**: 002-aastmt-policy-rulebook | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Deliver AASTMT Policy Rulebook inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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

## Project Structure

Future implementation follows the project-per-business-module modular monolith in
`docs/ARCHITECTURE.md`: `StudentRegistration.Client`, `StudentRegistration.Api`,
`StudentRegistration.Contracts`, `StudentRegistration.IdentityAccess`,
`StudentRegistration.Academics`, `StudentRegistration.Scheduling`,
`StudentRegistration.Registration`, `StudentRegistration.StaffAdministration`,
and `StudentRegistration.Infrastructure.SqlServer`, plus `tests/`. SPEC-002 owns
the rulebook, provenance, approval, and fail-closed semantics; SPEC-009 owns the
runtime policy aggregates and evaluator.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Feature Design

- Keep public-source findings and every unresolved `POLICY-Q` explicitly In
  Review; no unapproved numeric value becomes a production default.
- Publish a typed policy contract and boundary examples for SPEC-009 to
  implement. `PolicySet`, `PolicyRule`, and decision snapshots are reference
  models here, not downstream runtime prerequisites.
- Declare the offering-eligibility resource as an endpoint owned and delivered
  by SPEC-011; this rulebook contributes its decision shape only.

## Execution Strategy

1. Validate SPEC-001 and the institutional provenance/approval register.
2. Complete consistency analysis and freeze only approved policy inputs.
3. Record Registrar/SME and Ahmed ELbamby approval as the final planning gate.
4. Test the rulebook schema and boundary fixtures before publishing them;
   runtime evaluator work waits for approved SPEC-009 and endpoint work for
   approved SPEC-011.



## Non-Functional Requirements

- NFR-1: The same input and policy version MUST yield the same result.
- NFR-2: All boundary examples supplied by the Registrar MUST have automated
  regression tests.
- NFR-3: A policy decision query SHOULD complete within 100 ms p95 excluding
  initial data retrieval.
- NFR-4: Source URL, access date, approval actor, and effective period MUST be
  auditable.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
