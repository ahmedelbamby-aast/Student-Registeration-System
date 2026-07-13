# Research: Quality, Security, Scalability, and Operations

## Decisions

### Modular operations boundary

**Decision**: Keep feature logic in business-module projects and place health,
telemetry, shared-key, and security composition in
`StudentRegistration.Api/Operations`. Evidence schemas are documentation/test
artifacts, not domain entities.

### Shared Data Protection keys

**Decision**: Use SQL Server as a shared ASP.NET Core Data Protection key
repository for every POC API replica. Protect keys with a generated local
certificate outside Git; obtain POC secrets from User Secrets or environment
variables. The production provider is undecided and out of POC scope.

**Rationale**: This supports cross-replica sessions and protected optimizer
tokens without sticky sessions or a new distributed service. The cloud/vendor
secret-store implementation remains a deployment decision.

### Exact load model

**Decision**:

- target: 10 minutes, 75 submissions/s + 300 reads/s;
- required spike: 60 seconds, 200 registration submissions/s;
- reads: 50% discovery, 25% eligibility, 15% plan/timetable, 10% records;
- submissions: 70% valid unique, 20% expected rejection, 10% idempotent retry;
- target and spike: at least two stateless API replicas;
- existing 2x, 5x, and 120-minute soak: optional non-blocking diagnostics.

Correctness has zero tolerance in every profile. Target latency/error SLOs apply
at target; higher loads must remain bounded, recover, and never violate
invariants.

### Isolated non-production databases and deterministic fixtures

**Decision**: Development uses an isolated SQL database and every automated
run creates a uniquely named Testing database, applies migrations, invokes the
same versioned synthetic logical fixture generator, verifies readiness, and
disposes the database. Re-seeding one profile is idempotent; reset is explicit
  and guarded to Development/Testing. Testing is disposed per run;
  Development persists until explicit guarded reset. All data is synthetic;
  Git-ignored local credentials, logs, and exports are purged within seven days.
**Rationale**: Tests stay parallel-safe and reproducible without allowing seed
or destructive paths to reach production.
**Alternatives rejected**: Shared mutable test databases, seed data in
migrations, implicit startup reset, and environment checks based only on a
caller-provided Boolean.

### Generated credential evidence boundary

**Decision**: PIN/password plaintext exists only in transient process memory
until ASP.NET Identity hashes it. Security tests inspect source artifacts, SQL,
snapshots, telemetry, and reports for plaintext credential and full-profile
leakage; hash verification replaces exact hash-byte comparisons.
**Rationale**: Salted secure hashes are intentionally nondeterministic while
the surrounding logical fixture remains repeatable.
**Alternatives rejected**: Deterministic unsalted hashes, plaintext fixture
files, credential logging, and full-profile test reports.

### Threat model

**Decision**: Maintain a versioned STRIDE threat model covering browser/API/SQL
trust boundaries, identity/session, role/data scope, protected tokens,
registration races, Admin/audit/export, telemetry, secrets, and deployment.
Every threat has mitigation, validation evidence, residual risk, owner, and
review date.

### Accessibility evidence

**Decision**: Automation is necessary but insufficient. Critical routes require
manual keyboard and NVDA/Windows journeys with tester/tool version, route,
scenario, result, defects, and UX/QA sign-off.

### Browser and SQL POC platform

**Decision**: Real-SQL tests use SQL Server 2022 Developer compatibility level
160 through Docker/Testcontainers. Browser gates use current stable Chrome,
Edge, and Firefox plus a pinned Playwright WebKit version. WebKit is not
Safari; actual Safari/macOS validation is deferred.

### Release blocking

**Decision**: CI blocks for required code gates. Release additionally blocks
when signed threat, manual accessibility, load, recovery, or security evidence
is absent/stale, or any invariant/critical defect fails.

## Open Research

Production enrollment/load, secret-store/hosting values, and actual
Safari/macOS validation remain future decisions. POC changes require an
approved spec revision; no institutional value is guessed.
