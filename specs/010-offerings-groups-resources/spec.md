# Feature Specification: Offerings, Groups, and Resources

**Feature Branch**: 010-offerings-groups-resources
**Created**: 2026-07-12
**Status**: APPROVED
**Owner**: Backend Lead
**Normative detail**: [requirements.md](requirements.md)

## Context

Students need accurate, complete activity bundles with capacity, Lecturer/TA,
room, day, and time. Only offerings whose selectable groups satisfy the simple
demo staffing pattern and whose schedules are conflict-free should become
visible for registration.

## User Scenarios and Testing

### User Story 1 - Valid group publish (FR-1, FR-2, FR-3) (P1)

As a Authorized administrator, I need the Valid group publish (FR-1, FR-2, FR-3) behavior so that Offerings, Groups, and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given an offering has one group containing a Lecture with an available
Lecturer and a Tutorial with an available TA, capacity 30, room capacity 35,
and valid times<br>
When Admin validates and publishes<br>
Then the group becomes visible to eligible students with activity type, staff,
room/location, day, and time for both activities.
### User Story 2 - Room overlap (FR-3) (P1)

As a Authorized administrator, I need the Room overlap (FR-3) behavior so that Offerings, Groups, and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given two groups use the same room at overlapping times<br>
When publication is attempted<br>
Then publication is blocked with both groups and overlap interval.
### User Story 3 - Full group (FR-4) (P2)

As a Authorized administrator, I need the Full group (FR-4) behavior so that Offerings, Groups, and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given EnrolledCount equals Capacity<br>
When a student opens group details<br>
Then group is marked Full and cannot be selected.
### User Story 4 - Audited transactional publish/capacity edit (FR-5, FR-6, FR-7) (P2)

As a Authorized administrator, I need the Audited transactional publish/capacity edit (FR-5, FR-6, FR-7) behavior so that Offerings, Groups, and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given an authorized Admin has current versions and a capacity not below active
enrollment<br>
When offering publication or capacity change is committed<br>
Then the complete change is transactional and audited<br>
And stale/invalid capacity is rejected without partial publication.
### User Story 5 - Concurrent room publication (FR-3, FR-7, FR-8) (P3)

As a Authorized administrator, I need the Concurrent room publication (FR-3, FR-7, FR-8) behavior so that Offerings, Groups, and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given two draft groups request the same room and overlapping meeting interval<br>
When separate admins publish them concurrently<br>
Then exactly one group may publish<br>
And the loser receives 409 RESOURCE_CONFLICT after in-transaction revalidation.
### User Story 6 - Capacity reduction races allocation (FR-5, FR-9) (P3)

As a Authorized administrator, I need the Capacity reduction races allocation (FR-5, FR-9) behavior so that Offerings, Groups, and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given one seat remains and an admin attempts to reduce capacity while one
eligible student submits<br>
When both operations contend on the SectionGroup boundary<br>
Then either serialized outcome may win<br>
But Capacity is never below EnrolledCount and EnrolledCount never exceeds
Capacity.
### User Story 7 - Offering read and presentation quality (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Authorized administrator, I need the Offering read and presentation quality (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Offerings, Groups, and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

Given the approved read-load dataset, invalid publication fixtures, two
configured timezones, and a large admin list<br>
When performance, reason-code, time-display, and pagination tests execute<br>
Then offering reads are at most 300 ms p95<br>
And validation returns stable actionable codes<br>
And meeting display uses the term timezone unambiguously<br>
And admin lists remain bounded, paged, and filtered.
### User Story 8 - Child schedule mutation advances group version (FR-8, FR-10) (P3)

As a Authorized administrator, I need the Child schedule mutation advances group version (FR-8, FR-10) behavior so that Offerings, Groups, and Resources produces a verifiable outcome.

**Independent Test**: Execute AC-8 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-8)**

Given a registration has captured SectionGroup rowversion 8<br>
When an admin changes one meeting slot, room, staff assignment, or group state
and commits rowversion 9<br>
Then the registration re-read detects GROUP_CHANGED and cannot enroll against
rowversion 8<br>
And concurrent availability/publication uses one valid staff-term serial order.

