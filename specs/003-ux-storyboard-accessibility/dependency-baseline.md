# SPEC-003 Dependency Baseline

**Recorded:** 2026-07-13
**Feature:** SPEC-003 Frontend Page Design, Storyboard, Accessibility and Functional Testing

## SPEC-001 accepted contracts

- Source commit: `ab2a0acbdbc30e3a0b92d665c3d23fb55b1714ba`.
- `role-definition/1.0`, `permission-definition/1.0`, and `rbac-matrix/1.0`.
- Separate student/shared-staff entry and server-derived data scope.
- Runtime role/session behavior remains SPEC-007-owned.

## SPEC-002 accepted contracts

- Source commit: `393729aeaadf3fa7e90377a208b4ddbe5124a630`.
- `policy-rulebook/1.0` / `DEMO-POC-2026.1`.
- Stable decision/reason/provenance shape, 9/12/18 boundaries, field-level
  curriculum provenance, and fail-closed unapproved behavior.
- Runtime evaluation remains SPEC-009-owned.

## Frontend ownership boundary

SPEC-003 owns client-only presentation models, governed design/test metadata,
neutral tokens, reusable components, the 27 Page Design Records, and SYS-01.
It owns no server endpoint, runtime identity/policy entity, database mapping, or
academic decision. Other route pages remain owned by their route-manifest
implementation specifications.
