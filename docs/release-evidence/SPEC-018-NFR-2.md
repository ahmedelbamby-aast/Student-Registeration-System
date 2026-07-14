# SPEC-018 NFR-2 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-2

**Recorded:** 2026-07-14

**Owner:** Ahmed ELbamby

**Release result:** PENDING

**Production authority:** Not granted

## Current result

Profile-definition result: PASS. The executable harness defines these blocking
profiles across at least two replicas:

| Profile | Duration | Registration rate | Read rate | Projected requests | Classification |
|---|---:|---:|---:|---:|---|
| Required target | 10 minutes | 75/s | 300/s | 225,000 | Blocking |
| Required spike | 60 seconds | 200/s | 0/s | 12,000 | Blocking |

The target projects 45,000 submissions and 180,000 reads. Its read mix is 50%
discovery, 25% eligibility, 15% plan/timetable, and 10% registration records.
The submission mix for both profiles is 70% valid unique requests, 20%
expected domain rejection, and 10% idempotent retry/lost-response recovery.

The retained 2x, 5x, and 120-minute soak profiles are diagnostic only. Their
results cannot block POC completion or relax correctness requirements.

Runtime execution result: PENDING. No target or spike traffic was sent because
the authenticated discovery, eligibility, plan, optimizer, registration, and
record endpoints do not yet exist end to end.

## Activation condition

Activation condition: SPEC-007 through SPEC-014 must deliver the authenticated
runtime paths, migrated production-like synthetic fixture, atomic registration
write, reconciliation queries, and two independently addressable API replicas.
The exact target and spike definitions must then execute for their full
durations before NFR-2 can pass for release.
