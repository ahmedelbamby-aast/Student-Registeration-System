# SPEC-016 Scope Review

**Reviewed:** 2026-07-17
**Result:** PASS for the bounded non-production demo

The review covered StaffAdministration contracts, domain/application sources,
SQL adapters, endpoint mappings, client routes, migrations, and focused tests.
SPEC-016 adds no migration and reuses the canonical Scheduling, Identity,
Academic, and Registration entities.

## OS-1: grade entry, attendance, and messaging

PASS. The five staff APIs and STF-01 through STF-04 contain no grade,
attendance, notification, chat, email, or messaging command, DTO, control, or
route. Roster rows expose only UniversityId, DisplayName, and EnrollmentState.

## OS-2: capacity, policy, and term administration

PASS. Staff may read group capacity and server-current term context, but no
staff capacity, policy, user, offering, or term mutation route exists.
`Spec016Endpoints` maps only assignments, timetable, roster, and own
availability GET/PUT routes. Capacity shown in a group card is read-only.

## OS-3: unrelated groups and students

PASS. Staff identity and active Lecturer/TeachingAssistant context are derived
from authenticated claims. The SQL roster predicate requires the active Staff
row, teaching role, GroupStaffAssignment, and requested GroupId before any
Enrollment, Student, or ApplicationUser row is composed. Missing, removed, or
unrelated assignments return privacy-safe `STAFF_GROUP_NOT_FOUND` with no page.

## OS-4: automatic movement and Admin availability correction

PASS. Availability replacement changes only the canonical complete range set,
audit event, and required durable ScheduleImpactAlert in one transaction. It
does not modify CourseOffering, SectionGroup, MeetingSlot, Room, or
GroupStaffAssignment. No Admin availability PUT/POST/PATCH/DELETE route,
permission, editable control, correction notification, or correction-audit
workflow exists.

## Demo data-label note

The current approved identity schema does not persist a separate human display
name for Student accounts. The bounded roster adapter therefore projects the
persisted `ApplicationUser.UserName` as `DisplayName`; in synthetic seeds this
may equal UniversityId. This preserves the exact three-field privacy boundary
without adding a duplicate PII column or cross-spec migration. A future
production identity baseline may add an institution-approved display label.
