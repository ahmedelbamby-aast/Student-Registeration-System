# Research: Catalogue Prerequisites and Policy Administration

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
}
```

Endpoints: GET /api/admin/programs, POST /api/admin/courses, PUT
/api/admin/curricula/{id}, POST /api/admin/policies/{id}/validate, POST
/api/admin/policies/{id}/simulate, and POST /api/admin/policies/{id}/publish.

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
