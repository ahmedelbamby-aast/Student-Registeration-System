# SPEC-018 NFR-5 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-5

**Recorded:** 2026-07-14

**Owner:** Ahmed ELbamby

**Release result:** PENDING

**Production authority:** Not granted

## Current result

Replica-contract result: PASS. Every required target, spike, and target-mix
failover definition requires at least two stateless API replicas. The failover
variant reuses the exact 10-minute target traffic and removes one replica at
the five-minute midpoint so the other replica must continue with shared SQL
state and shared Data Protection keys.

The 2x, 5x, and 120-minute soak profiles are non-blocking; optional diagnostics
cannot block POC completion. They also cannot weaken zero-defect correctness or
replace either mandatory profile.

Runtime execution result: PENDING. No two-replica load deployment or measured
target, spike, or failover run is claimed by this artifact.

## Activation condition

Activation condition: two independently addressable API replicas must be
available with the shared SQL/key configuration, and SPEC-007 through SPEC-014
must supply all authenticated workload paths. The full target, spike, and
failover runs must then record replica identities/count, duration, rates,
correctness counters, and recovery behavior before NFR-5 can pass for release.
