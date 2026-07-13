# SPEC-004 Dependency Baseline

**Recorded:** 2026-07-13
**Feature:** SPEC-004 Architecture and Engineering Principles
**Result:** PASS

## SPEC-001 accepted contracts

- Source commit: `ab2a0acbdbc30e3a0b92d665c3d23fb55b1714ba`.
- Human approval: Ahmed ELbamby, 2026-07-13, non-production demo scope.
- Accepted contract versions: `role-definition/1.0`,
  `permission-definition/1.0`, `rbac-matrix/1.0`,
  `entry-point-boundary/1.0`, `role-boundary/1.0`, and
  `server-derived-scope/1.0`.
- Required commit-pinned paths: `spec.md`, `requirements.md`, `plan.md`,
  `data-model.md`, and `contracts/api.md`; their blobs at the accepted commit
  are the immutable SPEC-004 input.
- Accepted boundary: separate Student and shared Staff entry points,
  server-derived role/data scope, least privilege, and no client-claimed role.
- Runtime identity/session behavior remains owned by SPEC-007; SPEC-004 only
  governs project dependencies and composition.

## SPEC-003 accepted contracts

- Approved source baseline: commit
  `557876b7a65f862d47dcddde6070fd0c096bf145`.
- Human approval: Ahmed ELbamby, 2026-07-13, non-production demo scope.
- Accepted design contracts: `frontend-design-index/1.0`,
  `page-design-record/1.0`, `route-inventory/1.0`,
  `route-contributors/1.0`, route manifest `2.0.0`, page/API manifest `1.0.0`,
  endpoint manifest `2.0.0`, and component manifest `1.1.0`.
- Required commit-pinned paths: `spec.md`, `requirements.md`, `plan.md`,
  `data-model.md`, and `contracts/api.md`; their blobs at the approved baseline
  commit are the immutable SPEC-004 input.
- Accepted boundary: the Client presents server decisions; it does not own
  authorization, policy, capacity, conflict, registration, or persistence
  decisions. SPEC-004 owns no frontend route.

## Pending SPEC-003 amendment isolation

Commit `985390234a1eb3ae6db428e6c65220bfe6d4362a` contains the explicitly pending
PDR/index 1.1 draft and the pending SPEC-012 credit-load response amendment.
Those draft versions are not accepted as immutable SPEC-004 dependency pins.
They change no SPEC-004 module, endpoint, source writer, or deployment decision,
so they do not independently block architecture work after SPEC-004's own T005
gate passes. Any later architecture dependency on a draft field must wait for Ahmed
ELbamby's explicit approval and an updated immutable baseline.

## Dependency conclusion

The direct consumed edges are `SPEC-001 -> SPEC-004` and
`SPEC-003 -> SPEC-004`. The accepted transitive graph also contains
`SPEC-001 -> SPEC-003` and `SPEC-002 -> SPEC-003`; it is acyclic. Both direct
dependencies have Gate A demo approval, their runtime ownership remains intact,
and no pending frontend amendment is promoted by this record.
