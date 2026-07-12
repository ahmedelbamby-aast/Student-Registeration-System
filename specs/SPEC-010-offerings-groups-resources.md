# SPEC-010: Offerings, Groups, and Resources

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Backend Lead<br>
**Reviewers:** Admin, Lecturer/TA representatives, Data, QA<br>
**Target:** Sprint 2<br>

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

## Non-Functional Requirements

- NFR-1: Offering/group reads SHOULD complete within 300 ms p95 under approved
  read load.
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

## Edge Cases

- EC-1: Multi-slot group has one invalid slot -> entire group cannot publish.
- EC-2: Capacity change races with enrollment -> transaction/rowversion
  preserves Capacity >= EnrolledCount.
- EC-3: Staff becomes unavailable after publish -> flag affected group for
  admin resolution; do not silently move the class.
- EC-4: Overnight meeting slot -> reject in MVP unless separately specified.

## API Contracts

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
