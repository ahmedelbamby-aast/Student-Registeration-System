# Feature Specification: Domain Classes and API Contracts

**Feature Branch**: 006-domain-class-api-contracts
**Created**: 2026-07-12
**Status**: Approved (Gate A demo implementation, 2026-07-13)
**Owner**: Technical Lead
**Normative detail**: [requirements.md](requirements.md)

## Context

Blazor and the API need stable, minimal contracts while domain behavior stays
testable and infrastructure-independent. The proposed class relationships are
in docs/diagrams/CLASS_DIAGRAM.md.

## User Scenarios and Testing

### User Story 1 - Safe error (FR-3, NFR-3) (P1)

As a API consumer, I need the Safe error (FR-3, NFR-3) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given an unexpected database exception<br>
When an API request fails<br>
Then the response contains generic code, safe message and correlation ID<br>
And contains no stack trace, SQL, or connection information.
### User Story 2 - EF isolation (FR-2) (P1)

As a API consumer, I need the EF isolation (FR-2) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given an EF entity has an internal rowversion and navigation graph<br>
When an endpoint returns the resource<br>
Then only fields declared by the DTO contract are serialized.
### User Story 3 - Contract verification (NFR-1, NFR-2) (P2)

As a API consumer, I need the Contract verification (NFR-1, NFR-2) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given an approved endpoint contract<br>
When CI runs integration and OpenAPI checks<br>
Then success and documented error shapes match exactly.
### User Story 4 - Mutation time and concurrency contract (FR-4, FR-7) (P2)

As a API consumer, I need the Mutation time and concurrency contract (FR-4, FR-7) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given a time-dependent mutation with an idempotency/concurrency token<br>
When the request is canceled or retried<br>
Then the application service uses TimeProvider, the request-body
expectedRowVersion contract, and the declared idempotency behavior<br>
And produces no duplicate committed effect.
### User Story 5 - Bounded compatible listing (FR-5, FR-6) (P3)

As a API consumer, I need the Bounded compatible listing (FR-5, FR-6) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given a client requests an oversized page from an existing API version<br>
When the list endpoint validates the request<br>
Then it returns 400 PAGE_SIZE_INVALID rather than silently capping the request<br>
And a breaking shape change requires the approved versioning process.
### User Story 6 - Complete idempotency contract (FR-4, FR-8) (P3)

As a API consumer, I need the Complete idempotency contract (FR-4, FR-8) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given a retryable command contract is reviewed<br>
When its OpenAPI and integration cases are inspected<br>
Then same-key/same-payload processing and replay are explicit<br>
And same-key/different-payload returns 409 IDEMPOTENCY_KEY_REUSED<br>
And cancellation before commit versus response loss after commit has distinct
documented behavior.
### User Story 7 - Privacy-safe public context (FR-9, NFR-3) (P3)

As a API consumer, I need the Privacy-safe public context (FR-9, NFR-3) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

Given an unauthenticated visitor opens AUTH-01<br>
When GET /api/public/context succeeds<br>
Then only the FR-9 fields are returned<br>
And authenticated context, internal health, capacity, and personal data are
absent.
### User Story 8 - Application-service and serialization consistency (FR-1, NFR-4) (P3)

As a API consumer, I need the Application-service and serialization consistency (FR-1, NFR-4) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-8 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-8)**

Given every approved endpoint contract and representative command/query<br>
When architecture and JSON contract tests execute<br>
Then endpoints delegate business decisions to focused application services<br>
And field naming, UTC dates, timezone identifiers, and invariant decimal
formats are identical across responses.

### User Story 9 - Complete authenticated context (FR-10) (P3)

As an authenticated user, I need one complete, server-authoritative context so
that every role shell displays the same identity, session, term, and time state.

**Independent Test**: Execute AC-9 in requirements.md with approved SPEC-007
and SPEC-008 contributor fixtures.

**Acceptance Scenario (AC-9)**

Given SPEC-007 supplies an authenticated session and SPEC-008 supplies the
authoritative academic context<br>
When GET /api/context succeeds<br>
Then the response contains every FR-10 field and only authorized role contexts<br>
And serviceState and supportReferencePath are always present<br>
And activeRole is null only for an explicitly modeled dual-role
role-selection-required state<br>
And a missing required contributor returns a safe unavailable error rather
than a partial or browser-derived context.

## Edge Cases

- EC-1: Malformed JSON -> 400 VALIDATION_ERROR with no command execution.
- EC-2: Unsupported media type -> 415.
- EC-3: Canceled request before commit -> cancel safely; after commit,
  idempotent retry returns stored result.
- EC-4: Unauthenticated/unauthorized -> 401/403 with no protected data.

## Requirements

### Functional Requirements

- FR-1: Endpoints MUST delegate business decisions to focused application
  services.
- FR-2: Contracts MUST use DTOs/value identifiers and MUST NOT serialize EF
  entities, persistence/security internals, or secrets in responses.
  Authentication and account-lifecycle request DTOs MAY carry only the
  operation-required plaintext credential or recovery proof; those values MUST
  remain transient and MUST NOT be returned, persisted raw, or logged.
  `StudentRegistration.Contracts`
  MUST remain framework- and persistence-free: it references no ASP.NET Core,
  Blazor, EF Core, SQL Server, or business-module implementation type.
