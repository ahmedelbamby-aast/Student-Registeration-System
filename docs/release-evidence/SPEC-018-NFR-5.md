# SPEC-018 NFR-5 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-5

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** PASS

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

Measured registration evidence now exists in `SPEC-018-load-results.json`:

- the 10-minute 75/s submission target completed 45,000 requests, split
  22,500/22,500 across two logical stateless registration replicas; and
- the 60-second 200/s spike completed 12,000 requests, split 6,000/6,000.

The executable tests in `RequiredReplicaLoadProfiles.cs` and
`NFR-5EvidenceTests.cs` validate the recorded duration, rate, completion count,
replica count, and distribution. Optional profiles remain diagnostic only.

Runtime execution result: PASS. The exact target began with two independently addressable API replicas,
each hosted as a stateless application instance and sharing SQL and certificate-protected Data
Protection keys. Replica 1 handled 45,000 reads before removal; replica 2
handled 135,000 reads and continued alone after the five-minute failover.
Replica 1 then restarted and accepted the existing protected session before
the two-replica 200/s spike. The stateless replica topology, latency, and error
gates all passed.

Machine-readable evidence: `SPEC-018-load-results.json`.
