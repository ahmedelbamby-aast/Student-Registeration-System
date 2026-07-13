# Implementation Plan: Lecturer and Teaching Assistant Workspace

**Branch**: 016-lecturer-ta-workspace | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Deliver shared Lecturer/TA pages and assignment-scoped queries in
`StudentRegistration.StaffAdministration`. Availability is edited through a
Scheduling-module application port: SPEC-010 remains canonical owner of
`StaffTermAvailability` and `StaffAvailability`. A conflicting published
assignment creates a durable, auditable impact alert and revalidation state; it
never moves a class automatically.

## Technical Context

- **Runtime**: C#/.NET 10, Blazor WebAssembly, ASP.NET Core, EF Core/LINQ.
- **Modules**: `StudentRegistration.StaffAdministration` for workspace
  queries/pages contracts; `StudentRegistration.Scheduling` owns availability
  aggregate and mutation port.
- **Storage**: SQL Server; aggregate rowversion protects complete-range
  replacement; impact alerts are durable.
- **Roster**: bounded `Page<RosterRowDto>`, default 20/max 100, stable
  display-name then University-ID order, assignment authorization before query.
- **Performance**: dashboard/assignment p95 <= 300 ms.

## Workstreams and Order

1. Baseline IdentityAccess, Scheduling, Records, UX, and Quality dependencies;
   complete ownership and consistency analysis.
2. Freeze exact roster fields, audit metadata, availability port, deadline,
   impact alert lifecycle, routes, and API errors; obtain human approval last.
3. Write failing DTO/schema, endpoint, authorization, acceptance, aggregate
   race, publication race, accessibility, and performance tests.
4. Implement scoped reads and roster audit.
5. Implement availability orchestration through the Scheduling port and
   durable impact-alert creation/revalidation.
6. Map handlers and implement STF-01..STF-04 only after their behavior/E2E tests
   fail; produce release evidence.

## Design Decisions

### Ownership and Privacy

SPEC-010 owns `GroupStaffAssignment`, `StaffTermAvailability`, and
`StaffAvailability`. SPEC-016 consumes them and owns workspace projections,
`RosterRowDto`, and query services. `ScheduleImpactAlert` is also consumed
from SPEC-010/Scheduling. A roster exposes
only University ID, display name, and enrollment state; GPA, holds, contact
details, grades, and transcript data are forbidden.

## Constitution and Approval Gate

Server-side assignment scope is required for every object request. No source,
test, migration, or page work may begin until dependencies and this package are
Approved and the final human approval gate is recorded.

## Artifacts

- [Requirements](requirements.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Tasks](tasks.md)

## Complexity Tracking

The feature shares page templates and a narrow Scheduling application port. It
does not duplicate the availability aggregate or introduce automatic
rescheduling.
