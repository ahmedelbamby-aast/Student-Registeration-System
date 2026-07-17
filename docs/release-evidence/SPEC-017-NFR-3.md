# SPEC-017 NFR-3 Export Lifecycle Evidence

**Requirement:** Large audit exports SHOULD be asynchronous and bounded, must
publish exactly once across replicas, and must expire securely.  
**Source-under-test commit:** `e3c567f43e608597a66b9d8e5f1f0bcfc4b04594`  
**Measured:** 2026-07-17T20:43:00Z  
**Result: PASS.**

## Environment and reproduction

- Microsoft Windows 10.0.26200, .NET 10.0.9
- Synthetic demo fixtures only
- Two application-service replicas sharing one conditional job store and one
  durable artifact directory
- The separate real-SQL evidence uses the pinned SQL Server 2022 Developer
  container with two independent `DbContext` replicas

```text
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec017.NFR_3EvidenceTests" --logger "console;verbosity=minimal" -m:1 -p:BuildInParallel=false -p:TreatWarningsAsErrors=true
```

## Fixture and measurements

The maximum allowed 10,000 rows were serialized by two replicas racing to
publish the same claimed job. The asynchronous operation completed in
119.1840 ms and produced a 2,949,000-byte CSV, below the 10,485,760-byte hard
limit. The conditional store accepted one publication and rejected the other
after lease loss. The losing replica removed its unpublished file, leaving
one published artifact.

| Measurement | Result |
|---|---:|
| Rows | 10,000 |
| Replicas | 2 |
| Successful publications | 1 |
| Lease-lost publications | 1 |
| Artifacts after publication | 1 |
| Artifact bytes | 2,949,000 |
| 60-second lease | enforced |
| Retention | 60 seconds |
| Artifacts after observing at 61 seconds | 0 |

The machine-readable raw artifact is
`docs/release-evidence/SPEC-017-NFR-3-results.json`.

## Durable concurrency and secure expiry

`AuditExportSqlConcurrencyTests` independently proves the same two-replica
claim, idempotency, shared-file, expiry, next-job claim, and three-attempt
retry behavior against real SQL Server. Both SQL checks passed in the Phase 5
regression. The NFR test adds the maximum-size fixture and measurement.

The completed artifact remained owner/scope authorized, was deleted when its
retention elapsed, and the job transitioned to `Expired`. Subsequent download
therefore cannot return the file. This establishes bounded work, one published
artifact, and secure expiry for the approved demo profile; it is not a
production throughput SLA.
