# SPEC-004 Non-Production Demo Release Approval

**Status:** APPROVED
**Candidate:** SPEC-004 Architecture and Engineering Principles
**Decision authority:** Ahmed ELbamby
**Decision date:** 2026-07-13
**Approval scope:** Non-production design-capability demo architecture and its
executable conformance evidence only
**Production authority:** NOT APPROVED; fail closed

## Approved decision

Ahmed ELbamby approves SPEC-004 as a completed non-production demo
architecture slice. The decision accepts the simple project-per-business-module
modular monolith, the single SQL persistence boundary, the stateless-replica
seams, the bounded local lifecycle, and the transaction-aware audit foundation
proved by the T049 traceability gate.

This decision closes T050 only at the scope owned by SPEC-004. It does not
release the full Student Registration System, authorize an AASTMT Production
environment, waive Gates B-D, or approve a Production SQL Server edition,
topology, key store, certificate custodian, hosting platform, or deployment.

## Review perspectives

Ahmed is the project's sole developer and sole human approval authority. The
roles below are distinct review perspectives he evaluated; they do not identify
additional people or imply external institutional approval.

| Review perspective | Evidence and finding | Candidate disposition |
|---|---|---|
| Product Owner | The T049 matrix maps every SPEC-004 FR, NFR, AC, EC, and SC to passing evidence. The exact nine-project modular monolith, one deployable API, one database boundary, and explicit exclusions preserve the approved simple, extensible MVP scope. | APPROVED for the SPEC-004 demo slice |
| Domain owner / Technical Lead | Module ownership, the acyclic dependency allowlist, narrow ports, DTO isolation, the composition-only API, direct EF Core/LINQ use, prohibited-complexity rules, and ADR governance match the approved architecture domain. | APPROVED for the SPEC-004 demo slice |
| QA | `TraceabilityEvidenceTests` passed 2/2 and rejects missing, duplicate, evidence-free, or non-PASS rows. Focused architecture, acceptance, integration, contract, and quality evidence covers the required stack, fault-injected dependency rules, stateless key ring, local retention, domain purity, and real-SQL audit commit/fault/rollback paths. Any later regression still rejects release. | APPROVED at the current SPEC-004 evidence boundary |
| Security | The demo uses one SQL-backed Data Protection key ring protected by an external certificate, has no sticky-session correctness dependency, exposes no key endpoint, and fails Production startup when repository or encryption authority is absent. Safe audit summaries remain caller obligations. Production repository/provider selection, secret storage, application-identity grants, certificate custody, backup, recovery, and rotation require separate institutional Security/DevOps authority. | APPROVED for demo controls; Production NOT APPROVED and FAIL CLOSED |
| Accessibility | SPEC-004 owns no frontend route or interactive control; the traceability matrix records `ROUTE-NONE`. The architecture creates no accessibility behavior to accept or reject. WCAG 2.2 AA, keyboard, responsive, browser, and usability evidence remains with SPEC-003, the route-owning features, and later release gates. | NOT APPLICABLE; boundary ACCEPTED without waiving downstream gates |
| Data / concurrency | One Infrastructure-owned `StudentRegistrationDbContext` preserves the local transaction boundary. Real SQL evidence proves business state and append-only audit commit together and both roll back on an injected audit failure or caller rollback; the writer never starts or commits its own transaction. This does not approve Production SQL topology or replace SPEC-014 seat-allocation and collision gates. | APPROVED for the SPEC-004 persistence and atomic-audit boundary |
| Operations | Docker Development and disposable Testcontainers Testing use the approved SQL Server 2022 Developer compatibility-160 demo profile. The key-rotation/recovery and repository-bounded seven-day artifact-retention runbooks are present, and Production authority remains guarded. Production hosting, replica orchestration, SQL topology, certificate operations, backup/restore, recovery objectives, observability, and deployment remain later SPEC-018/Gate-D decisions. | APPROVED for non-production demo operations; Production NOT APPROVED |

## Evidence admitted to the decision

- `SPEC-004-traceability.md` records PASS for FR-1 through FR-9, NFR-1
  through NFR-4, AC-1 through AC-7, EC-1 through EC-3, SC-1 through SC-3,
  and the no-owned-route boundary. Its automated gate passed 2/2.
- `SPEC-004-NFR-1.md` through `SPEC-004-NFR-4.md` contain measurable
  dependency, lifecycle, two-replica, and domain-purity evidence.
- `SPEC-004-scope-review.md` confirms that independent module deployment,
  Kubernetes/service mesh, separate read/write databases, and distributed
  transaction protocols remain excluded.
- `docs/architecture/module-boundaries.md`,
  `docs/architecture/persistence-boundary.md`, and `docs/adr/README.md` define
  the governed architecture and change process.
- `ops/runbooks/data-protection-keys.md` and
  `ops/runbooks/local-artifact-retention.md` bound the approved demo operating
  procedures while preserving the Production authority gate.
- The real-SQL atomic-audit tests prove caller-owned commit, injected-failure
  rollback, caller rollback, append-only mapping, and independence from the
  downstream SPEC-017 query/export capability.

## Explicitly retained fail-closed decisions

The following are not granted by Ahmed's demo approval and must not be inferred
from a passing SPEC-004 gate:

1. Production SQL Server edition, licensing, primary/replica topology,
   connection authority, or database deployment.
2. Production Data Protection repository, secret provider, application
   identity and least-privilege grants, certificate issuance/custody, key
   backup, rotation, restore, or recovery authority.
3. Production hosting, load balancer, replica orchestration, observability,
   backup/restore operations, release execution, or deployment authority.
4. Institutional Security/DevOps sign-off, official AASTMT architecture or
   policy authorization, the full-system release, or Gates B-D.

Production startup therefore continues to reject absent repository and
encryption approvals with
`PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED`. Gate D and SPEC-018 must later
prove the production-like security, accessibility, load, recovery, and
operations conditions applicable to the complete system.

## Approval record

**Decision:** APPROVED for the non-production SPEC-004 demo architecture
**Approved by:** Ahmed ELbamby
**Decision date:** 2026-07-13
**Basis:** Ahmed ELbamby authorized the simplest best-practice demo decisions
that preserve modularity and scalability without unnecessary complexity. The
completed T049 PASS matrix supplies the evidence for that decision.

This closes T050 for SPEC-004 without changing any Production, institutional,
downstream feature, or full-system release gate.
