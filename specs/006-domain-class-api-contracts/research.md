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

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
