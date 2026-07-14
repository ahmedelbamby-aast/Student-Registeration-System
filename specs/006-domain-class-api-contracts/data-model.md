# Data Model: Domain Classes and API Contracts

## Owned Shared Contract Types

- **ApiError** is the shared safe error schema owned by SPEC-006.
- **Page&lt;T&gt;** is the shared bounded list schema owned by SPEC-006.
- **AppContextDto** is the composed authenticated response schema owned by
  SPEC-006; SPEC-007 and SPEC-008 own its source data and SPEC-008 owns the
  endpoint handler.
- **TermSummaryDto** is the shared `{ id, code, label, state, rowVersion }`
  schema owned by SPEC-006 and composed from SPEC-008 AcademicTerm data;
  lifecycle state is Draft, RegistrationOpen, RegistrationClosed, Teaching,
  Completed, or Archived.
- **PublicContextDto** is the privacy-safe unauthenticated response schema owned
  by SPEC-006; SPEC-008 supplies its values and owns the endpoint handler.
- No concept in this document is a SQL entity or aggregate root.

## Detailed Model

| Type | Required purpose |
|---|---|
| ApiError | Stable machine code, safe message, correlation ID, optional field errors, and authorized current version only |
| Page&lt;T&gt; | Bounded items with page, pageSize, totalCount, and applied deterministic sort |
| AppContextDto | Complete server-authoritative authenticated identity, session, role, term, time, window, service, and support context |
| TermSummaryDto | Minimal versioned authoritative term reference used only inside context DTOs |
| PublicContextDto | Public server time/timezone, nullable authoritative term labels, window state, and service state only |

## Contract Rules

- JSON names, UTC timestamps, timezone IDs, invariant decimals, identifiers,
  pagination metadata, and errors are serialized consistently.
- The `StudentRegistration.Contracts` project is framework- and
  persistence-free. It references no ASP.NET Core, Blazor, EF Core, SQL Server,
  or business-module implementation type.
- Contract types never contain EF navigation properties, persisted
  password/security internals, secret-bearing response fields, or unauthorized
  identifiers. The explicitly tested authentication and account-lifecycle
  request DTOs may carry only operation-required transient credentials or
  recovery proofs.
- `AppContextDto` composition must be complete, server-authoritative, and
  authorization-filtered. A missing or failed required contributor returns a
  safe unavailable error with no partial success DTO.
- A present SPEC-008 contributor may authoritatively report no applicable
  teaching or registration term. That valid null is not contributor failure;
  null `registrationTerm` requires `registrationWindowState = "none"`.
  Nullable `PublicContextDto` term labels use the same rule.
- `activeRole` is null only for a dual-role `role-selection-required` state;
  `serviceState` and canonical `supportReferencePath` are always present.
- Each endpoint contract records success, validation,
  authentication/authorization, conflict/concurrency, and unexpected-error
  outcomes; an impossible category is explicitly `not applicable` with a
  reason.
- Persistence and lifecycle rules belong to feature aggregates and SPEC-005,
  not to these transport/value types.
