# Operational Contracts and Release-Evidence Model

## Owned Runtime Contracts

- **HealthSummary**: Public safe health response.
- **OperationalMetric**: Restricted, PII-minimized metric contract.

## Governed Telemetry and Evidence Artifacts

- **StructuredLog** and **Trace** are governed telemetry schemas, not domain
  entities.
- **BackupEvidence**, **ReleaseEvidence**, **ThreatModel**,
  **LoadTestEvidence**, and **AccessibilityManualEvidence** are versioned
  release artifacts, not application EF entities.

## Detailed Model

| Artifact/contract | Required handling |
|---|---|
| Structured log | correlation, safe metadata, bounded fallback; no credentials/full records |
| Metric | aggregate name/value/dimensions/observed time; controlled cardinality |
| Trace | correlation and timing with allow-listed PII-minimized attributes |
| Data Protection key | shared SQL repository; POC protected by generated local certificate outside Git; production provider undecided |
| Threat model | version, scope/diagram, assets, STRIDE threats, mitigations, residual risk, owner/reviewer/date |
| Load evidence | code/data/config version, replicas, exact profile/mix/duration, latency/error/invariant results |
| Accessibility manual evidence | tester, date, route/scenario, keyboard, NVDA/version, result, defect links, UX/QA sign-off |
| Backup/restore evidence | backup IDs/times, restore environment, measured RPO/RTO, integrity/reconciliation result |
| Release evidence | versioned links and pass/fail for every release gate |
| Non-production data profile | synthetic-only; SQL Server 2022 Developer compatibility 160 via Docker/Testcontainers; per-run Testing disposal; guarded Development reset |

## Integrity and Retention Rules

- Evidence is immutable after sign-off; corrections create a superseding
  version with rationale.
- POC startup fails closed if its shared key repository or generated local
  certificate is unavailable; production protection remains undecided.
- Telemetry uses server timestamps/correlation and explicit redaction tests.
- Each Testing database has a unique run identity and cannot reuse the
  Development database. Database readiness requires migrations then the
  complete versioned synthetic seed; failed/partial bootstrap is not ready.
- Seed is idempotent for one profile version. Reset is separately invoked and
  requires positive Development/Testing environment and connection-target
  validation before destructive work.
- Fixture determinism applies to logical IDs/relationships/academic values,
  not SQL rowversions or salted ASP.NET Identity password-hash bytes.
- Plaintext PIN/passwords and full student profiles are forbidden in durable
  operational/evidence artifacts.
- Git-ignored local credentials, logs, and exports are purged within seven days.
- Required browser evidence names current stable Chrome, Edge, Firefox, and the
  pinned WebKit version; it never labels WebKit as Safari.
- Evidence retention/access follows SPEC-005 and approved security policy;
  unknown institutional periods fail closed.
