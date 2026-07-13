# Data Model: Lecturer and Teaching Assistant Workspace

## Owned Models

- **StaffAssignmentDto**: Authorized workspace projection.
- **RosterRowDto**: Minimal assigned-group roster projection.

## Consumed Canonical Models

- **GroupStaffAssignment**, **StaffTermAvailability**, and
  **StaffAvailability** are owned by SPEC-010 in
  `StudentRegistration.Scheduling`.
- **ScheduleImpactAlert** is owned by SPEC-010/Scheduling; SPEC-016 receives
  its identifier/state through the Scheduling application port.
- SPEC-016 edits availability only through the Scheduling application port; it
  does not map or persist a duplicate aggregate.
- Admin may consume a bounded read-only projection and copy declared ranges
  into offering-planning input; no Admin mutation model, permission, editable
  state, notification, or correction-audit model is introduced.

## Detailed Model

| Model/field | Type | Constraints |
|---|---|---|
| StaffTermAvailability | consumed aggregate root | unique StaffId + TermId; deadline; rowversion; complete range set |
| StaffAvailability | consumed child value | parent aggregate; day/start/end/type; no independent mutation/version |
| StaffAssignmentDto | projection | current authorized group, role, partners, room, meetings, capacity, roster count |
| RosterRowDto | projection | UniversityId, DisplayName, EnrollmentState only |
| Page<RosterRowDto> | projection | default 20, max 100, total; DisplayName then UniversityId |
| ScheduleImpactAlert | consumed durable entity | SPEC-010-owned affected group/resource versions, reason, detected time and revalidation state |
| ScheduleImpactAlert.State | enum | Open, Revalidated, Resolved; consumed unchanged from SPEC-010 |

## Integrity Rules

- Complete-range replacement locks/advances SPEC-010's aggregate rowversion;
  child mutation cannot bypass it.
- Deadline, current assignments, publication versions, and alert need are
  re-read with server time inside the Scheduling transaction.
- A conflicting accepted update and its Open alert commit together. Unique
  active alert by (StaffId, GroupId, AvailabilityVersion, GroupVersion) makes
  retry idempotent.
- Alert revalidation/resolution uses expected rowversion and is audited; the
  workspace MUST NOT invent an acknowledgement state or transition.
- ScheduleImpactAlert remains distinct from an availability-correction
  notification or correction-audit workflow.
- Roster authorization is checked before query; audit metadata records actor,
  group, purpose, outcome, row count, and correlation ID without roster rows.
