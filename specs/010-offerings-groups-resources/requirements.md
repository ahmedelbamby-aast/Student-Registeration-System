# SPEC-010: Offerings, Groups, and Resources

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Backend Lead<br>
**Reviewers:** Admin, Lecturer/TA representatives, Data, QA<br>
**Target:** Sprint 2<br>
**Dependencies:** SPEC-003, SPEC-005, SPEC-006, SPEC-009, SPEC-018<br>

## Context

Students need accurate groups with capacity, Lecturer/TA, room, day and time.
Only complete, conflict-free administrative schedules should become visible
for registration.

## Functional Requirements

- FR-1: Admin MUST create term course offerings and one or more section groups.
- FR-2: Each group MUST have code, capacity, state, meeting slots, room(s), and
  required Lecturer/TA assignments before publish.
- FR-3: Publish validation MUST reject staff overlap/unavailability, room
  overlap/unavailability, room capacity below group capacity, invalid slots,
  missing roles, and duplicate offering/group codes.
- FR-4: Students MUST NOT select full, unpublished, cancelled, or closed
  groups.
- FR-5: Capacity MUST NOT be set below active EnrolledCount.
- FR-6: Staff assignments/availability and resource changes MUST be
  optimistic-concurrency protected and audited.
- FR-7: Publication MUST be transactional.
- FR-8: Publication MUST lock every touched offering, room, and staff resource
  in stable identifier order and revalidate overlaps/availability inside the
  same transaction to prevent concurrent write skew.
- FR-9: Capacity edits and registration seat allocation MUST serialize on the
  same SectionGroup database row/version and preserve
  0 <= EnrolledCount <= Capacity for every outcome.
- FR-10: Every group-state, meeting-slot, room-assignment, and staff-assignment
  mutation MUST lock and advance the owning SectionGroup rowversion. Staff
  availability mutation and group publication MUST also share the versioned
  staff-term availability boundary defined by SPEC-016.

## Non-Functional Requirements

- NFR-1: Offering/group reads SHOULD complete within 300 ms p95 at the
  SPEC-018 300-read-requests-per-second target.
- NFR-2: Publication validation MUST produce stable actionable reason codes.
- NFR-3: Meeting display MUST use term timezone and unambiguous day/time.
- NFR-4: Large admin lists MUST be paged/filtered.

## Acceptance Criteria

### AC-1: Valid group publish (FR-1, FR-2, FR-3)
Given a group has capacity 30, room capacity 35, valid times, and available
Lecturer/TA<br>
When Admin validates and publishes<br>
Then the group becomes visible to eligible students with all details.

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

## Data Models

| Field/example | Type | Constraints |
|---|---|---|
| CourseOffering | entity | unique term + course |
| SectionGroup.Capacity | integer | >= EnrolledCount; nonnegative |
| MeetingSlot | value/entity | EndLocal > StartLocal |
| GroupStaffAssignment | bridge | unique group + staff + teaching role |
| StaffAvailability | range | valid term/day/start/end; rowversion |

## Out of Scope

- OS-1: Institution-wide timetable generation.
- OS-2: Automatic reassignment of Lecturer, TA, or room for one student.
- OS-3: Waitlist and seat reservation.
- OS-4: Normal admin force-over-capacity action.
