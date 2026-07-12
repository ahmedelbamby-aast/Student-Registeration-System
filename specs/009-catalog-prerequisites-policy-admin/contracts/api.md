# API Contract: Catalogue, Prerequisites, and Policy Administration

## Feature Contract

```typescript
interface CourseAdminDto {
  id: string;
  code: string;
  title: string;
  credits: number;
  active: boolean;
  rowVersion: string;
}
interface CatalogueValidationResult {
  valid: boolean;
  errors: Array<{ row?: number; code: string; message: string }>;
  previewToken?: string;
  expectedDraftRowVersion: string;
  dependencyVersions: Record<string, string>;
}
interface PublishVersionRequest {
  expectedDraftRowVersion: string;
  previewToken: string;
  clientRequestId: string;
}
```

Endpoints: GET /api/admin/programs, POST /api/admin/courses, PUT
/api/admin/curricula/{id}, POST /api/admin/policies/{id}/validate, POST
/api/admin/policies/{id}/simulate, and POST /api/admin/policies/{id}/publish.
Every update/publish request uses expected version; retryable create/publish
uses clientRequestId. Stale preview/version and idempotency payload mismatch
return 409 STALE_PREVIEW, STALE_VERSION, or IDEMPOTENCY_KEY_REUSED.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
