# SPEC-018 NFR-9 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-9

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** PASS

**Production authority:** Not granted

## Current result

Coverage-gate definition result: PASS. The executable gate requires
>= 90% branch coverage and fails when no branches exist, covered branches are
invalid, or the ratio is below `0.90`.

Coverage measurement result: EXECUTED AND PASSED. The focused integration run
executed 133 behavior tests and all 133 passed. The Cobertura report produced
these manifest-owner results by summing covered and valid branches in the
matched classes:

| Owner | Matched classes | Covered / valid | Branch rate |
|---|---:|---:|---:|
| SPEC-011 eligibility | 17 | 351 / 375 | 93.60% |
| SPEC-012 conflict/plan | 30 | 168 / 186 | 90.32% |
| SPEC-014 capacity/concurrency | 23 | 104 / 112 | 92.86% |

The machine-readable aggregate and source-report SHA-256 are recorded in
`SPEC-018-NFR-9-coverage.json`. Per-class below-threshold counts remain
diagnostic; the approved manifest gate measures each named owner's total
covered branches divided by total valid branches.

Runtime execution result: PASS. Every measured owner is at or above 90%, the
behavior suite is fully green, and no production code was excluded from the
existing owner patterns to obtain the result.

## Behavior remains authoritative

For this requirement, coverage never replaces behavior tests. A future
percentage can pass only while the owning boundary, authorization, real-SQL concurrency, and invariant tests also pass. In particular:

- Eligibility requires GPA, prerequisite, hold, standing, credit, and policy
  boundary decisions with stable explanations.
- conflict logic requires overlap boundaries, manual resolution, deterministic
  alternatives, and protected-option validation.
- capacity requires real-SQL contention, idempotency, atomicity, zero
  overbooking, zero duplicate enrollment, and zero partial submission.
