# SPEC-018 NFR-4 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-4

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** PASS

**Production authority:** Not granted

## Current result

Invariant-gate result: PASS. The executable evaluator requires all of these
counters to equal zero and independently rejects each nonzero value:

- zero overbooking;
- zero duplicate active offering enrollment; and
- zero partial atomic submissions.

The counters are mandatory for target, spike, and replica-failover profiles.
Expected capacity, policy, and conflict rejections are normal domain outcomes;
they do not relax or hide an invariant failure.

Measured SQL evidence now exists in `SPEC-014-load-results.json`:

| Profile | Requests | Overbooked | Duplicate active enrollment | Partial commit | Result |
|---|---:|---:|---:|---:|---|
| 10-minute 75/s target | 45,000 | 0 | 0 | 0 | PASS |
| 60-second 200/s spike | 12,000 | 0 | 0 | 0 | PASS |
| 100 requests / 30 seats collision | 100 | 0 | 0 | 0 | PASS: exactly 30 accepted |

The executable SPEC-018 test reads that committed machine-readable artifact
and independently rejects any nonzero invariant counter.

Runtime execution result: PASS. The exact mixed target removed API replica 1
at the five-minute point, routed the remaining target traffic through replica
2, and then restarted replica 1 before the spike. Independent reconciliation
after the target, the 200/s spike, and the collision run found zero
overbooking, zero duplicate active offering enrollment, and zero partial
atomic submission. The target's latency/error SLO failure does not alter or
hide this measured correctness result.

Machine-readable evidence: `SPEC-018-load-results.json`.
