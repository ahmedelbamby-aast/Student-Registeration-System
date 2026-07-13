# SPEC-018: Quality, Security, Scalability, and Operations

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** Approved for demo implementation by Ahmed ELbamby on 2026-07-13<br>
**Owner:** QA, DevOps, and Security Leads<br>
**Reviewers:** All leads, Product Owner, Registrar<br>
**Target:** Sprint 0-S8<br>
**Dependencies:** SPEC-001, SPEC-003, SPEC-004, SPEC-005, SPEC-006<br>

## Context

Correct functional behavior is insufficient if registration fails under peak
load, exposes student data, becomes inaccessible, or cannot recover. This
cross-cutting spec defines measurable gates. Its approved POC platform and load
values are engineering targets, not an AASTMT production forecast or SLA.

## Functional Requirements

- FR-1: CI MUST run restore, formatting, warnings-as-errors build, unit,
  architecture, real-SQL integration, migration, E2E, and security checks.
  Real-SQL tests MUST use SQL Server 2022 Developer at compatibility level 160
  through Docker/Testcontainers. Every run MUST create its own isolated
  Testing database, apply migrations before the synthetic seed, verify it, and
  dispose it without sharing Development or production state.
- FR-2: Critical domain rules and capacity logic MUST have automated boundary
  and concurrency tests before implementation is accepted. Those tests MUST
  cover deterministic logical fixture rebuilds, idempotent re-seed, explicit
  reset, and rejection of seed/reset outside Development or Testing. Testing
  databases MUST be disposed per run; Development MUST persist until an
  explicit guarded reset. All data MUST be synthetic. Git-ignored local
  credentials, logs, and exports MUST be purged within seven days.
- FR-3: The application MUST expose authenticated-safe health, logs, metrics,
  traces, and correlation IDs.
- FR-4: Operations MUST monitor latency, throughput, unexpected error rate,
  business rejection codes, optimizer time, SQL latency, lock waits, deadlocks,
  capacity conflicts, and counter reconciliation.
- FR-5: Backup/restore, migration rollback, and application rollback MUST be
  rehearsed before release.
- FR-6: POC secrets MUST come from .NET User Secrets or environment variables
  and MUST NOT appear in Git, checked-in configuration, or logs. The local Data
  Protection certificate MUST be generated outside Git. The production secret
  provider remains undecided and outside POC scope. Generated PIN/password
  plaintext MUST remain transient: only an ASP.NET Identity password hash may
  persist, and plaintext credentials MUST NOT appear in SQL, migrations,
  fixtures, snapshots, telemetry, or evidence.
- FR-7: Application replicas MUST remain stateless and use one shared SQL
  Server Data Protection key repository protected at rest by the generated
  local certificate from FR-6. Cross-replica authentication and protected
  option-token tests MUST pass with no sticky session. This POC mechanism MUST
  NOT be presented as the undecided production protection provider.
- FR-8: Critical flows MUST pass automated accessibility checks plus manual
  keyboard and representative NVDA/Windows screen-reader journeys. A dated
  evidence record MUST identify tester, assistive technology/version, route,
  scenario, result, defect links, and UX/QA sign-off; automation alone cannot
  satisfy this requirement. Browser gates MUST cover current stable Chrome,
  Edge, and Firefox plus a pinned Playwright WebKit version. WebKit MUST NOT be
  labeled Safari; actual Safari/macOS validation is deferred.
- FR-9: A versioned STRIDE threat model MUST cover trust boundaries, assets,
  identity/session, authorization/data scope, protected option tokens,
  registration races, Admin/audit/export, SQL, telemetry, secrets, and
  deployment. Security review MUST record mitigations, residual risk, and
  owner. Release MUST be blocked by an unreviewed/stale threat model,
  unresolved critical/high security issue, invariant failure, or
  critical/major core usability defect.

## Non-Functional Requirements

- NFR-1: The POC validation environment MUST support 25,000 synthetic accounts
  and 5,000 concurrent authenticated sessions. It MUST use the same versioned
  synthetic logical fixture generator as Development/Testing at a larger
  profile size; no real person or institutional record is permitted.
- NFR-2: The blocking POC load suite MUST run target traffic for 10 minutes at
  75 registration submissions/s plus 300 reads/s, and a 60-second spike at 200
  registration submissions/s, across at least two API replicas. Existing 2x,
  5x, and soak profiles MAY run as non-blocking diagnostics and MUST NOT block
  POC completion.
- NFR-3: Catalogue p95 MUST be <= 300 ms, commit p95 MUST be <= 2 s, and
  optimizer p95 MUST be <= 500 ms for the approved workload.
- NFR-4: Tests MUST demonstrate zero overbooking, zero duplicate active
  offering enrollment, and zero partial atomic submissions at the mandatory
  target, 200/s spike, and replica-failover profiles.
- NFR-5: Mandatory target and 200/s spike tests MUST run across at least two
  stateless API replicas. Optional 2x, 5x, or 120-minute soak results are
  diagnostic only; failures in those optional profiles MUST NOT block the POC.
