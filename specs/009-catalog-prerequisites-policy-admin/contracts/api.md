# API Contract: Catalogue, Prerequisites, and Policy Administration

## Feature Contract

```typescript
interface CatalogueFieldProvenanceDto {
  sourceReference: string;
  accessedOn: string;
  sourceKind: "official-source" | "synthetic-demo";
  syntheticFields: string[];
}
interface CourseAdminDto {
  id: string;
  code: string;
  title: string;
  credits: number;
  active: boolean;
  provenance: CatalogueFieldProvenanceDto;
  rowVersion: string;
}
interface ProgramAdminDto {
  id: string;
  code: string;
  displayName: string;
  active: boolean;
  provenance: CatalogueFieldProvenanceDto;
}
interface CurriculumCourseAdminDto {
  programCode: string;
  courseCode: string;
  level: number;
  termSequence?: number;
  required: boolean;
  provenance: CatalogueFieldProvenanceDto;
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
  | { kind: "set-prerequisites"; courseCode: string; requiredCourseCodes: string[]; provenance: CatalogueFieldProvenanceDto };
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
  accessedOn: string;
  contentHash: string;
  syntheticFieldCount: number;
  rowVersion: string;
  errors: Array<{ row?: number; field?: string; code: string; message: string }>;
  publishedVersionId?: string;
}
interface CreateImportRequest {
  draftId: string;
  source: string;
  accessedOn: string;
  contentHash: string;
  syntheticFields: string[];
  clientRequestId: string;
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
interface PolicyRuleAdminDto {
  id?: string;
  code: string;
  valueType: "number" | "boolean" | "string" | "string-list";
  value: number | boolean | string | string[];
  effectiveFromUtc: string;
  effectiveToUtc?: string;
  sourceReference: string;
  sourceKind: "official-source" | "synthetic-demo";
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
  policyVersion: string;
  ruleResults: Array<{ ruleCode: string; passed: boolean; requiredValue?: string; currentValue?: string; sourceReference: string; sourceKind: "official-source" | "synthetic-demo" }>;
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
Catalogue validation also rejects absent/malformed field provenance. Official-
source records retain the URL/access date and enumerate any synthetic gap
fields; fully local records use `synthetic-demo` and cannot be presented as
official curriculum data.

## Authorization and endpoint contracts

Every endpoint below requires authenticated Admin role plus the independent
`CataloguePolicy.Manage` permission. Admin role membership alone does not
grant access. Authorization occurs before draft, import, policy, version, or
rowversion lookup. Mutations also require same-origin antiforgery validation.
An unauthorized response never confirms resource existence or returns a
current version.

All list endpoints use default page 1/size 20 and maximum size 100. Invalid
page values return `400 PAGE_SIZE_INVALID` without silent capping. Optional
text filters are trimmed and limited to 3 through 50 characters. Stable sorts
always end in immutable ID.

| # | Endpoint | Request and success | Authorization | Documented non-success outcomes |
|---:|---|---|---|---|
| 01 | `GET /api/admin/programs` | Optional query, active filter, page/pageSize, and allow-listed sort; `200 Page<ProgramAdminDto>`. Default sort `code,id`; allowed primary sorts code and display name. | `CataloguePolicy.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; `503 CATALOGUE_UNAVAILABLE`; `500 INTERNAL_ERROR`. Conflict is not applicable. |
| 02 | `GET /api/admin/catalogue/versions` | Optional scope/state filters, page/pageSize, and allow-listed sort; `200 Page<CatalogueVersionSummaryDto>`. Default sort `publishedAtUtc-desc,id`; allowed primary sorts version, scope, state, and published time. Results are immutable summaries. | `CataloguePolicy.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; `503`; `500`. Conflict is not applicable. |
| 03 | `GET /api/admin/catalogue/drafts/{draftId}` | Named draft identifier; `200 CatalogueDraftDto`. | `CataloguePolicy.Manage` plus institutional-admin scope | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 DRAFT_NOT_FOUND`; `503`; `500`. No rowversion is disclosed on denial. |
| 04 | `PUT /api/admin/catalogue/drafts/{draftId}` | `CatalogueDraftMutationRequest`; `200 CatalogueDraftDto`. The allow-listed operations apply atomically and advance content hash/rowversion; any prior preview is invalidated. | `CataloguePolicy.Manage` plus institutional-admin scope and antiforgery | `400 VALIDATION_ERROR/PROVENANCE_REQUIRED/PROVENANCE_INVALID`; `401`; `403`; authorized `404`; `409 STALE_VERSION/DRAFT_NOT_EDITABLE`; `503`; `500`. No partial operation is committed. |
| 05 | `POST /api/admin/catalogue/imports` | `CreateImportRequest`; `201 ImportBatchDto`. Source reference, access date, server-verified content hash, and explicit synthetic-field manifest are required. `clientRequestId` is payload-bound in the authenticated actor plus draft scope. | `CataloguePolicy.Manage` and antiforgery | `400 VALIDATION_ERROR/PROVENANCE_REQUIRED`; `401`; `403`; authorized `404 DRAFT_NOT_FOUND`; `409 IDEMPOTENCY_KEY_REUSED/DRAFT_NOT_EDITABLE`; `503`; `500`. Same-key/same-payload replays the created batch. |
| 06 | `GET /api/admin/catalogue/imports/{importId}` | Named import identifier; `200 ImportBatchDto` with bounded privacy-safe row errors. At most 100 errors are returned; the response reports a total error count and continuation state when more exist. | `CataloguePolicy.Manage` plus institutional-admin scope | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 IMPORT_NOT_FOUND`; `503`; `500`. Raw imported rows/content are never returned. |
| 07 | `POST /api/admin/catalogue/imports/{importId}/validate` | Expected import and draft rowversions; `200 CatalogueValidationResult`. Validation covers the complete normalized graph, duplicate codes, credits, missing references, cycles, and official/synthetic field provenance. A valid result includes a signed preview token bound to actor, scope, canonical content/import hash, dependency versions, and expiry. | `CataloguePolicy.Manage` plus institutional-admin scope and antiforgery | `400 VALIDATION_ERROR/PROVENANCE_INVALID/UNKNOWN_RULE_TYPE`; `401`; `403`; authorized `404`; `409 STALE_VERSION/IMPORT_NOT_VALIDATABLE`; `503`; `500`. Invalid graph results are a `200` validation result with stable row errors, not partial publication. |
| 08 | `POST /api/admin/catalogue/imports/{importId}/publish` | `PublishVersionRequest`; `201 CatalogueVersionSummaryDto`. The command locks normalized catalogue scope, revalidates the graph and dependencies, supersedes the prior active version, publishes one immutable version, advances import/draft state, and appends one audit fact in one transaction. | `CataloguePolicy.Manage` plus institutional-admin scope and antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404`; `409 STALE_PREVIEW/STALE_VERSION/IDEMPOTENCY_KEY_REUSED/PUBLICATION_CONFLICT`; `503`; `500`. Same-key/same-payload replays the stored result; audit or storage failure rolls back all effects. |
| 09 | `GET /api/admin/policies` | Optional scope/state/effective-date filters, page/pageSize, and allow-listed sort; `200 Page<PolicySetAdminDto>`. Default sort `effectiveFromUtc-desc,id`; each rule includes source reference and source kind. | `CataloguePolicy.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; `503 POLICY_UNAVAILABLE`; `500`. Conflict is not applicable. |
| 10 | `POST /api/admin/policies` | New draft scope/version plus typed `PolicySetOperation` values and `clientRequestId`; `201 PolicySetAdminDto`. The key is bound to actor, normalized scope, and canonical typed payload. | `CataloguePolicy.Manage` and antiforgery | `400 VALIDATION_ERROR/UNKNOWN_RULE_TYPE/RULE_VALUE_TYPE_MISMATCH`; `401`; `403`; `409 POLICY_SCOPE_EXISTS/IDEMPOTENCY_KEY_REUSED`; `503`; `500`. |
| 11 | `PUT /api/admin/policies/{policySetId}` | `PolicySetMutationRequest`; `200 PolicySetAdminDto`. Expected policy-set rowversion is required; only allow-listed typed operations are accepted and any old preview is invalidated. | `CataloguePolicy.Manage` plus institutional-admin scope and antiforgery | `400 VALIDATION_ERROR/UNKNOWN_RULE_TYPE/RULE_VALUE_TYPE_MISMATCH/PROVENANCE_REQUIRED`; `401`; `403`; authorized `404`; `409 STALE_VERSION/POLICY_NOT_EDITABLE`; `503`; `500`. No partial rule edit is committed. |
| 12 | `POST /api/admin/policies/{policySetId}/validate` | Expected policy-set rowversion; `200 CatalogueValidationResult`. Validation requires the complete typed demo rule set, effective dates, unique scope/priority, source classification, and no executable expression. A valid response includes a bound preview token. | `CataloguePolicy.Manage` plus institutional-admin scope and antiforgery | `400 VALIDATION_ERROR/UNKNOWN_RULE_TYPE/RULE_VALUE_TYPE_MISMATCH/PROVENANCE_REQUIRED`; `401`; `403`; authorized `404`; `409 STALE_VERSION/POLICY_NOT_VALIDATABLE`; `503`; `500`. |
| 13 | `POST /api/admin/policies/{policySetId}/simulate` | `PolicySimulationRequest`; `200 PolicySimulationResult`. The fixed policy version/input yields deterministic ordered rule results including policy version, source reference, source kind, required/current values, and pass/fail. Simulation performs no durable mutation. | `CataloguePolicy.Manage` plus institutional-admin scope and antiforgery | `400 VALIDATION_ERROR/SIMULATION_FIXTURE_INVALID`; `401`; `403`; authorized `404`; `409 POLICY_NOT_VALIDATED`; `503`; `500`. |
| 14 | `POST /api/admin/policies/{policySetId}/publish` | `PolicyPublishRequest`; `201 PolicySetAdminDto`. Publication locks normalized policy scope, revalidates preview/dependencies, creates one immutable published version, supersedes the prior version, and appends one audit fact atomically. | `CataloguePolicy.Manage` plus institutional-admin scope and antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404`; `409 STALE_PREVIEW/STALE_VERSION/IDEMPOTENCY_KEY_REUSED/PUBLICATION_CONFLICT`; `503`; `500`. Concurrent confirmations have one winner; same-payload replay returns the stored result. |

Draft/import and policy expected-version validation occurs before content
mutation but after authorization. Publication acquires the normalized scope
lock and repeats validation inside the transaction; a preview is evidence for
confirmation, not authority to skip current checks. Deterministic business
rejections `STALE_PREVIEW`, `STALE_VERSION`, and
`IDEMPOTENCY_KEY_REUSED` are replayable for the same canonical request.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
