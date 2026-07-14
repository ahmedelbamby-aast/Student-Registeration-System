# Implementation Plan: Academic Term and Student Profile

**Branch**: 008-academic-term-student-profile | **Date**: 2026-07-14 | **Spec**: [spec.md](spec.md)
**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby; the
clarified narrative contract has standing approval as of 2026-07-14.
Production institutional data-source approval remains separate.

## Summary

Deliver Academic Term and Student Profile inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: ASP.NET Core, Blazor WebAssembly, Entity Framework Core, LINQ
**Storage**: SQL Server with Code First migrations plus canonical-owner
synthetic seed contribution after the Development/per-run Testing migration
completes
**Testing**: xUnit plus API, real-SQL integration, concurrency, accessibility,
browser, and a 25,000-account shared-SQL 10-minute two-replica
300-authenticated-context-read/s load test
**Project Type**: Web application with hosted WebAssembly client and server API
**Performance Goals**: Governed by SPEC-018 and feature NFRs
**Constraints**: Atomic writes, WCAG 2.2 AA, stateless APIs, bounded payloads,
no client-authoritative decisions
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

1. Resolve server time, zero/one Teaching term, zero/one RegistrationOpen term,
   and the deterministic Published window in one Academics query boundary.
   Ambiguous term state fails unavailable; window time is half-open and
   selection is open, otherwise earliest upcoming, otherwise latest closed,
   with UTC/ID tie-breaks.
2. Compose those values with SPEC-007 session data in the shared SPEC-006
   `AppContextDto`, using its one nullable canonical
   `RegistrationWindowSummaryDto`; do not create an Academic AppContext DTO.
3. Persist explicit term/window lifecycle plus rowversions; compute
   upcoming/open/closed at read time. Reject a second RegistrationOpen/Teaching
   term, direct Draft-to-Published update, or edits to Published scope/interval.
   Store globally unique term CreationClientRequestId/CreationPayloadHash for
   payload-bound POST replay without a seventh entity.
4. Use the SPEC-008-owned `StudentTermAcademicState` for hold/profile versus
   registration serialization. Prove the upstream protocol with a test
   consumer and a no-commit callback; leave real seat/enrollment conformance to
   dependent SPEC-014.
5. Keep term and student Admin list/mutation handlers in Academics; publish
   bounded audit facts for SPEC-017 rather than duplicating writes.
6. Contribute the complete existing Student/profile graph to the shared
   versioned non-production seed profile. Values are synthetic and logically
   deterministic by profile version/ordinal, re-seeding is idempotent, and no
   production record or unapproved demographic field is introduced.
7. Page transcript attempts and provenance independently with the canonical
   default 20/maximum 100 contract. Return all active holds together, cap them
   at 100, and fail the profile closed rather than returning a partial set.
   Limit term windows/version entries and profile operations to 20, bound
   searches/strings/errors, and expose hold IDs only in the named Admin detail.
8. Treat transcript history as append-only: a correction appends a new attempt
   linked to the current same-student/course/term leaf. Enforce one successor,
   acyclic chains, leaf-only summaries, exact term, and student-term version.
9. Use a deliberately conservative publication invariant: no two Published
   windows may overlap anywhere in one term, regardless of scope.
10. Keep application services behind narrow Academics ports, implement durable
     queries/commands and atomic audit in the SQL Server adapter, register them
     in the API composition root, and expose one small client API facade to the
     four owned pages.
11. Consume explicit `AcademicTerms.Manage` and `AcademicProfiles.Manage`
     permissions plus `Context.Read` and `AcademicProfile.ReadOwn`; register
     their executable policies and server-derived claims. Student self and a
     named permitted Admin are allowed; Lecturer/TA profile reads are denied.
     Exercise keyboard, semantic, validation, denied, stale, and automated
     accessibility states for every changed page.
12. Freeze complete request/response/status/auth/error matrices for all ten
    endpoints before failing implementation tests. Term creation is
    payload-idempotent; publication/profile correction use expected versions
    and one atomic transaction with safe cancellation/refetch semantics.
13. Update the canonical shared AppContext/window contract source, nullable
    frontend context/shell projection, and Testing migration-seed bootstrapper
    through exact test-first delivery paths. Reconcile the governed class
    diagram without creating an Academics-to-Identity dependency.

## Execution and Gate Order

Ahmed ELbamby's Gate A demo approval is recorded. Dependency baselines,
complete endpoint contracts, and consistency analysis remain the first
execution tasks. Create failing
model/contract, acceptance, race, deterministic synthetic-profile, and
workstream tests; deliver domain/application behavior and the Academics seed
contributor; deliver SQL adapters and composition; add handlers and the client
API facade; then add route E2E/pages, focused accessibility checks, and release
evidence.
Every handler and page is preceded by its contract and behavior tests.
The shared DTO extension, executable permission/claim issuance, nullable shell
projection, Testing bootstrapper, and class-diagram reconciliation each receive
an exact failing test or documentation assertion before delivery.
Production data-source, Gate B-D, release, and official AASTMT go-live
approvals remain separate.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)



## Non-Functional Requirements

- NFR-1: Time-dependent behavior MUST use TimeProvider and boundary tests.
- NFR-2: Authenticated dashboard context MUST remain at or below 300 ms p95
  during a continuous 10-minute run of 300 GET /api/context reads per second
  across two independently addressable stateless replicas sharing SQL and the
  approved 25,000-account fixture, with fewer than 0.1% unexpected failures.
- NFR-3: SPEC-008 instants MUST be stored in UTC datetime2 and every term MUST
  use a valid IANA timezone identifier. Recurring meeting persistence remains
  owned and verified by downstream SPEC-010.
- NFR-4: Student academic data MUST be restricted to self and approved staff
  scopes. Non-production fixtures MUST be wholly synthetic, and logs, traces,
  snapshots, and test reports MUST NOT contain a full student profile.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
