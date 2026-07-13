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
  provenance: CatalogueFieldProvenanceDto;
  rowVersion: string;
}
interface CatalogueFieldProvenanceDto {
  sourceReference: string;
  accessedOn: string;
  sourceKind: "official-source" | "synthetic-demo";
  syntheticFields: string[];
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

### Curated demo curriculum and provenance

**Decision**: Seed the exact 19-course snapshot in
`docs/DEMO_CURRICULUM.md`, curated from the official AASTMT College of
Artificial Intelligence Data Science curriculum and accessed 2026-07-13. This
is a demo fixture, not a live scraper and not a claim that the snapshot is the
complete or current official catalogue.

The public program list does not publish per-course credit values. The demo
therefore assigns three credits per snapshot course and supplies required/
active flags and fixture identifiers locally; those fields are
`synthetic-demo-only` in the provenance manifest and UI. Codes, titles, term
placement, and the documented prerequisite facts are `official-source`. Any
later locally created course uses a `DEMO-` code and the `synthetic-demo`
classification.

**Rationale**: Nineteen records provide clean prerequisite chains, the DS413
GPA/earned-credit boundary, and an 18-credit plan while keeping one canonical
curriculum document and avoiding an unverified full import.

**Alternatives rejected**: Runtime web scraping, presenting synthetic credits
as official values, and building a complete curriculum before the POC proves
the registration workflow.

### Simple demo policy baseline

**Decision**: Publish typed demo rules for an open registration window,
prerequisites, course-specific GPA/earned-credit gates, academic standing,
normal target/maximum 18 credits, GPA-below-2.0 maximum 12 credits, selectable
capacity, and timetable conflict. The 18/12 load values are Ahmed-approved
demo rules rather than claims about current official AASTMT policy. Advisor,
overload, waiver, and other exception workflows are excluded.

**Rationale**: This is the smallest explainable rule set that proves policy
configuration and meaningful eligible/ineligible outcomes end to end.

**Alternatives rejected**: A generic policy DSL and implementing institutional
exception workflows in the demo.

### Admin API ownership
**Decision**: Academics owns catalogue/policy reads and mutations; SPEC-017
consumes bounded audit/report facts instead of duplicating owner handlers.
**Rationale**: One writer per aggregate prevents divergent validation.
**Alternatives rejected**: Parallel Admin handlers in an operations module.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
