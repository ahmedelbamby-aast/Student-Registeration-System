# Research: Domain Classes and API Contracts

## Decisions

### Modular boundary
**Decision**: Own this capability in the Contracts module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
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

interface RegistrationWindowSummaryDto {
  id: string;
  state: "open" | "upcoming" | "closed";
  opensAtUtc: string;
  closesAtUtc: string;
  rowVersion: string;
}

interface AppContextDto {
  serverTimeUtc: string;
  timeZoneId: string;
  teachingTerm: TermSummaryDto | null;
  registrationTerm: TermSummaryDto | null;
  registrationWindowState: "open" | "upcoming" | "closed" | "none";
  registrationWindow: RegistrationWindowSummaryDto | null;
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
  teachingTermLabel: string | null;
  registrationTermLabel: string | null;
  registrationWindowState: "open" | "upcoming" | "closed" | "none";
  serviceState: "available" | "maintenance" | "unavailable";
}
```

Feature endpoints are defined in SPEC-007 through SPEC-017. SPEC-008 owns the
two context handlers; SPEC-006 owns their shared schemas only. Generic example
resource and command paths were rejected because every literal method/path must
have one feature owner and canonical contract.

Null term values mean the authoritative contributor reported that no applicable
term exists. Missing or failed required contributors produce 503
`CONTEXT_UNAVAILABLE`; partial context responses are rejected so clients cannot
confuse an outage with a valid absence.

A null registration term requires `registrationWindowState = "none"` and a
null `registrationWindow`. A present window is the single matched window,
requires a non-null registration term, and has a state equal to
`registrationWindowState`. The public contract remains exactly six fields and
does not expose the matched-window ID, UTC interval, row version, or any
personal/session data. T011 is the sole shared source writer for
`AppContextDto`, `TermSummaryDto`, and `RegistrationWindowSummaryDto`; SPEC-008
consumes those contracts and remains the sole owner of both context handlers.

### Pagination and concurrency protocol
**Decision**: Page-number pagination uses default 20, maximum 100, invalid-value
400 responses, and a feature-declared stable sort with a unique-ID tie-breaker.
Versioned update/delete DTOs use request-body `expectedRowVersion` and return
409 `STALE_VERSION`; MVP does not also support If-Match/412.
**Rationale**: One explicit protocol is simpler to implement and test than two
equivalent concurrency mechanisms or cap-versus-reject ambiguity.
**Alternatives rejected**: Silent page-size caps, unspecified ordering,
If-Match plus body tokens, and generic pseudo endpoints.

### OpenAPI drift gate
**Decision**: Once approved, version-pinned downstream handlers and a real
generator exist, generate deterministic OpenAPI and compare an approved
baseline semantically in CI.
**Rationale**: A semantic gate detects real endpoint/schema/security drift while
ignoring formatting and ordering noise.
**Alternatives rejected**: Documentation-only review, generating a speculative
baseline from schemas with no handlers, and byte-for-byte output comparison.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
