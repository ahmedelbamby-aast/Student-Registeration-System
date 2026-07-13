# Implementation Plan: Schedule Recommendations

**Branch**: 013-schedule-recommendations | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Implement bounded deterministic recommendations in the
`StudentRegistration.Registration` module. The module consumes published
course/group and policy snapshots through upstream public ports, returns up to
three complete schedules, explains infeasibility precisely, and applies an
option only through a replica-safe signed option token and an atomic
registration-plan version check.

## Technical Context

- **Runtime**: C#/.NET 10, ASP.NET Core API, Blazor WebAssembly.
- **Module**: `src/StudentRegistration.Registration/` with `Domain`,
  `Application`, and `Endpoints` folders.
- **Composition**: `StudentRegistration.Api`; shared DTO conventions only in
  `StudentRegistration.Contracts`.
- **Persistence**: no durable ScheduleOption table; current plan/version data
  uses the existing SQL Server/EF Core infrastructure.
- **Replica safety**: option tokens use ASP.NET Core Data Protection with the
  shared production key repository required by SPEC-018.
- **Performance**: p95 <= 500 ms for eight courses with ten groups each.
- **Accessibility**: SPEC-003 owns the canonical page; this spec contributes
  deterministic states, reasons, and actions to STU-04.

## Workstreams and Order

1. Baseline SPEC-003, SPEC-010, SPEC-011, SPEC-012, and SPEC-018; complete consistency analysis.
2. Freeze token payload, optimizer input snapshot, diagnostics, and API
   contracts; obtain Ahmed ELbamby's approval as the last planning gate.
3. Write failing model, contract, acceptance, edge, deterministic, tamper,
   stale-version, cancellation, and performance tests.
4. Implement constrained-first search and deterministic score/tie behavior.
5. Implement inclusion-minimal diagnostics and bounded budget handling.
6. Implement token protection/validation and atomic plan application.
7. Map endpoints only after the preceding behavior tests fail for expected
   reasons; then verify STU-04 contribution and release evidence.

## Design Decisions

- `ScheduleOption`, `ScoreComponent`, `OptimizerConfiguration`, and `OptimizationDiagnostic` are
  transient Registration-module values, not EF entities.
- The signed token binds student, plan, plan rowversion, option groups,
  catalogue/group/policy/configuration versions, correlation ID, issued time,
  and expiry. Any mismatch or signature failure is rejected before mutation.
- A blocking set is inclusion-minimal: removing any member from that set makes
  that reported hard conflict no longer hold. Results are deterministically
  ordered by set size and stable course/group identifiers.
- Recommendation never reserves capacity; SPEC-014 remains final authority.

## Constitution and Dependency Gates

The design is a single-deployable modular monolith, contains no remote call in
a transaction, keeps server decisions authoritative, and creates planning
artifacts only. Implementation remains blocked while this package or a required
dependency is In Review or its approval checklist is incomplete.

## Artifacts

- [Requirements](requirements.md)
- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Tasks](tasks.md)

## Complexity Tracking

No microservice, message broker, durable option store, or external solver is
introduced. OR-Tools remains excluded until measurement justifies it.
