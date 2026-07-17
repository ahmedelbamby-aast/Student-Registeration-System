# SPEC-017 Phase 6 Evidence

Date: 2026-07-17  
Branch: `codex/017-admin-operations-audit-reporting`  
Tasks: T092-T095 (95/101 total checked)

## Measurable non-functional evidence

| Task | Requirement | Fixture and calculation | Result |
|---|---|---|---|
| T092 | NFR-1 metrics freshness | four fixed-clock boundary cases, 32 observations; live through 60 seconds, stale at 61 seconds, timestamp preserved | PASS |
| T093 | NFR-2 audit first page | real SQL Server 2022, 100,000 merged rows, five warm-ups, 30 samples; nearest-rank p95 47.4728 ms against 1,000 ms | PASS |
| T094 | NFR-3 export lifecycle | two replicas, maximum 10,000-row export, 2,949,000-byte artifact; one publication, 60-second lease/retention, zero files after secure expiry | PASS |
| T095 | NFR-4 control coverage | authorization, audit, concurrency, anti-forgery, and validation matrix plus the 103-check Phase 5 regression | PASS |

## Reproduction result

```text
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec017.NFR_" --logger "console;verbosity=normal" -m:1 -p:BuildInParallel=false -p:TreatWarningsAsErrors=true
```

Result: 9 passed, 0 failed. The run included the pinned SQL Server container
and repeated the live NFR-2 measurement at 36.2355 ms p95, also below the
threshold. The checked-in raw artifact retains the separately recorded
47.4728 ms sample set so the exact published calculation remains reproducible.

The Release solution build also succeeded with 0 warnings and 0 errors. The
repository specification validator reported SPEC-017 with 22 artifacts, 101
tasks, score 100, automated gates PASS, human approval APPROVED, and no scoped
finding. Its overall result remains FAIL only for disclosed work in other
specifications.

All artifacts bind the measured implementation to commit
`e3c567f43e608597a66b9d8e5f1f0bcfc4b04594`. They describe the approved demo
profile and do not claim a production SLA or production approval.
