# Feature Specification: Offerings Groups and Resources

**Feature Branch**: 010-offerings-groups-resources
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Backend Lead
**Normative detail**: [requirements.md](requirements.md)

## Context

Students need accurate groups with capacity, Lecturer/TA, room, day and time.
Only complete, conflict-free administrative schedules should become visible
for registration.

## User Scenarios and Testing

### User Story 1 - Valid group publish (FR-1, FR-2, FR-3) (P1)

As a Authorized administrator, I need the Valid group publish (FR-1, FR-2, FR-3) behavior so that Offerings Groups and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given a group has capacity 30, room capacity 35, valid times, and available
Lecturer/TA<br>
When Admin validates and publishes<br>
Then the group becomes visible to eligible students with all details.
### User Story 2 - Room overlap (FR-3) (P1)

As a Authorized administrator, I need the Room overlap (FR-3) behavior so that Offerings Groups and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given two groups use the same room at overlapping times<br>
When publication is attempted<br>
Then publication is blocked with both groups and overlap interval.
### User Story 3 - Full group (FR-4) (P2)

As a Authorized administrator, I need the Full group (FR-4) behavior so that Offerings Groups and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given EnrolledCount equals Capacity<br>
When a student opens group details<br>
Then group is marked Full and cannot be selected.
### User Story 4 - Audited transactional publish/capacity edit (FR-5, FR-6, FR-7) (P2)

As a Authorized administrator, I need the Audited transactional publish/capacity edit (FR-5, FR-6, FR-7) behavior so that Offerings Groups and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

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

## Requirements

### Functional Requirements

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

### Key Entities

- **CourseOffering**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SectionGroup**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **MeetingSlot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Room**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **GroupStaffAssignment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StaffAvailability**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Only complete and conflict-free groups become visible for registration.
- **SC-2**: Every visible group identifies capacity, staff, location, day, and time.
- **SC-3**: Capacity can never be configured below active enrollment.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Out of Scope

- OS-1: Institution-wide timetable generation.
- OS-2: Automatic reassignment of Lecturer, TA, or room for one student.
- OS-3: Waitlist and seat reservation.
- OS-4: Normal admin force-over-capacity action.
