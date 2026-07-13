# SPEC-010: Offerings, Groups, and Resources

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** APPROVED<br>
**Owner:** Backend Lead<br>
**Reviewers:** Admin, Lecturer/TA representatives, Data, QA<br>
**Target:** Sprint 2<br>
**Dependencies:** SPEC-003, SPEC-005, SPEC-006, SPEC-009, SPEC-018<br>

## Context

Students need accurate, complete activity bundles with capacity, Lecturer/TA,
room, day, and time. Only offerings whose selectable groups satisfy the simple
demo staffing pattern and whose schedules are conflict-free should become
visible for registration.

## Functional Requirements

- FR-1: Admin MUST create term course offerings and one or more section groups.
- FR-2: Before a course offering can enter Published/Open registration state,
  each student-selectable group MUST be a complete activity bundle with code,
  capacity, state, and at least one Lecture meeting assigned to at least one
  Lecturer, plus at least one Tutorial meeting or Laboratory meeting (or both).
  Each present Tutorial and Laboratory activity MUST be assigned at least one
  Teaching Assistant. Every activity MUST expose its type, assigned staff
  names, room/location, day, and start/end time to students. `Tutorial` is the
  canonical activity value; the UI MAY display it as `Section`.
- FR-3: Publish validation MUST reject staff overlap/unavailability, room
  overlap/unavailability, room capacity below group capacity, invalid slots,
  an incomplete FR-2 activity bundle or missing activity role, and duplicate
  offering/group codes.
- FR-4: Students MUST NOT select full, unpublished, cancelled, or closed
  groups.
- FR-5: Capacity MUST NOT be set below active EnrolledCount.
- FR-6: SPEC-010 MUST own the `StaffTermAvailability` aggregate and child
  `StaffAvailability` ranges. Staff edit their own declarations through the
  SPEC-016 workspace contract. Admin MAY view the declarations and import them
  into offering planning as read-only inputs, but MUST NOT create, replace,
  edit, or override availability in this POC. The Admin surface MUST expose
  no availability mutation endpoint or editable
  availability control. All staff assignment, staff-owned availability, room,
  and resource changes are concurrency protected and audited.
- FR-7: Publication MUST be transactional.
- FR-8: Publication MUST lock every touched offering, room, and staff resource
  in stable order: CourseOffering, SectionGroup IDs, Room IDs, then
  StaffTermAvailability IDs. It MUST revalidate overlaps, availability,
  staffing rules, room capacity, and every expected dependency version inside
  the same transaction to prevent concurrent write skew.
- FR-9: Capacity edits and registration seat allocation MUST serialize on the
  same SectionGroup database row/version and preserve
  0 <= EnrolledCount <= Capacity for every outcome.
- FR-10: Every group-state, meeting-slot, room-assignment, and staff-assignment
  mutation MUST lock and advance the owning SectionGroup rowversion. Staff
  availability mutation and group publication MUST also share the versioned
  staff-term availability boundary defined and owned by SPEC-010; SPEC-016
  consumes that aggregate through the Scheduling application contract.

## Non-Functional Requirements

- NFR-1: Offering/group reads SHOULD complete within 300 ms p95 at the
  SPEC-018 300-read-requests-per-second target.
- NFR-2: Publication validation MUST produce stable actionable reason codes.
- NFR-3: Meeting display MUST use term timezone and unambiguous day/time.
- NFR-4: Large admin lists MUST be paged/filtered.

## Acceptance Criteria

### AC-1: Valid group publish (FR-1, FR-2, FR-3)
Given an offering has one group containing a Lecture with an available
Lecturer and a Tutorial with an available TA, capacity 30, room capacity 35,
and valid times<br>
When Admin validates and publishes<br>
Then the group becomes visible to eligible students with activity type, staff,
room/location, day, and time for both activities.

### AC-2: Room overlap (FR-3)
Given two groups use the same room at overlapping times<br>
When publication is attempted<br>
Then publication is blocked with both groups and overlap interval.

### AC-3: Full group (FR-4)
Given EnrolledCount equals Capacity<br>
When a student opens group details<br>
Then group is marked Full and cannot be selected.

