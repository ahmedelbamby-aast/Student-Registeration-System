# Data Model: Domain Classes and API Contracts

## Owned Shared Contract Types

- **ApiError**, **Page**, **CommandResult**, and **DomainValue** are shared serialized/value contracts owned by SPEC-006.
- **AppContextDto** is a composed shared response schema owned by SPEC-006; SPEC-007 and SPEC-008 own its source data and SPEC-008 owns the endpoint handler.
- **TermSummaryDto** is the shared `{ id, code, label, state, rowVersion }` schema owned by SPEC-006 and composed from SPEC-008 AcademicTerm data; lifecycle state is Draft, RegistrationOpen, RegistrationClosed, Teaching, Completed, or Archived.
- No concept in this document is a SQL entity or aggregate root.

## Detailed Model

| Type | Purpose |
|---|---|
| Strong ID/value object | Prevent accidental entity/primitive mixing |
| Command/result | One application use case |
| API request/response DTO | Versioned client contract |
| TermSummaryDto | Minimal versioned term reference for composed context |
| Domain entity/aggregate | Invariant behavior; infrastructure independent |

## Contract Rules

- JSON names, UTC timestamps, timezone IDs, invariant decimals, identifiers,
  pagination metadata, and errors are serialized consistently.
- Contract types never contain EF navigation properties, password/security
  internals, or unauthorized identifiers.
- `AppContextDto` composition must be complete, server-authoritative, and
  authorization-filtered; missing required contributors fails safely.
- `activeRole` is absent only for a dual-role `role-selection-required` state;
  `serviceState` and canonical `supportReferencePath` are always present.
- Persistence and lifecycle rules belong to feature aggregates and SPEC-005,
  not to these transport/value types.
