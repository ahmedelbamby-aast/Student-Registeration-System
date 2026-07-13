# Clarification Record: Quality, Security, Scalability, and Operations

**Reviewed**: 2026-07-14
**Automated result**: PASS - no hidden NEEDS CLARIFICATION marker.
**Human approval**: APPROVED by Ahmed ELbamby on 2026-07-13 for non-production demo implementation (Gate A)

## Resolved demo fixture decision

Ahmed approved isolated migration-first Development and per-run Testing SQL
databases using versioned deterministic logical synthetic fixtures, idempotent
seed, and explicit guarded reset. Generated PIN/password plaintext is transient;
only ASP.NET Identity hashes may persist, and no credential/full student
profile may enter logging, tracing, snapshots, or test/release evidence.

## Resolved Q5 operating profile

Ahmed approved SQL Server 2022 Developer compatibility level 160 through
Docker/Testcontainers; current Chrome/Edge/Firefox plus pinned Playwright
WebKit (not labeled Safari); 25,000 synthetic accounts/5,000 sessions; blocking
75 submissions/s + 300 reads/s target and 200 submissions/s spike across at
least two replicas; per-run Testing disposal; guarded Development reset;
Git-ignored local artifacts purged within seven days; User Secrets/environment
variables; and SQL-backed shared keys protected by a generated local
certificate outside Git. Production secret/provider and actual Safari/macOS
validation remain outside POC scope.

The specification was reviewed for scope, actors, data, business rules, errors,
concurrency, security, accessibility, dependencies, and measurable outcomes.
Unknown production or release decisions remain registered in
docs/OPEN_DECISIONS.md or docs/POLICY_RESEARCH.md with an owner and fail-closed
rule. Gate A does not resolve or waive those later Gate B-D obligations and is
not official AASTMT production authorization.

Ahmed ELbamby reverified the Gate A approval on 2026-07-14 after the
consistency-only load/task correction. The approval remains current for
non-production demo implementation; Gate B-D, release, and production
authority remain separate.
