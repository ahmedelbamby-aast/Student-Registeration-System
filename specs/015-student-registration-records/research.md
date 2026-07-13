# Research: Student Registration Records

## Decisions

### Receipt persistence

**Decision**: Do not create a separate RegistrationReceipt table.
SPEC-014's accepted RegistrationSubmission atomically stores the unique
Reference and immutable ReceiptSnapshot with enrollments, decision snapshot,
audit, and final result. SPEC-015 owns read projections only.

**Rationale**: This removes a transaction/dependency cycle and guarantees lost
responses/idempotent replays return the same receipt without duplicate writes.

### Read scope

**Decision**: Student routes are authenticated-self; history spans terms and accepts an optional TermId filter, while current timetable resolves the server's active term. Admin
inspection uses explicit student+term list/detail endpoints guarded by
`RegistrationRecords.Read` and audited. Lecturer/TA remain limited to the
minimal roster projection in SPEC-016.

**Rationale**: Explicit resource scope is easier to authorize and test than a
generic “staff may inspect” rule.

### Pagination and history

**Decision**: Page number defaults to 1, page size to 20/max 100, with stable
SubmittedAtUtc-desc/SubmissionId ordering. Snapshots are immutable and remain
readable for the SPEC-005 retention period.

### UI and excluded actions

**Decision**: STU-06 and STU-07 render equivalent calendar/list or print
representations. Drop, withdrawal, and correction controls/endpoints are
absent until a separate approved policy/workflow spec exists.

## Open Research

Institutional retention values remain governed by SPEC-005 and must fail closed
if not approved; no value is invented here.
