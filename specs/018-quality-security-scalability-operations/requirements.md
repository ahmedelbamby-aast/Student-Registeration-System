# SPEC-018: Quality, Security, Scalability, and Operations

**Author:** Ahmed ELbamby<br>
**Date:** 2026-07-12<br>
**Status:** In Review<br>
**Owner:** QA, DevOps, and Security Leads<br>
**Reviewers:** All leads, Product Owner, Registrar<br>
**Target:** Sprint 0-S8<br>
**Dependencies:** SPEC-001, SPEC-003, SPEC-004, SPEC-005, SPEC-006<br>

## Context

Correct functional behavior is insufficient if registration fails under peak
load, exposes student data, becomes inaccessible, or cannot recover. This
cross-cutting spec defines measurable gates. Initial load values are hypotheses
until AASTMT provides enrollment/traffic forecasts.

## Functional Requirements

- FR-1: CI MUST run restore, formatting, warnings-as-errors build, unit,
  architecture, real-SQL integration, migration, E2E, and security checks.
- FR-2: Critical domain rules and capacity logic MUST have automated boundary
  and concurrency tests before implementation is accepted.
- FR-3: The application MUST expose authenticated-safe health, logs, metrics,
  traces, and correlation IDs.
- FR-4: Operations MUST monitor latency, throughput, unexpected error rate,
  business rejection codes, optimizer time, SQL latency, lock waits, deadlocks,
  capacity conflicts, and counter reconciliation.
- FR-5: Backup/restore, migration rollback, and application rollback MUST be
  rehearsed before release.
- FR-6: Production secrets MUST use an approved secret store and MUST NOT
  appear in Git/config/logs.
- FR-7: Application replicas MUST share Data Protection keys and remain
  stateless.
- FR-8: Critical flows MUST pass automated and manual accessibility tests.
- FR-9: Release MUST be blocked by unresolved critical/high security issues,
  invariant failures, or critical/major core usability defects.

## Non-Functional Requirements

- NFR-1: The production-like validation environment MUST support a planning
  baseline of 25,000 accounts and 5,000 concurrent authenticated sessions,
  pending S0 rebaseline.
- NFR-2: The system MUST support 75 submissions/s for 10 min, 200/s for 60 s,
  and 300 read/s.
- NFR-3: Catalogue p95 MUST be <= 300 ms, commit p95 MUST be <= 2 s, and
  optimizer p95 MUST be <= 500 ms for the approved workload.
- NFR-4: Tests MUST demonstrate zero overbooking, duplicate active offering
  enrollment, and partial atomic submission.
- NFR-5: Availability MUST be 99.9% during announced registration windows.
- NFR-6: Unexpected server failure rate MUST be < 0.1% at target load.
- NFR-7: RPO MUST be <= 5 minutes and RTO <= 1 hour.
- NFR-8: Critical flows MUST meet WCAG 2.2 AA.
- NFR-9: Eligibility/conflict/capacity code SHOULD reach >= 90% branch
  coverage; coverage never replaces behavior tests.

## Acceptance Criteria

### AC-1: Target load (FR-4, NFR-2, NFR-3, NFR-4)
Given a production-like database and target traffic mix<br>
When target load runs for the specified duration<br>
Then p95 budgets and unexpected error rate pass<br>
And no capacity/duplicate/partial invariant fails.

### AC-2: Double and spike load (NFR-2, NFR-4)
Given target correctness already passes<br>
When 2x target and a short 5x spike run<br>
Then invariant correctness remains zero-defect<br>
And any graceful degradation is documented against approved thresholds.

### AC-3: Restore rehearsal (FR-5, NFR-7)
Given a production-like backup and clean recovery environment<br>
When the runbook is executed<br>
Then data is restored within RTO<br>
And measured data loss is within RPO<br>
And integrity/reconciliation checks pass.

### AC-4: Accessibility gate (FR-8, NFR-8)
Given critical student/staff routes in staging<br>
When automated, keyboard, and representative screen-reader tests run<br>
Then no serious automated issue or critical/major manual barrier remains.

### AC-5: Security release gate (FR-6, FR-9)
Given dependency, secret, static/dynamic and authorization reviews complete<br>
When release readiness is evaluated<br>
Then no unresolved critical/high finding remains<br>
And protected resources pass negative ownership/role tests.

### AC-6: CI quality sequence (FR-1)
Given a pull request changes application behavior<br>
When CI executes<br>
Then restore/format/build, unit/architecture, SQL integration/migration,
E2E/accessibility and security checks run in the approved order<br>
And any required gate failure blocks merge.

### AC-7: Complete operational proof (FR-2, FR-3, FR-7, NFR-1, NFR-5, NFR-6, NFR-9)
Given the 25,000-account/5,000-session production-like fixture, two stateless
replicas with shared Data Protection keys, observability collectors, and the
critical eligibility/conflict/capacity suites<br>
When the release evidence pipeline and registration-window soak execute<br>
Then every boundary/concurrency test passes<br>
And safe health/log/metric/trace signals are available<br>
And availability is at least 99.9% during the test window<br>
And unexpected failure rate is below 0.1%<br>
And critical rule branch coverage is at least 90%.

## Edge Cases

- EC-1: Observability exporter unavailable -> application remains functional
  with bounded buffering/fallback logs.
- EC-2: One app instance fails -> load balancer removes it; other instance
  continues with shared auth keys/database.
- EC-3: SQL unavailable -> fail safely, no partial result; health turns
  unhealthy and user gets reference ID.
- EC-4: Migration validation differs from production compatibility -> stop
  deployment before application traffic.
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
| Structured log | Safe metadata; no credentials/full student records |
| Metric | Aggregated numerical/dimensional values |
| Trace | Correlation and timing with PII-minimized attributes |
| Backup | Encrypted, access-controlled, tested |
| Load/accessibility/security report | Versioned release evidence |

## Out of Scope

- OS-1: Final production hosting/vendor selection.
- OS-2: Kubernetes by default.
- OS-3: 24/7 SLO outside announced registration windows until approved.
- OS-4: Arbitrary collection of student PII in telemetry.
