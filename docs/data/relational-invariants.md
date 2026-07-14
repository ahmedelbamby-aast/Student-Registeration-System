# Relational Invariants Workstream

**Contract version:** `relational-invariants/1.0`<br>
**Requirements:** FR-2, FR-3, FR-4, FR-5, FR-6<br>
**Bounded delivery:** design-time contract only<br>
**Runtime source dependency:** None<br>
**Fail-closed boundary:** unapproved production behavior remains blocked

SPEC-005 owns conformance, not downstream runtime models. Owner specifications
implement persistence mappings through their approved contributions to the
single Infrastructure.SqlServer DbContext. Runtime model, mapping, migration,
and SQL verification remain deferred until each owner dependency is approved,
implemented, and version-pinned.

## Governed contracts

- FR-2: `specs/005-erd-data-lifecycle/contracts/unique-invariants.md`
- FR-3: `specs/005-erd-data-lifecycle/contracts/check-constraints.md`
- FR-4: `specs/005-erd-data-lifecycle/contracts/concurrency-tokens.md`
- FR-5: `specs/005-erd-data-lifecycle/contracts/immutable-history.md`
- FR-6: this cross-owner foreign-key and enforcement summary

## Requirement-to-owner and evidence map

| Requirement | Canonical owner contributions | Persistence contribution | Enforcement class | Future real-SQL release evidence |
|---|---|---|---|---|
| FR-2 uniqueness | SPEC-007 IdentityAccess; SPEC-008 Academics; SPEC-009 Catalogue; SPEC-010 Scheduling; SPEC-012 and SPEC-014 Registration | Owner mappings composed by Infrastructure.SqlServer; Academics contributes globally unique AcademicTerm.CreationClientRequestId and filtered-unique non-null TranscriptAttempt.SupersedesAttemptId | Database constraint plus payload-binding validation | Unique/alternate-key inspection, term-create replay/mismatch behavior, transcript branch rejection, and duplicate-key behavior against SQL Server |
| FR-3 capacity/time bounds | SPEC-008 Academics and SPEC-010 Scheduling | AcademicContext and Scheduling mappings | Database constraint plus Transactional application validation | Check-constraint inspection, invalid-write rejection, and allocation predicate evidence |
| FR-4 concurrency | SPEC-007, SPEC-008, SPEC-009, SPEC-010, SPEC-012, SPEC-014, and SPEC-017 | Each owner mapping contributes its ERD rowversion tokens | Database constraint plus conditional update | Token metadata, stale-write `409 STALE_VERSION`, aggregate advancement, and contention evidence |
| FR-5 history | SPEC-008 transcript; SPEC-009 policy/catalogue; SPEC-014 submission/enrollment/snapshot; SPEC-004 audit | Append-only or superseding owner mappings; transcript corrections append a sourced row linked by `SupersedesAttemptId` | Filtered unique successor constraint and Transactional application validation | Mutation-denial; same-student/course/term, current-leaf, unique-successor and acyclic-chain checks; replay; and historical-query evidence |
| FR-6 offering/group integrity | SPEC-010 owns SectionGroup; SPEC-014 owns Enrollment | Scheduling alternate key plus Registration composite foreign key | Database constraint | Composite-FK metadata and wrong-offering insert rejection |

## FR-6 cross-owner relationship

The durable relationship is
`Enrollment(GroupId, OfferingId) -> SectionGroup(Id, OfferingId)`. It mirrors
the ERD rule: Alternate key SectionGroup(Id, OfferingId), referenced by
Enrollment, so a group cannot be paired with another offering.

Catalogue bridges similarly use same-catalogue-version composite foreign keys
so Program, Course, curriculum, and prerequisite members cannot cross a
CatalogueVersion boundary.

## Verification boundary

Database constraint covers uniqueness, alternate keys, composite foreign keys,
row-local checks, and configured concurrency tokens. Academics owns
RegistrationWindow overlap validation under its term/window publication locks;
Scheduling owns meeting/staff/room overlap validation. Transactional
application validation also covers aggregate child changes, conditional seat
allocation, and publication rules that SQL checks cannot express alone.

Future real-SQL release evidence must inspect the composed SQL Server schema
and exercise each rejection/concurrency path. This document is not that
evidence and does not authorize production schema creation or execution.
