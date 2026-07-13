# SPEC-002 Dependency Baseline

**Recorded:** 2026-07-13
**Feature:** SPEC-002 AASTMT Policy Rulebook
**Dependency:** SPEC-001 Product Charter and RBAC

## Accepted upstream version

- Repository commit: `ab2a0acbdbc30e3a0b92d665c3d23fb55b1714ba`.
- SPEC-001 status: Gate A approved; governed-contract implementation slice in
  progress with 26/42 tasks complete.
- Role vocabulary: `role-definition/1.0`.
- Permission vocabulary: `permission-definition/1.0`.
- RBAC matrix: `rbac-matrix/1.0`.
- Scope contract: server-derived, API-enforced, fail closed.

SPEC-002 consumes the product boundary, authorization principles, and
traceability admission rule. It owns no identity role, runtime authorization
policy, frontend route, API handler, database entity, or migration.

The incomplete SPEC-001 tasks are downstream Gate C/runtime/NFR evidence and
do not alter the approved governance dependency consumed by this rulebook.
