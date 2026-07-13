# SPEC-005 Dependency Baseline

**Recorded:** 2026-07-13<br>
**Feature:** SPEC-005 ERD and Data Lifecycle<br>
**Result:** PASS

## SPEC-002 accepted contracts

- Completion commit: `bf3e2e34f0134428b199b6d2de12114c10cd656e`.
- Human approval: Ahmed ELbamby, 2026-07-13, non-production demo scope.
- Accepted artifacts: `policy-rulebook/1.0`, published profile
  `DEMO-POC-2026.1`, typed rule registry `policy-rule-types/1.0`, immutable
  policy/version decisions, deterministic boundary examples, and explicit
  official/demo/synthetic provenance classifications.
- Commit-pinned inputs: `spec.md`, `requirements.md`, `plan.md`,
  `data-model.md`, and `contracts/api.md` under
  `specs/002-aastmt-policy-rulebook/`.
- Consumed boundary: SPEC-005 preserves policy and source provenance in the
  relational contract. Runtime PolicySet/PolicyRule evaluation remains owned
  by SPEC-009.

## SPEC-004 accepted contracts

- Completion commit: `6b087936c0d8b6341c656ade698247561caa3996`.
- Human approval: Ahmed ELbamby, 2026-07-13, including the approved corrected
  executable baseline.
- Accepted artifacts: module-boundary schema `1.0`, architecture-decision
  schema `1.0`, persistence manifest `2.1.0`, entity-ownership manifest
  `2.0.0`, and the `Gate-A-2026-07-13` persistence boundary.
- Commit-pinned inputs: `spec.md`, `requirements.md`, `plan.md`,
  `data-model.md`, and `contracts/api.md` under
  `specs/004-architecture-engineering-principles/`.
- Consumed boundary: one nine-project modular monolith, one
  `StudentRegistrationDbContext`, module-owned entity/mapping contributions,
  Infrastructure.SqlServer-owned composition and migrations, EF Core/LINQ,
  and SQL Server 2022 Developer compatibility level 160 for Development and
  Testing only.
- `AuditEvent` write-model/mapping ownership remains SPEC-004; SPEC-017 owns
  authorized audit query/export behavior.

## Dependency conclusion

The direct graph is `SPEC-002 -> SPEC-005` and `SPEC-004 -> SPEC-005`; its
accepted transitive dependencies are acyclic. Both dependencies are complete
and their governed boundaries are sufficient for SPEC-005 design-time ERD and
data-lifecycle contracts. Production SQL edition/topology, deployment window,
retention, backup authority, and release approval remain unresolved and fail
closed at their later production gates.
