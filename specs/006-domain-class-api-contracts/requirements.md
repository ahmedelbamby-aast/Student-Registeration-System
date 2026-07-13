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
  teaching term, registration term/window, authenticated display name,
  authorized roles, the active authorized role context, session state and
  expiry, service state, and canonical `supportReferencePath`. `activeRole` MAY
  be null only while a dual-role user is in the explicitly modeled
  `role-selection-required` session state. SPEC-007 supplies session/user/role
  values; SPEC-008 supplies time/term/window values and owns GET /api/context
  and GET /api/public/context handlers. Missing required contributors MUST fail
  safely; the browser MUST NOT synthesize a partial context.

## Non-Functional Requirements

- NFR-1: CI MUST generate deterministic OpenAPI from the approved application,
  compare it semantically with the versioned baseline, and reject any
  unapproved endpoint, operation, schema, status-code, or security drift.
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
Then the application service uses TimeProvider, the request-body
expectedRowVersion contract, and the declared idempotency behavior<br>
And produces no duplicate committed effect.

### AC-5: Bounded compatible listing (FR-5, FR-6)
Given a client requests an oversized page from an existing API version<br>
When the list endpoint validates the request<br>
Then it returns 400 PAGE_SIZE_INVALID rather than silently capping the request<br>
And a breaking shape change requires the approved versioning process.

### AC-6: Complete idempotency contract (FR-4, FR-8)
Given a retryable command contract is reviewed<br>
When its OpenAPI and integration cases are inspected<br>
Then same-key/same-payload processing and replay are explicit<br>
And same-key/different-payload returns 409 IDEMPOTENCY_KEY_REUSED<br>
And cancellation before commit versus response loss after commit has distinct
documented behavior.

### AC-7: Privacy-safe public context (FR-9, NFR-3)
Given an unauthenticated visitor opens AUTH-01<br>
When GET /api/public/context succeeds<br>
Then only the FR-9 fields are returned<br>
And authenticated context, internal health, capacity, and personal data are
absent.

### AC-8: Application-service and serialization consistency (FR-1, NFR-4)
Given every approved endpoint contract and representative command/query<br>
When architecture and JSON contract tests execute<br>
Then endpoints delegate business decisions to focused application services<br>
And field naming, UTC dates, timezone identifiers, and invariant decimal
formats are identical across responses.

### AC-9: Complete authenticated context (FR-10)
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
  sort: string;
}

interface TermSummaryDto {
  id: string;
  code: string;
  label: string;
  state: "draft" | "registrationOpen" | "registrationClosed" |
    "teaching" | "completed" | "archived";
  rowVersion: string;
}

interface AppContextDto {
  serverTimeUtc: string;
  timeZoneId: string;
  teachingTerm?: TermSummaryDto;
  registrationTerm?: TermSummaryDto;
  registrationWindowState: "open" | "upcoming" | "closed" | "none";
  serviceState: "available" | "maintenance" | "unavailable";
  displayName: string;
  authorizedRoles: string[];
  activeRole: string | null;
  sessionState: "active" | "expiring" | "role-selection-required";
  expiresAtUtc: string;
  supportReferencePath: string;
}

interface PublicContextDto {
  serverTimeUtc: string;
  timeZoneId: string;
  teachingTermLabel?: string;
  registrationTermLabel?: string;
  registrationWindowState: "open" | "upcoming" | "closed" | "none";
  serviceState: "available" | "maintenance" | "unavailable";
}
```

Feature endpoints are defined and owned by SPEC-007 through SPEC-017. SPEC-008
owns `GET /api/public/context` and `GET /api/context`; SPEC-006 owns only the
shared response schemas and protocol rules.

Pagination request rule: omitted `page`/`pageSize` means `1`/`20`; maximum
`pageSize` is `100`; invalid values return 400 `PAGE_SIZE_INVALID`; every
feature endpoint declares its allow-listed sort and unique-ID tie-breaker.

Concurrency request rule: `expectedRowVersion` is a required request-body
field for versioned update/delete operations. An authorized stale write returns
409 `STALE_VERSION`; 412/`If-Match` is not part of the MVP protocol.

OpenAPI rule: the deterministic generated document is semantically compared
with the approved baseline in CI. Formatting/order-only differences are
ignored; operation/schema/status/security drift requires explicit approval.

## Data Models

| Type | Role and owner | Purpose |
|---|---|---|
| Strong ID/value object | Shared contract convention, SPEC-006 | Prevent accidental entity/primitive mixing |
| Command/result | Shared envelope convention, SPEC-006; feature payload owned downstream | One application use case |
| API request/response DTO | Shared protocol here; feature DTO owned by its endpoint spec | Versioned client contract |
| AppContextDto | Composed schema SPEC-006; contributors SPEC-007/SPEC-008; handler SPEC-008 | One authenticated context response |
| TermSummaryDto | Shared schema SPEC-006; composed from SPEC-008 AcademicTerm | id, code, label, Draft/RegistrationOpen/RegistrationClosed/Teaching/Completed/Archived state, rowVersion |
| Domain entity/aggregate | Canonical feature owner | Invariant behavior; never exposed directly |

## Out of Scope

- OS-1: GraphQL, gRPC, and public third-party API.
- OS-2: Generic CRUD endpoints for every entity.
- OS-3: A mediator library unless approved handler volume justifies it.
- OS-4: Breaking-change version until an actual breaking change is proposed.
