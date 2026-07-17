# SPEC-015 Implementation Readiness

**Reviewed:** 2026-07-17  
**Baseline state:** FROZEN AND APPROVED FOR DEPENDENCY-ORDERED NON-PRODUCTION DEMO WORK  
**Gate A approval:** Ahmed Elbamby, 2026-07-13; dependency baseline revalidated 2026-07-17  
**Result:** PASS

- [x] Constitution compliance is recorded against version 1.1.0, preserves the
  simple modular monolith, and requests no exception.
- [x] SPEC-003, SPEC-008, SPEC-014, and SPEC-018 accepted statuses, exact
  artifact SHA-256 fingerprints, consumed contracts, and deferred boundaries
  are recorded in `dependency-baseline.md`.
- [x] Every direct dependency link resolves and the graph
  `003/008/014/018 -> 015` is acyclic.
- [x] SPEC-014 remains the sole owner/writer of RegistrationSubmission,
  Enrollment, DecisionSnapshot, Reference, ReceiptSnapshot, the S6Registration
  migration, and the atomic transaction. SPEC-015 owns read projections, query
  services, five read-only endpoints, and STU-06/STU-07 only.
- [x] Accepted submissions project the one canonical reference/receipt
  snapshot; rejected submissions project the replayable rejection with
  `noPartialRegistration = true` and never fabricate an accepted receipt.
- [x] Student reads require the exact `RegistrationRecords.ReadOwn` permission
  and authenticated self scope. Admin reads require the exact
  `RegistrationRecords.Read` permission plus explicit StudentId and TermId
  scope and audited inspection. Lecturer/TA have no general registration-record
  endpoint.
- [x] Another student's submission identifier returns privacy-safe 404 after
  authorization and before resource/snapshot disclosure. AC-3 and T026 use
  that exact outcome and include the authorized Admin/audit half.
- [x] Lists use canonical page 1/size 20, maximum 100, optional TermId only for
  the student history list, and stable `SubmittedAtUtc DESC, SubmissionId`
  ordering.
- [x] Receipt/history/current-timetable DTOs remain bounded read models. The
  exact current-timetable success/error matrix is completed by T021 before its
  contract, behavior, route, or handler work proceeds.
- [x] Calendar and chronological list/table remain equivalent; STU-06/STU-07
  inherit the approved responsive, focus, keyboard, WCAG 2.2 AA, browser,
  visual, and manual assistive-technology obligations.
- [x] STU-06/STU-07 route work remains blocked until finalized SPEC-014 and
  SPEC-015 contributor hashes are recorded in the downstream route pin.
- [x] Historical receipt and decision snapshots preserve original term,
  course/group, credits, meeting-bound Lecturer/TA, room/location, day/time,
  PolicySet/version, and server-time meaning after later catalogue edits.
- [x] Demo retention uses DEC-07 and synthetic-only data. Production/real-data
  retention remains unapproved and fail closed.
- [x] Drop, withdrawal, correction, seat decrement, email/SMS receipt,
  public/shareable receipt, transcript replacement, a second receipt table, a
  reporting database, a queue, and distributed infrastructure remain absent.
- [x] Every later implementation task retains its documented test-first
  predecessor and may be checked only after the named artifact plus expected-red
  or passing evidence exists.

## Trace review

- FR-1 through FR-8 map to T034-T049 through bounded test/delivery pairs;
  canonical persistence ownership is additionally guarded by T008-T014.
- NFR-1 through NFR-4 map to T029 and T055-T058.
- AC-1 through AC-6 map one-to-one to T024-T029; corrected T026 proves both
  privacy-safe student denial and authorized audited Admin inspection.
- EC-1 through EC-4 map one-to-one to T030-T033.
- Student/Admin endpoints map to T015-T023 and the single handler task T054.
- STU-06 and STU-07 map to T050-T053 without transferring design ownership
  from SPEC-003 or registration-write ownership from SPEC-014.
- Scope, complete traceability, and final approvals map to T059-T064.

## Automated validation evidence

- `.specify/scripts/powershell/Test-AllSpecs.ps1 -Phase Implementation` reports
  SPEC-003, SPEC-008, SPEC-014, SPEC-015, and SPEC-018 individually at
  requirements score 100 with automated gates PASS and human approval APPROVED.
- The same repository-wide run reports `overall: FAIL` for existing SPEC-009
  through SPEC-013 declaration/handler/source-writer findings. No failure is
  attributed to SPEC-015 or one of its four direct dependency packages, so
  this checklist does not misstate the repository-wide run as a global pass or
  authorize work outside SPEC-015.
- SPEC-014 tasks are 123/123 checked and its current implementation-readiness
  result is PASS.
- The solution builds with 0 warnings and 0 errors, specification tests pass
  106/106, architecture tests pass 23/23, and the focused shared-boundary suites
  pass after the narrow port correction.
- SPEC-015 began with 64/64 tasks unchecked. T001-T007 may be checked only after
  their named planning evidence exists.

## Deferred gates

- T015, T018, and T021 must freeze the final endpoint contracts before their
  linked contract/behavior tests and T054 handler.
- T050-T053 must wait for the downstream exact contributor pin; a `design-only`
  or `not-pinned` dependency record is not route implementation authority.
- The SPEC-014 load artifact fingerprint must be refreshed from an actual
  harness run before final repository-wide quality evidence is claimed.
- Gate B-D, production deployment, official AASTMT authorization, real-data
  retention, and release sign-off remain separate and fail closed.

## Approval currency

Ahmed Elbamby's current 2026-07-17 instruction explicitly revalidates the
non-production demo baseline after the accepted SPEC-008 2026-07-14
clarification and SPEC-014 2026-07-17 reconciliation. The approval and
clarification records contain that revalidation. Any later accepted
dependency-hash, route-owner, authorization, retention, receipt/snapshot, or
scope change returns the affected work to In Review.

Dependency-ordered SPEC-015 work may start at T008 only after T001-T007 are
checked from their named evidence. No later task is complete merely because
this readiness gate passes.
