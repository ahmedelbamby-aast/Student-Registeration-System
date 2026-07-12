# Feature Specification: Student Registration Records

**Feature Branch**: 015-student-registration-records
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Product Owner
**Normative detail**: [requirements.md](requirements.md)

## Context

After an atomic result, students and authorized staff need a reliable receipt,
current timetable, historical terms, and the exact policy decision used.
Rejected submissions must clearly state that no partial registration occurred.

## User Scenarios and Testing

### User Story 1 - Accepted receipt (FR-1, FR-2) (P1)

As a Student, I need the Accepted receipt (FR-1, FR-2) behavior so that Student Registration Records produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given a registration commits successfully<br>
When the result page loads<br>
Then it shows a unique reference and every registered group with staff,
location, day/time, credits, policy version, and server timestamp.
### User Story 2 - Atomic rejection (FR-3) (P1)

As a Student, I need the Atomic rejection (FR-3) behavior so that Student Registration Records produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given one selected group became full and transaction rolled back<br>
When the rejection is shown<br>
Then GROUP_FULL and resolution action are displayed<br>
And the message explicitly says no subjects were partially registered.
### User Story 3 - Ownership (FR-5, NFR-2) (P2)

As a Student, I need the Ownership (FR-5, NFR-2) behavior so that Student Registration Records produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given Student A knows Student B's submission identifier<br>
When Student A requests it<br>
Then the API returns 403/404 according to security policy<br>
And no Student B data is returned.
### User Story 4 - Historical snapshot (FR-7) (P2)

As a Student, I need the Historical snapshot (FR-7) behavior so that Student Registration Records produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given a room/group display name changes after registration<br>
When the original receipt is inspected<br>
Then the stored historical snapshot remains available with original details.
### User Story 5 - Current/history views without unapproved actions (FR-4, FR-6, FR-8) (P3)

As a Student, I need the Current/history views without unapproved actions (FR-4, FR-6, FR-8) behavior so that Student Registration Records produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given a student has current and historical registrations<br>
When both timetable views are opened<br>
Then calendar and accessible list/table show equivalent authorized records<br>
And no drop/correction action appears before its workflow is approved.

## Edge Cases

- EC-1: Result response lost -> idempotent lookup returns receipt.
- EC-2: Receipt render service error -> safe retry by reference.
- EC-3: No registrations -> meaningful empty state with eligible next action.
- EC-4: Historical term archived -> remains read-only and accessible.

## Requirements

### Functional Requirements

- FR-1: Successful submission MUST produce a unique receipt/reference.
- FR-2: Receipt MUST include term, course/group, credits, Lecturer/TA, room,
  day/time, policy version, and submission time.
- FR-3: A rejected atomic submission MUST state the reason and that no partial
  enrollment was created.
- FR-4: Students MUST view their current registrations/timetable and historical
  terms.
- FR-5: Authorized staff/admin MAY inspect records within server-enforced scope.
- FR-6: Calendar and printable table/list MUST present equivalent schedule data.
- FR-7: Decision snapshots and historical group details MUST retain their
  original meaning after later edits.
- FR-8: Drop/correction actions MUST be absent until approved policy/workflow
  is specified.

### Key Entities

- **RegistrationReceipt**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationSubmission**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Enrollment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **DecisionSnapshot**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Every accepted registration has a durable, uniquely identifiable receipt.
- **SC-2**: Rejected atomic submissions clearly state that no partial registration occurred.
- **SC-3**: Current and historical records remain understandable after later catalogue changes.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-014](../014-registration-capacity-concurrency/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Out of Scope

- OS-1: Drop/withdraw/correction workflow until AASTMT approves SPEC changes.
- OS-2: Email/SMS receipt.
- OS-3: Public/shareable receipt link.
- OS-4: Transcript replacement.
