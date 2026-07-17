# SPEC-005: ERD and Data Lifecycle

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** Approved (Gate A demo implementation, 2026-07-13)<br>
**Owner:** Data/Backend Lead<br>
**Reviewers:** Architect, Registrar, QA, Security<br>
**Target:** Sprint 0-S1<br>
**Dependencies:** SPEC-002, SPEC-004<br>

## Context

Registration correctness depends on relational constraints, immutable academic
history, effective-dated policies, and short atomic transactions. The complete
proposed model is in docs/diagrams/ERD.md.

## Functional Requirements

- FR-1: EF Core Code First migrations MUST define the approved ERD. The demo
  MUST run SQL Server 2022 Developer at compatibility level 160, provisioned
  through Docker for Development and Testcontainers for Testing. The
  non-production bootstrap MUST create an isolated Development database and an
  isolated per-run Testing database from those migrations before seeding; each
  Testing database MUST be disposed after its run, Development MUST persist
  until an explicit guarded reset, and seed rows MUST NOT be embedded in schema
  migrations. This demo runtime does not approve a production edition or
  topology.
- FR-2: Student University ID, course/program/term codes, group codes, active
  enrollment, submission idempotency, AcademicTerm creation request IDs, and
  non-null transcript successor references MUST have database uniqueness
  guards. `AcademicTerm.CreationClientRequestId` is globally unique and its
  required `CreationPayloadHash` binds POST create replay to one canonical
  payload; this adds no idempotency entity.
- FR-3: Capacity and time/date bounds MUST have database check constraints.
- FR-4: Mutable aggregate roots MUST use SQL Server rowversion where specified.
- FR-5: Transcript attempts, published policies, decision snapshots, and audit
  events MUST preserve historical meaning. A transcript correction MUST append
  against the current leaf, retain the predecessor's StudentId, TermId, and
  CourseCode, and form a unique-successor acyclic chain.
- FR-6: Enrollment/group references MUST guarantee the group belongs to the
  selected offering.
- FR-7: Production migrations MUST be reviewed scripts/bundles, not automatic
  startup migrations.
- FR-8: Data provenance MUST be recorded for imported academic/catalogue data.
  The Development and Testing seed profiles MUST use synthetic data, a
  versioned deterministic logical fixture, stable unique identifiers, and
  explicit synthetic provenance; real institutional/student data MUST NOT be
  loaded into either demo profile. Seed is idempotent for the same profile
  version; destructive reset is a separate explicit operation guarded to
  Development/Testing and MUST reject every other environment.
- FR-9: The ERD MUST model the unique SPEC-008 `StudentTermAcademicState` as
  the shared student-term registration boundary and MUST use
  `RegistrationSubmission` as the single registration-submission
  idempotency record containing
  owner/scope/key, canonical payload hash, processing state, immutable
  deterministic result, `ReceivedAtUtc` as its creation instant,
  `UpdatedAtUtc`, nullable `CompletedAtUtc`, and uniqueness on owner/scope/key.
  A second `IdempotencyRecord` entity MUST NOT be created.

## Non-Functional Requirements

- NFR-1: Every query in the approved critical-query inventory (student
  discovery/detail, plan read/write/validation, submission/replay, roster,
  audit, and operational metrics) that touches a table forecast above 10,000
  rows MUST have a reviewed actual SQL Server plan on the production-like data
  fixture. An unbounded full scan MUST fail the gate unless a time-bounded Data
  Lead exception records the plan, measured p95, reason, owner, and expiry.
- NFR-2: A production-like migration rehearsal MUST complete inside 80% of the
  numeric deployment window approved by AASTMT Operations and include tested
  rollback instructions. Until that institutional window is recorded, this
  production deployment/release readiness remains fail closed until approved.
- NFR-3: Backup/restore MUST meet SPEC-018 RPO/RTO.
- NFR-4: Sensitive fields MUST be minimized and excluded from unsafe logs.
  Generated PIN/password plaintext MUST exist only transiently while the
  owning identity component hashes it with ASP.NET Identity; SQL rows,
  migrations, checked-in fixtures, snapshots, logs, traces, and test evidence
  MUST contain only the password hash or redacted metadata, never plaintext
  credentials or full student profiles. Local credential artifacts, logs, and
  exports MUST be Git-ignored and removed no later than seven days after
  creation.

## Acceptance Criteria

### AC-1: Duplicate enrollment guard (FR-2)
Given a student already has an enrollment for an offering<br>
When another concurrent insert uses the same student/offering<br>
Then the database rejects the duplicate<br>
And the API maps it to the stable conflict response.

### AC-2: Invalid capacity (FR-3)
Given a group has 20 active enrollments<br>
When capacity is changed to 19<br>
Then the database/application command rejects the change.

### AC-3: Historical policy (FR-5)
Given policy version 2026.1 is published<br>
When version 2026.2 supersedes it<br>
Then 2026.1 remains immutable and queryable by historical submission.

### AC-4: Code First invariant model (FR-1, FR-4, FR-6, FR-8)
Given isolated empty Development and per-run Testing SQL databases<br>
When the approved Code First migrations run before the versioned synthetic
seed and schema/seed inspection tests execute<br>
Then rowversion and offering/group referential constraints match the ERD<br>
And seeded academic rows have deterministic logical values and synthetic
provenance<br>
And only ASP.NET Identity password hashes, never plaintext PINs/passwords, are
persisted<br>
And the Testing database is disposed after the run while Development persists
until an explicit guarded reset.

