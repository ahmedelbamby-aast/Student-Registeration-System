# Implementation Plan: Quality, Security, Scalability, and Operations

**Branch**: 018-quality-security-scalability-operations | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Approved for non-production demo implementation by Ahmed ELbamby on 2026-07-13 (Gate A).

## Summary

Create the cross-cutting release gates and POC evidence for a
single-deployable modular monolith: CI, threat model, accessibility, safe
telemetry, two-replica session protection, exact load profiles, recovery, and
release blocking. The approved POC targets are not production sizing or an SLA.

## Technical Context

- **Runtime**: .NET 10, ASP.NET Core/Blazor WebAssembly, EF Core, SQL Server
  2022 Developer compatibility level 160 via Docker/Testcontainers.
- **Composition**: health/security/telemetry wiring in
  `StudentRegistration.Api/Operations`; feature modules emit only defined
  signals.
- **Replica keys**: shared SQL Server Data Protection repository protected by a
  generated local certificate outside Git; secrets come from User Secrets or
  environment variables. The production provider is undecided/out of POC.
- **Testing**: xUnit, real-SQL integration, Playwright/E2E, accessibility tools
  plus manual screen-reader evidence, authorization/security analysis, load,
  fault injection, restore rehearsal, and isolated per-run Testing databases
  migrated before deterministic synthetic seed.
- **Evidence**: immutable/versioned reports under `docs/release-evidence`;
  application operational records are not domain entities unless explicitly
  stated.

## Workstreams and Order

1. Baseline SPEC-001/003/004/005/006 and complete cross-spec consistency.
2. Freeze the threat model, CI gate order, telemetry schema, exact load
   profiles, non-production database/fixture contract, credential leakage
   controls, Data Protection/secret design, accessibility protocol, and
   recovery evidence against Ahmed ELbamby's recorded 2026-07-13 Gate A
   approval.
3. Write failing schemas, contracts, acceptance, failure, authorization,
   cross-replica key, accessibility, and load-harness tests.
4. Implement CI/security and shared-key configuration.
5. Implement health, logging, metrics, tracing, and degraded behavior.
6. Rehearse recovery/rollback and execute the blocking target and 200/s spike
   across at least two replicas; keep 2x/5x/soak runs optional and non-blocking.
7. Record signed manual keyboard/screen-reader evidence and threat-model review.
8. Block release unless every correctness, security, accessibility, recovery,
   and evidence gate passes.

## Design Decisions

### Exact Load Profiles

- **Target**: 10 minutes at 75 submissions/s plus 300 reads/s.
- **Required spike**: 60 seconds at 200 registration submissions/s.
- **Replica floor**: at least two stateless API replicas for target and spike.
- **Read mix**: 50% offering discovery, 25% eligibility detail, 15% current
  plan/timetable, 10% registration-record reads.
- **Submission mix**: 70% valid unique requests, 20% expected full/policy/
  conflict rejections, 10% idempotent retries/lost-response recovery.
- **Optional diagnostics**: existing 2x, 5x, and 120-minute soak profiles may
  run for evidence but do not block POC completion.

### Non-production SQL and fixture safety

- Development uses its isolated database; each automated run receives a
  uniquely named isolated Testing database that is disposed after the run.
- The approved Code First migrations finish before the synthetic seed runs.
  The seed is versioned, logically deterministic, and idempotent; reset is a
  separate explicit Development/Testing-only operation.
- Generated credential plaintext is transient. Only ASP.NET Identity hashes
  may reach SQL; credentials and full student profiles are prohibited from
  source fixtures, migrations, snapshots, telemetry, and evidence.
- Logical fixture equality excludes SQL rowversions and salted password-hash
  bytes; tests verify credentials through the approved hasher.
- Development persists until an explicit guarded reset; all data is synthetic.
- Local credentials, logs, and exports remain Git-ignored and are purged within
  seven days.

### Browser matrix

- Current stable Chrome, Edge, and Firefox are required.
- A pinned Playwright WebKit version is required but is never labeled Safari.
- Actual Safari/macOS validation is deferred outside the POC.

## Constitution and Approval Gate

Gate A authorizes non-production demo implementation while dependency,
threat-model, policy-provenance, and consistency baselines remain valid. Gate
B-D evidence, including security, accessibility, load, recovery, and release
approval, remains mandatory before the corresponding release milestones. Gate
A is not production deployment authorization.

## Artifacts

- [Requirements](requirements.md)
- [Research](research.md)
- [Data/evidence model](data-model.md)
- [API contract](contracts/api.md)
- [Tasks](tasks.md)

## Complexity Tracking

The plan reuses SQL Server for shared keys and durable state. Kubernetes,
message brokers, service decomposition, and vendor-specific hosting remain out
of scope.
