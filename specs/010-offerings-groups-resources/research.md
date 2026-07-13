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
  staff: Array<{
    meetingSlotId: string;
    activityType: "Lecture" | "Tutorial" | "Laboratory";
    role: "Lecturer" | "TeachingAssistant";
    name: string;
  }>;
  meetings: Array<{
    id: string;
    activityType: "Lecture" | "Tutorial" | "Laboratory";
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
lists, create/update, validate/publish, and a bounded read-only Admin view of
staff-declared availability documented in contracts/api.md. No Admin
availability mutation route exists.
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

### Staffing and availability ownership
**Decision**: Ahmed ELbamby approved DEC-11's simple demo rule. Every
Published/Open student-selectable group is a complete activity bundle with at
least one Lecture assigned to a Lecturer and at least one Tutorial or
Laboratory (or both); each present Tutorial/Laboratory is assigned to a TA.
`Tutorial` is canonical and the UI may display `Section`. Staff assignments
target the existing meeting/activity, so no additional entity is introduced.
DEC-12 keeps availability edits staff-owned through SPEC-016. Admin may view
the declarations and import them into offering planning as read-only inputs,
but cannot create, replace, edit, or override them in this POC. A staff-owned
change affecting a published group creates the durable schedule-impact alert.
**Rationale**: The rule demonstrates publish-time staffing validation and gives
students complete staff/room/time detail without a generalized curriculum or
teaching-pattern engine.
**Alternatives rejected**: Allowing lecture-only published groups, requiring
both Tutorial and Laboratory for every course, inventing a separate Activity
entity, any Admin availability mutation or override route, silent Admin
overwrites, and unpublished role exceptions.

### Resource dependency versions
**Decision**: Validation/publish bind offering, group, room, and
StaffTermAvailability versions and lock them in stable type/ID order.
**Rationale**: This prevents room/staff write skew across replicas.
**Alternatives rejected**: Check-then-publish and offering-only rowversion.



## Open Research

DEC-11 and DEC-12 are resolved for this demo. A future production Admin
mutation or override workflow requires a separately approved specification;
it is not implied by the read-only POC view/import behavior.
