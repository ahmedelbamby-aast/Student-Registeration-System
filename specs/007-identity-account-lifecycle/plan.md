# Implementation Plan: Identity and Account Lifecycle

**Branch**: 007-identity-account-lifecycle | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Design complete; DEC-01, DEC-02, and DEC-13 plus human approval are pending. Implementation is not authorized.

## Summary

Deliver Identity and Account Lifecycle inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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
- [SPEC-004](../004-architecture-engineering-principles/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Project Structure

Identity domain, application services, and endpoints belong in
`src/StudentRegistration.IdentityAccess/{Domain,Application,Endpoints}`.
The same-origin composition root is `src/StudentRegistration.Api`, shared DTO
conventions are in `src/StudentRegistration.Contracts`, browser pages remain
in `src/StudentRegistration.Client`, and SQL mappings are implemented by
`src/StudentRegistration.Infrastructure.SqlServer`. No business handler is
placed in a generic Server or Domain project.

## Feature Design

1. `ApplicationUser` is the security-stamp root; role assignments and
   challenge/abuse state are shared durable Identity data.
2. Student activation and recovery call approved institutional verification
   ports and atomically consume single-use challenges.
3. Staff authentication creates an MFA challenge and issues a session only
   after the approved provider verifies it.
4. Recovery, password change, and revoke-all rotate the security stamp;
   protected APIs validate it on every replica.
5. Identity owns Admin pre-provisioned import/list/status/role commands and
   locks its singleton AdminSecurityGuard to serialize the final-enabled-Admin
   invariant; SPEC-017 delegates to Identity and consumes the resulting audit
   facts and monitoring projections.
6. Identity owns append-only SecurityEvent facts and writes the shared
   SPEC-004 AuditEvent in the same role transaction; downstream SPEC-017 may
   query both. SPEC-018 owns key-ring operations.

## Execution and Gate Order

Dependency baselines and cross-spec consistency analysis run first. The final
planning action is Ahmed ELbamby's approval after DEC-01, DEC-02, and DEC-13
are resolved. Implementation then proceeds test-first: contract/model and
acceptance tests, domain/application delivery, endpoint handlers, frontend E2E
tests/pages, and measurable release evidence. No handler precedes its linked
behavior and contract tests.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)



## Non-Functional Requirements

- NFR-1: Login SHOULD respond within 500 ms p95 under the SPEC-018
  production-like authenticated-session load, excluding MFA-provider latency.
- NFR-2: Authentication errors MUST NOT reveal whether an account exists.
- NFR-3: Password/credential configuration MUST follow current ASP.NET Core
  Identity and AASTMT security policy.
- NFR-4: Every protected endpoint MUST have positive/negative authorization
  tests.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
