# SPEC-017 NFR-4 Admin Action Coverage Evidence

**Requirement:** NFR-4

**Implementation commit:** `e3c567f43e608597a66b9d8e5f1f0bcfc4b04594`

**Recorded UTC:** 2026-07-17T19:42:04Z

**Environment:** Windows, .NET 10, synthetic demo fixtures, and SQL Server
2022 containers for the real-SQL/two-replica checks.

## Test matrix

| Control | Executable evidence | Result |
|---|---|---:|
| Authorization | positive/negative named-permission endpoint policies and owner-command absence/delegation checks | 8 passed, 0 failed |
| Audit | merged append-only read projection, request/download audit, and caller-transaction rollback | 8 passed, 0 failed |
| Concurrency | expected versions, bound preview/replay/mismatch, two-replica export lease, and final-Admin serialization | 6 passed, 0 failed |
| Anti-forgery | the only SPEC-017 mutation, `POST /api/admin/exports`, requires framework antiforgery metadata | 1 passed, 0 failed |
| Validation | all five API outcome contracts and metrics/audit/export application validation | 23 passed, 0 failed |

The complete Phase 5 regression matrix was also green: 103 focused checks,
0 failed, across contract, application, authorization, integration,
acceptance, E2E, component, accessibility, visual, architecture, and migration
suites. AC-9's separate performance/measurement rows are handled by NFR-1
through NFR-3 and are not represented as passing by this matrix.

## Raw result artifact

The machine-readable result is
`docs/release-evidence/SPEC-017-NFR-4-results.json`, schema
`spec017-nfr4-results/1.0`. It records the tested commit, environment, every
control family, passed/failed counts, and the Phase 5 regression counts.

## Reproduction

From the repository root, run:

```powershell
dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --configuration Release --filter "FullyQualifiedName~StudentRegistration.QualityTests.Specs.Spec017.NFR_4EvidenceTests" --logger "console;verbosity=minimal" -p:TreatWarningsAsErrors=true
```

The quality test verifies the raw artifact and binds every control to its
named executable source. No production identity, credential, cookie, or
student record is used.

**Result: PASS.**
