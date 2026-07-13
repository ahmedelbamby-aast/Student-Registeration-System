# Implementation Plan: Eligibility and Subject Discovery

**Branch**: 011-eligibility-subject-discovery | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby on 2026-07-13, including the Policy SME review perspective.

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
- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Project Structure

Eligibility orchestration, projections, and owner endpoints belong in
`src/StudentRegistration.Registration/{Domain,Application,Endpoints}`. Pages
remain in `StudentRegistration.Client`; upstream Academics/Scheduling data is
consumed through public module interfaces, and SQL query implementations live
in `StudentRegistration.Infrastructure.SqlServer`. No upstream entity is
redefined in Registration.

## Feature Design

1. Resolve one versioned academic, policy, catalogue, offering, and group input
   set for the authenticated student and authorized term.
2. Evaluate all relevant approved rules and create complete per-rule reasons;
   missing required input fails closed. The demo rule set explicitly covers
   window, standing, holds, prerequisites, course GPA/earned credits, 18-credit
   normal target/maximum, 12-credit probation maximum, capacity, and exact
   meeting conflict without advisor/exception workflows.
3. Project current/projected/applicable load plus group activity/staff/room/
   time/capacity/version details from upstream modules without treating
   advisory capacity as a reservation.
4. Apply bounded server search/filter/sort/page only after eligibility
   evaluation and expose a dedicated complete detail endpoint.
5. STU-02/STU-03 render stable reasons and non-color states from SPEC-003.

## Execution and Gate Order

Ahmed ELbamby's Gate A demo approval, including the Policy SME review
perspective, is recorded. Dependency and policy baselines plus consistency
analysis remain the first execution tasks. Failing projection/contract,
acceptance, search, quality, and frontend tests precede application, handler,
and page delivery. Gate B-D, release, production policy use, and official
AASTMT go-live approvals remain separate.

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
