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
interface ProgramAdminDto {
  id: string;
  code: string;
  displayName: string;
  active: boolean;
}
interface CurriculumCourseAdminDto {
  programCode: string;
  courseCode: string;
  level: number;
  termSequence?: number;
  required: boolean;
}
interface CatalogueDraftDto {
  id: string;
  scope: string;
  basedOnVersionId?: string;
  state: "editing" | "validated" | "published" | "abandoned";
  canonicalContentHash: string;
  rowVersion: string;
  programs: ProgramAdminDto[];
  courses: CourseAdminDto[];
  curricula: CurriculumCourseAdminDto[];
}
type CatalogueDraftOperation =
  | { kind: "upsert-program"; program: ProgramAdminDto }
  | { kind: "upsert-course"; course: CourseAdminDto }
  | { kind: "upsert-curriculum-course"; curriculumCourse: CurriculumCourseAdminDto }
  | { kind: "remove-curriculum-course"; programCode: string; courseCode: string }
  | { kind: "set-prerequisites"; courseCode: string; requiredCourseCodes: string[] };
interface CatalogueDraftMutationRequest {
  expectedDraftRowVersion: string;
  reason: string;
  source: string;
  operations: CatalogueDraftOperation[];
}
interface CatalogueVersionSummaryDto {
  id: string;
  scope: string;
  version: string;
  state: "published" | "superseded";
  source: string;
  publishedAtUtc: string;
}
interface ImportBatchDto {
  id: string;
  draftId: string;
  state: "uploaded" | "validating" | "invalid" | "validated" | "publishing" | "published" | "failed";
  source: string;
  contentHash: string;
  rowVersion: string;
  errors: Array<{ row?: number; field?: string; code: string; message: string }>;
  publishedVersionId?: string;
}
interface CreateImportRequest { draftId: string; source: string; contentHash: string; clientRequestId: string; }
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
interface PolicyRuleAdminDto {
  id?: string;
  code: string;
  valueType: "number" | "boolean" | "string" | "string-list";
  value: number | boolean | string | string[];
  effectiveFromUtc: string;
  effectiveToUtc?: string;
  sourceReference: string;
}
interface PolicySetAdminDto {
  id: string;
  scope: string;
  version: string;
  state: "draft" | "validated" | "published" | "superseded";
  rowVersion: string;
  rules: PolicyRuleAdminDto[];
}
type PolicySetOperation =
  | { kind: "upsert-rule"; rule: PolicyRuleAdminDto }
  | { kind: "remove-rule"; ruleId: string };
interface PolicySetMutationRequest {
  expectedPolicySetRowVersion: string;
  reason: string;
  operations: PolicySetOperation[];
}
interface PolicyPublishRequest {
  expectedPolicySetRowVersion: string;
  previewToken: string;
  clientRequestId: string;
}
interface PolicySimulationRequest {
  policySetId: string;
  studentContextFixtureId: string;
  requestedCourseCodes: string[];
}
interface PolicySimulationResult {
  eligible: boolean;
  ruleResults: Array<{ ruleCode: string; passed: boolean; requiredValue?: string; currentValue?: string; sourceReference: string }>;
}
```

Endpoints: GET /api/admin/programs, GET /api/admin/catalogue/versions, GET and
PUT /api/admin/catalogue/drafts/{draftId}, POST /api/admin/catalogue/imports,
GET /api/admin/catalogue/imports/{importId}, POST
/api/admin/catalogue/imports/{importId}/validate, POST
/api/admin/catalogue/imports/{importId}/publish, GET and POST
/api/admin/policies, PUT /api/admin/policies/{policySetId}, POST
/api/admin/policies/{policySetId}/validate, POST /api/admin/policies/{policySetId}/simulate, and
POST /api/admin/policies/{policySetId}/publish.

List endpoints default to page size 20, reject values above 100 with `400
PAGE_SIZE_INVALID`, and use normalized code then immutable ID as the stable
tie-break. Import creation uploads/associates immutable content with declared
source metadata; status reads expose row errors without publishing. Draft and
policy mutations require body expected rowversions. Validate returns a signed
preview bound to actor, scope, canonical content hash, dependency versions,
and expiry. Publish consumes that preview and a `clientRequestId`; same-payload
replay returns the stored result, while payload mismatch returns `409
IDEMPOTENCY_KEY_REUSED`.
If the signed preview, draft rowversion, content hash, or dependency versions
are no longer current, publication returns `409 STALE_PREVIEW` without writes.
Catalogue and policy mutations accept only the discriminated operation
allow-lists above. Arbitrary property names, navigation graphs, and untyped
values are rejected; rule `value` must match its declared `valueType`.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
