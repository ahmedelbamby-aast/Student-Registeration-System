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

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
