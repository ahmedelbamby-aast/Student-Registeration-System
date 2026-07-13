# Implementation Plan: ERD and Data Lifecycle

**Branch**: 005-erd-data-lifecycle | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Deliver ERD and Data Lifecycle inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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
- [SPEC-004](../004-architecture-engineering-principles/spec.md)

## Project Structure

Future implementation follows the project-per-business-module modular monolith
in `docs/ARCHITECTURE.md`. Each canonical feature owner defines its runtime
aggregate and EF configuration contribution inside its business-module project;
`StudentRegistration.Infrastructure.SqlServer` alone composes the single
`StudentRegistrationDbContext` and owns migrations. SPEC-005 owns the ERD,
entity-ownership matrix, relational invariant catalogue, lifecycle rules, and
schema conformance—not downstream runtime classes or mappings.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Feature Design

- Assign every ERD entity to exactly one canonical feature owner and identify
  SPEC-005 as schema-governance owner only.
- Define keys, checks, rowversion boundaries, immutable-history rules,
  provenance, indexing, migration, backup, and rollback requirements as a
  contract consumed by owner specs.
- Maintain an explicit critical-query inventory and production-like row-count
  fixture; require actual-plan evidence and expiring Data Lead exceptions.
- Keep the AASTMT Operations deployment-window duration In Review and block
  readiness until a numeric window is approved; rehearsal target is 80% of it.
- Use `RegistrationSubmission` as the single durable idempotency record owned
  by SPEC-014; do not introduce a second `IdempotencyRecord` entity.

## Execution Strategy

1. Validate SPEC-002/SPEC-004, reconcile ERD ownership, and complete
   consistency analysis without depending on downstream runtime source.
2. Record Data Lead and Ahmed ELbamby approval as the final planning gate.
3. Test the ownership/invariant/lifecycle schemas before publishing them.
4. Each later canonical owner implements and tests its own EF mapping; deferred
   cross-module schema conformance runs only after those owner specs are
   approved and implemented.
5. The dependency-ordered slice owners in `.specify/persistence-manifest.json`
   generate one S1 initial migration and reviewed S2/S4/S6/S7 incrementals.
   Every slice proves fresh apply, prior-version upgrade, rollback/script
   safety, shared snapshot parity, and ERD conformance before its end-to-end
   sprint exit; Gate D repeats the full chain.

## Non-Functional Requirements

- NFR-1: Every approved critical query touching a forecast table above 10,000
  rows MUST have a reviewed actual plan; unbounded scans require an expiring
  Data Lead exception with measured evidence.
- NFR-2: Rehearsal MUST complete inside 80% of the numeric AASTMT Operations
  deployment window; readiness remains fail closed until that window is approved.
- NFR-3: Backup/restore MUST meet SPEC-018 RPO/RTO.
- NFR-4: Sensitive fields MUST be minimized and excluded from unsafe logs.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
