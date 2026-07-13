# Research: Catalogue, Prerequisites, and Policy Administration

## Decisions

### Modular boundary
**Decision**: Own this capability in the Academics module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
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

The Admin contract exposes explicit catalogue version, draft, import lifecycle,
and policy endpoints. Manual edits target a draft; imports target a draft and
retain status/errors; only validated, current previews can publish an
immutable version.

### Catalogue versioning
**Decision**: Separate editable `CatalogueDraft`, immutable
`CatalogueVersion`, and versioned `ImportBatch` aggregates.
**Rationale**: The model makes preview invalidation, import errors, provenance,
and historical meaning explicit without event sourcing or a generic workflow
engine.
**Alternatives rejected**: Editing published rows and one overloaded table
with implicit states.

### Admin API ownership
**Decision**: Academics owns catalogue/policy reads and mutations; SPEC-017
consumes bounded audit/report facts instead of duplicating owner handlers.
**Rationale**: One writer per aggregate prevents divergent validation.
**Alternatives rejected**: Parallel Admin handlers in an operations module.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
