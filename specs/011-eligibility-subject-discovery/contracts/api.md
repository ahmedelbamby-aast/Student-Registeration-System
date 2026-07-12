# API Contract: Eligibility and Subject Discovery

## Feature Contract

```typescript
interface OfferingEligibilityDto {
  offeringId: string;
  courseCode: string;
  title: string;
  credits: number;
  eligible: boolean;
  reasons: Array<{ code: string; passed: boolean; message: string }>;
  policyVersion: string;
  groups: GroupDto[];
}
```

Endpoint: GET /api/student/terms/{termId}/offerings with q, eligibility,
credits, day, availability, page, and pageSize.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
