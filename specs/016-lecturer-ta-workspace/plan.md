# Implementation Plan: Lecturer and Teaching Assistant Workspace

**Branch**: 016-lecturer-ta-workspace | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Approved for non-production demo implementation by Ahmed ELbamby on 2026-07-13; assignment-scoped line-approval amendment approved 2026-07-20 (Gate A).

**2026-07-20 owner amendment:** Lecturer and TeachingAssistant are separate,
single-role account contexts; the combined-role demo fixture is frozen.

**2026-07-20 registration amendment:** Lecturer and TeachingAssistant may
decide only pending registration lines for currently assigned groups through
the narrow SPEC-014 approval contract. They gain no general student,
capacity, policy, term, or plan-acceptance authority.

## Summary

Deliver shared Lecturer/TA pages and assignment-scoped queries in
`StudentRegistration.StaffAdministration`. Availability is edited through a
Scheduling-module application port: SPEC-010 remains canonical owner of
`StaffTermAvailability` and `StaffAvailability`. A conflicting published
assignment creates a durable, auditable impact alert and revalidation state; it
never moves a class automatically. Admin may select an availability aggregate
ID and rowversion as an immutable offering-planning dependency, with no range
copy and no correction/override workflow.

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
   read-only Admin consumption boundary, impact alert lifecycle, routes, and
   API errors against Ahmed ELbamby's recorded 2026-07-13 Gate A approval.
3. Write failing DTO/schema, endpoint, authorization, acceptance, aggregate
   race, publication race, accessibility, and performance tests.
4. Implement scoped reads and roster audit.
5. Implement availability orchestration through the Scheduling port and
   durable impact-alert creation/revalidation.
6. Map handlers and implement STF-01..STF-04 only after their behavior/E2E tests
   fail; produce release evidence.
7. Add failing permission/resource-authorization, minimal-projection,
   decision-idempotency/audit, assignment-ended race, component, E2E,
   accessibility, browser, and all-role capacity-language tests.
8. Add the bounded staff approval queue/detail/decision flow and unified UI
   components only after amended tests fail, then refresh trace/release
   evidence without overwriting the earlier baseline.

## Design Decisions

### Ownership and Privacy

SPEC-010 owns `GroupStaffAssignment`, `StaffTermAvailability`, and
`StaffAvailability`. SPEC-016 consumes them and owns workspace projections,
`RosterRowDto`, and query services. `ScheduleImpactAlert` is also consumed
from SPEC-010/Scheduling. A roster exposes
only University ID, display name, and enrollment state; GPA, holds, contact
details, grades, and transcript data are forbidden.

Staff own availability edits. Admin consumes only SPEC-010's bounded read-only
view and may select its aggregate ID and rowversion as an immutable
offering-planning dependency. No
Admin availability correction/override route, permission, editable control,
notification workflow, or correction-audit flow is part of the POC.

### Registration line approval

SPEC-014 owns pending lines, holds, decisions, plan finalization, and decision
transactions. SPEC-016 contributes only assignment-scoped staff queries,
resource authorization, and UI. A current GroupStaffAssignment is required at
read and write time. The projection is PII-minimized, a staff decision cannot
alter capacity/policy or directly accept a plan, and unified SPEC-003
roadmap/capacity/status/timeline/decision components are reused.

## Constitution and Approval Gate

Server-side assignment scope is required for every object request. Gate A
authorizes non-production demo implementation while this package and dependency
baselines remain Approved. Gate B-D evidence and production/release approval
remain separate and mandatory for their respective milestones.

## Artifacts

- [Requirements](requirements.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Tasks](tasks.md)

## Complexity Tracking

The feature shares page templates and a narrow Scheduling application port. It
does not duplicate the availability aggregate, add an Admin correction facade,
or introduce automatic rescheduling.
