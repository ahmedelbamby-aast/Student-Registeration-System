# Implementation Plan: Catalogue, Prerequisites, and Policy Administration

**Branch**: 009-catalog-prerequisites-policy-admin | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby on 2026-07-13.

**Owner-approved policy amendment (2026-07-20):** Ahmed ELbamby's explicit
instruction approves the roadmap, exact-three-credit, prerequisite-shape, and
bounded overload rules added to SPEC-009. Implementation remains gated on the
new unchecked amendment tasks.

## Summary

Deliver Catalogue, Prerequisites, and Policy Administration inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Project Structure

Catalogue and policy domain, application, and endpoints belong in
`src/StudentRegistration.Academics/{Domain,Application,Endpoints}`. The API
composition root, shared contracts, client, and SQL mappings remain in their
respective `StudentRegistration.Api`, `.Contracts`, `.Client`, and
`.Infrastructure.SqlServer` projects. No generic business Server/Domain
project or second catalogue writer is introduced.

## Feature Design

1. Manual changes occur only in `CatalogueDraft`; the initial POC draft is the
   19-course official-source snapshot in `docs/DEMO_CURRICULUM.md`, and import batches
   preserve source/access/hash/status/row errors plus field-level synthetic
   classifications.
2. Validation computes canonical content/dependency hashes and returns a
   bounded signed preview.
3. Confirmation locks one normalized publication scope, revalidates, and
   atomically writes the immutable version, activation change, idempotency
   result, and audit fact.
4. PolicySet uses the same draft/preview/version discipline with typed rules,
   never arbitrary executable expressions. The rule set proves window,
   prerequisite, GPA/earned-credit, standing, 18-credit normal and 12-credit
   probation limits, capacity, and conflict behavior. A bounded 19-21-credit
   path requires CGPA at least 3.0. Every term-2-or-later self-registration
   plan uses downstream per-subject approval; plans above 21 fail. Generic
   advisor and arbitrary exception workflows remain absent.
5. `CurriculumCourse` is the programme/cohort roadmap. Validation enforces
   exactly three credits, prerequisite-free term-1 roots, and at least one
   valid prerequisite for every later subject.
6. Replace validation-only/unavailable catalogue handlers with owner-service
   and SQL persistence calls, expose an accessible subject/roadmap editor, and
   seed Development with the approved published roadmap.
7. ADM-05 consumes only Academics owner APIs plus SPEC-017 audit/report
   contributions.

## Execution and Gate Order

Ahmed ELbamby's Gate A demo approval is recorded. Dependency baselines plus
consistency analysis remain the first execution tasks. Failing model/contract,
acceptance, race, and workstream tests precede domain/application delivery;
handlers and the page follow their contract/behavior and E2E tests. Gate B-D,
release, production catalogue/policy publication, and official AASTMT go-live
approvals remain separate.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)



## Non-Functional Requirements

- NFR-1: Import validation for 10,000 rows SHOULD finish within 30 seconds in
  staging.
- NFR-2: Simulation MUST be deterministic for the same version/input.
- NFR-3: Publication MUST be transactional.
- NFR-4: Every published change MUST have actor, reason, source, and timestamp.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
