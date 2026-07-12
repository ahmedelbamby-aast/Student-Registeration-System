# Feature Specification: Catalogue Prerequisites and Policy Administration

**Feature Branch**: 009-catalog-prerequisites-policy-admin
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Registrar/Policy SME and Backend Lead
**Normative detail**: [requirements.md](requirements.md)

## Context

Public College-of-AI curriculum pages contain missing and inconsistent
references. Production catalogue and rules need an approved import, validation
preview, source provenance, and controlled publication.

## User Scenarios and Testing

### User Story 1 - Missing prerequisite (FR-2, FR-3) (P1)

As a Authorized administrator, I need the Missing prerequisite (FR-2, FR-3) behavior so that Catalogue Prerequisites and Policy Administration produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given an import references course IN321 that does not exist<br>
When validation runs<br>
Then the row is rejected with source row and missing code<br>
And no part of that import is published.
### User Story 2 - Prerequisite cycle (FR-3) (P1)

As a Authorized administrator, I need the Prerequisite cycle (FR-3) behavior so that Catalogue Prerequisites and Policy Administration produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given Course A requires B and B requires A<br>
When the curriculum is validated<br>
Then publication is blocked with the cycle path.
### User Story 3 - Policy simulation (FR-4, FR-5) (P2)

As a Authorized administrator, I need the Policy simulation (FR-4, FR-5) behavior so that Catalogue Prerequisites and Policy Administration produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a draft Project I rule requiring GPA 2.0 and 96 credits<br>
When simulated with GPA 2.1 and 95 credits<br>
Then it fails with the earned-credit reason<br>
And identifies the draft policy version/source.
### User Story 4 - Governed catalogue publish (FR-1, FR-6, FR-7) (P2)

As a Authorized administrator, I need the Governed catalogue publish (FR-1, FR-6, FR-7) behavior so that Catalogue Prerequisites and Policy Administration produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given an authorized Admin has a valid draft course/curriculum/policy version<br>
When publication is confirmed<br>
Then the prior published version remains immutable and is superseded<br>
And an unauthorized user cannot publish it.

## Edge Cases

- EC-1: Duplicate course code differs only by case/spacing -> normalize and
  reject duplicate.
- EC-2: Published course is referenced by history -> deactivate/supersede, do
  not delete.
- EC-3: Concurrent policy publish -> one succeeds; stale version gets 409.
- EC-4: Unknown rule type/config -> reject draft validation.

## Requirements

### Functional Requirements

- FR-1: Admin MUST manage programs, curricula, courses, credit values, status,
  prerequisites, minimum grades/GPA/earned credits, and cohort scope.
- FR-2: Imports MUST provide preview, row-level validation, provenance, and
  all-or-nothing publication.
- FR-3: The system MUST detect missing references, duplicate codes, invalid
  credits, and prerequisite cycles before publish.
- FR-4: Admin MUST manage typed effective-dated PolicySet/PolicyRule values.
- FR-5: Admin MUST simulate a policy decision against test student inputs
  before publication.
- FR-6: Published catalogue/policy versions MUST be immutable and superseded.
- FR-7: Only approved Admin/Registrar permissions MAY publish.

### Key Entities

- **Program**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **Course**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **CurriculumCourse**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **CoursePrerequisite**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicySet**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **PolicyRule**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.
- **ImportBatch**: Feature-owned concept; attributes and relationships are refined in requirements.md and the shared ERD.

## Success Criteria

- **SC-1**: Invalid or cyclic catalogue relationships cannot be published.
- **SC-2**: Every published catalogue and policy version is immutable and auditable.
- **SC-3**: Administrators can preview and explain all validation failures before publication.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- [SPEC-002](../002-aastmt-policy-rulebook/spec.md)
- [SPEC-005](../005-erd-data-lifecycle/spec.md)
- [SPEC-006](../006-domain-class-api-contracts/spec.md)
- [SPEC-008](../008-academic-term-student-profile/spec.md)
- [SPEC-018](../018-quality-security-scalability-operations/spec.md)

## Out of Scope

- OS-1: Scraping public web pages as production catalogue source.
- OS-2: Arbitrary policy scripting.
- OS-3: Silent auto-correction of referential errors.
- OS-4: Deleting historical course/policy records.
