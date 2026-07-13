# API Contract: Offerings, Groups, and Resources

## Feature Contract

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
interface OfferingValidationResult {
  valid: boolean;
  reasons: Array<{ code: string; message: string; resourceIds: string[]; overlapStartLocal?: string; overlapEndLocal?: string }>;
  previewToken?: string;
  dependencyVersions: { offering: string; groups: Record<string, string>; rooms: Record<string, string>; staffTermAvailability: Record<string, string> };
}
interface StaffTermAvailabilityDto {
  staffId: string;
  termId: string;
  deadlineUtc: string;
  rowVersion: string;
  ranges: Array<{ dayOfWeek: number; startLocal: string; endLocal: string; kind: "available" | "unavailable" }>;
}
interface AdminAvailabilityCorrectionRequest {
  expectedStaffTermRowVersion: string;
  previewToken: string;
  clientRequestId: string;
  reason: string;
  ranges: StaffTermAvailabilityDto["ranges"];
}
interface ScheduleImpactAlertDto {
  id: string;
  groupId: string;
  staffTermAvailabilityId?: string;
  reasonCode: string;
  state: "open" | "revalidated" | "resolved";
  rowVersion: string;
}
```

Endpoints: GET /api/offerings/{offeringId}, GET /api/groups/{groupId}, GET
/api/admin/offerings, POST /api/admin/offerings, PUT
/api/admin/groups/{groupId}, POST /api/admin/offerings/{offeringId}/validate, POST
/api/admin/offerings/{offeringId}/publish, GET /api/admin/rooms, POST
/api/admin/rooms, PUT /api/admin/rooms/{roomId}, GET
/api/admin/staff-availability, and PUT
/api/admin/staff/{staffId}/terms/{termId}/availability; plus GET
/api/admin/schedule-impact-alerts, POST
/api/admin/schedule-impact-alerts/{alertId}/revalidate, and POST
/api/admin/schedule-impact-alerts/{alertId}/resolve.
Every group-state, capacity, meeting, room, and staff-assignment mutation
requires the owning group rowversion. Retryable create/publish uses
clientRequestId; stale resources return 409 GROUP_CHANGED,
RESOURCE_CONFLICT, or IDEMPOTENCY_KEY_REUSED without partial publication.

Admin list endpoints use default page size 20, maximum 100, server filtering,
and stable code/ID tie-breaks. Validation returns a preview bound to actor,
offering canonical content, all dependency versions, and expiry. Publish locks
resources in the documented stable order and rejects any changed dependency.
The Admin availability PUT is a correction, not ordinary ownership: it
requires DEC-12 permission, reason, current root version, signed preview,
idempotency key, audit fact, staff notification, and schedule-impact alert.
Alert list is bounded/filterable. Revalidate requires the expected alert,
group, room, and staff-term versions and records the validation result; resolve
requires expected alert rowversion, resolution reason, and proof of a passing
revalidation. Changed dependencies return `409 STALE_VERSION` and no alert
state change.

## Shared Rules

- All protected operations require server-validated authentication and role/data-scope authorization.
- Validation errors use stable codes and actionable, privacy-safe messages.
- Mutation requests support idempotency or concurrency tokens where retries can duplicate or contest a write.
- Dates use ISO 8601 and the server-configured academic term.
- Lists are bounded and paginated; filtering and sorting are server-side.
