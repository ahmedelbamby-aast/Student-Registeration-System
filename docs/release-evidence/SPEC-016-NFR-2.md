# SPEC-016 NFR-2 Evidence

Every direct staff object route is protected by the `Context.Read` policy and
projects through `StaffWorkspaceQueries`, which derives the authenticated
application-user identity and active teaching role on the server. Roster
authorization is part of the SQL reader predicate before enrollment/student
rows are composed; a missing or removed assignment returns the privacy-safe
`STAFF_GROUP_NOT_FOUND` outcome.

**Automated evidence:** `NFR-2EvidenceTests` plus the 7-case
`StaffWorkspaceScopeTests` authorization suite passed.

No client role claim, staff ID, group scope, or broad roster search is trusted.
