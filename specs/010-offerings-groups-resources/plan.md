# Implementation Plan: Offerings, Groups, and Resources

**Branch**: 010-offerings-groups-resources | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: APPROVED for Gate A demo implementation by Ahmed ELbamby on 2026-07-13, including staff-owned availability and read-only Admin use.

## Summary

Deliver Offerings, Groups, and Resources inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

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
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Project Structure

Scheduling domain, application, and endpoints belong in
`src/StudentRegistration.Scheduling/{Domain,Application,Endpoints}`. The API
composition root, shared contracts, pages, and SQL mappings use
`StudentRegistration.Api`, `.Contracts`, `.Client`, and
`.Infrastructure.SqlServer`. SPEC-016 consumes Scheduling availability ports;
it does not create a second StaffAdministration aggregate.

## Feature Design

1. CourseOffering owns versioned SectionGroups; each student-selectable group
   is one complete activity bundle containing typed meeting slots, room
   references, per-activity staff assignments, capacity, and state.
2. Apply the approved simple demo staffing rule: every published/open bundle
   has a Lecture with a Lecturer plus a Tutorial or Laboratory (or both), and
   every present Tutorial/Laboratory has a TA. Display activity type, staff,
   room/location, day, and time together. `Tutorial` is canonical; the UI may
   label it `Section`.
3. Scheduling owns Room and StaffTermAvailability resource versions plus
   persistent ScheduleImpactAlert state.
4. Validation binds every group/room/staff dependency version; publication
   locks the canonical stable order and repeats all checks in one transaction.
5. Staff edit availability through SPEC-016. Admin may view and import those
   declarations into offering planning as read-only inputs; no Admin
   availability edit or override route exists in
   the POC. Staff-owned changes can create Scheduling-owned impact alerts.
6. ADM-06/ADM-07 use Scheduling owner APIs; SPEC-017 contributes monitoring
   and audit views without duplicate writers.

## Execution and Gate Order

Ahmed ELbamby's Gate A demo approval is recorded. Dependency baselines,
staff-owned/read-only Admin availability review, and consistency analysis remain the first
execution tasks. Failing model/contract, acceptance/race, and consolidated
workstream tests precede all domain, application, handler, and page delivery.
Gate B-D, release, production deployment, and official AASTMT go-live
approvals remain separate.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)



## Non-Functional Requirements

- NFR-1: Offering/group reads SHOULD complete within 300 ms p95 at the
  SPEC-018 300-read-requests-per-second target.
- NFR-2: Publication validation MUST produce stable actionable reason codes.
- NFR-3: Meeting display MUST use term timezone and unambiguous day/time.
- NFR-4: Large admin lists MUST be paged/filtered.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
