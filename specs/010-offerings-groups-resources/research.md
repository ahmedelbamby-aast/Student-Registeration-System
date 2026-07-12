# Research: Offerings, Groups, and Resources

## Decisions

### Modular boundary
**Decision**: Own this capability in the Scheduling module of the modular monolith.
**Rationale**: It provides a clear extension seam without premature distributed-system cost.
**Alternatives rejected**: A microservice per feature and direct client-to-database access.

### Authority and consistency
**Decision**: Validate permissions, term state, policy, conflicts, and durable changes on the server, with database enforcement for contested writes.
**Rationale**: Browser state is stale and untrusted during registration peaks.
**Alternatives rejected**: Client-only validation and check-then-write capacity logic.

### Feature contract
```typescript
interface GroupDto {
  id: string;
  groupCode: string;
  capacity: number;
  enrolledCount: number;
  registrationPaused: boolean;
  state: "draft" | "published" | "closed" | "cancelled";
  staff: Array<{ role: "Lecturer" | "TeachingAssistant"; name: string }>;
  meetings: Array<{
    dayOfWeek: number;
    startLocal: string;
    endLocal: string;
    roomCode: string;
    location: string;
  }>;
  rowVersion: string;
}
interface OfferingMutationRequest {
  expectedOfferingRowVersion: string;
  expectedGroupRowVersions: Record<string, string>;
  previewToken?: string;
  clientRequestId: string;
}
```

Endpoints: GET /api/offerings/{id}, GET /api/groups/{id}, POST
/api/admin/offerings, PUT /api/admin/groups/{id}, POST
/api/admin/offerings/{id}/validate, and POST
/api/admin/offerings/{id}/publish.
Every group-state, capacity, meeting, room, and staff-assignment mutation
requires the owning group rowversion. Retryable create/publish uses
clientRequestId; stale resources return 409 GROUP_CHANGED,
RESOURCE_CONFLICT, or IDEMPOTENCY_KEY_REUSED without partial publication.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
