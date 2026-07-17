# SPEC-015 NFR-2 Authorization Evidence

**Recorded:** 2026-07-17
**Requirement:** record access has ownership and role-scope tests
**Result:** PASS

The focused `NFR_2EvidenceTests` run passed against the actual five-endpoint
authorization pipeline and real SQL adapter. It proves:

- a student using `RegistrationRecords.ReadOwn` receives privacy-safe 404 for
  another student's known submission identifier and no reference is leaked;
- an Admin using the exact `RegistrationRecords.Read` permission can inspect
  the explicitly scoped student/term record;
- the successful Admin inspection creates exactly one minimized audit event;
- Lecturer, Teaching Assistant, Admin without the exact permission, and
  Student without the exact permission receive 403.

The focused SPEC-015 quality run passed all six NFR tests, including this
end-to-end authorization test.
