# Research: Offerings Groups and Resources

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
```

Endpoints: GET /api/offerings/{id}, GET /api/groups/{id}, POST
/api/admin/offerings, PUT /api/admin/groups/{id}, POST
/api/admin/offerings/{id}/validate, and POST
/api/admin/offerings/{id}/publish.

## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
