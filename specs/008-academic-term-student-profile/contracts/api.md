# API Contract: Academic Term and Student Profile

## Feature Contract

```typescript
interface StudentAcademicContextDto {
  universityId: string;
  programCode: string;
  cohort: string;
  currentGpa: number;
  earnedCredits: number;
  standing: string;
  blockingHolds: Array<{ code: string; message: string }>;
  dataAsOfUtc: string;
  provenance: string;
}
```

Endpoints: GET /api/context, GET /api/students/me/academic-context; admin
mutation contracts live in SPEC-017.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