- FR-3: Errors MUST use stable machine code, safe message, correlation ID, and
  optional field details.
- FR-4: Every versioned update/delete request MUST carry
  `expectedRowVersion` in its request body. A stale authorized request MUST
  return 409 `STALE_VERSION` and MAY return the current version; unauthorized
  callers MUST receive 403 without current-version or resource data.
  Retryable create/confirm/submit commands MUST require an idempotency key;
  cancellation behavior before and after commit MUST be documented per endpoint.
- FR-5: Listing endpoints MUST use page-number pagination with default page
  size 20 and maximum 100. `page < 1`, `pageSize < 1`, or `pageSize > 100`
  MUST return 400 `PAGE_SIZE_INVALID`; no silent cap is allowed. Every listing
  MUST declare a stable default sort ending in a unique identifier tie-breaker.
- FR-6: API versioning policy MUST be defined before the first breaking change.
- FR-7: Domain code MUST use TimeProvider abstraction for current time.
- FR-8: Idempotency contracts MUST define owner/scope, server-canonical payload,
  atomic first claim, same-payload processing/replay, different-payload
  IDEMPOTENCY_KEY_REUSED, and which final rejections are replayable.
- FR-9: Public AUTH-01 status MUST use GET /api/public/context, returning only
  server time/timezone, public teaching/registration term labels, window state,
  maintenance state, and no user, role, student, capacity, or internal-health
  data.
- FR-10: The shared `AppContextDto` contract MUST contain server time/timezone,
  teaching term, registration term, `registrationWindowState`, and a nullable
  `RegistrationWindowSummaryDto` containing the single matched window's ID,
  computed state, UTC opening/closing instants, and row version; authenticated display name,
  authorized roles, the active authorized role context, session state and
  expiry, service state, and canonical `supportReferencePath`. `activeRole` MAY
  be null only while a dual-role user is in the explicitly modeled
  `role-selection-required` session state. SPEC-007 supplies session/user/role
  values; SPEC-008 supplies time/term/window values and owns GET /api/context
  and GET /api/public/context handlers. Missing required contributors MUST fail
  safely with no partial success body; the browser MUST NOT synthesize a
  partial context. A present SPEC-008 contribution MAY authoritatively report
  no applicable teaching term or no applicable registration term. That valid
  absence is distinct from a missing contributor: a null `registrationTerm`
  requires `registrationWindowState = "none"` and a null
  `registrationWindow`. A present window requires a non-null registration term
  and a state equal to `registrationWindowState`, while contributor failure
  returns a safe unavailable error instead of `AppContextDto`.

### Non-Functional Requirements

- NFR-1: CI MUST generate deterministic OpenAPI from the approved application,
  compare it semantically with the versioned baseline, and reject any
  unapproved endpoint, operation, schema, status-code, or security drift.
- NFR-2: Every success/error response in approved feature specs MUST have a
  contract/integration test.
- NFR-3: Responses MUST NOT leak stack traces, SQL text, secrets, hashes, or
  unauthorized identifiers.
- NFR-4: JSON field naming and date/decimal formats MUST be consistent.

### Key Entities

- **ApiError**: Shared safe error response owned by SPEC-006.
- **Page&lt;T&gt;**: Shared bounded list response owned by SPEC-006.
- **AppContextDto**: Composed authenticated response schema owned by SPEC-006.
  SPEC-007 supplies session/user/authorized-role context; SPEC-008 supplies
  authoritative time/term/window context and owns the endpoint handler.
- **TermSummaryDto**: Shared `{ id, code, label, state, rowVersion }` response
  type owned by SPEC-006 and composed from SPEC-008 AcademicTerm data.
- **RegistrationWindowSummaryDto**: Shared authenticated-context value
  `{ id, state, opensAtUtc, closesAtUtc, rowVersion }` owned by SPEC-006 and
  composed from the single matched SPEC-008 RegistrationWindow.
- **PublicContextDto**: Privacy-safe unauthenticated context response owned by
  SPEC-006; SPEC-008 supplies its values and owns the endpoint handler.

These six concepts are serialized/value contracts, not SQL entities or
aggregate roots.

## Success Criteria

- **SC-1**: Every declared endpoint interface explicitly records success,
  validation, authentication/authorization, conflict/concurrency, and
  unexpected-error outcomes. A category the operation cannot produce MUST be
  marked `not applicable` with a reason rather than omitted.
- **SC-2**: No persistence-internal field or credential is part of a response
  contract. Secret-bearing request fields are limited to the explicitly tested
  authentication and account-lifecycle inputs and are handled transiently.
- **SC-3**: All externally visible dates, identifiers, pagination, and error formats are consistent.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-004](../004-architecture-engineering-principles/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| SYS-01 | /status/{code} | SystemStatusPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-003 |

## Out of Scope

- OS-1: GraphQL, gRPC, and public third-party API.
- OS-2: Generic CRUD endpoints for every entity.
- OS-3: A mediator library unless approved handler volume justifies it.
- OS-4: Breaking-change version until an actual breaking change is proposed.
