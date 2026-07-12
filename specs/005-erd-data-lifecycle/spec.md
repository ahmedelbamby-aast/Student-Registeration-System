# Feature Specification: ERD and Data Lifecycle

**Feature Branch**: 005-erd-data-lifecycle
**Created**: 2026-07-12
**Status**: In Review
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

Given the approved Code First model is migrated to an empty SQL Server<br>
When schema inspection and seed import tests run<br>
Then rowversion and offering/group referential constraints match the ERD<br>
And imported academic rows retain source provenance.
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
Then critical tables above 10,000 rows have reviewed indexed plans<br>
And migration rehearsal completes inside the approved deployment window<br>
And restore meets SPEC-018 RPO/RTO<br>
And sensitive fields are absent from unsafe logs.

## Edge Cases

- EC-1: Migration fails partway -> deployment stops and follows tested rollback.
- EC-2: Import references missing prerequisite -> preview rejects row and
  publish remains blocked.
- EC-3: rowversion is stale -> return 409 with current version, no lost update.
- EC-4: Enrollment counter mismatch -> alert and reconcile through controlled
  operation; do not silently alter history.

## Requirements

### Functional Requirements

- FR-1: EF Core Code First migrations MUST define the approved ERD.
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
- FR-9: The ERD MUST model a unique student-term registration guard and an
  idempotency record containing owner/scope, canonical payload hash, processing
  state, immutable deterministic result, created/updated/completed timestamps,
  and uniqueness on owner/scope/key.

### Non-Functional Requirements

- NFR-1: No query on a table expected above 10,000 rows MAY rely on an
  unreviewed full scan in a critical path.
- NFR-2: A production-like migration rehearsal MUST complete inside the
  approved deployment window with rollback instructions.
- NFR-3: Backup/restore MUST meet SPEC-018 RPO/RTO.
- NFR-4: Sensitive fields MUST be minimized and excluded from unsafe logs.

### Key Entities

- **ApplicationUser**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Student**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Staff**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Program**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Course**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **AcademicTerm**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicySet**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **CourseOffering**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **SectionGroup**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationPlan**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **RegistrationSubmission**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Enrollment**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **AuditEvent**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: All required uniqueness, capacity, relationship, and historical invariants are explicitly modeled.
- **SC-2**: Every imported academic record retains source provenance.
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
