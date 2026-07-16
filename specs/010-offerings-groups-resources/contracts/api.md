# API Contract: Offerings, Groups, and Resources

## Feature DTOs

```typescript
type ActivityType = "Lecture" | "Tutorial" | "Laboratory";
type GroupState = "draft" | "published" | "closed" | "cancelled";

interface MeetingDto {
  id: string;
  activityType: ActivityType;
  dayOfWeek: number;
  startLocal: string;
  endLocal: string;
  roomId: string;
  roomCode: string;
  location: string;
}

interface GroupStaffDto {
  meetingSlotId: string;
  activityType: ActivityType;
  staffId: string;
  role: "Lecturer" | "TeachingAssistant";
  name: string;
}

interface GroupDto {
  id: string;
  offeringId: string;
  groupCode: string;
  capacity: number;
  enrolledCount: number;
  registrationPaused: boolean;
  state: GroupState;
  selectable: boolean;
  nonSelectableReasons: Array<{ code: string; message: string }>;
  staff: GroupStaffDto[];
  meetings: MeetingDto[];
  rowVersion: string;
}

interface CourseOfferingDto {
  id: string;
  termId: string;
  courseId: string;
  courseCode: string;
  courseTitle: string;
  state: "draft" | "published" | "closed" | "cancelled";
  groups: GroupDto[];
  rowVersion: string;
}

interface CourseOfferingSummaryDto {
  id: string;
  termId: string;
  courseId: string;
  courseCode: string;
  courseTitle: string;
  state: "draft" | "published" | "closed" | "cancelled";
  groupCount: number;
  rowVersion: string;
}

interface CreateOfferingRequest {
  termId: string;
  courseId: string;
  groups: Array<{
    groupCode: string;
    capacity: number;
  }>;
  clientRequestId: string;
}

interface MeetingMutation {
  requestMeetingKey: string;
  id?: string;
  activityType: ActivityType;
  dayOfWeek: number;
  startLocal: string;
  endLocal: string;
  roomId: string;
}

interface StaffAssignmentMutation {
  requestMeetingKey: string;
  staffId: string;
  role: "Lecturer" | "TeachingAssistant";
}

interface UpdateGroupRequest {
  expectedOfferingRowVersion: string;
  expectedGroupRowVersion: string;
  expectedRoomRowVersions: Record<string, string>;
  expectedStaffTermAvailabilityRowVersions: Record<string, string>;
  groupCode: string;
  capacity: number;
  registrationPaused: boolean;
  state?: "draft" | "closed" | "cancelled";
  meetings: MeetingMutation[];
  staffAssignments: StaffAssignmentMutation[];
  reason: string;
}

interface DependencyVersions {
  offering: string;
  groups: Record<string, string>;
  rooms: Record<string, string>;
  staffTermAvailability: Record<string, string>;
}

interface ValidateOfferingRequest {
  expectedOfferingRowVersion: string;
  expectedGroupRowVersions: Record<string, string>;
  expectedRoomRowVersions: Record<string, string>;
  expectedStaffTermAvailabilityRowVersions: Record<string, string>;
}

interface OfferingValidationResult {
  valid: boolean;
  reasons: Array<{
    code: string;
    message: string;
    resourceIds: string[];
    overlapStartLocal?: string;
    overlapEndLocal?: string;
  }>;
  previewToken?: string;
  dependencyVersions: DependencyVersions;
}

interface ScheduleImpactValidationSnapshotDto {
  valid: boolean;
  reasons: Array<{
    code: string;
    message: string;
    resourceIds: string[];
    overlapStartLocal?: string;
    overlapEndLocal?: string;
  }>;
  groupRowVersion: string;
  roomRowVersions: Record<string, string>;
  staffTermAvailabilityRowVersions: Record<string, string>;
  validatedAtUtc: string;
}

interface PublishOfferingRequest extends ValidateOfferingRequest {
  previewToken: string;
  clientRequestId: string;
  reason: string;
}

interface RoomDto {
  id: string;
  code: string;
  location: string;
  capacity: number;
  state: "available" | "unavailable";
  rowVersion: string;
}

interface CreateRoomRequest {
  code: string;
  location: string;
  capacity: number;
  state: "available" | "unavailable";
  clientRequestId: string;
}

interface UpdateRoomRequest {
  expectedRowVersion: string;
  code: string;
  location: string;
  capacity: number;
  state: "available" | "unavailable";
  reason: string;
}

interface StaffTermAvailabilityDto {
  id: string;
  staffId: string;
  staffName: string;
  termId: string;
  deadlineUtc: string;
  rowVersion: string;
  ranges: Array<{
    dayOfWeek: number;
    startLocal: string;
    endLocal: string;
    kind: "available" | "unavailable";
  }>;
}

interface ScheduleImpactAlertDto {
  id: string;
  groupId: string;
  staffTermAvailabilityId?: string;
  roomId?: string;
  reasonCode: string;
  state: "open" | "revalidated" | "resolved";
  detectedGroupRowVersion: string;
  detectedRoomRowVersion?: string;
  detectedStaffTermAvailabilityRowVersion?: string;
  lastValidation?: ScheduleImpactValidationSnapshotDto;
  detectedAtUtc: string;
  revalidatedAtUtc?: string;
  resolvedAtUtc?: string;
  rowVersion: string;
}

interface RevalidateScheduleImpactAlertRequest {
  expectedAlertRowVersion: string;
  expectedGroupRowVersion: string;
  expectedRoomRowVersions: Record<string, string>;
  expectedStaffTermAvailabilityRowVersions: Record<string, string>;
}

interface ResolveScheduleImpactAlertRequest {
  expectedAlertRowVersion: string;
  reason: string;
}
```

