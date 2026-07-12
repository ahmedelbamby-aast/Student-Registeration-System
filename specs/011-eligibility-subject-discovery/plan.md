# Implementation Plan: Eligibility and Subject Discovery

**Branch**: 011-eligibility-subject-discovery | **Date**: 2026-07-12 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Deliver Eligibility and Subject Discovery inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Project Structure

Future implementation paths are src/StudentRegistration.Client, src/StudentRegistration.Server, src/StudentRegistration.Domain, src/StudentRegistration.Infrastructure, and tests/. These paths are declarations only and do not exist yet.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Non-Functional Requirements

- NFR-1: Discovery SHOULD respond within 300 ms p95 at 300 read requests/s.
- NFR-2: Search input MUST be parameterized and limited in length.
- NFR-3: Eligibility MUST be deterministic for a fixed input/version.
- NFR-4: Eligibility/status MUST not rely on color alone.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