### AC-5: Controlled production migration (FR-7)
Given a reviewed migration bundle and production-like backup<br>
When deployment rehearsal runs<br>
Then migration is applied as a controlled step rather than app startup<br>
And rollback instructions restore the prior verified state.

### AC-6: Database-backed registration serialization and idempotency (FR-2, FR-4, FR-9)
Given the Code First model is migrated to SQL Server<br>
When parallel transactions lock the shared StudentTermAcademicState boundary
and claim one idempotency key<br>
Then database uniqueness and concurrency controls permit one canonical owner
and payload<br>
And a different payload cannot reuse that key<br>
And the stored deterministic result survives application-process restart.

### AC-7: Data operational quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
Given a production-like database, reviewed migration bundle, backup, critical
query plans, and privacy-safe logging fixture<br>
When the data release gate executes<br>
Then every inventoried critical query touching a forecast table above 10,000
rows has a reviewed actual plan and no unapproved unbounded scan<br>
And migration rehearsal completes inside 80% of the approved numeric window<br>
And restore meets SPEC-018 RPO/RTO<br>
And sensitive fields are absent from unsafe logs.

## Edge Cases

- EC-1: Migration/bootstrap fails partway, or a seed/reset targets an
  environment other than Development or Testing -> stop without treating the
  database as ready; production data is never seeded or reset.
- EC-2: Import references missing prerequisite -> preview rejects row and
  publish remains blocked.
- EC-3: rowversion is stale -> return 409 with current version, no lost update.
- EC-4: Enrollment counter mismatch -> alert and reconcile through controlled
  operation; do not silently alter history.

## API Contracts

Database design is exposed only through approved feature endpoints. The
student registration-record resource is owned by SPEC-015; shared DTO rules are
owned by SPEC-006. SPEC-005 owns no API endpoint.

`GET /api/student/registrations` is the canonical SPEC-015 read resource used
to observe the approved registration projection; it never exposes EF entities.

```typescript
interface PersistenceConformanceDescriptor {
  entityName: string;
  canonicalOwnerSpec: string;
  invariantIds: string[];
  erdVersion: string;
}
```

`PersistenceConformanceDescriptor` is repository verification metadata, not an
HTTP response DTO and not a persisted runtime entity.

## Data Models

The entities, fields, relationships, keys, checks, indexes, schemas, and
lifecycle are normative in docs/diagrams/ERD.md after approval.

| Field/example | Canonical owner | Type | Constraints |
|---|---|---|---|
| ApplicationUser.UniversityId | SPEC-007 | string | normalized, filtered unique for pre-provisioned student identities |
| Student.ApplicationUserId | SPEC-008 | uniqueidentifier | unique FK to Identity-owned ApplicationUser |
| AcademicTerm lifecycle | SPEC-008 | Draft, RegistrationOpen, RegistrationClosed, Teaching, Completed, Archived | explicit server-controlled lifecycle with rowversion |
| AcademicTerm creation replay | SPEC-008 | globally unique CreationClientRequestId plus CreationPayloadHash | payload-bound POST create replay on the AcademicTerm row; no separate idempotency entity |
| RegistrationWindow lifecycle | SPEC-008 | Draft, Published, EmergencyClosed, Superseded | persisted lifecycle; Upcoming/Open/Closed is computed from authoritative time, never stored as lifecycle |
| Student academic profile | SPEC-008 | ProgramCode, Cohort, CurrentGpa, EarnedCredits, Standing, IsActive, provenance/data version/as-of time | complete sourced profile linked uniquely to ApplicationUser |
| TranscriptAttempt | SPEC-008 | immutable sourced attempt | correction targets the current leaf, retains StudentId/TermId/CourseCode, and appends through a filtered-unique non-null SupersedesAttemptId; no branch, cycle, or in-place rewrite |
| StudentHold | SPEC-008 | term-scoped sourced effective interval | code/message, blocking flag, provenance, and active-period checks |
| SectionGroup.Version | SPEC-010 | rowversion | concurrency token |
| Enrollment offering/group | SPEC-014 | composite FK | group must belong to offering |
| ImportedRecord.Source | owning import feature | string | required provenance |
| Non-production seed profile | SPEC-005 contract; canonical owners write their rows | configuration, not an entity | Development or per-run Testing only; versioned deterministic logical fixture; idempotent seed; explicit guarded reset |
| ApplicationUser password credential | SPEC-007 | password hash | ASP.NET Identity hash only; no plaintext PIN/password persistence |
| StudentTermAcademicState | SPEC-008, consumed by SPEC-014 | entity | unique student + term; shared serialization boundary advanced in the registration transaction |
| RegistrationSubmission | SPEC-014 | entity and idempotency record | unique owner + scope + key; payload hash, state, result, ReceivedAtUtc creation instant, UpdatedAtUtc, nullable CompletedAtUtc |

Each canonical owner implements its entity and EF configuration in its module.
`StudentRegistration.Infrastructure.SqlServer` composes those configurations
into the one DbContext and owns migrations. SPEC-005 supplies conformance rules
and does not require downstream source during its own approval.

## Out of Scope

- OS-1: Database-per-module or read replica in MVP.
- OS-2: Hard deletion/retention schedule until AASTMT privacy approval.
- OS-3: Automatically resolving invalid imported curriculum data.
- OS-4: Direct production schema mutation outside migrations.