`Tutorial` is the stored and serialized activity value. A client may display
it as `Section` but must not change the API value. Each staff assignment
targets one meeting. A Lecturer assignment satisfies only a Lecture; a
Teaching Assistant assignment satisfies only its Tutorial or Laboratory.

Within one `UpdateGroupRequest`, every meeting has a required, non-empty,
case-sensitive `requestMeetingKey` that is unique after trimming. Existing
meetings also provide their immutable `id`; new meetings omit `id`. Every
`StaffAssignmentMutation.requestMeetingKey` must exactly match one meeting in
the same request. Unknown, duplicate, blank, or multiply matched keys return
`400 VALIDATION_ERROR`. Array position is never an identity, and
`meetingSlotId`/`meetingOrdinal` are not accepted mutation references.

`UpdateGroupRequest.state` is optional and omission preserves the current
group state. Endpoint 05 may retain a Published group while changing another
approved field, or move a group to Draft, Closed, or Cancelled, but it cannot
set a group or its offering to Published. The only Draft-to-Published
transition is endpoint 07, which publishes the offering and its eligible
groups in one transaction.

### Stable validation and selection codes

`OfferingValidationResult.reasons` uses only the applicable stable codes below:

- `MISSING_LECTURE`, `MISSING_LECTURER`,
  `MISSING_TUTORIAL_OR_LABORATORY`, `MISSING_TEACHING_ASSISTANT`;
- `INVALID_ACTIVITY_ROLE`, `INVALID_SLOT`, `OVERNIGHT_SLOT_NOT_SUPPORTED`;
- `ROOM_CONFLICT`, `ROOM_UNAVAILABLE`, `ROOM_CAPACITY_TOO_SMALL`;
- `STAFF_CONFLICT`, `STAFF_UNAVAILABLE`;
- `DUPLICATE_OFFERING`, `DUPLICATE_GROUP_CODE`;
- `STALE_DEPENDENCY`.

Student `nonSelectableReasons` uses `GROUP_FULL`, `GROUP_UNPUBLISHED`,
`GROUP_CLOSED`, `GROUP_CANCELLED`, or `REGISTRATION_PAUSED`.
`GROUP_CHANGED` is a write/registration concurrency result.
It is never emitted by an ordinary offering or group GET. `selectable` and
these reasons describe only current Scheduling-owned capacity,
registration-pause, and lifecycle state. They do not evaluate SPEC-011
prerequisite, repeat, programme, or other student eligibility.

