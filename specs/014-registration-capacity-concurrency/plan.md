# Implementation Plan: Registration Capacity and Concurrency

**Branch**: 014-registration-capacity-concurrency | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Deliver Registration Capacity and Concurrency inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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
- [SPEC-011](../011-eligibility-subject-discovery/spec.md)
- [SPEC-012](../012-schedule-builder-conflicts/spec.md)
- [SPEC-013](../013-schedule-recommendations/spec.md)
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

- NFR-1: There MUST be zero group overbooking, duplicate active offering
  enrollment, partial schedule commit, or combined same-student policy/timetable
  violation in every target and spike concurrency test.
- NFR-2: Submission p95 MUST be at most 2 seconds at 75 submissions per second
  for 10 minutes using the production-like dataset.
- NFR-3: A 200-submission-per-second, 60-second spike MUST preserve every NFR-1
  invariant across at least two application replicas.
- NFR-4: Database transactions MUST be short, cancellation-aware before commit,
  and contain no HTTP, message-broker, email, or other remote call.
- NFR-5: Expected conflicts MUST not count as server failures; unexpected
  failure rate MUST remain below 0.1% at target load.
- NFR-6: Deadlock count, lock-wait p95, idempotent replay count, conflict-code
  count, and reconciliation mismatch count MUST be observable without logging
  student credentials or full academic records.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
