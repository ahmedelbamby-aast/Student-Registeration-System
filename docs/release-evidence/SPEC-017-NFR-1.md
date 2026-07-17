# SPEC-017 NFR-1 Metrics-Freshness Evidence

**Requirement:** Operational metrics SHOULD be no more than 60 seconds stale
and show the observation timestamp.  
**Source-under-test commit:** `e3c567f43e608597a66b9d8e5f1f0bcfc4b04594`  
**Measured:** 2026-07-17 12:00:00Z  
**Result: PASS.**

## Environment and command

- Operating system: Microsoft Windows 10.0.26200, X64
- .NET SDK: 10.0.301
- Target framework: net10.0
- Time zone: Africa/Cairo
- Execution mode: deterministic fixed-time in-process application query

```text
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec017.NFR_1EvidenceTests" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false
```

## Fixture and raw measurements

The fixture executes the real `AdminMetricsQuery` at one fixed UTC clock using
4 deterministic cases, 8 required metric series per case, and 32 metric
observations in total. Each case records the source state, source observation
age, expected and actual availability, and expected and actual observation
timestamp. The machine-readable measurements are checked in at
`docs/release-evidence/SPEC-017-NFR-1-measurements.json` and the automated test
replays every row against the application query.

| Case | Source | Age | Actual state | Actual observation time |
|---|---|---:|---|---|
| fresh-0-seconds | Available | 0 s | Live | 2026-07-17T12:00:00Z |
| fresh-boundary-60-seconds | Available | 60 s | Live | 2026-07-17T11:59:00Z |
| stale-61-seconds | Available | 61 s | Stale | 2026-07-17T11:58:59Z |
| unavailable-300-seconds | Unavailable | 300 s | Degraded | 2026-07-17T11:55:00Z |

## Threshold calculation

- `max(live observation age) = 60 seconds <= 60 seconds`
- `first stale observation age = 61 seconds`
- `observation timestamps preserved = true`

The 60-second boundary remains live; the first older available observation is
stale. An unavailable source is degraded and retains its last observation
timestamp rather than being presented as current or fabricated as zero.

## Scope

This is deterministic engineering evidence for the SPEC-017 application
freshness rule. It proves the boundary and timestamp behavior of the real query
with the complete required metric set. It is not a production telemetry-backend
availability or end-to-end polling-latency SLA claim.