`OFFERING_NOT_VALIDATABLE` means only that the offering lifecycle/state
prohibits validation or publication, such as Closed, Cancelled, or already
Published where a Draft-only operation is required. Missing staffing,
room/staff overlap, availability, or capacity rules are validation results:
endpoint 06 returns `200 valid=false`, and endpoint 07 returns its declared
specific stale/resource conflict without reclassifying it as
`OFFERING_NOT_VALIDATABLE`.

Unknown codes use the shared safe fallback and correlation ID; the client does
not reinterpret them as success.

## Authorization, paging, and disclosure

- `Catalogue.ReadAvailable` permits a Student to read only student-safe
  published offering/group data. `Offerings.Manage` permits an Admin to read
  and mutate institutional scheduling data. Role membership alone is
  insufficient; server-issued permission and resource scope are checked on
  every request.
- Authorization occurs before existence, state, or rowversion lookup.
  Unauthorized responses contain no protected identifier, current version, or
  existence confirmation.
- Every Admin mutation requires same-origin antiforgery validation.
- List endpoints use page-number pagination. Omitted `page`/`pageSize` means
  `1`/`20`; maximum `pageSize` is `100`. Invalid values return
  `400 PAGE_SIZE_INVALID` without silent capping. Optional text query values
  are trimmed and must contain 3 through 50 characters.
- Every sort is allow-listed and ends in immutable `id`. The response uses the
  shared `Page<T>` contract and echoes the applied canonical sort.

## Endpoint contracts

