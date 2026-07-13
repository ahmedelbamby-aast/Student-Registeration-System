# SPEC-004 out-of-scope review

- Review date: 2026-07-13
- Reviewer and sole project approver: Ahmed ELbamby
- Scope: `src`, tests, SPEC-004 contracts, project references, migrations,
  composition/routes, and local deployment assets
- Result: OS-1 through OS-4 remain excluded from the MVP

## Inspection method

The review enumerated all source projects and searched the scoped artifacts for
web hosts/routes, migrations and DbContext declarations, independent-deployment
or orchestration assets, split read/write database terms, and distributed
transaction APIs. It also compared the result with the executable module DAG,
persistence-boundary tests, and prohibited-complexity contract.

Observed facts:

- There are exactly nine governed source projects and only
  `StudentRegistration.Api` uses `Microsoft.NET.Sdk.Web`.
- `StudentRegistration.Api/Composition/ModuleRegistration.cs` is the only
  `WebApplication.CreateBuilder` composition root; no endpoint routes exist yet.
- No migration files exist yet. One production DbContext declaration exists:
  `StudentRegistrationDbContext` in `Infrastructure.SqlServer`.
- No Kubernetes, Helm, Istio, service-mesh, or independently deployable module
  asset exists. Matches in tests/contracts are rejection rules, not delivery.
- No read/write database split, `TransactionScope`, MSDTC, two-phase commit, or
  distributed transaction implementation exists. Matches in tests/contracts
  are rejection rules.

## OS-1 — Independent module deployments

**Excluded.** Business modules are in-process class libraries behind one API
composition root. The project-reference DAG enforces modular ownership without
creating network boundaries or separate deployment units. A future change
requires measured operational need, an ADR, and updated architecture tests.

## OS-2 — Kubernetes and service mesh

**Excluded.** The demo supports two stateless application replicas through
shared SQL state and a shared encrypted Data Protection key ring. Kubernetes,
service mesh, and distributed-lock infrastructure are neither necessary nor
present. Production hosting topology remains an explicit Security/DevOps gate.

## OS-3 — Separate read/write databases

**Excluded.** One SQL Server database and one
`StudentRegistrationDbContext` preserve the registration and audit transaction
boundary. Reads use projections and `AsNoTracking` rather than a second store.
Adding another database requires an approved replacement consistency design.

## OS-4 — Distributed transaction protocol

**Excluded.** Persistence work uses focused local EF/SQL transactions. No
distributed transaction coordinator, two-phase commit, or cross-service commit
protocol is present. Atomic audit work joins the caller's existing local
transaction and never creates or commits a second transaction.

## Release decision

The review passes only while all four exclusions remain true. The automated
architecture and prohibited-complexity gates must reject drift, and any request
to admit an excluded item requires Ahmed's approval plus an ADR and new tests.
