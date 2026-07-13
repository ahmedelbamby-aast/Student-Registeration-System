# Capacity and Temporal Check Constraints

**Contract version:** `check-constraints/1.0`<br>
**Requirement:** FR-3<br>
**Bounded delivery:** design-time contract only<br>
**Runtime source dependency:** None<br>
**Fail-closed boundary:** unapproved production behavior remains blocked

These rules distinguish row-local database checks from transactional
cross-row validation. They do not claim that a database constraint or runtime
command already exists.

## Row-local database checks

- `SectionGroup.Capacity >= 0`.
- `0 <= SectionGroup.EnrolledCount <= SectionGroup.Capacity`.
- `MeetingSlot.EndLocal > MeetingSlot.StartLocal`.
- `StaffAvailability.EndLocal > StaffAvailability.StartLocal`.
- `AcademicTerm.TeachingEnds > AcademicTerm.TeachingStarts`.
- `RegistrationWindow.ClosesUtc > RegistrationWindow.OpensUtc`.

These rules mirror the ERD rule: Check Capacity >= 0 and 0 <= EnrolledCount <=
Capacity, plus Check EndLocal > StartLocal and registration/term end > start.
Invalid capacity or temporal values are rejected, never clamped or
auto-corrected. Reducing capacity below EnrolledCount is rejected.

## Transactional predicates

`RegistrationPaused = false` is an atomic allocation predicate, not a check
constraint. The full ERD predicate is
`SectionGroup.RegistrationPaused = false`; allocation must test it in the same
transaction as the contested seat update.

SQL constraints cannot express arbitrary overlapping time ranges. Scheduling
publication validates them transactionally and only publishes the fully valid
state. Owner-specification tests and future real-SQL evidence must demonstrate
both the row-local constraints and these transactional application checks.
