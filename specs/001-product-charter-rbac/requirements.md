# SPEC-001: Product Charter and RBAC

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** Approved (Gate A demo implementation, 2026-07-13)<br>
**Owner:** Product Owner<br>
**Reviewers:** Registrar/Policy SME, UX, Architecture, Data, QA, Security, DevOps<br>
**Target:** Sprint 0<br>
**Dependencies:** None<br>

## Context

AASTMT College of AI needs one registration experience spanning student
discovery, schedule building, atomic submission, and role-scoped operations.
The product must be easy during registration peaks and remain extensible
without an initially distributed architecture.

This charter fixes the MVP boundary and authorization model so later feature
specs do not invent scope or permission rules.

## Functional Requirements

- FR-1: The system MUST support Student, Admin, Lecturer, and
  TeachingAssistant roles.
- FR-2: Students MUST use a student entry point; Admin/Lecturer/TA MUST share a
  staff entry point.
- FR-3: The server MUST derive role and data scope and MUST NOT trust a
  client-selected role. Admin is not an implicit superuser:
  `AcademicTerms.Manage` and `AcademicProfiles.Manage` are distinct Admin-only
  permissions, and neither the Admin role nor either permission implies the
  other or any unlisted capability. `AcademicProfiles.Manage` permits only a
  required-term, bounded University ID/name locator returning minimal fields,
  followed by named StudentId plus AcademicTermId scope for detail or
  correction.
- FR-4: The system MUST support the end-to-end student flow from login through
  an atomic registration receipt.
- FR-5: The system MUST expose role-scoped staff/admin workspaces.
- FR-6: MVP scope and non-goals MUST match docs/PROJECT_PLAN.md.
- FR-7: Every implementation story MUST trace to an approved spec and
  acceptance criterion.

## Non-Functional Requirements

- NFR-1: Critical flows MUST meet WCAG 2.2 AA.
- NFR-2: The design MUST support the approved SPEC-018 scale targets without
  changing domain behavior.
- NFR-3: Authorization MUST be enforced by the API for every protected action.
- NFR-4: The initial solution MUST remain one deployable modular monolith.

## Acceptance Criteria

### AC-1: Student boundary (FR-1, FR-2)
Given a user opens the public landing page<br>
When the user selects Student<br>
Then only student activation/login actions are presented<br>
And no staff role can be selected.

### AC-2: Staff role derivation (FR-2, FR-3)
Given a valid staff account with Lecturer claims<br>
When the user authenticates through the shared staff page<br>
Then the server routes the user to the Lecturer context<br>
And changing a client route does not grant Admin data.

### AC-3: Scope traceability (FR-6, FR-7)
Given a proposed implementation story<br>
When it is evaluated for Sprint readiness<br>
Then it references an approved SPEC-NNN/FR-N and SPEC-NNN/AC-N<br>
And work is rejected when no approved contract exists.

### AC-4: End-to-end role coverage (FR-4, FR-5)
Given the Gate C staging release<br>
When representatives execute the approved Student, Admin, Lecturer and TA
journeys<br>
Then the student can complete the atomic registration flow<br>
And every staff role reaches only its approved workspace.

### AC-5: Product quality boundary (NFR-1, NFR-2, NFR-3, NFR-4)
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

## API Contracts

Detailed contracts belong to SPEC-006 and feature specs. The charter's
role/context boundary is observed through `GET /api/context`, whose canonical
handler owner is SPEC-008; SPEC-001 owns no endpoint.

## Data Models

| Concept | Role in SPEC-001 | Canonical runtime owner | Required values |
|---|---|---|---|
| RoleDefinition | Governed vocabulary artifact | SPEC-001; consumed by SPEC-007 | Student, Admin, Lecturer, TeachingAssistant |
| PermissionDefinition | Governed capability/data-scope artifact | SPEC-001; consumed by SPEC-007 | Stable server policy name and allowed operations |
| RbacMatrix | Governed mapping artifact | SPEC-001; consumed by SPEC-007 | Role, permission, data-scope rule, denied operations |

SPEC-001 owns the product/RBAC contract and conformance evidence. It does not
own identity persistence or `RolePolicies.cs`; those are delivered only by
approved SPEC-007 tasks. Runtime `RoleAssignment` is only a referenced
SPEC-007 entity.

## Out of Scope

- OS-1: Payment, grade entry, attendance, waitlist, advisor workflow, and
  notifications.
- OS-2: Public staff registration.
- OS-3: Client-side-only authorization.
- OS-4: Multi-tenancy and native mobile applications.