| # | Endpoint | Query/request and success | Authorization | Documented non-success outcomes |
|---:|---|---|---|---|
| 01 | `GET /api/offerings/{offeringId}` | Named offering ID; `200 CourseOfferingDto`. Student callers receive only Published data and student-safe reasons. Admin callers may receive any state. | Student with `Catalogue.ReadAvailable` and self/published scope, or Admin with `Offerings.Manage` and institutional-admin scope | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 OFFERING_NOT_FOUND`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Conflict is not applicable. |
| 02 | `GET /api/groups/{groupId}` | Named group ID; `200 GroupDto` with capacity/state/version and every activity's type, staff, room/location, day, and time. `selectable` is server-authored. | Student with `Catalogue.ReadAvailable` and self/published scope, or Admin with `Offerings.Manage` | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 GROUP_NOT_FOUND`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Conflict is not applicable. |
| 03 | `GET /api/admin/offerings` | Optional `termId`, `state`, `query`, `page`, `pageSize`, `sort`; `200 Page<CourseOfferingSummaryDto>`. Default sort `courseCode,id`; allowed primary sorts are course code and state. Nested groups are not returned by this list. | Admin plus `Offerings.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Conflict is not applicable. |
| 04 | `POST /api/admin/offerings` | `CreateOfferingRequest`; `201 CourseOfferingDto` in Draft state with one or more initial Draft groups. Group codes are unique after canonicalization. `clientRequestId` is bound to actor, institutional scope, and the server-canonical payload. The create and privacy-safe audit append commit atomically. | Admin plus `Offerings.Manage` and antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 TERM_NOT_FOUND/COURSE_NOT_FOUND`; `409 OFFERING_EXISTS/GROUP_CODE_EXISTS/TERM_NOT_EDITABLE/IDEMPOTENCY_KEY_REUSED`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Same-key/same-payload replays the recorded success or deterministic rejection. |
| 05 | `PUT /api/admin/groups/{groupId}` | `UpdateGroupRequest`; `200 GroupDto`. The complete group graph is validated and changed atomically; group and affected parent/resource versions are required. Capacity, lifecycle, meeting, room, and staff-assignment changes append their privacy-safe audit in the same transaction. This endpoint cannot transition a group or offering into Published; endpoint 07 is the exclusive publisher. | Admin plus `Offerings.Manage`, institutional-admin scope, and antiforgery | `400 VALIDATION_ERROR/INVALID_ACTIVITY_ROLE/INVALID_SLOT`; `401`; `403`; authorized `404 GROUP_NOT_FOUND/ROOM_NOT_FOUND/STAFF_NOT_FOUND`; `409 STALE_VERSION/GROUP_CHANGED/RESOURCE_CONFLICT/CAPACITY_BELOW_ENROLLED/GROUP_NOT_EDITABLE`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. No child, state, capacity, alert, or audit effect commits partially. |
| 06 | `POST /api/admin/offerings/{offeringId}/validate` | `ValidateOfferingRequest`; `200 OfferingValidationResult`. Staffing, capacity, availability, and conflict failures are `200 valid=false` with stable actionable reasons. A valid result includes a signed preview bound to actor, offering canonical content, all dependency versions, and expiry. | Admin plus `Offerings.Manage`, institutional-admin scope, and antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 OFFERING_NOT_FOUND`; `409 STALE_VERSION/OFFERING_NOT_VALIDATABLE`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. `OFFERING_NOT_VALIDATABLE` is lifecycle/state-only; validation writes no publication state or audit. |
| 07 | `POST /api/admin/offerings/{offeringId}/publish` | `PublishOfferingRequest`; `200 CourseOfferingDto`. Locks CourseOffering, sorted groups, sorted rooms, then sorted staff-term roots; revalidates all rules/versions; publishes and appends audit atomically. This is the only endpoint that transitions Draft offerings/groups to Published. | Admin plus `Offerings.Manage`, institutional-admin scope, and antiforgery | `400 VALIDATION_ERROR/PREVIEW_REQUIRED`; `401`; `403`; authorized `404 OFFERING_NOT_FOUND`; `409 STALE_PREVIEW/STALE_VERSION/GROUP_CHANGED/RESOURCE_CONFLICT/CAPACITY_BELOW_ENROLLED/OFFERING_NOT_VALIDATABLE/IDEMPOTENCY_KEY_REUSED`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. `OFFERING_NOT_VALIDATABLE` is lifecycle/state-only. Same-key/same-payload replays the stored result; no partial publication or audit occurs. |
| 08 | `GET /api/admin/rooms` | Optional `query`, `state`, `minimumCapacity`, `page`, `pageSize`, `sort`; `200 Page<RoomDto>`. Default sort `code,id`; allowed primary sorts are code, location, and capacity. | Admin plus `Offerings.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Conflict is not applicable. |
| 09 | `POST /api/admin/rooms` | `CreateRoomRequest`; `201 RoomDto`. The canonical payload is bound to `clientRequestId` in actor plus institutional scope. Room creation and its privacy-safe audit append commit atomically. | Admin plus `Offerings.Manage` and antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; `409 ROOM_CODE_EXISTS/IDEMPOTENCY_KEY_REUSED`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Same-key/same-payload replays the recorded success or deterministic rejection. |
| 10 | `PUT /api/admin/rooms/{roomId}` | `UpdateRoomRequest`; `200 RoomDto`. Expected room version is required. A change affecting Published groups creates or refreshes durable impact alerts, and every room change appends a privacy-safe audit, in the same transaction. | Admin plus `Offerings.Manage`, institutional-admin scope, and antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 ROOM_NOT_FOUND`; `409 STALE_VERSION/ROOM_CODE_EXISTS/ROOM_CAPACITY_CONFLICT/RESOURCE_CONFLICT`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. No room, alert, or audit effect commits partially. |
| 11 | `GET /api/admin/staff-availability` | Required `termId`; optional `staffId`, `query`, `page`, `pageSize`, `sort`; `200 Page<StaffTermAvailabilityDto>`. Default sort `staffName,id`; allowed primary sorts are staff name and deadline. The complete range set is read-only. | Admin plus `Offerings.Manage`; Admin does not receive `Availability.ManageOwn` through this route | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; authorized `404 TERM_NOT_FOUND`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Conflict is not applicable. |
| 12 | `GET /api/admin/schedule-impact-alerts` | Optional `state`, `groupId`, `reasonCode`, `page`, `pageSize`, `sort`; `200 Page<ScheduleImpactAlertDto>`. Default sort `detectedAtUtc-desc,id`; allowed primary sorts are detected time, state, and reason code. | Admin plus `Offerings.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`; `401`; `403`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Conflict is not applicable. |
| 13 | `POST /api/admin/schedule-impact-alerts/{alertId}/revalidate` | `RevalidateScheduleImpactAlertRequest`; `200 ScheduleImpactAlertDto`. The server locks the alert and submitted group/room/staff-term versions, rechecks them, and durably stores a token-free validation snapshot containing those dependency versions. Revalidation advances the alert rowversion and never silently resolves it. | Admin plus `Offerings.Manage`, institutional-admin scope, and antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 ALERT_NOT_FOUND`; `409 STALE_VERSION/GROUP_CHANGED/RESOURCE_CONFLICT/ALERT_NOT_OPEN`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. Changed dependencies produce no alert-state update. |
| 14 | `POST /api/admin/schedule-impact-alerts/{alertId}/resolve` | `ResolveScheduleImpactAlertRequest`; `200 ScheduleImpactAlertDto`. Resolution requires a non-empty reason and the current alert rowversion. The server locks the alert and the dependency identities stored by its latest passing revalidation, then requires their current versions to match that snapshot before resolving and auditing atomically. | Admin plus `Offerings.Manage`, institutional-admin scope, and antiforgery | `400 VALIDATION_ERROR`; `401`; `403`; authorized `404 ALERT_NOT_FOUND`; `409 STALE_VERSION/REVALIDATION_REQUIRED/REVALIDATION_FAILED/ALERT_NOT_OPEN`; `503 SCHEDULING_UNAVAILABLE`; `500 INTERNAL_ERROR`. No silent, stale, unaudited, or partial resolution is committed. |

