# SPEC-002 NFR-1 Determinism Evidence

**Requirement:** The same input and policy version MUST yield the same result.  
**Measured:** 2026-07-13  
**Configuration:** Release, .NET 10.0.9, x64  
**Automated test:** `tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-1EvidenceTests.cs`

## Method

The test loaded the governed `DEMO-POC-2026.1` fixtures through the shared
SPEC-002 test-only harness. It required the exact PB-01 through PB-19 ID set.
For each of those 19 fixed inputs, it evaluated one baseline decision and then
evaluated the exact same input and policy version 25 more times.

Each complete immutable `PolicyDecision` was serialized and compared with the
baseline. This comparison includes eligibility, policy version, evaluation
instant, bounded input summary, approval/effective metadata, every ordered
reason result and provenance record, primary reason, maximum credits,
fail-closed flags, and deterministic fingerprint.

## Measured result

| Measure | Value |
|---|---:|
| Approved boundary inputs exercised | 19 |
| Baseline decisions | 19 |
| Repeats per input | 25 |
| Repeated decisions compared | 475 |
| Total decisions evaluated | 494 |
| Full-decision differences | 0 |

**Result: PASS.** All 475 repeated full decisions were byte-for-byte identical
to their fixture's complete baseline serialization.

## Governance-reference scope

This evidence verifies the governed SPEC-002 reference contract and its
approved PB-01 through PB-19 fixtures through the repository's test-only
harness. It does not claim that the harness is the production evaluator, does
not exercise a database or API, and is not runtime release evidence. Runtime
policy evaluation remains owned by SPEC-009 and must repeat this determinism
gate against its implementation before release.
