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
/api/admin/staff-availability, GET
/api/admin/schedule-impact-alerts, POST
/api/admin/schedule-impact-alerts/{alertId}/revalidate, and POST
/api/admin/schedule-impact-alerts/{alertId}/resolve.
Every group-state, capacity, meeting, room, and staff-assignment mutation
requires the owning group rowversion. Retryable create/publish uses
clientRequestId; stale resources return 409 GROUP_CHANGED,
RESOURCE_CONFLICT, or IDEMPOTENCY_KEY_REUSED without partial publication.

`Tutorial` is the canonical API activity value; a client may render it as
`Section` without changing the value. Each staff item targets one meeting slot
and repeats its activity type so clients can present each Lecture/Tutorial/
Laboratory with its staff, room/location, day, and time. Publish rejects any
student-selectable group without a Lecturer-staffed Lecture and a TA-staffed
Tutorial or Laboratory; if both Tutorial and Laboratory are present, each has
at least one TA assignment.

Admin list endpoints use default page size 20, maximum 100, server filtering,
and stable code/ID tie-breaks. Validation returns a preview bound to actor,
offering canonical content, all dependency versions, and expiry. Publish locks
resources in the documented stable order and rejects any changed dependency.
GET /api/admin/staff-availability is bounded and read-only. Admin may view the
staff-declared ranges and import them into offering planning as read-only
inputs, but no Admin route, permission, request contract, or editable control
may create, replace, correct, or override StaffTermAvailability in this POC.
Staff-owned availability changes use the SPEC-016 command contract and create
a schedule-impact alert when a published group is affected.
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
