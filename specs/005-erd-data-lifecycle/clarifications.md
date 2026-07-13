# Clarification Record: ERD and Data Lifecycle

**Reviewed**: 2026-07-13
**Automated result**: PASS - no hidden NEEDS CLARIFICATION marker.
**Human approval**: APPROVED by Ahmed ELbamby on 2026-07-13 for Gate A demo implementation

## Resolved demo database decision

Ahmed approved isolated migration-first Development and per-run Testing SQL
databases populated only with versioned synthetic deterministic logical
fixtures. Re-seeding is idempotent; reset is explicit and guarded to those two
environments. Generated PIN/password plaintext is transient and only ASP.NET
Identity hashes may be persisted or observed in durable evidence.

Ahmed also approved SQL Server 2022 Developer compatibility level 160 through
Docker for Development and Testcontainers for per-run Testing. Testing is
disposed after each run; Development persists until guarded reset; real data
is prohibited; and Git-ignored local credential artifacts, logs, and exports
are removed within seven days. This does not approve a production edition or
topology.

The specification was reviewed for scope, actors, data, business rules, errors,
concurrency, security, accessibility, dependencies, and measurable outcomes.
Unknown product/institutional decisions are registered in
docs/OPEN_DECISIONS.md or docs/POLICY_RESEARCH.md with an owner and fail-closed
planning rule. They are not invented requirements; unresolved production-only
data-retention, migration-window, and deployment decisions remain fail closed
at their applicable later gate and do not revoke this demo implementation
approval.
