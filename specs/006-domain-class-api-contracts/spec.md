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
- FR-4: Mutation endpoints MUST support cancellation and appropriate
  idempotency/concurrency tokens.
- FR-5: Listing endpoints MUST use bounded pagination.
- FR-6: API versioning policy MUST be defined before the first breaking change.
- FR-7: Domain code MUST use TimeProvider abstraction for current time.

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

## Out of Scope

- OS-1: GraphQL, gRPC, and public third-party API.
- OS-2: Generic CRUD endpoints for every entity.
- OS-3: A mediator library unless approved handler volume justifies it.
- OS-4: Breaking-change version until an actual breaking change is proposed.
