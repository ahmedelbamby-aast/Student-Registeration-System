# API Contract: AASTMT Policy Rulebook

## Feature Contract

```typescript
interface PolicyDecisionDto {
  eligible: boolean;
  policyVersion: string;
  evaluatedAtUtc: string;
  results: Array<{
    reasonCode: string;
    passed: boolean;
    explanation: string;
    sourceUrl: string;
    overridePossible: boolean;
  }>;
}
```

Endpoints: POST /api/admin/policies/{id}/simulate and GET
/api/student/offerings/{id}/eligibility.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
