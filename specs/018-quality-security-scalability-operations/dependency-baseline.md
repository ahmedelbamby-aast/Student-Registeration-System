# SPEC-018 Dependency Baseline

**Recorded:** 2026-07-14<br>
**Feature:** SPEC-018 Quality, Security, Scalability, and Operations<br>
**Result:** PASS FOR DEPENDENCY-ORDERED NON-PRODUCTION DEMO WORK

## SPEC-003 frontend-quality baseline

- Accepted commit: `8ee8f724af3b60bbbc464532bc68de6f173247e5`.
- Accepted inputs: `requirements.md`, `plan.md`, `data-model.md`, and
  `contracts/api.md` under `specs/003-ux-storyboard-accessibility/`.
- Accepted versions: page-design governance `1.1`, route manifest `2.1.0`,
  page/API manifest `1.1.0`, component manifest `1.1.0`, and immutable Page
  Design Records version `1.0` in `design-only` state.
- Consumed boundary: WCAG 2.2 AA, keyboard/focus, responsive, browser,
  deterministic fixture, visual-baseline, flake, and signed manual
  assistive-technology evidence contracts. `design-only` and `not-pinned`
  never authorize route source or claim executable browser evidence.

## SPEC-001 product and authorization baseline

- Accepted commit: `641c8ff66eb0f2f1b004552cf226b3c64e56a8b7`.
- Accepted inputs: `requirements.md`, `plan.md`, `data-model.md`, and
  `contracts/api.md` under `specs/001-product-charter-rbac/`.
- Accepted versions: `role-definition/1.0`, `permission-definition/1.0`, and
  `rbac-matrix/1.0`.
- Consumed boundary: Student, Admin, Lecturer, and TeachingAssistant are the
  only role tokens; authorization and data scope are server-derived. SPEC-018
  verifies the boundary but does not implement identity or role assignment.

## SPEC-004 architecture baseline

- Accepted completion commit:
  `6b087936c0d8b6341c656ade698247561caa3996`.
- Accepted inputs: `requirements.md`, `plan.md`, `data-model.md`, and
  `contracts/api.md` under
  `specs/004-architecture-engineering-principles/`.
- Accepted versions: module-boundary schema `1.0`, architecture-decision
  schema `1.0`, and persistence manifest `2.1.0`.
- Consumed boundary: the exact nine-project modular monolith, one
  Infrastructure.SqlServer-owned DbContext, composition-only API, pure module
  Domain code, narrow ports, atomic shared audit writer, two stateless replica
  support, and SQL-backed Data Protection key persistence protected by an
  external local certificate. Production key authorities remain fail closed.

## SPEC-005 persistence and lifecycle baseline

- Accepted design-contract commit:
  `d88892f97e2164fc3ee60b530187c1f4a658ceb4`; this is not a runtime migration,
  restoration, performance, or release-evidence completion claim.
- Accepted inputs: `requirements.md`, `plan.md`, `data-model.md`, and
  `contracts/api.md` under `specs/005-erd-data-lifecycle/`.
- Accepted versions: persistence manifest `2.1.0`, unique invariants `1.0`,
  check constraints `1.0`, concurrency tokens `1.0`, and immutable history
  `1.0`.
- Consumed boundary: migration-first SQL Server 2022 Developer compatibility
  160, isolated Testcontainers Testing databases, persistent-until-reset
  Development, deterministic synthetic logical fixtures, hash-only durable
  credentials, controlled migrations, and fail-closed production retention,
  topology, deployment-window, backup, and restore authority.

## SPEC-006 shared-contract baseline

- Accepted shared-foundation commit:
  `df6774ce57157b05b4af21380ce8afd2412ce422`.
- Accepted inputs: `requirements.md`, `plan.md`, `data-model.md`, and
  `contracts/api.md` under `specs/006-domain-class-api-contracts/`.
- Accepted versions: ApiError schema `1.0`, Page schema `1.0`, specification
  manifest `2.0.2`, entity-ownership manifest `2.0.1`, and workstream manifest
  `2.0.1`.
- Consumed boundary: privacy-safe errors, DTO isolation, deterministic JSON,
  page default `20`/maximum `100`, optimistic-concurrency and idempotency
  metadata, TimeProvider, and complete context composition rules.
- Deferred-runtime boundary: generated OpenAPI, real endpoint response proof,
  downstream contributor pins, and release evidence remain pending and are not
  inherited by SPEC-018.

## Reconciled cross-cutting baseline

- Blocking POC load evidence is exactly the 10-minute target at 75
  submissions/s plus 300 reads/s and the 60-second 200-submissions/s spike,
  both across at least two stateless replicas. Existing 2x, 5x, and 120-minute
  soak profiles are optional diagnostics and cannot relax correctness.
- Required correctness remains zero overbooking, zero duplicate active
  offering enrollment, and zero partial atomic submission.
- The direct graph `001/003/004/005/006 -> 018` is valid and acyclic. No
  dependency grants production deployment, official AASTMT authority, route
  implementation, downstream feature completion, or Gate B-D/release
  approval.
