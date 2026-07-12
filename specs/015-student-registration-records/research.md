# Research: Student Registration Records

## Decisions

### Modular boundary
**Decision**: Own this capability in the Registration module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
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

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