## Create canonicalization and replay

Canonicalization happens before a create idempotency claim is hashed:

- identifiers must parse and are serialized in their canonical form;
- text is Unicode NFKC-normalized and trimmed; codes are then uppercased with
  invariant rules, while display text preserves case;
- room-location whitespace runs collapse to one ASCII space;
- enum values use their declared lowercase serialized value and numeric values
  use their invariant decimal representation;
- offering groups are canonicalized by normalized group code and sorted by
  that code, then capacity; request array order is not significant; and
- `clientRequestId` identifies the claim but is not part of the payload hash.

Blank or over-limit canonical text and duplicate normalized group codes return
`400 VALIDATION_ERROR`; no server-generated suffix or silent duplicate removal
is allowed. Endpoint 04 records and replays, for the same actor/scope/key/hash,
its `201` result or the deterministic rejection observed by the first completed attempt:
`VALIDATION_ERROR`, `TERM_NOT_FOUND`, `COURSE_NOT_FOUND`, `OFFERING_EXISTS`,
`GROUP_CODE_EXISTS`, or `TERM_NOT_EDITABLE`. Endpoint 09 likewise records and
replays its `201` result, `VALIDATION_ERROR`, or `ROOM_CODE_EXISTS`.
A different canonical payload under the same claim returns
`409 IDEMPOTENCY_KEY_REUSED` and does not replace the recorded result.
Authentication/authorization, antiforgery, `SCHEDULING_UNAVAILABLE`, and
`INTERNAL_ERROR` failures are never recorded as idempotent business results.

## Concurrency, idempotency, and cancellation