### User Story 9 - Staff-owned availability and read-only Admin use (FR-6, FR-8, FR-10) (P3)

As an authorized Admin, I need to view and import staff declarations into
offering planning as read-only inputs so that availability ownership remains
with staff and affected published groups are revalidated.

**Independent Test**: Execute AC-9 with one staff-term aggregate, one affected
published group, the endpoint registry, and an alert persistence double.

**Acceptance Scenario (AC-9)**

Given a staff-owned declaration and an authorized Admin using ADM-07<br>
When the Admin views or imports that declaration into offering planning<br>
Then the declaration remains read-only and no edit or override action is exposed<br>
And an attempted Admin availability mutation has no mapped endpoint and changes nothing<br>
And a staff-owned change affecting a published group creates a durable schedule impact alert.

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

## Requirements

### Functional Requirements

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
- FR-6: Staff assignments/availability and resource changes MUST be
  optimistic-concurrency protected and audited. Staff own and edit their
  declarations through the SPEC-016 workspace contract. Admin MAY view the
  declarations and import them into offering planning as read-only inputs, but
  MUST NOT create, replace, edit, or override availability in this POC; no
  Admin availability mutation endpoint exists.
- FR-7: Publication MUST be transactional.
- FR-8: Publication MUST lock every touched offering, room, and staff resource
  in stable identifier order and revalidate overlaps/availability inside the
  same transaction to prevent concurrent write skew.
- FR-9: Capacity edits and registration seat allocation MUST serialize on the
  same SectionGroup database row/version and preserve
  0 <= EnrolledCount <= Capacity for every outcome.
- FR-10: Every group-state, meeting-slot, room-assignment, and staff-assignment
  mutation MUST lock and advance the owning SectionGroup rowversion. Staff
  availability mutation and group publication share the SPEC-010-owned
  versioned StaffTermAvailability boundary; SPEC-016 consumes it.

### Non-Functional Requirements

- NFR-1: Offering/group reads SHOULD complete within 300 ms p95 at the
  SPEC-018 300-read-requests-per-second target.
- NFR-2: Publication validation MUST produce stable actionable reason codes.
- NFR-3: Meeting display MUST use term timezone and unambiguous day/time.
- NFR-4: Large admin lists MUST be paged/filtered.

### Key Entities

- **CourseOffering**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SectionGroup**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **MeetingSlot**: Feature-owned activity occurrence; canonical type is Lecture, Tutorial, or Laboratory and attributes/relationships are refined in requirements.md and the shared ERD.
- **Room**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **GroupStaffAssignment**: Feature-owned assignment tying staff and teaching role to a group activity/meeting; attributes and relationships are refined in requirements.md and the shared ERD.
- **StaffAvailability**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StaffTermAvailability**: SPEC-010-owned aggregate root for the complete staff-plus-term range set.
- **ScheduleImpactAlert**: SPEC-010-owned durable revalidation state for availability/resource changes affecting published groups.

## Success Criteria

- **SC-1**: Only offerings whose selectable groups satisfy the complete FR-2 activity bundle and conflict checks become visible for registration.
- **SC-2**: Every visible group identifies capacity and, for every Lecture/Tutorial/Laboratory activity, its assigned staff, room/location, day, and time.
- **SC-3**: Capacity can never be configured below active enrollment.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-003](../003-ux-storyboard-accessibility/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-009](../009-catalog-prerequisites-policy-admin/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Frontend Route Ownership

| Route ID | Route template | Future Blazor page | Responsibility |
|---|---|---|---|
| STU-03 | /student/subjects/{offeringId} | SubjectDetailsPage.razor | Feature contract contributor; does not edit page; design SPEC-003, implementation SPEC-011 |
| ADM-06 | /admin/offerings | OfferingAdministrationPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-010 |
| ADM-07 | /admin/resources | ResourceAdministrationPage.razor | Canonical page implementation owner; design SPEC-003, implementation SPEC-010 |

## Out of Scope

- OS-1: Institution-wide timetable generation.
- OS-2: Automatic reassignment of Lecturer, TA, or room for one student.
- OS-3: Waitlist and seat reservation.
- OS-4: Normal admin force-over-capacity action.
