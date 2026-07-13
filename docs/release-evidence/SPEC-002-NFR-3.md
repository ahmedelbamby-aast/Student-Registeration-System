# SPEC-002 NFR-3 Informational Contract-Harness Benchmark

**Requirement:** A policy decision query SHOULD complete within 100 ms p95, excluding initial data retrieval.  
**Measured:** 2026-07-13  
**Automated test:** `tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-3EvidenceTests.cs`  
**Evidence classification:** Informational reference-oracle benchmark  
**Runtime release status:** PENDING in SPEC-009 and SPEC-018

The ownership boundary for this result is defined in
`docs/release-evidence/SPEC-002-CONTRACT-TEST-BOUNDARY.md`.

## Measurement environment

| Item | Measured value |
|---|---|
| Build configuration | Release |
| .NET SDK | 10.0.301 |
| Runtime | .NET 10.0.9, x64 |
| Operating system | Microsoft Windows 10.0.26200 |
| Processor | 12th Gen Intel(R) Core(TM) i7-12700H |
| CPU topology | 14 cores / 20 logical processors |

## Method

The harness and all governed artifacts were loaded before timing, so initial
file/data retrieval was excluded. The test then executed 512 unmeasured warmup
evaluations to cover JIT and hot paths. It measured 10,000 calls to `Evaluate`
with `Stopwatch.GetTimestamp`, cycling deterministically through all 19
approved boundary inputs. Samples were sorted, and p50/p95 used the nearest-rank
index `ceil(percentile * sampleCount) - 1`.

## Measured result

| Measure | Value |
|---|---:|
| Warmup evaluations | 512 |
| Measured evaluations | 10,000 |
| p50 | 0.011400 ms |
| p95 | 0.017100 ms |
| Maximum observed | 3.866400 ms |
| Referenced NFR p95 ceiling | 100 ms |

**Result classification: INFORMATIONAL.** The test-only reference oracle
measured a p95 of 0.017100 ms. This number is useful for detecting large
regressions in the executable governance fixture, but it is not a server or
release result.

**Runtime release result: PENDING.** No server, API, database, publication
store, or production-like concurrent workload was measured. SPEC-009 must
replay the fixtures against its runtime evaluator, and SPEC-018 must publish
the approved production-like performance and load evidence before the NFR-3
release gate can pass. This benchmark is not an AASTMT SLA.
