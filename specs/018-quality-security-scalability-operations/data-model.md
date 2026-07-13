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
| Data Protection key | shared SQL repository; encrypted at rest; key material from approved secret-store interface |
| Threat model | version, scope/diagram, assets, STRIDE threats, mitigations, residual risk, owner/reviewer/date |
| Load evidence | code/data/config version, replicas, exact profile/mix/duration, latency/error/invariant results |
| Accessibility manual evidence | tester, date, route/scenario, keyboard, NVDA/version, result, defect links, UX/QA sign-off |
| Backup/restore evidence | backup IDs/times, restore environment, measured RPO/RTO, integrity/reconciliation result |
| Release evidence | versioned links and pass/fail for every release gate |

## Integrity and Retention Rules

- Evidence is immutable after sign-off; corrections create a superseding
  version with rationale.
- Production startup fails closed if shared key repository or required
  certificate/key material is unavailable.
- Telemetry uses server timestamps/correlation and explicit redaction tests.
- Evidence retention/access follows SPEC-005 and approved security policy;
  unknown institutional periods fail closed.
