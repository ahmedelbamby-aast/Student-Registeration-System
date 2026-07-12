# Research: Academic Term and Student Profile

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

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
