# Implementation Plan: Admin Operations, Audit, and Reporting

**Branch**: 017-admin-operations-audit-reporting | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Deliver Admin Operations, Audit, and Reporting inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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

- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-014](../014-registration-capacity-concurrency/spec.md)
- [SPEC-015](../015-student-registration-records/spec.md)
- [SPEC-016](../016-lecturer-ta-workspace/spec.md)
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

- NFR-1: Operational metrics SHOULD be no more than 60 seconds stale and show
  observation timestamp.
- NFR-2: Audit search SHOULD return first page within 1 second p95 at approved
  retention volume.
- NFR-3: Export generation MUST be asynchronous/bounded for large data and
  expire securely.
- NFR-4: Admin actions MUST have authorization, audit, concurrency, and
  validation tests.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
