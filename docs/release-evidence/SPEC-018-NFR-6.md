# SPEC-018 NFR-6 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-6

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** PASS

**Production authority:** Not granted

## Current result

Failure-rate gate result: PASS. The executable gate calculates unexpected
server failures divided by all target requests and requires the result to be
strictly below 0.1% (`rate < 0.001`). Exactly 0.1% fails. Validation, policy,
capacity, conflict, full-group, and idempotent-retry outcomes are classified by
their expected domain codes; expected domain rejections are not server failures.

Measured owner evidence is available:

- the real SQL submission target completed 45,000 requests at 75/s for ten
  minutes with 0 unexpected server failures (0%); and
- the real-cookie, shared-SQL two-API-replica read target completed 180,000
  authenticated requests at 300/s for ten minutes with 0 unexpected failures.

The SPEC-018 executable test reads the committed submission artifact and
recomputes the strict threshold result. Expected domain rejections remain
separate from unexpected failures.

Runtime execution result: PASS. The exact simultaneous run completed all
225,000 target requests with zero unexpected failures (0%), strictly below the
0.1% ceiling. Expected business rejections remained separately classified and
were not counted as server failures.
