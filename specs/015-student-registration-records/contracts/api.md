# API Contract: Student Registration Records

## Feature Contract

```typescript
interface RegistrationReceiptDto {
  submissionId: string;
  reference: string;
  term: TermSummaryDto;
  submittedAtUtc: string;
  policyVersion: string;
  groups: GroupDto[];
  totalCredits: number;
}
```

Endpoints: GET /api/student/registrations, GET
/api/student/registrations/{submissionId}, GET
/api/student/registrations/current/timetable.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
