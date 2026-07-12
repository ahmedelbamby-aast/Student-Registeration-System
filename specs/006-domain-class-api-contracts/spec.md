# Feature Specification: Domain Classes and API Contracts

**Feature Branch**: 006-domain-class-api-contracts
**Created**: 2026-07-12
**Status**: In Review
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
Then the application service uses TimeProvider and the declared token behavior<br>
And produces no duplicate committed effect.
### User Story 5 - Bounded compatible listing (FR-5, FR-6) (P3)

As a API consumer, I need the Bounded compatible listing (FR-5, FR-6) behavior so that Domain Classes and API Contracts produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given a client requests an oversized page from an existing API version<br>
When the list endpoint validates the request<br>
Then it caps/rejects the size according to contract<br>
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
  entities or password/security internals.
- FR-3: Errors MUST use stable machine code, safe message, correlation ID, and
  optional field details.
- FR-4: Update/delete endpoints MUST require an expected rowversion or
  If-Match value; retryable create/confirm/submit commands MUST require an
  idempotency key; cancellation behavior before and after commit MUST be
  documented per endpoint.
- FR-5: Listing endpoints MUST use bounded pagination.
- FR-6: API versioning policy MUST be defined before the first breaking change.
- FR-7: Domain code MUST use TimeProvider abstraction for current time.
- FR-8: Idempotency contracts MUST define owner/scope, server-canonical payload,
  atomic first claim, same-payload processing/replay, different-payload
  IDEMPOTENCY_KEY_REUSED, and which final rejections are replayable.
- FR-9: Public AUTH-01 status MUST use GET /api/public/context, returning only
  server time/timezone, public teaching/registration term labels, window state,
  maintenance state, and no user, role, student, capacity, or internal-health
  data.

### Non-Functional Requirements

- NFR-1: OpenAPI output MUST match implementation in CI.
- NFR-2: Every success/error response in approved feature specs MUST have a
  contract/integration test.
- NFR-3: Responses MUST NOT leak stack traces, SQL text, secrets, hashes, or
  unauthorized identifiers.
- NFR-4: JSON field naming and date/decimal formats MUST be consistent.

### Key Entities

- **ApiError**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Page**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **AppContext**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **CommandResult**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **DomainValue**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Every declared interface has explicit success, validation, authorization, conflict, and unexpected-error outcomes.
- **SC-2**: No persistence-internal or credential field is part of a public contract.
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
