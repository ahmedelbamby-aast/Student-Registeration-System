# Data Model: Academic Term and Student Profile

**Standing approval**: Ahmed ELbamby approved this clarified non-production
demo model on 2026-07-14. Production and later release gates remain separate.

## Canonical Ownership and Consumption

- **AcademicTerm**, **RegistrationWindow**, **Student**, **TranscriptAttempt**, **StudentHold**, and **StudentTermAcademicState** are canonical entities owned by SPEC-008.
- SPEC-006 owns `TermSummaryDto`; SPEC-008 consumes that contract and MUST NOT redefine its lifecycle vocabulary.
- SPEC-006 owns the one shared `RegistrationWindowSummaryDto`; SPEC-008 supplies
  its values to the nullable shared AppContext property and MUST NOT define an
  academic duplicate.
- SPEC-008 logically owns its academic transport DTOs, while their physical C#
  definitions live once under `StudentRegistration.Contracts.Academics` so
  Client and Academics share a dependency-neutral contract without exposing
  domain entities.
- Registration features consume the SPEC-008 student-term version and lock protocol without redefining these entities.

## Detailed Model

| Entity | Key fields |
|---|---|
| AcademicTerm | code, teaching dates, timezone, Draft/RegistrationOpen/RegistrationClosed/Teaching/Completed/Archived lifecycle consumed by the canonical SPEC-006 TermSummaryDto, globally unique CreationClientRequestId, CreationPayloadHash, rowversion |
| RegistrationWindow | term, normalized scope, OpensAtUtc, ClosesAtUtc, Draft/Published/EmergencyClosed/Superseded lifecycle, rowversion; upcoming/open/closed is computed; maximum 20 per term and no Published window overlaps another window in the same term |
| Student | unique ApplicationUserId from SPEC-007; imported ProgramCode/cohort, GPA, credits, standing, active, rowversion |
| TranscriptAttempt | immutable imported CourseCode/term/grade/status/source reference plus normalized optional SupersedesAttemptId; same student/course/term chain, one successor per prior leaf, and no cycle; no FK to downstream SPEC-009 catalogue definitions |
| StudentHold | student, term, type, blocking flag, effective period, source |
| StudentTermAcademicState | unique student + term, rowversion; lock before hold/profile mutation and registration submission |

## Synthetic Fixture Contract

- Each seed-profile version and stable fixture ordinal maps to one synthetic
  SPEC-007 ApplicationUser link and one SPEC-008 Student.
- The fixture populates the complete existing profile fields: ProgramCode,
  cohort, GPA, credits, standing, transcript attempts, active blocking and
  non-blocking holds, StudentTermAcademicState, synthetic provenance, data
  version, and as-of time.
- Rebuilding the same seed version reproduces the same logical values and
  relationship graph. SQL rowversion values and salted identity password-hash
  bytes are intentionally not compared for deterministic equality.
- The fixture contains no production student data and introduces no
  demographic/contact field that is absent from the approved model.
- Seed readiness fails closed if any profile has more than 100 simultaneously
  active holds; a partial hold set is never marked ready.

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
- The Academics schema stores stable imported ProgramCode and CourseCode source
  references so SPEC-008 has no FK to downstream SPEC-009. Catalogue/policy
  validation resolves those codes through SPEC-009 public contracts later.
- Window publication locks the AcademicTerm then every candidate/existing
  window by stable ID, rechecks that no Published windows overlap anywhere in
  the term regardless of scope, and advances affected versions atomically.
- A service-ready context resolves exactly one RegistrationOpen term and at
  most one Teaching term. Zero RegistrationOpen terms is an authoritative
  absence; multiple candidate terms are invalid and produce no partial context.
  Transitions lock terms in stable ID order and reject a second
  RegistrationOpen or Teaching term.
- Window containment is the half-open UTC interval
  `OpensAtUtc <= serverNowUtc < ClosesAtUtc`. Selection is open first, otherwise
  earliest upcoming by `(OpensAtUtc, Id)`, otherwise latest closed by
  `(ClosesAtUtc DESC, Id)`. Published interval/scope values are immutable;
  publish, emergency-close, and supersede are explicit lifecycle operations.
- Each term has at most 20 windows. A term command's window collection and
  expected-window-version map have the same limit and matching existing IDs.
- A profile/hold mutation locks `StudentTermAcademicState` before changing
  academic inputs; every correction supplies the exact TermId and expected
  student-term version. SPEC-014 registration consumes the same upstream
  boundary.
- Transcript attempts are append-only. A correction creates a new attempt with
  `SupersedesAttemptId` pointing to the prior record; it never updates the
  prior academic-history row. The reference must identify the current leaf for
  the same student/course/term, cannot be self-referential, and has a unique
  successor. The service rejects a cycle or fork, and transcript summaries
  count only current leaves.
- Transcript-attempt and provenance read projections use independently bounded
  canonical pages (default 20, maximum 100). Active holds are returned as one
  complete set with a maximum of 100; a larger set makes the profile not ready.
- All SPEC-008 instants persist as UTC datetime2 and AcademicTerm validates one
  IANA timezone identifier. Recurring meeting day/time persistence remains
  SPEC-010-owned.
- Seed contribution occurs only after migrations, is idempotent for the same
  profile version, and fails the database-readiness check if any required
  academic value or synthetic provenance is missing.
- `AcademicTerm.CreationClientRequestId` is globally unique and nonblank;
  `CreationPayloadHash` is the server-canonical create payload hash. Creation,
  its initial windows, replay metadata, and audit commit atomically. A matching
  retry replays the same aggregate; a different hash is rejected. These fields
  introduce no seventh entity or generic idempotency table.
- Publication and profile correction use required expected rowversions plus one
  atomic SQL transaction. Cancellation before commit leaves no mutation/audit;
  response loss after commit is resolved through stale-version refetch.
- Profile correction accepts 1..20 typed operations. Reason is 10..500 trimmed
  characters; source/source-reference are nonblank and at most 200. Hold Code
  and Message are trimmed normalized values. Feature error details are bounded
  to 20 keys, 5 messages per key, and 256 characters per message.
