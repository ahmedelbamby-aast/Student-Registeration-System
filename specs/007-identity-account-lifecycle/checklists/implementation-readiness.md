# SPEC-007 Implementation Readiness

**Baseline state:** FROZEN AND APPROVED FOR DEPENDENCY-ORDERED DEMO WORK<br>
**Frozen:** 2026-07-14 under Ahmed ELbamby's Gate A approval

- [x] SPEC-003, SPEC-004, SPEC-005, SPEC-006, and SPEC-018 exact commits,
  versions, consumed contracts, and deferred-runtime boundaries are recorded
  in `dependency-baseline.md`.
- [x] AUTH-02 through AUTH-05, STU-08, ADM-03, and the SYS-01 contribution are
  mapped to approved SPEC-003 Page Design Records. Their current `design-only`
  and `not-pinned` states authorize test-first contributor work, not evidence
  claims; ADM-03 remains partially gated by SPEC-017.
- [x] `ActivateStudentRequest` consistently contains `universityId`,
  `initialPassword`, and `newPassword`; confirmation is client-only. Initial
  verification, password-hash replacement, and activation consumption are one
  atomic server transition.
- [x] `StudentActivation` is canonically owned by SPEC-007 in entity-ownership
  manifest `2.0.2`, appears in specification and persistence manifests, and
  references ApplicationUser rather than downstream Student.
- [x] `identity-persistence-owner-extension/1.1` and the canonical ERD freeze
  the exact security/lockout, recovery, assignment, abuse, import, and singleton
  guard fields before EF mapping; no field is left as an unmodeled runtime add-on.
- [x] Recovery delivery is provider-neutral and fail closed. The endpoint never
  returns the proof; an Identity-owned port supports an in-memory Testing
  adapter, a bounded Git-ignored Development adapter, and only an explicitly
  approved future production adapter.
- [x] The 14 FR, 4 NFR, 10 AC, 6 EC, 3 SC, four exclusions, 16 endpoints,
  seven route responsibilities, nine owned entities, eight workstreams, and
  T001-T133 execution trace are internally consistent.
- [x] IdentityAccess remains one bounded module in the nine-project modular
  monolith. It exposes narrow ports/endpoints, uses the single shared DbContext,
  and adds no broker, cache, microservice, generic repository, second identity
  system, or second factor.
- [x] Credentials and recovery proofs are transient or hashed as appropriate;
  no plaintext secret enters SQL, migrations, source, logs, telemetry,
  snapshots, or evidence. Development local artifacts are ignored and removed
  within seven days; Testing uses isolated in-memory/process inputs.
- [x] Shared SQL state and Data Protection keys preserve authentication,
  security-stamp invalidation, role state, challenge use, and abuse controls
  across at least two stateless replicas without sticky-session correctness.
- [x] SPEC-007 owns its complete EF mapping contribution but no migration.
  SPEC-008 retains sole ownership of `S1IdentityAcademicFoundation` after both
  identity and academic mappings are implemented.
- [x] ApiError, DTO isolation, pagination, concurrency, idempotency,
  antiforgery, cookie, TimeProvider, AppContext, audit-transaction, and
  production fail-closed rules are frozen for test-first delivery.
- [x] Public Identity DTOs use the bounded `Contracts.Identity` namespace,
  expose one aggregate `expectedRowVersion`, and expose no security stamp,
  password/recovery hash, or EF internal. Workstream manifest `2.0.2` records
  the corrected activation, recovery, security, and Admin-race intent.
- [x] Generated OpenAPI, downstream contributor pins, real browser and
  accessibility evidence, measured load/session evidence, complete SQL
  migration execution, traceability, and seven-perspective release approval
  remain later tasks and must not be reported as passing early.
- [x] Ahmed ELbamby's approval authorizes these best-practice demo decisions.
  Production, official AASTMT, and Gate B-D/release authority remain separate.

Dependency-ordered implementation may start after T001-T007 are recorded as
complete. Every model, service, handler, and page delivery still follows its
designated failing test. Any requirement, ownership, endpoint/route,
dependency, persistence, production-authority, or deployment-shape change
requires renewed analysis and Ahmed ELbamby's approval.
