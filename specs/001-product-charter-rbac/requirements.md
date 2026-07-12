# SPEC-001: Product Charter and RBAC

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
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
  client-selected role.
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

## Edge Cases

- EC-1: User holds Lecturer and TA roles -> offer only the authorized contexts.
- EC-2: Staff has no supported role -> deny access with safe no-role message.
- EC-3: A requested enhancement is outside MVP -> create/review a new spec
  rather than adding it silently.

## API Contracts

Detailed contracts belong to SPEC-006 and feature specs. The charter's
role/context boundary is observed through GET /api/context.

## Data Models

| Concept | Required values |
|---|---|
| Role | Student, Admin, Lecturer, TeachingAssistant |
| Permission | Stable server policy name and allowed operations |
| RoleAssignment | User, role, effective dates, assigning actor |

## Out of Scope

- OS-1: Payment, grade entry, attendance, waitlist, advisor workflow, and
  notifications.
- OS-2: Public staff registration.
- OS-3: Client-side-only authorization.
- OS-4: Multi-tenancy and native mobile applications.
