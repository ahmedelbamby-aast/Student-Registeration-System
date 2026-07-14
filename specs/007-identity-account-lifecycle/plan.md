# Implementation Plan: Identity and Account Lifecycle

**Branch**: 007-identity-account-lifecycle | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby on 2026-07-13. DEC-13 remains a future production decision.

## Summary

Deliver Identity and Account Lifecycle inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: ASP.NET Core, Blazor WebAssembly, Entity Framework Core, LINQ
**Storage**: SQL Server with Code First migrations
**Testing**: xUnit plus API, integration, concurrency, accessibility, and browser tests as applicable
**Project Type**: Web application with hosted WebAssembly client and server API
**Performance Goals**: NFR-1's 10-minute 25-login/s two-replica profile plus
SPEC-018's cross-feature load and correctness gates
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

Browser/API identity DTOs are shared through only two bounded files in
`StudentRegistration.Contracts.Identity`: `AuthenticationContracts.cs` and
`AdministrationContracts.cs`. They contain transport shapes only, never EF or
security internals.

## Feature Design

1. `ApplicationUser` is the security-stamp root; role assignments and
   challenge/abuse state are shared durable Identity data.
2. A Development/Testing-only bootstrap generates pre-provisioned identities,
   unique synthetic University IDs, and initial PIN/passwords, persisting only
   ASP.NET Core Identity hashes; first use verifies the issued credential,
   replaces it with the student's new-password hash, and atomically activates
   the student.
3. Staff authentication directly verifies the pre-provisioned local password
   and account state, derives roles on the server, and issues a session without
   MFA, 2FA, or a pre-authentication/self-asserted role selector. A multi-role
   user may later choose only from the server-returned authorized role set.
   The shared host configures one `__Host-StudentRegistration.Session` cookie
   (Secure, HttpOnly, SameSite=Strict, Path=/, no Domain, non-persistent
   60-minute absolute lifetime) and ASP.NET Core antiforgery using the
   `XSRF-TOKEN` cookie/`X-XSRF-TOKEN` header pattern for every mutation.
4. Recovery, password change, and revoke-all rotate the security stamp;
   protected APIs validate it on every replica. Recovery proof delivery uses
   `Application/Ports/IAccountRecoveryProofDelivery.cs`: Testing injects an
   in-memory adapter, Development may use a bounded Git-ignored local adapter,
   and Production fails closed until its institutional adapter is approved.
5. Identity owns Admin pre-provisioned import/list/status/role commands. Every
   status or role command that could reduce the enabled-Admin set locks the
   singleton AdminSecurityGuard, rechecks that set, and serializes the
   final-enabled-Admin invariant; SPEC-017 delegates to Identity and consumes
   the resulting audit facts and monitoring projections.
6. Identity owns append-only SecurityEvent facts and writes the shared
   SPEC-004 AuditEvent in the same role transaction; downstream SPEC-017 may
   query both. SPEC-004 owns the SQL-backed Data Protection foundation;
   SPEC-018 governs its security and operational use.

## Execution and Gate Order

Ahmed ELbamby's Gate A demo approval is recorded. Dependency baselines and
cross-spec consistency analysis remain the first execution tasks; DEC-13 is a
future production concern and does not block demo implementation. Delivery
then proceeds test-first: contract/model and
acceptance tests, domain/application delivery, endpoint handlers, frontend E2E
tests/pages, and measurable release evidence. No handler precedes its linked
behavior and contract tests. Gate B-D, release, production deployment, and
official AASTMT go-live approvals remain separate.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)



## Non-Functional Requirements

- NFR-1: Login SHOULD respond within 500 ms p95 for 10 minutes at 25
  attempts/second across two replicas and 25,000 synthetic accounts using the
  80% valid, 15% invalid, and 5% already-locked mix; unexpected errors remain
  below 1% and identity invariants remain intact.
- NFR-2: Authentication errors MUST NOT reveal whether an account exists.
- NFR-3: Demo configuration pins IdentityV3/PBKDF2 at 100,000 or more
  iterations, 15-128 characters without composition rules, a versioned
  blocked-password list, five-attempt/five-minute password lockout, and
  five-attempt/15-minute activation/recovery proofs. Official AASTMT credential
  policy remains unverified and therefore fail-closed for Production.
- NFR-4: Every protected endpoint MUST have positive/negative authorization
  tests.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
