# Clarification Record: Academic Term and Student Profile

**Reviewed**: 2026-07-14
**Automated result**: PASS - no hidden NEEDS CLARIFICATION marker.
**Human approval**: APPROVED by Ahmed ELbamby on 2026-07-14 for the clarified
Gate A non-production demo contract

## Resolved demo profile decision

Ahmed approved synthetic Development and Testing students populated with the
complete existing academic-profile field set. The logical profile is
deterministic by seed version/fixture ordinal, contains synthetic provenance,
and excludes production data and unapproved demographic/contact fields.

The specification was reviewed for scope, actors, data, business rules, errors,
concurrency, security, accessibility, dependencies, and measurable outcomes.
Unknown production-only product/institutional decisions remain registered in
docs/OPEN_DECISIONS.md or docs/POLICY_RESEARCH.md with an owner and fail-closed
rule. They are not invented requirements and do not extend this demo approval
to production data integration, official AASTMT go-live, or later release
gates.

## Resolved 2026-07-14 implementation-boundary decisions

- The shared SPEC-006 `AppContextDto` gains one nullable canonical nested
  `RegistrationWindowSummaryDto` containing ID, computed state, open/close UTC
  instants, and rowversion. SPEC-008 supplies values and defines no duplicate
  Academic AppContext DTO. Service state remains exactly available,
  maintenance, or unavailable.
- Transcript attempts and provenance use independent canonical pages with
  default 20 and maximum 100. Every active hold is returned together, with a
  maximum of 100; exceeding the cap makes the profile not ready and fails
  closed instead of truncating holds.
- Every profile correction includes `termId`. Transcript corrections append a
  new immutable attempt linked to the superseded attempt; prior history is
  never overwritten.
- Publication conservatively rejects any overlap between Published windows in
  the same term, regardless of scope.
- SPEC-008 proves its student-term lock/version protocol with a registration
  test consumer and a no-commit callback. Actual seat/enrollment conformance
  remains owned by dependent SPEC-014.
- Dashboard context must sustain a continuous 10-minute, two-replica load of
  300 reads per second with p95 at or below 300 ms and fewer than 0.1%
  unexpected failures.
- SPEC-008 persists instants in UTC datetime2 and validates the term's IANA
  timezone. Recurring meeting persistence remains SPEC-010-owned.
- Canonical `ApiError.currentVersion` represents the directly contested
  aggregate and bounded `fieldErrors` represent validation. Multi-version or
  overlap conflicts require a bounded aggregate refetch before retry; no
  feature-specific error extension is introduced.
- NFR-4 consistently requires self/approved-staff scope, wholly synthetic
  non-production fixtures, and no full student profile in logs, traces,
  snapshots, or test reports.
- A window is open on the half-open interval
  `OpensAtUtc <= serverNowUtc < ClosesAtUtc`. Available context admits one
  RegistrationOpen term and at most one Teaching term; ambiguity returns
  CONTEXT_UNAVAILABLE. Window selection is open, otherwise earliest upcoming,
  otherwise latest closed, with UTC/ID tie-breaks.
- Term/window commands carry explicit lifecycle. Draft-to-Published belongs to
  the publish endpoint, Published scope/interval is immutable, and governed
  emergency-close/supersede transitions remain available.
- Term creation alone uses payload-bound idempotency through globally unique
  AcademicTerm CreationClientRequestId/CreationPayloadHash. Publication and
  profile correction use required expected rowversions and an atomic
  transaction; no seventh entity or generic idempotency table is added.
- A term has at most 20 windows/version entries and a correction has at most 20
  typed operations. Reason is 10..500 characters, source/reference at most 200,
  search 3..50, and feature field errors at most 20 keys, 5 messages/key, and
  256 characters/message.
- Admin student location requires TermId plus a nonblank bounded query and
  returns only minimal fields. Named detail/correction binds StudentId+TermId
  and may include hold IDs; Student self responses omit them. Lecturer and TA
  are denied academic-profile access.
- Transcript supersession is normalized, same student/course/term, current-leaf
  only, unique-successor, and acyclic; current summaries count only leaves.
- All ten endpoints must record complete request, success, validation,
  authorization, conflict, unavailable, and unexpected-error behavior before
  implementation tests begin.
- The feature load is exactly 300 authenticated GET /api/context reads/second
  for ten minutes across two independently addressable stateless replicas
  sharing SQL and the approved 25,000-account fixture, with p95 <= 300 ms and
  unexpected failures < 0.1%.
- Implementation must reconcile the class diagram, shared AppContext/window
  source, executable permission claims/policies, nullable frontend shell, and
  Testing bootstrapper through explicit test-first delivery paths.

Ahmed ELbamby's standing approval applies to these simple POC decisions and
does not authorize production data, Gate B-D, release, or official AASTMT
go-live.
