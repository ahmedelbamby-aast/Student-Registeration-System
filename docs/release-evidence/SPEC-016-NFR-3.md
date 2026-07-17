# SPEC-016 NFR-3 Evidence

`RosterRowDto` has exactly three serialized fields: `UniversityId`,
`DisplayName`, and `EnrollmentState`. The SQL projection filters active
enrollments, selects only those fields, and applies stable display-name then
University-ID ordering. GPA, standing, holds, contact data, grades, transcript,
password, and unrelated identifiers are never projected.

Successful and denied access writes contain only actor, group, purpose,
outcome, row count, correlation ID, and server time. Audit metadata never
contains roster rows or student values.

**Automated evidence:** `NFR-3EvidenceTests`, the exact-field model tests,
and the 12-case SPEC-016 integration suite passed.
