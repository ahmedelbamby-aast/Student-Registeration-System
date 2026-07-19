# SPEC-018 NFR-2 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-2

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** PASS

**Production authority:** Not granted

## Current result

Profile-definition result: PASS. The executable harness defines these blocking
profiles across at least two replicas:

| Profile | Duration | Registration rate | Read rate | Projected requests | Classification |
|---|---:|---:|---:|---:|---|
| Required target | 10 minutes | 75 submissions/s | 300 reads/s | 225,000 | Blocking |
| Required spike | 60 seconds | 200 submissions/s | 0 reads/s | 12,000 | Blocking |

The target projects 45,000 submissions and 180,000 reads. Its read mix is 50%
discovery, 25% eligibility, 15% plan/timetable, and 10% registration records.
The submission mix for both profiles is 70% valid unique requests, 20%
expected domain rejection, and 10% idempotent retry/lost-response recovery.

The retained 2x, 5x, and 120-minute soak profiles are diagnostic only. Their
results cannot block POC completion or relax correctness requirements.

Runtime execution result: PASS. The exact profiles executed against the
migrated 25,000-account/5,000-session shared-SQL fixture with SQL Server 2022
compatibility level 160 and `READ_COMMITTED_SNAPSHOT ON`:

- target: 45,000 submissions plus 180,000 reads over ten minutes, with the
  exact 90,000/45,000/27,000/18,000 read split and failover at five minutes;
- spike: 12,000 submissions over 60 seconds at 200 submissions/s; and
- two stateless replicas were used and replica 1 was restarted after failover.

All 225,000 target requests completed with zero unexpected failures. Target
submission p95 was 27.5353 ms and catalogue discovery p95 was 35.9788 ms. The
required 12,000-request spike completed with zero unexpected failures, and all
target, spike, collision, and post-repair invariants were zero. The checked-in
machine-readable evidence is `SPEC-018-load-results.json`.

Graceful degradation documented: spike end-to-end submission p95 was
5,892.3845 ms. The approved p95 budgets apply to the target workload, not the
spike; spike availability, completion, recovery, and correctness all passed.
