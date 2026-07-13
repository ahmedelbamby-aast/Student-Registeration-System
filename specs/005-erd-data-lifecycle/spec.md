# Feature Specification: ERD and Data Lifecycle

**Feature Branch**: 005-erd-data-lifecycle
**Created**: 2026-07-12
**Status**: Approved (Gate A demo implementation, 2026-07-13)
**Owner**: Data/Backend Lead
**Normative detail**: [requirements.md](requirements.md)

## Context

Registration correctness depends on relational constraints, immutable academic
history, effective-dated policies, and short atomic transactions. The complete
proposed model is in docs/diagrams/ERD.md.

## User Scenarios and Testing

### User Story 1 - Duplicate enrollment guard (FR-2) (P1)

As a Data Lead, I need the Duplicate enrollment guard (FR-2) behavior so that ERD and Data Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given a student already has an enrollment for an offering<br>
When another concurrent insert uses the same student/offering<br>
Then the database rejects the duplicate<br>
And the API maps it to the stable conflict response.
### User Story 2 - Invalid capacity (FR-3) (P1)

As a Data Lead, I need the Invalid capacity (FR-3) behavior so that ERD and Data Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given a group has 20 active enrollments<br>
When capacity is changed to 19<br>
Then the database/application command rejects the change.
### User Story 3 - Historical policy (FR-5) (P2)

As a Data Lead, I need the Historical policy (FR-5) behavior so that ERD and Data Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given policy version 2026.1 is published<br>
When version 2026.2 supersedes it<br>
Then 2026.1 remains immutable and queryable by historical submission.
### User Story 4 - Code First invariant model (FR-1, FR-4, FR-6, FR-8) (P2)

As a Data Lead, I need the Code First invariant model (FR-1, FR-4, FR-6, FR-8) behavior so that ERD and Data Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

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
### User Story 5 - Controlled production migration (FR-7) (P3)

As a Data Lead, I need the Controlled production migration (FR-7) behavior so that ERD and Data Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given a reviewed migration bundle and production-like backup<br>
When deployment rehearsal runs<br>
Then migration is applied as a controlled step rather than app startup<br>
And rollback instructions restore the prior verified state.
### User Story 6 - Database-backed registration and idempotency guards (FR-2, FR-4, FR-9) (P3)

As a Data Lead, I need the Database-backed registration and idempotency guards (FR-2, FR-4, FR-9) behavior so that ERD and Data Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-6 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-6)**

Given the Code First model is migrated to SQL Server<br>
When parallel transactions claim one student-term guard and one idempotency
key<br>
Then database uniqueness and concurrency controls permit one canonical owner
and payload<br>
And a different payload cannot reuse that key<br>
And the stored deterministic result survives application-process restart.
### User Story 7 - Data operational quality gate (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Data Lead, I need the Data operational quality gate (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that ERD and Data Lifecycle produces a verifiable outcome.

**Independent Test**: Execute AC-7 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-7)**

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

## Requirements

### Functional Requirements

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
  enrollment, and submission idempotency MUST have database uniqueness guards.
- FR-3: Capacity and time/date bounds MUST have database check constraints.
- FR-4: Mutable aggregate roots MUST use SQL Server rowversion where specified.
- FR-5: Transcript attempts, published policies, decision snapshots, and audit
  events MUST preserve historical meaning.
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
- FR-9: The ERD MUST model a unique student-term registration guard and MUST
  use `RegistrationSubmission` as the single idempotency record containing
  owner/scope/key, canonical payload hash, processing state, immutable
  deterministic result, created/updated/completed timestamps, and uniqueness
  on owner/scope/key. A second `IdempotencyRecord` entity MUST NOT be created.

### Non-Functional Requirements

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

### Key Entities

- **SPEC-007 owned**: ApplicationUser, Staff.
- **SPEC-008 owned**: Student, AcademicTerm.
- **SPEC-009 owned**: Program, Course, PolicySet.
- **SPEC-010 owned**: CourseOffering, SectionGroup.
- **SPEC-012 owned**: RegistrationPlan.
- **SPEC-014 owned**: StudentTermRegistrationGuard, RegistrationSubmission,
  and Enrollment. `RegistrationSubmission` is the idempotency record.
- **SPEC-004 owned**: AuditEvent. SPEC-017 owns authorized audit query/export,
  not the transaction-aware write model.

SPEC-005 owns the cross-module ERD, relational-invariant catalogue, ownership
matrix, and data-lifecycle contract—not any of these runtime entities.

## Success Criteria

- **SC-1**: All required uniqueness, capacity, relationship, and historical invariants are explicitly modeled.
- **SC-2**: Every imported or synthetic seeded academic record retains source provenance, and rebuilding the same seed version reproduces the same logical identities and academic values.
- **SC-3**: Migration, backup, and rollback requirements are documented before schema implementation.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-002](../002-aastmt-policy-rulebook/spec.md)
- [SPEC-004](../004-architecture-engineering-principles/spec.md)

## Frontend Route Ownership

No route is directly owned. Any later UI exposure requires a SPEC-003 route-manifest amendment before implementation.

## Out of Scope

- OS-1: Database-per-module or read replica in MVP.
- OS-2: Hard deletion/retention schedule until AASTMT privacy approval.
- OS-3: Automatically resolving invalid imported curriculum data.
- OS-4: Direct production schema mutation outside migrations.
