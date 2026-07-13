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
  expectedRoomRowVersions: Record<string, string>;
  expectedStaffTermAvailabilityRowVersions: Record<string, string>;
  previewToken: string;
  clientRequestId: string;
}
```

Endpoints include offering/group reads, bounded Admin offering/room/resource
lists, create/update, validate/publish, and the separately authorized Admin
staff-availability correction contract documented in contracts/api.md.
Every group-state, capacity, meeting, room, and staff-assignment mutation
requires the owning group rowversion. Retryable create/publish uses
clientRequestId; stale resources return 409 GROUP_CHANGED,
RESOURCE_CONFLICT, or IDEMPOTENCY_KEY_REUSED without partial publication.

### Availability ownership
**Decision**: Scheduling/SPEC-010 owns `StaffTermAvailability` and its complete
child range set. SPEC-016 supplies staff-facing commands through a Scheduling
port; it does not own or redefine the aggregate.
**Rationale**: Offering publication and staff edits share one upstream
version/lock boundary without a dependency cycle.
**Alternatives rejected**: Downstream SPEC-016 ownership and independent range
rowversions.

### Staffing and Admin correction
**Decision**: Apply DEC-11 by activity type and DEC-12 by staff ownership plus
a separately authorized, previewed, reasoned, versioned, audited Admin
correction with notification.
**Rationale**: These defaults are least-privilege and fail safely while keeping
institutional exceptions explicit.
**Alternatives rejected**: Requiring both roles for every activity, silent
Admin overwrites, and unpublished role exceptions.

### Resource dependency versions
**Decision**: Validation/publish bind offering, group, room, and
StaffTermAvailability versions and lock them in stable type/ID order.
**Rationale**: This prevents room/staff write skew across replicas.
**Alternatives rejected**: Check-then-publish and offering-only rowversion.



## Open Research

No unresolved requirement clarification remains. External institutional approvals are tracked as release prerequisites and configuration provenance, not guessed defaults.
