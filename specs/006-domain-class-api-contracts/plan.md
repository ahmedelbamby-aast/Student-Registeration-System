# Implementation Plan: Domain Classes and API Contracts

**Branch**: 006-domain-class-api-contracts | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Approved for Gate A demo implementation on 2026-07-13; Gates B-D
and production release approval remain required.

## Summary

Deliver Domain Classes and API Contracts inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: .NET BCL and System.Text.Json only for `StudentRegistration.Contracts`; ASP.NET Core and Blazor WebAssembly consume the contracts from their owning projects
**Storage**: None owned; the Contracts project references neither EF Core nor SQL Server and consumes only SPEC-005 persistence-boundary contracts
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

- [SPEC-004](../004-architecture-engineering-principles/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)

## Project Structure

Future implementation follows the project-per-business-module modular monolith
in `docs/ARCHITECTURE.md`. Shared DTO conventions and cross-module identifiers
live in `StudentRegistration.Contracts`; endpoint handlers live in the owning
business module; `StudentRegistration.Api` is composition only. No generic
Server, Domain, Application, or Infrastructure project is introduced.
`StudentRegistration.Contracts` remains framework- and persistence-free: no
ASP.NET Core, Blazor, EF Core, SQL Server, or business-module implementation
reference is permitted.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Feature Design

- Own only the stable shared serialized/value types `ApiError`, `Page<T>`,
  `AppContextDto`, `TermSummaryDto`, `RegistrationWindowSummaryDto`, and
  `PublicContextDto`, plus the narrow expected-rowversion and idempotency-key
  metadata required by FR-4/FR-8.
- SPEC-007 contributes session/user/authorized-role fields; SPEC-008 contributes
  authoritative time, teaching/registration term, and window fields and owns
  the context endpoint handlers. SPEC-006 owns neither feature state nor those
  endpoint implementations.
- Expose matched-window ID, computed state, UTC interval, and row version only
  in authenticated `AppContextDto`; keep `PublicContextDto` at exactly its six
  privacy-safe fields.
- An authoritative SPEC-008 response may report no applicable teaching or
  registration term; null `registrationTerm` then requires window state
  `none`. A missing/failed contributor produces a safe unavailable response
  with no partial context DTO.
- Standardize page-number pagination at default 20 and maximum 100, stable sort
  with a unique-ID tie-breaker, required `expectedRowVersion` mutation fields,
  and `409 STALE_VERSION` responses.
- Require every endpoint contract to record success, validation,
  authentication/authorization, conflict/concurrency, and unexpected-error
  outcomes, using an explained `not applicable` entry where appropriate.
- After approved version-pinned handlers and a real generator exist, generate
  a deterministic OpenAPI baseline and reject unapproved semantic drift in CI;
  do not approve an empty or design-only baseline.

## Execution Strategy

1. Validate SPEC-004/SPEC-005 and complete DTO, endpoint-owner, error,
   pagination, concurrency, and OpenAPI consistency analysis.
2. Verify Ahmed ELbamby's 2026-07-13 approval in the Technical Lead review
   perspective before implementation; retain later release and production
   contract approvals.
3. Write failing shared-contract and OpenAPI-baseline tests before contract
   source; feature endpoint handlers remain in their approved owner specs.
4. Run downstream contract integration only against approved, version-pinned
   feature contracts.



## Non-Functional Requirements

- NFR-1: CI MUST generate deterministic OpenAPI from the approved application,
  compare it semantically with the versioned baseline, and reject any
  unapproved endpoint, operation, schema, status-code, or security drift.
- NFR-2: Every success/error response in approved feature specs MUST have a
  contract/integration test.
- NFR-3: Responses MUST NOT leak stack traces, SQL text, secrets, hashes, or
  unauthorized identifiers.
- NFR-4: JSON field naming and date/decimal formats MUST be consistent.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
