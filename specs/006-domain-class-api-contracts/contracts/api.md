# API Contract: Domain Classes and API Contracts

## Feature Contract

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
  teachingTerm: TermSummaryDto | null;
  registrationTerm: TermSummaryDto | null;
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
  teachingTermLabel: string | null;
  registrationTermLabel: string | null;
  registrationWindowState: "open" | "upcoming" | "closed" | "none";
  serviceState: "available" | "maintenance" | "unavailable";
}
```

`activeRole` is non-null for `active` and `expiring`. It is null only when
the authenticated user has multiple authorized roles and `sessionState` is
`role-selection-required`; no client-selected value grants authorization.

A term field is null only when the authoritative academic-term contributor
successfully reports that no applicable term exists. A missing or failed
required contributor returns 503 `CONTEXT_UNAVAILABLE` as `ApiError`; the API
does not return a partial context or use null to hide composition failure.

Feature endpoints are defined and owned in SPEC-007 through SPEC-017. SPEC-008
owns `GET /api/public/context` and `GET /api/context`. SPEC-006 owns the shared
schemas and protocol only; it owns no context handler or generic resource/command
endpoint.

### Pagination protocol

- Omitted `page` and `pageSize` mean `1` and `20`; maximum `pageSize` is `100`.
- `page < 1`, `pageSize < 1`, or `pageSize > 100` returns 400
  `PAGE_SIZE_INVALID`; servers do not silently cap.
- Each listing contract declares an allow-listed default sort ending with its
  unique identifier as a deterministic tie-breaker. `Page.sort` echoes the
  applied canonical sort.
- Page-number reads reflect committed state at each request; after a concurrent
  mutation the client refetches from page 1 rather than treating pages as a
  snapshot.

### Optimistic concurrency protocol

- Versioned update/delete request DTOs contain required `expectedRowVersion`.
- An authorized stale request returns 409 `STALE_VERSION` and may include
  `ApiError.currentVersion`; unauthorized requests return 403 without resource
  or version disclosure.
- `If-Match` and 412 are outside the MVP protocol.

### OpenAPI protocol

Before a downstream endpoint can pass its release gate, CI must generate a
deterministic OpenAPI document and perform a semantic diff against an approved
versioned baseline. Unapproved operation, schema, status-code, or security
drift fails; formatting/order-only differences do not. SPEC-006 cannot create
that baseline until approved, version-pinned handlers and a real OpenAPI
generator exist; design-time schemas alone are not runtime API evidence.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Retryable create/confirm/submit commands require their feature-owned idempotency key; versioned update/delete DTOs require `expectedRowVersion` and use 409 `STALE_VERSION`.
- Dates use ISO 8601 and the server-configured academic term.
- Lists use the exact bounded pagination and deterministic sorting protocol above.
