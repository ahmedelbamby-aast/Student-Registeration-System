# SPEC-018 NFR-3 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-3

**Recorded:** 2026-07-14

**Owner:** Ahmed ELbamby

**Release result:** PENDING

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

Runtime execution result: PENDING. No catalogue, optimizer, or atomic commit
p95 sample set exists, so this document records no measured percentile and
makes no performance-pass claim.

## Activation condition

Activation condition: SPEC-009 through SPEC-014 must deliver the production
code paths and production-like SQL fixture. The mandatory 10-minute target must
then capture reproducible catalogue, optimizer, and commit samples with the
code/data/config versions before NFR-3 can pass for release.
