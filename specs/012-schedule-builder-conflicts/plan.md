# Implementation Plan: Schedule Builder and Conflicts

**Branch**: 012-schedule-builder-conflicts | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Design complete; DEC-06 remains fail-closed/disabled and human approval is pending. Implementation is not authorized.

## Summary

Deliver Schedule Builder and Conflicts inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-011](../011-eligibility-subject-discovery/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Project Structure

Plan/conflict domain, application, and endpoints belong in
`src/StudentRegistration.Registration/{Domain,Application,Endpoints}`. Pages
remain in `StudentRegistration.Client`; persistence mappings are in
`StudentRegistration.Infrastructure.SqlServer`; Academics/Scheduling are
consumed through module interfaces. No generic Server/Domain business project
or duplicate upstream aggregate is introduced.

## Feature Design

1. One RegistrationPlan root owns a complete student/term selection and
   rowversion; PUT replaces it atomically.
2. Resolve current group/activity/meeting versions from Scheduling and run the
   deterministic half-open interval detector.
3. Return exact overlap plus accessible change/remove actions and a dependency-
   version ValidationSnapshot; stale/unavailable groups block review.
4. Keep capacity advisory and validation side-effect-free; SPEC-014 revalidates
   everything for submission.
5. Disable travel-buffer conflicts until DEC-06 is institutionally approved.

## Execution and Gate Order

Dependency baselines, DEC-06/fail-closed review, and consistency analysis
precede Ahmed ELbamby's final approval. Failing model/contract, acceptance,
concurrency, domain, and frontend tests precede every delivery and handler.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)



## Non-Functional Requirements

- NFR-1: Conflict recalculation SHOULD complete within 200 ms p95 for 8 courses
  with 10 meeting slots each.
- NFR-2: Conflict results MUST be deterministic.
- NFR-3: Calendar and chronological list MUST contain equivalent content.
- NFR-4: Plan editing MUST reject lost updates with 409.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
