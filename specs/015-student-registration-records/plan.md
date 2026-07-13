# Implementation Plan: Student Registration Records

**Branch**: 015-student-registration-records | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

## Summary

Build scoped read models and Blazor pages over the canonical SPEC-014
registration aggregate. SPEC-015 does not introduce a second receipt table:
it projects the unique reference and immutable receipt/decision snapshot that
SPEC-014 commits atomically with a registration result.

## Technical Context

- **Runtime**: C#/.NET 10, Blazor WebAssembly, ASP.NET Core, EF Core/LINQ.
- **Module**: `src/StudentRegistration.Registration/` read application and
  endpoints; pages remain in `src/StudentRegistration.Client/Pages/`.
- **Storage**: read-only queries over SQL Server Code First entities owned by
  SPEC-014; no duplicate persistence ownership.
- **EF mapping contribution**:
  `RegistrationReceiptModelConfiguration.cs` verifies/projects the canonical
  submission Reference/ReceiptSnapshot without writing a second receipt table;
  SPEC-004 remains the sole DbContext writer.
- **Authorization**: student self-scope; Admin inspection only through explicit
  student+term scoped endpoints and permissions.
- **Performance**: receipt first-page/detail p95 <= 300 ms at approved load.

## Workstreams and Order

1. Baseline SPEC-003, SPEC-008, SPEC-014, and SPEC-018; run consistency analysis.
2. Freeze projections, pagination, self/Admin authorization, immutable snapshot,
   empty/archive states, and API contracts; obtain human approval last.
3. Write failing projection/model, endpoint contract, ownership, acceptance,
   snapshot-retention, accessibility, and performance tests.
4. Implement bounded current/history/receipt queries and Admin-scoped
   inspection after tests fail.
5. Map handlers after behavior tests; implement STU-06/STU-07 only after the
   SPEC-003 page contracts and E2E tests fail.
6. Produce retention, PII-minimization, accessibility, and trace evidence.

## Design Decisions

### Ownership

SPEC-014 owns `RegistrationSubmission`, `Enrollment`,
`DecisionSnapshot`, `Reference`, and `ReceiptSnapshot`. SPEC-015 owns only
the receipt/history DTOs, query services, endpoints, and canonical student
pages. No drop, withdrawal, or correction command is introduced.

## Constitution and Approval Gate

All queries are server-authorized, paginated, and stable-sorted; student history supports cross-term browsing with an optional term filter. Application
work remains prohibited until dependencies and this package are Approved and
Ahmed ELbamby's approval is the final completed planning gate.

## Artifacts

- [Requirements](requirements.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Tasks](tasks.md)

## Complexity Tracking

The read model is a projection over existing data; there is no reporting
database, public receipt link, or asynchronous messaging.