| Endpoint(s) | Concurrency and idempotency | Cancellation/result recovery |
|---|---|---|
| 01-03, 08, 11-12 | Read committed state; no idempotency key and no conflict outcome. Page reads are not snapshots across requests. | Cancellation stops the read and returns no success body. The client may repeat the GET. |
| 04 and 09 | Atomic first claim by authenticated actor, institutional/endpoint scope, `clientRequestId`, and server-canonical payload. Same payload replays the recorded success or deterministic rejection; different payload returns `409 IDEMPOTENCY_KEY_REUSED`. | Cancellation before the transaction commit rolls back the claim, create, and audit. Response loss after commit is recovered with the same key and canonical payload. |
| 05 and 10 | Required request-body expected rowversion. A stale authorized write returns `409 STALE_VERSION`; child/resource version conflicts return the declared 409 code. No idempotency key is used. | Cancellation before commit rolls back every mutation, impact alert, and audit effect. After uncertain response loss, the client refetches the resource before deciding whether to retry with a current version. |
| 06 | Expected versions bind the validation input. The signed preview is evidence for confirmation, not authority to skip later checks. No durable idempotency claim is created. | Cancellation stops validation and returns no preview. The client may repeat validation with current versions. |
| 07 | `clientRequestId` is scoped to actor plus offering. The canonical payload includes offering ID, expected dependency versions, preview identity, and reason. Accepted results and deterministic `STALE_PREVIEW`, `STALE_VERSION`, `GROUP_CHANGED`, `RESOURCE_CONFLICT`, `CAPACITY_BELOW_ENROLLED`, and `OFFERING_NOT_VALIDATABLE` results are replayable for the same key/payload. | Cancellation before commit rolls back publication, audit, and claim. Response loss after commit is recovered by same-key/same-payload replay. A deadlock retry reruns the whole idempotent transaction in stable lock order. |
| 13 | The request supplies expected alert/group/room/staff-term versions. The transaction locks and rechecks them, stores a token-free validation snapshot with those exact dependency versions, and advances the alert rowversion. No idempotency key is used. | Cancellation before commit rolls back the snapshot, state, rowversion, and audit effects. After uncertain response loss, refetch the alert before retrying. |
| 14 | The request supplies the current alert rowversion and reason. The transaction locks the alert, then the group/room/staff-term dependencies identified by the latest passing snapshot in stable order, and requires every current version to equal that stored snapshot. Any mismatch returns `409 REVALIDATION_REQUIRED`. | Cancellation before commit rolls back resolution and audit. After uncertain response loss, refetch the alert; retry only when it remains unresolved and with its current rowversion. |

`If-Match` and HTTP 412 are outside the MVP. Only an authorized stale request
may receive a safe current version. Unexpected failures use the shared
privacy-safe `ApiError`; no stack trace, SQL text, secret, raw availability payload,
or unauthorized identifier is returned.

Every non-success response body is the shared `ApiError`. Dependency or service
outage is `503 SCHEDULING_UNAVAILABLE`; an unexpected server failure is
`500 INTERNAL_ERROR`. These names apply consistently to every endpoint and are
not shortened to bare `503` or `500` contract entries.

## Explicit Admin availability mutation absence

There is no Admin route, permission, request DTO, command handler, or editable
control for:

```text
PUT /api/admin/staff/{staffId}/terms/{termId}/availability
POST /api/admin/staff/{staffId}/terms/{termId}/availability
PATCH /api/admin/staff/{staffId}/terms/{termId}/availability
DELETE /api/admin/staff/{staffId}/terms/{termId}/availability
```

`GET /api/admin/staff-availability` returns a bounded read-only projection.
Importing availability into offering planning means selecting its ID and
rowversion as a validation dependency in `UpdateGroupRequest`,
`ValidateOfferingRequest`, or `PublishOfferingRequest`; it does not copy, replace, or mutate the staff-owned range set.
Staff mutation is owned by the SPEC-016 workspace contract with
`Availability.ManageOwn`.

## Shared rules

- Dates and instants use ISO 8601. Meeting day/time is interpreted using the
  server-configured academic-term timezone, never the browser timezone.
- Validation codes are stable, actionable, bounded, and privacy-safe.
- Every endpoint delegates business decisions to focused Scheduling
  application services; HTTP handlers do not implement scheduling policy.
- Deterministic generated OpenAPI must semantically match this approved
  contract before release.
