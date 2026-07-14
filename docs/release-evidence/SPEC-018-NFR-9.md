# SPEC-018 NFR-9 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-9

**Recorded:** 2026-07-14

**Owner:** Ahmed ELbamby

**Release result:** PENDING

**Production authority:** Not granted

## Current result

Coverage-gate definition result: PASS. The executable gate requires
>= 90% branch coverage and fails when no branches exist, covered branches are
invalid, or the ratio is below `0.90`.

Coverage measurement result: NOT EXECUTED. Eligibility is owned by SPEC-011,
conflict behavior by SPEC-012, and capacity/concurrency behavior by SPEC-014.
Those downstream implementations are not present, so there is no source-owner
coverage report and no measured percentage to publish.

Runtime execution result: PENDING. A sample ratio used to test gate math is not
coverage evidence.

## Behavior remains authoritative

For this requirement, coverage never replaces behavior tests. A future
percentage can pass only while the owning boundary, authorization, real-SQL concurrency, and invariant tests also pass. In particular:

- Eligibility requires GPA, prerequisite, hold, standing, credit, and policy
  boundary decisions with stable explanations.
- conflict logic requires overlap boundaries, manual resolution, deterministic
  alternatives, and protected-option validation.
- capacity requires real-SQL contention, idempotency, atomicity, zero
  overbooking, zero duplicate enrollment, and zero partial submission.

## Activation condition

Activation condition: SPEC-011, SPEC-012, and SPEC-014 must deliver their
canonical source and complete behavior suites. CI must then collect a
versioned branch report scoped to those owner assemblies, prove at least 90%,
and retain passing behavior/concurrency evidence before NFR-9 can pass.
