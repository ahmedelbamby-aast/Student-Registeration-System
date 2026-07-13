# Feature Specification: Product Charter and RBAC

**Feature Branch**: 001-product-charter-rbac
**Created**: 2026-07-12
**Status**: In Review
**Owner**: Product Owner
**Normative detail**: [requirements.md](requirements.md)

## Context

AASTMT College of AI needs one registration experience spanning student
discovery, schedule building, atomic submission, and role-scoped operations.
The product must be easy during registration peaks and remain extensible
without an initially distributed architecture.

This charter fixes the MVP boundary and authorization model so later feature
specs do not invent scope or permission rules.

## User Scenarios and Testing

### User Story 1 - Student boundary (FR-1, FR-2) (P1)

As a Product Owner, I need the Student boundary (FR-1, FR-2) behavior so that Product Charter and RBAC produces a verifiable outcome.

**Independent Test**: Execute AC-1 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-1)**

Given a user opens the public landing page<br>
When the user selects Student<br>
Then only student activation/login actions are presented<br>
And no staff role can be selected.
### User Story 2 - Staff role derivation (FR-2, FR-3) (P1)

As a Product Owner, I need the Staff role derivation (FR-2, FR-3) behavior so that Product Charter and RBAC produces a verifiable outcome.

**Independent Test**: Execute AC-2 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-2)**

Given a valid staff account with Lecturer claims<br>
When the user authenticates through the shared staff page<br>
Then the server routes the user to the Lecturer context<br>
And changing a client route does not grant Admin data.
### User Story 3 - Scope traceability (FR-6, FR-7) (P2)

As a Product Owner, I need the Scope traceability (FR-6, FR-7) behavior so that Product Charter and RBAC produces a verifiable outcome.

**Independent Test**: Execute AC-3 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-3)**

Given a proposed implementation story<br>
When it is evaluated for Sprint readiness<br>
Then it references an approved SPEC-NNN/FR-N and SPEC-NNN/AC-N<br>
And work is rejected when no approved contract exists.
### User Story 4 - End-to-end role coverage (FR-4, FR-5) (P2)

As a Product Owner, I need the End-to-end role coverage (FR-4, FR-5) behavior so that Product Charter and RBAC produces a verifiable outcome.

**Independent Test**: Execute AC-4 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-4)**

Given the Gate C staging release<br>
When representatives execute the approved Student, Admin, Lecturer and TA
journeys<br>
Then the student can complete the atomic registration flow<br>
And every staff role reaches only its approved workspace.
### User Story 5 - Product quality boundary (NFR-1, NFR-2, NFR-3, NFR-4) (P3)

As a Product Owner, I need the Product quality boundary (NFR-1, NFR-2, NFR-3, NFR-4) behavior so that Product Charter and RBAC produces a verifiable outcome.

**Independent Test**: Execute AC-5 in requirements.md without relying on another story in this feature.

**Acceptance Scenario (AC-5)**

Given the MVP release candidate and SPEC-018 production-like load profile<br>
When accessibility, authorization, architecture, and scale gates execute<br>
Then critical flows meet WCAG 2.2 AA<br>
And every protected request is API-authorized<br>
And the modular monolith meets SPEC-018 targets without distributed services.

## Edge Cases

- EC-1: User holds Lecturer and TA roles -> offer only the authorized contexts.
- EC-2: Staff has no supported role -> deny access with safe no-role message.
- EC-3: A requested enhancement is outside MVP -> create/review a new spec
  rather than adding it silently.

## Requirements

### Functional Requirements

- FR-1: The system MUST support Student, Admin, Lecturer, and
  TeachingAssistant roles.
- FR-2: Students MUST use a student entry point; Admin/Lecturer/TA MUST share a
  staff entry point.
- FR-3: The server MUST derive role and data scope and MUST NOT trust a
  client-selected role.
- FR-4: The system MUST support the end-to-end student flow from login through
  an atomic registration receipt.
- FR-5: The system MUST expose role-scoped staff/admin workspaces.
- FR-6: MVP scope and non-goals MUST match docs/PROJECT_PLAN.md.
- FR-7: Every implementation story MUST trace to an approved spec and
  acceptance criterion.

### Non-Functional Requirements

- NFR-1: Critical flows MUST meet WCAG 2.2 AA.
- NFR-2: The design MUST support the approved SPEC-018 scale targets without
  changing domain behavior.
- NFR-3: Authorization MUST be enforced by the API for every protected action.
- NFR-4: The initial solution MUST remain one deployable modular monolith.

### Key Entities

- **RoleDefinition**: Governed vocabulary artifact owned by SPEC-001; SPEC-007 owns runtime identity representation.
- **PermissionDefinition**: Governed capability/data-scope artifact owned by SPEC-001; SPEC-007 owns executable authorization policies.
- **RbacMatrix**: Governed role-to-permission matrix artifact owned by SPEC-001.

Runtime `RoleAssignment` is referenced from, and owned only by, SPEC-007; it is
not a SPEC-001 entity.

## Success Criteria

- **SC-1**: 100% of MVP capabilities are assigned to an accountable owner and acceptance scenario.
- **SC-2**: Every protected role has an explicit permission boundary before implementation begins.
- **SC-3**: No delivery task is admitted without an approved requirement and acceptance reference.

## Assumptions

- Server time, the configured academic term, authenticated identity, and authorization scope are authoritative.
- Approved upstream specifications provide their published contracts; failures are handled safely and do not bypass policy.
- Policy values that lack institutional approval remain configurable and fail closed.

## Dependencies

- None; this is a root specification.

## Frontend Route Ownership

No route is directly owned. Any later UI exposure requires a SPEC-003 route-manifest amendment before implementation.

## Out of Scope

- OS-1: Payment, grade entry, attendance, waitlist, advisor workflow, and
  notifications.
- OS-2: Public staff registration.
- OS-3: Client-side-only authorization.
- OS-4: Multi-tenancy and native mobile applications.
