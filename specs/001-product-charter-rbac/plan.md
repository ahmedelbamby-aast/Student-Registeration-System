# Implementation Plan: Product Charter and RBAC

**Branch**: 001-product-charter-rbac | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Approved for Gate A demo implementation on 2026-07-13; Gates B-D
and production release approval remain required.

## Summary

Deliver Product Charter and RBAC inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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

- None; this is a root specification.

## Project Structure

Future implementation follows the project-per-business-module modular monolith in
`docs/ARCHITECTURE.md`: `StudentRegistration.Client`, `StudentRegistration.Api`,
`StudentRegistration.Contracts`, `StudentRegistration.IdentityAccess`,
`StudentRegistration.Academics`, `StudentRegistration.Scheduling`,
`StudentRegistration.Registration`, `StudentRegistration.StaffAdministration`,
and `StudentRegistration.Infrastructure.SqlServer`, plus `tests/`. SPEC-001 owns
governance artifacts and permission vocabulary; SPEC-007 owns runtime identity,
role assignments, authorization policies, and staff/student login behavior.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Feature Design

- Maintain one normative role/permission matrix for Student, Admin, Lecturer,
  and TeachingAssistant, including dual-role and no-supported-role outcomes.
- Keep Admin authorization capability-based: `AcademicTerms.Manage` and
  `AcademicProfiles.Manage` are independent named grants, with no implicit
  superuser or permission-inference behavior. Bound academic-profile location
  to a required term plus University ID/name query and minimal locator fields;
  require named StudentId plus AcademicTermId for detail and correction.
- Treat `Role` and `Permission` as governed vocabulary in this charter, not
  runtime persistence entities. `RoleAssignment` is referenced from SPEC-007.
- Prove product scope through conformance and release-evidence tests; do not
  write `RolePolicies.cs` from this cross-cutting charter.

## Execution Strategy

1. Validate the root scope, consistency analysis, and traceability inventory.
2. Verify Ahmed ELbamby's 2026-07-13 Gate A approval record before starting
   implementation; retain the later Gate B-D and production release gates.
3. Write failing charter/permission conformance tests before governance
   artifacts or release evidence.
4. Defer runtime authorization implementation to approved SPEC-007 tasks and
   verify this charter through its published contract.



## Non-Functional Requirements

- NFR-1: Critical flows MUST meet WCAG 2.2 AA.
- NFR-2: The design MUST support the approved SPEC-018 scale targets without
  changing domain behavior.
- NFR-3: Authorization MUST be enforced by the API for every protected action.
- NFR-4: The initial solution MUST remain one deployable modular monolith.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
