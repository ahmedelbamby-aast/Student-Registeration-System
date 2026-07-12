# SPEC-009: Catalogue, Prerequisites, and Policy Administration

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Registrar/Policy SME and Backend Lead<br>
**Reviewers:** Admin representative, Data, QA, Security<br>
**Target:** Sprint 2<br>
**Dependencies:** SPEC-002, SPEC-003, SPEC-005, SPEC-006, SPEC-008, SPEC-018<br>

## Context

Public College-of-AI curriculum pages contain missing and inconsistent
references. Production catalogue and rules need an approved import, validation
preview, source provenance, and controlled publication.

## Functional Requirements

- FR-1: Admin MUST manage programs, curricula, courses, credit values, status,
  prerequisites, minimum grades/GPA/earned credits, and cohort scope.
- FR-2: Imports MUST provide preview, row-level validation, provenance, and
  all-or-nothing publication.
- FR-3: The system MUST detect missing references, duplicate codes, invalid
  credits, and prerequisite cycles before publish.
- FR-4: Admin MUST manage typed effective-dated PolicySet/PolicyRule values.
- FR-5: Admin MUST simulate a policy decision against test student inputs
  before publication.
- FR-6: Published catalogue/policy versions MUST be immutable and superseded.
- FR-7: Only approved Admin/Registrar permissions MAY publish.
- FR-8: Every update/publish confirmation MUST include the expected draft
  version and a preview token bound to actor, scope, canonical draft content,
  dependency versions, and expiry.
- FR-9: Catalogue/policy publication MUST lock the affected publication scope,
  revalidate references and conflicts inside one transaction, and atomically
  create its immutable version and audit event.
- FR-10: Retryable import/publish commands MUST use an idempotency key; replay
  of the same key/payload returns its stored result and reuse with a different
  payload returns 409 IDEMPOTENCY_KEY_REUSED.

## Non-Functional Requirements

- NFR-1: Import validation for 10,000 rows SHOULD finish within 30 seconds in
  staging.
- NFR-2: Simulation MUST be deterministic for the same version/input.
- NFR-3: Publication MUST be transactional.
- NFR-4: Every published change MUST have actor, reason, source, and timestamp.

## Acceptance Criteria

### AC-1: Missing prerequisite (FR-2, FR-3)
Given an import references course IN321 that does not exist<br>
When validation runs<br>
Then the row is rejected with source row and missing code<br>
And no part of that import is published.

### AC-2: Prerequisite cycle (FR-3)
Given Course A requires B and B requires A<br>
When the curriculum is validated<br>
Then publication is blocked with the cycle path.

### AC-3: Policy simulation (FR-4, FR-5)
Given a draft Project I rule requiring GPA 2.0 and 96 credits<br>
When simulated with GPA 2.1 and 95 credits<br>
Then it fails with the earned-credit reason<br>
And identifies the draft policy version/source.

### AC-4: Governed catalogue publish (FR-1, FR-6, FR-7)
Given an authorized Admin has a valid draft course/curriculum/policy version<br>
When publication is confirmed<br>
Then the prior published version remains immutable and is superseded<br>
And an unauthorized user cannot publish it.

### AC-5: Concurrent publication has one winner (FR-6, FR-8, FR-9)
Given two admins preview conflicting changes for the same policy scope and
expected version<br>
When both confirmations execute concurrently<br>
Then exactly one immutable version is published<br>
And the other receives 409 STALE_PREVIEW with the current version.

### AC-6: Edited draft invalidates preview (FR-8, FR-10)
Given an admin receives a preview token and then edits the draft<br>
When the old token is confirmed twice with one idempotency key<br>
Then confirmation is rejected as STALE_PREVIEW<br>
And retry returns the same rejection<br>
And no publication or duplicate audit event is created.

### AC-7: Catalogue and policy quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given a 10,000-row staging import, fixed simulation input/version, and
fault-injected publication fixture<br>
When the feature quality gate executes<br>
Then import validation finishes within 30 seconds<br>
And simulations are deterministic<br>
And publication is all-or-nothing<br>
And each published change records actor, reason, source, and timestamp.

## Edge Cases

- EC-1: Duplicate course code differs only by case/spacing -> normalize and
  reject duplicate.
- EC-2: Published course is referenced by history -> deactivate/supersede, do
  not delete.
- EC-3: Concurrent policy publish -> one succeeds; stale version gets 409.
- EC-4: Unknown rule type/config -> reject draft validation.
- EC-5: Audit persistence fails during publish -> the policy/catalogue version
  and activation change roll back in the same local SQL transaction.

## API Contracts

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

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| Course.Code | string | normalized unique, not null |
| Course.Credits | decimal | positive approved range |
| CoursePrerequisite | composite key | course != required course; acyclic graph |
| PolicySet.Version | string | unique in scope; published immutable |
| ImportRowError.SourceRow | integer | required when input row is known |

## Out of Scope

- OS-1: Scraping public web pages as production catalogue source.
- OS-2: Arbitrary policy scripting.
- OS-3: Silent auto-correction of referential errors.
- OS-4: Deleting historical course/policy records.
