# Feature Specification: Lecturer and Teaching Assistant Workspace

**Feature Branch**: 016-lecturer-ta-workspace
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Product Owner
**Normative detail**: [requirements.md](requirements.md)

## Context

Lecturers and TAs share staff components but have different assignment scopes.
They need their timetable, partner staff, authorized group rosters, and
availability without access to policy, user, or unrelated student data.

## User Scenarios and Testing

### User Story 1 - Scoped roster (FR-2, FR-5) (P1)

As a Lecturer or Teaching Assistant, I need the Scoped roster (FR-2, FR-5) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given a TA is assigned to Group A but not Group B<br>
When the TA requests Group A and Group B rosters<br>
Then Group A is returned with approved minimal fields<br>
And Group B is denied with no data.
### User Story 2 - Shared page, different scope (FR-1, FR-3) (P1)

As a Lecturer or Teaching Assistant, I need the Shared page, different scope (FR-1, FR-3) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given Lecturer and TA users open the same assignments route<br>
When server responses are rendered<br>
Then each sees only role-appropriate current assignments<br>
And the role context is stated near the page heading.
### User Story 3 - Availability deadline (FR-6) (P2)

As a Lecturer or Teaching Assistant, I need the Availability deadline (FR-6) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given the availability deadline has passed<br>
When staff attempts an update<br>
Then the command is rejected with deadline/server time<br>
And existing availability remains unchanged.
### User Story 4 - Staff detail and privilege boundary (FR-4, FR-7) (P2)

As a Lecturer or Teaching Assistant, I need the Staff detail and privilege boundary (FR-4, FR-7) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given staff opens an assigned group and attempts an Admin capacity route<br>
When both requests are authorized<br>
Then assigned subject/staff/room/time/capacity details are returned<br>
And the Admin operation is denied.
### User Story 5 - Post-publication availability warning (FR-8) (P3)

As a Lecturer or Teaching Assistant, I need the Post-publication availability warning (FR-8) behavior so that Lecturer and Teaching Assistant Workspace produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given an approved availability change conflicts with a published assignment<br>
When the change is saved through the allowed process<br>
Then Admin receives an affected-group warning<br>
And no class, room or staff assignment moves automatically.

## Edge Cases

- EC-1: Staff has both Lecturer and TA assignments -> display authorized
  contexts without duplicate group entries.
- EC-2: Assignment removed while page open -> stale refresh denies roster.
- EC-3: Concurrent availability edit -> stale version gets 409.
- EC-4: No assignments -> clear empty state, no broad search access.

## Requirements

### Functional Requirements

- FR-1: Lecturer and TA MUST use the shared staff login and shared workspace
  templates.
- FR-2: The API MUST scope assignments/timetable/rosters to the authenticated
  staff user's current assignments.
- FR-3: Lecturer MUST see assigned lecture groups; TA MUST see assigned
  tutorial/lab groups according to server data.
- FR-4: Staff MUST view group code, subject, role partners, room, meeting
  slots, capacity and roster count.
- FR-5: Staff MAY view the minimum authorized roster fields for assigned
  groups.
- FR-6: Staff MUST create/edit own availability before deadline using
  concurrency protection.
- FR-7: Staff MUST NOT manage policy, users, capacity, terms, or unrelated
  rosters.
- FR-8: Availability changes after schedule publication MUST trigger an admin
  warning and MUST NOT silently move a class.

### Key Entities

- **StaffAssignment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **StaffAvailability**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RosterRow**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **GroupSummary**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Staff can view only assignments and roster data within their current scope.
- **SC-2**: Lecturer and TA contexts use shared journeys without merging their permissions.
- **SC-3**: Availability changes never silently move a published class.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-007](../007-identity-account-lifecycle/spec.md)
- [SPEC-010](../010-offerings-groups-resources/spec.md)
- [SPEC-015](../015-student-registration-records/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Out of Scope

- OS-1: Grade entry, attendance entry, or messaging.
- OS-2: Staff capacity/policy/term administration.
- OS-3: Access to unrelated groups/students.
- OS-4: Staff-driven automatic room/time changes.
