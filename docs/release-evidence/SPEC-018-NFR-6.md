# SPEC-018 NFR-6 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-6

**Recorded:** 2026-07-14

**Owner:** Ahmed ELbamby

**Release result:** PENDING

**Production authority:** Not granted

## Current result

Failure-rate gate result: PASS. The executable gate calculates unexpected
server failures divided by all target requests and requires the result to be
strictly below 0.1% (`rate < 0.001`). Exactly 0.1% fails. Validation, policy,
capacity, conflict, full-group, and idempotent-retry outcomes are classified by
their expected domain codes; expected domain rejections are not server failures.

Runtime execution result: PENDING. The zero and boundary counts used by the
unit tests prove only rate math. They are not measured target traffic and do
not establish an application failure rate.

## Activation condition

Activation condition: the full 10-minute required target must execute against
SPEC-007 through SPEC-014 runtime paths with safe response classification and
correlation. Versioned evidence must record total requests, unexpected server
failures, the computed rate, and the target-run identity before NFR-6 can pass
for release.
