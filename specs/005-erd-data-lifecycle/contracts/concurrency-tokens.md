# Concurrency Token Catalogue

**Contract version:** `concurrency-tokens/1.1`<br>
**Requirement:** FR-4<br>
**Bounded delivery:** design-time contract only<br>
**Runtime source dependency:** None<br>
**Fail-closed boundary:** unapproved production behavior remains blocked

Every entity below contains a rowversion field in the approved ERD. Canonical
owner specifications implement the token and Infrastructure.SqlServer later
composes and verifies it; this catalogue creates no runtime mapping.

## Complete ERD rowversion inventory

- APPLICATION_USER: rowversion
- ROLE_ASSIGNMENT: rowversion
- STUDENT_ACTIVATION: rowversion
- ACCOUNT_RECOVERY_CHALLENGE: rowversion
- AUTHENTICATION_ABUSE_STATE: rowversion
- IDENTITY_IMPORT_BATCH: rowversion
- STUDENT: rowversion
- CATALOGUE_DRAFT: rowversion
- CATALOGUE_VERSION: rowversion
- STUDENT_TERM_ACADEMIC_STATE: rowversion
- ACADEMIC_TERM: rowversion
- REGISTRATION_WINDOW: rowversion
- POLICY_SET: rowversion
- COURSE_OFFERING: rowversion
- SECTION_GROUP: rowversion
- ROOM: rowversion
- STAFF_TERM_AVAILABILITY: rowversion
- SCHEDULE_IMPACT_ALERT: rowversion
- REGISTRATION_PLAN: rowversion
- STUDENT_TERM_REGISTRATION_GUARD: rowversion
- ENROLLMENT: rowversion
- EXPORT_JOB: rowversion
- ADMIN_SECURITY_GUARD: rowversion

## Update and conflict behavior

The common rule is rowversion on mutable aggregate roots and admin records. A
stale expected token produces `409 STALE_VERSION` with the current version and
no lost update; it never silently overwrites the newer aggregate.

- Every group-state, MeetingSlot, room, and GroupStaffAssignment mutation locks
  and advances its owning SectionGroup.Version.
- Every availability range replacement locks and advances the owning
  StaffTermAvailability.Version.
- ExportJob uses compare-and-set rowversion plus an expiring lease so only one
  worker owns an export attempt and an expired lease can be recovered safely.
- Final-Admin mutation locks AdminSecurityGuard before recounting enabled
  Admins and committing the protected role change.

rowversion is not the capacity allocator; the database-checked seat predicate
and transaction provide allocation correctness. The unique
`StudentTermAcademicState` row plus the SPEC-008
`ExecuteRegistrationBoundaryAsync` transaction protocol provide student-term
serialization; SPEC-014 does not add a second guard. Future owner-mapping and
real-SQL tests must prove token configuration, stale-write mapping, and
aggregate advancement.
