# Data Model: Academic Term and Student Profile

## Canonical Ownership and Consumption

- **AcademicTerm**, **RegistrationWindow**, **Student**, **TranscriptAttempt**, **StudentHold**, and **StudentTermAcademicState** are canonical entities owned by SPEC-008.
- SPEC-006 owns `TermSummaryDto`; SPEC-008 consumes that contract and MUST NOT redefine its lifecycle vocabulary.
- Registration features consume the SPEC-008 student-term version and lock protocol without redefining these entities.

## Detailed Model

| Entity | Key fields |
|---|---|
| AcademicTerm | code, teaching dates, timezone, Draft/RegistrationOpen/RegistrationClosed/Teaching/Completed/Archived lifecycle consumed by the canonical SPEC-006 TermSummaryDto, rowversion |
| RegistrationWindow | term, normalized scope, OpensUtc, ClosesUtc, Draft/Published/EmergencyClosed/Superseded lifecycle, rowversion; upcoming/open/closed is computed |
| Student | unique ApplicationUserId from SPEC-007; imported ProgramCode/cohort, GPA, credits, standing, active, rowversion |
| TranscriptAttempt | imported CourseCode/term/grade/status/source reference; no FK to downstream SPEC-009 catalogue definitions |
| StudentHold | type, blocking flag, effective period, source |
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

## Integrity Rules

- Foreign keys and unique constraints enforce durable identity and relationship rules.
- Concurrency-sensitive aggregates use database-checked versioning or atomic conditional writes.
- Audit timestamps use server time; academic activity references an explicit academic term.
- Deletion and retention behavior follow the project data-lifecycle specification.
- The Academics schema stores stable imported ProgramCode and CourseCode source
  references so SPEC-008 has no FK to downstream SPEC-009. Catalogue/policy
  validation resolves those codes through SPEC-009 public contracts later.
- Window publication locks the AcademicTerm then candidate/existing windows by
  stable ID, rechecks overlap, and advances affected versions atomically.
- A profile/hold mutation locks `StudentTermAcademicState` before changing
  academic inputs; SPEC-014 registration consumes the same upstream boundary.
- Seed contribution occurs only after migrations, is idempotent for the same
  profile version, and fails the database-readiness check if any required
  academic value or synthetic provenance is missing.
