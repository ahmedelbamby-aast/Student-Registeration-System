# SPEC-018 NFR-3 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-3

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** PASS

**Production authority:** Not granted

## Current result

Budget-contract result: PASS. The target-load gate uses these inclusive p95
ceilings:

- Catalogue p95 <= 300 ms.
- commit p95 <= 2,000 ms.
- optimizer p95 <= 500 ms.

The 200/s spike still collects latency and must stay bounded and recover, but
the approved exact p95 release budgets apply to the target workload. Spike and
optional diagnostics cannot silently redefine these target limits.

Measured component/owner evidence is now available and passes each numerical
budget:

| Path | Measured p95 | Budget | Evidence boundary |
|---|---:|---:|---|
| Catalogue discovery | 0.284 ms | <= 300 ms | 300 bounded `OfferingSearchQuery` reads in `SPEC-011-NFR-1.md` |
| Registration commit | 27.5353 ms | <= 2,000 ms | 45,000 SQL submissions over ten minutes in `SPEC-018-load-results.json` |
| Optimizer | 2.148 ms | <= 500 ms | 50 real optimizer runs on 8 courses x 10 groups in `SPEC-013-NFR-1.md` |

The SPEC-018 executable evidence test re-measures catalogue and optimizer
locally and validates the immutable recorded SQL submission value against the
same thresholds.

Runtime execution result: PASS. In the exact simultaneous target, catalogue
discovery p95 was 35.9788 ms and end-to-end submission p95 was 27.5353 ms.
The measured optimizer component remains within 500 ms. All three approved
budgets passed without changing rates, mix, duration, or thresholds.
