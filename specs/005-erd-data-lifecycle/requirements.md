# SPEC-005: ERD and Data Lifecycle

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** Data/Backend Lead<br>
**Reviewers:** Architect, Registrar, QA, Security<br>
**Target:** Sprint 0-S1<br>
**Dependencies:** SPEC-002, SPEC-004<br>

## Context

Registration correctness depends on relational constraints, immutable academic
history, effective-dated policies, and short atomic transactions. The complete
proposed model is in docs/diagrams/ERD.md.

## Functional Requirements

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

## Non-Functional Requirements

- NFR-1: No query on a table expected above 10,000 rows MAY rely on an
  unreviewed full scan in a critical path.
- NFR-2: A production-like migration rehearsal MUST complete inside the
  approved deployment window with rollback instructions.
- NFR-3: Backup/restore MUST meet SPEC-018 RPO/RTO.
- NFR-4: Sensitive fields MUST be minimized and excluded from unsafe logs.

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
Given the approved Code First model is migrated to an empty SQL Server<br>
When schema inspection and seed import tests run<br>
Then rowversion and offering/group referential constraints match the ERD<br>
And imported academic rows retain source provenance.

### AC-5: Controlled production migration (FR-7)
Given a reviewed migration bundle and production-like backup<br>
When deployment rehearsal runs<br>
Then migration is applied as a controlled step rather than app startup<br>
And rollback instructions restore the prior verified state.

### AC-6: Database-backed registration and idempotency guards (FR-2, FR-4, FR-9)
Given the Code First model is migrated to SQL Server<br>
When parallel transactions claim one student-term guard and one idempotency
key<br>
Then database uniqueness and concurrency controls permit one canonical owner
and payload<br>
And a different payload cannot reuse that key<br>
And the stored deterministic result survives application-process restart.

### AC-7: Data operational quality gate (NFR-1, NFR-2, NFR-3, NFR-4)
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

## API Contracts

Database design is exposed only through approved feature endpoints, for
example GET /api/student/registrations; feature DTOs are defined in SPEC-006
onward.

## Data Models

The entities, fields, relationships, keys, checks, indexes, schemas, and
lifecycle are normative in docs/diagrams/ERD.md after approval.

| Field/example | Type | Constraints |
|---|---|---|
| Student.UniversityId | string | normalized, unique, not null |
| SectionGroup.Version | rowversion | concurrency token |
| Enrollment offering/group | composite FK | group must belong to offering |
| ImportedRecord.Source | string | required provenance |
| StudentTermRegistrationGuard | entity | unique student + term; rowversion/serialization boundary |
| IdempotencyRecord | entity | unique owner + scope + key; payload hash, state, result, timestamps |

## Out of Scope

- OS-1: Database-per-module or read replica in MVP.
- OS-2: Hard deletion/retention schedule until AASTMT privacy approval.
- OS-3: Automatically resolving invalid imported curriculum data.
- OS-4: Direct production schema mutation outside migrations.