### AC-4: Audited transactional publish/capacity edit (FR-5, FR-6, FR-7)
Given an authorized Admin has current versions and a capacity not below active
enrollment<br>
When offering publication or capacity change is committed<br>
Then the complete change is transactional and audited<br>
And stale/invalid capacity is rejected without partial publication.

### AC-5: Concurrent room publication (FR-3, FR-7, FR-8)
Given two draft groups request the same room and overlapping meeting interval<br>
When separate admins publish them concurrently<br>
Then exactly one group may publish<br>
And the loser receives 409 RESOURCE_CONFLICT after in-transaction revalidation.

### AC-6: Capacity reduction races allocation (FR-5, FR-9)
Given one seat remains and an admin attempts to reduce capacity while one
eligible student submits<br>
When both operations contend on the SectionGroup boundary<br>
Then either serialized outcome may win<br>
But Capacity is never below EnrolledCount and EnrolledCount never exceeds
Capacity.

### AC-7: Offering read and presentation quality (NFR-1, NFR-2, NFR-3, NFR-4)
Given the approved read-load dataset, invalid publication fixtures, two
configured timezones, and a large admin list<br>
When performance, reason-code, time-display, and pagination tests execute<br>
Then offering reads are at most 300 ms p95<br>
And validation returns stable actionable codes<br>
And meeting display uses the term timezone unambiguously<br>
And admin lists remain bounded, paged, and filtered.

### AC-8: Child schedule mutation advances group version (FR-8, FR-10)
Given a registration has captured SectionGroup rowversion 8<br>
When an admin changes one meeting slot, room, staff assignment, or group state
and commits rowversion 9<br>
Then the registration re-read detects GROUP_CHANGED and cannot enroll against
rowversion 8<br>
And concurrent availability/publication uses one valid staff-term serial order.

### AC-9: Staff-owned availability and read-only Admin use (FR-6, FR-8, FR-10)
Given staff owns a current term availability declaration and an authorized
Admin opens ADM-07<br>
When the Admin views or imports the declaration into offering planning<br>
Then the declaration is read-only and no edit or override action is exposed<br>
And the Admin availability mutation route is absent and an attempted mutation
changes no StaffTermAvailability state<br>
And a staff-owned update affecting a published group creates a durable
ScheduleImpactAlert requiring Admin revalidation.

## Edge Cases

- EC-1: Multi-slot group has one invalid slot -> entire group cannot publish.
- EC-2: Capacity change races with enrollment -> transaction/rowversion
  preserves Capacity >= EnrolledCount.
- EC-3: Staff becomes unavailable after publish -> flag affected group for
  admin resolution; do not silently move the class.
- EC-4: Overnight meeting slot -> reject in MVP unless separately specified.
- EC-5: A transaction touches multiple rooms/staff/groups -> acquire every
  resource lock in stable type-and-ID order; a deadlock retry reruns the whole
  idempotent transaction, never a partial publication.

## API Contracts

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
GET /api/admin/staff-availability is bounded and read-only; it may supply
staff-declared ranges as inputs to offering planning but exposes no Admin
availability mutation, edit, or override route.

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| CourseOffering | entity | unique term + course |
| SectionGroup.Capacity | integer | >= EnrolledCount; nonnegative |
| MeetingSlot | value/entity | Lecture/Tutorial/Laboratory; EndLocal > StartLocal; room/day/start/end required |
| GroupStaffAssignment | bridge | unique meeting slot + staff + teaching role; Lecturer only covers Lecture; TA only covers Tutorial/Laboratory |
| StaffTermAvailability | aggregate root | unique staff + term; deadline; complete range-set rowversion |
| StaffAvailability | child range | parent aggregate; valid day/start/end/type; no independent rowversion |
| ScheduleImpactAlert | durable child/entity | affected group/resource/version, reason, detected time, revalidation state |

## Out of Scope

- OS-1: Institution-wide timetable generation.
- OS-2: Automatic reassignment of Lecturer, TA, or room for one student.
- OS-3: Waitlist and seat reservation.
- OS-4: Normal admin force-over-capacity action.