- NFR-6: Unexpected server failure rate MUST be < 0.1% at target load.
- NFR-7: RPO MUST be <= 5 minutes and RTO <= 1 hour.
- NFR-8: Critical flows MUST meet WCAG 2.2 AA.
- NFR-9: Eligibility/conflict/capacity code SHOULD reach >= 90% branch
  coverage; coverage never replaces behavior tests.

## Acceptance Criteria

### AC-1: Target load (FR-4, NFR-2, NFR-3, NFR-4)
Given a production-like database and the exact NFR-2 target traffic mix<br>
When 75 submissions/s plus 300 reads/s run for 10 minutes<br>
Then p95 budgets and unexpected error rate pass<br>
And no capacity/duplicate/partial invariant fails.

### AC-2: Approved spike load (NFR-2, NFR-4, NFR-5)
Given target correctness already passes<br>
When 200 registration submissions/s run for 60 seconds across at least two API
replicas<br>
Then invariant correctness remains zero-defect<br>
And any graceful degradation is documented against approved POC thresholds.

### AC-3: Restore rehearsal (FR-5, NFR-7)
Given a production-like backup and clean recovery environment<br>
When the runbook is executed<br>
Then data is restored within RTO<br>
And measured data loss is within RPO<br>
And integrity/reconciliation checks pass.

### AC-4: Accessibility gate (FR-8, NFR-8)
Given critical student/staff routes in staging<br>
When automated checks and the recorded manual keyboard and NVDA journeys run<br>
Then no serious automated issue or critical/major manual barrier remains<br>
And the dated tester/tool/route/result/defect/sign-off evidence is complete.

### AC-5: Security release gate (FR-6, FR-9)
Given the versioned STRIDE model plus dependency, secret, static/dynamic and
authorization reviews are complete<br>
When release readiness is evaluated<br>
Then the threat model has owners/mitigations/residual-risk approval and no
unresolved critical/high finding remains<br>
And repository/database/telemetry/evidence scans find no plaintext generated
PIN/password or full student profile<br>
And protected resources pass negative ownership/role tests.

### AC-6: CI quality sequence (FR-1)
Given a pull request changes application behavior<br>
When CI executes<br>
Then restore/format/build, unit/architecture, SQL integration/migration,
E2E/accessibility and security checks run in the approved order<br>
And SQL Server 2022 Developer compatibility level 160 runs through
Docker/Testcontainers with a fresh per-run Testing database migrated before an
idempotent synthetic seed and disposed afterward<br>
And any required gate failure blocks merge.

### AC-7: Complete operational proof (FR-2, FR-3, FR-7, NFR-1, NFR-5, NFR-6, NFR-9)
Given the versioned synthetic 25,000-account/5,000-session fixture, SQL Server
2022 Developer compatibility level 160 through Docker/Testcontainers, and at
least two stateless replicas with the approved local shared-key setup<br>
When the required POC evidence pipeline and target/spike suites execute<br>
Then every boundary/concurrency test passes<br>
And safe health/log/metric/trace signals are available<br>
And unexpected failure rate is below 0.1%<br>
And critical rule branch coverage is at least 90%.

## Edge Cases

- EC-1: Observability exporter unavailable -> application remains functional
  with bounded buffering/fallback logs.
- EC-2: One app instance fails -> load balancer removes it; other instance
  continues with shared auth keys/database.
- EC-3: SQL unavailable -> fail safely, no partial result; health turns
  unhealthy and user gets reference ID.
- EC-4: Migration validation differs from production compatibility, or a
  bootstrap/reset environment guard does not prove Development/Testing -> stop
  before application traffic or any destructive/seed mutation.
- EC-5: Load targets prove unrealistic -> rebaseline by approved spec change,
  never silently relax correctness.

## API Contracts

```typescript
interface HealthSummary {
  status: "healthy" | "degraded" | "unhealthy";
  version: string;
  timestampUtc: string;
}
interface OperationalMetric {
  name: string;
  value: number;
  observedAtUtc: string;
  dimensions: Record<string, string>;
}
```

GET /api/health exposes only the safe HealthSummary. GET /api/operations/metrics
is restricted. Health detail and metrics MUST expose no secrets/topology.

## Data Models

| Operational data | Retention/handling |
|---|---|
| Structured log | Git-ignored local artifact; safe metadata; no credentials/full student records; purge within seven days |
| Metric | Aggregated numerical/dimensional values |
| Trace | Correlation and timing with PII-minimized attributes |
| Backup | Encrypted, access-controlled, tested |
| Load/accessibility/security report | Versioned release evidence; local exports Git-ignored and purged within seven days |
| Non-production data profile | Synthetic-only fixture; SQL Server 2022 Developer compatibility 160 via Docker/Testcontainers; Testing disposed per run; Development persists to guarded reset |

## Out of Scope

- OS-1: Final production hosting/vendor/secret provider and actual Safari/macOS validation.
- OS-2: Kubernetes by default.
- OS-3: 24/7 SLO outside announced registration windows until approved.
- OS-4: Arbitrary collection of student PII in telemetry.
