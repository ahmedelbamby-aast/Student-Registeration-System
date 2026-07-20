# Implementation Plan: Registration Capacity, Seat Holds, and Approval

**Branch**: `014-registration-approval-seat-holds` | **Date**: 2026-07-20 | **Spec**: [spec.md](spec.md)<br>
**Status**: Approved for non-production demo implementation by Ahmed Elbamby on 2026-07-20.

## Summary

Extend the existing SQL-linearized registration transaction with durable
per-line pending approvals and capacity-consuming seat holds. Required
prerequisite-root roadmap subjects are enrolled automatically in program term
one; self-service begins in program term two. Admin can decide every line,
while Lecturer and Teaching Assistant decisions are limited to current group
assignments. A plan converts all holds to enrollments only when every line is
approved, and any rejection or window-close expiry releases every hold.

## Technical Context

**Language/Version**: C# / .NET 10<br>
**Dependencies**: ASP.NET Core, Blazor WebAssembly, EF Core, LINQ<br>
**Storage**: SQL Server Code First; one local serializable transaction and sorted row-lock order<br>
**Testing**: xUnit model, application, real-SQL concurrency, authorization,
contract, accessibility, browser, and load tests

## Constitution Check

- [x] Ahmed Elbamby remains the only permitted Git author and committer.
- [x] Requirements, acceptance criteria, amendment tasks, routes, and entities are traceable.
- [x] The design remains a modular monolith with Registration as aggregate owner.
- [x] Server authority, privacy-safe authorization, concurrency, accessibility, scale, and observability are explicit.
- [x] Ahmed Elbamby's 2026-07-20 request and clarifications approve this non-production implementation amendment.

## Dependencies

- SPEC-001 owns `RegistrationApproval.DecideAll` and `RegistrationApproval.DecideAssigned`.
- SPEC-002 owns probation, the 18-credit normal limit, the 19-21 overload CGPA gate, and >21 rejection.
- SPEC-003 owns the unified shared frontend pattern and route/component records.
- SPEC-005/006 own ERD, persistence, API, security, and operations contracts.
- SPEC-008 owns student program-term academic state.
- SPEC-009 owns CurriculumCourse roadmaps, exact-three-credit courses, and prerequisites.
- SPEC-010 owns SectionGroup capacity counters and staff assignments.
- SPEC-011-013 remain advisory discovery/plan/recommendation inputs.
- SPEC-015-017 consume registration outcomes for student, staff, and Admin views.
- SPEC-018 owns operational quality, load, security, and release gates.

## Project Structure

- Domain/application: `src/StudentRegistration.Registration/`
- Shared academic and scheduling boundaries: `src/StudentRegistration.Academics/` and `src/StudentRegistration.Scheduling/`
- SQL persistence: `src/StudentRegistration.Infrastructure.SqlServer/`
- API composition/endpoints: `src/StudentRegistration.Api/`
- Unified role UI: `src/StudentRegistration.Client/`
- Verification: `tests/StudentRegistration.*Tests/`

## Implementation Sequence

1. Add authoritative program-term ordinal and roadmap-root selection.
2. Add submission lines, seat holds, immutable decisions, first-term batches,
   held-seat counter, constraints, and model tests.
3. Add atomic hold creation, decision authorization, final conversion,
   rejection/expiry release, reconciliation, and concurrency tests.
4. Add Admin/staff/student contracts, endpoints, projections, and unified UI.
5. Add the reviewed incremental migration, development seed, browser evidence,
   accessibility/security/load verification, and startup documentation.

## Design Artifacts

- [research.md](research.md)
- [data-model.md](data-model.md)
- [contracts/api.md](contracts/api.md)
- [concurrency-matrix.md](concurrency-matrix.md)
- [quickstart.md](quickstart.md)
- [tasks.md](tasks.md)

## Complexity Tracking

No constitution exception is required. Durable holds are part of the existing
registration aggregate and SQL transaction boundary; no distributed lock,
queue, waitlist, or second service is introduced.
