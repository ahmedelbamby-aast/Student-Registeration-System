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
- `AcademicTerm.TeachingEndsOn > AcademicTerm.TeachingStartsOn`.
- `RegistrationWindow.ClosesAtUtc > RegistrationWindow.OpensAtUtc`.
- `StudentHold.EffectiveToUtc IS NULL OR StudentHold.EffectiveToUtc > StudentHold.EffectiveFromUtc`.
- `RegistrationWindow.ScopeType` is `all-students`, `program`, or `cohort`;
  `ScopeValue` is null only for `all-students` and required otherwise.

These rules mirror the ERD rule: Check Capacity >= 0 and 0 <= EnrolledCount <=
Capacity, plus Check EndLocal > StartLocal and valid registration-window,
term, and hold ranges. Invalid capacity, temporal, or normalized scope values
are rejected, never clamped or auto-corrected. Reducing capacity below
EnrolledCount is rejected.

## Transactional predicates

`RegistrationPaused = false` is an atomic allocation predicate, not a check
constraint. The full ERD predicate is
`SectionGroup.RegistrationPaused = false`; allocation must test it in the same
transaction as the contested seat update.

SQL constraints cannot express arbitrary overlapping time ranges. Academics
validates RegistrationWindow overlaps within the affected term under stable
term/window locks before publication. Scheduling separately validates
meeting, staff, and room overlaps before publishing a fully valid offering.
Owner-specification tests and future real-SQL evidence must demonstrate both
the row-local constraints and these owner-specific transactional checks.
