# SPEC-006: Domain Classes and API Contracts

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Technical Lead<br>
**Reviewers:** Frontend, Backend, QA, Security<br>
**Target:** Sprint 0-S1<br>
**Dependencies:** SPEC-004, SPEC-005<br>

## Context

Blazor and the API need stable, minimal contracts while domain behavior stays
testable and infrastructure-independent. The proposed class relationships are
in docs/diagrams/CLASS_DIAGRAM.md.

## Functional Requirements

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

## Non-Functional Requirements

- NFR-1: OpenAPI output MUST match implementation in CI.
- NFR-2: Every success/error response in approved feature specs MUST have a
  contract/integration test.
- NFR-3: Responses MUST NOT leak stack traces, SQL text, secrets, hashes, or
  unauthorized identifiers.
- NFR-4: JSON field naming and date/decimal formats MUST be consistent.

## Acceptance Criteria

### AC-1: Safe error (FR-3, NFR-3)
Given an unexpected database exception<br>
When an API request fails<br>
Then the response contains generic code, safe message and correlation ID<br>
And contains no stack trace, SQL, or connection information.

### AC-2: EF isolation (FR-2)
Given an EF entity has an internal rowversion and navigation graph<br>
When an endpoint returns the resource<br>
Then only fields declared by the DTO contract are serialized.

### AC-3: Contract verification (NFR-1, NFR-2)
Given an approved endpoint contract<br>
When CI runs integration and OpenAPI checks<br>
Then success and documented error shapes match exactly.

### AC-4: Mutation time and concurrency contract (FR-4, FR-7)
Given a time-dependent mutation with an idempotency/concurrency token<br>
When the request is canceled or retried<br>
Then the application service uses TimeProvider and the declared token behavior<br>
And produces no duplicate committed effect.

### AC-5: Bounded compatible listing (FR-5, FR-6)
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

## API Contracts

```typescript
interface ApiError {
  code: string;
  message: string;
  correlationId: string;
  fieldErrors?: Record<string, string[]>;
  currentVersion?: string;
}

interface Page<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

interface AppContextDto {
  serverTimeUtc: string;
  timeZoneId: string;
  teachingTerm?: TermSummaryDto;
  registrationTerm?: TermSummaryDto;
  registrationWindowState: "open" | "upcoming" | "closed" | "none";
  roles: string[];
}
```

Feature endpoints are defined in SPEC-007 through SPEC-017.

Examples: GET /api/context, GET /api/resources?page=1&pageSize=20, and POST
/api/commands with the feature-specific DTO.

## Data Models

| Type | Purpose |
|---|---|
| Strong ID/value object | Prevent accidental entity/primitive mixing |
| Command/result | One application use case |
| API request/response DTO | Versioned client contract |
| Domain entity/aggregate | Invariant behavior; infrastructure independent |

## Out of Scope

- OS-1: GraphQL, gRPC, and public third-party API.
- OS-2: Generic CRUD endpoints for every entity.
- OS-3: A mediator library unless approved handler volume justifies it.
- OS-4: Breaking-change version until an actual breaking change is proposed.
