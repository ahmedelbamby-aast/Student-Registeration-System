# Implementation Plan: Eligibility and Subject Discovery

**Branch**: 011-eligibility-subject-discovery | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby on 2026-07-13, including the Policy SME review perspective.

**Owner-approved progression amendment (2026-07-20):** Ahmed ELbamby's
explicit instruction approves roadmap-aware term progression, term-1
automatic enrollment, term-2-or-later self-registration, and the bounded
19-21-credit approval-required state. Implementation remains gated on the new
unchecked amendment tasks.

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
consumed through narrow provider `Application.Ports`, Registration owns the
`ICurrentPlanReader` input, and SQL query implementations live in
`StudentRegistration.Infrastructure.SqlServer`. The live reader returns a
versioned empty plan until SPEC-012 contributes its implementation. No
upstream entity, writable plan, or writable eligibility model is redefined in
Registration.

## Feature Design

1. Resolve one versioned academic, policy, catalogue, offering, and group input
   set for the authenticated student and authorized term.
2. Evaluate the governed SPEC-002 window/standing/hold/prerequisite/load/
   repeat/capacity/conflict registry and create complete per-rule reasons;
   missing required input fails closed. Consume SPEC-009 `CurriculumCourse` as
   the programme/cohort roadmap: matching-cohort term-1 roots are automatic
   and self-registration begins at term 2. The rules cover 18-credit normal,
   12-credit probation, per-subject approval for every term-2-or-later
   self-registration, 19-21 credits only for CGPA at least 3.0, and rejection
   above 21. Generic advisor and arbitrary exception workflows remain absent.
3. Project current/projected/applicable load and complete dependency versions
   plus lifecycle-only group state, non-selectable reasons, and nested
   meeting/staff/room/time/capacity/version details. Capacity includes total,
   enrolled, held, and available counts without holder PII; pending approval
   is distinct from enrollment and uses the SPEC-010/SPEC-014 bounded hold.
4. Apply bounded server search/filter/sort/page only after eligibility
   evaluation and expose a dedicated complete detail endpoint.
5. STU-02/STU-03 render stable reasons and non-color states from SPEC-003.
6. Replace mock-only confidence with live SQL/browser fixtures covering a
   published roadmap, automatic term-1 state, normal self-registration,
   approval-required overload, held capacity, rejection, and window close.

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
