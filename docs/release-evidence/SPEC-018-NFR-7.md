# SPEC-018 NFR-7 Release Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-7

**Recorded:** 2026-07-18

**Owner:** Ahmed ELbamby

**Release result:** PASS

**Production authority:** Not granted

## Current result

Protocol result: PASS. The versioned recovery runbook defines end-to-end RPO
and RTO clocks, isolated target validation, backup/restore, migration rollback,
application rollback, integrity checks, reconciliation, immutable evidence,
and fail-closed release handling.

The executable objective gate accepts only non-negative measurements where
RPO <= 5 minutes (300 seconds) and RTO <= 1 hour (3,600 seconds). The inclusive
boundary passes; either value above its limit fails.

Recovery rehearsal result: PASS. On 2026-07-17 UTC, the executable Docker
rehearsal migrated an isolated SQL Server 2022 Developer compatibility-160
source database, created a checksum-protected full backup, verified it, and
restored it to a differently named isolated recovery database.

Runtime execution result: PASS. The measured RPO was 1 second (target <= 300)
and measured RTO was 2 seconds (target <= 3,600). `DBCC CHECKDB`, six restored
migration-history rows, restored compatibility level 160, and zero restored
overbooking/duplicate-enrollment violations all passed. The immutable
aggregate record is `SPEC-018-NFR-7-recovery.json`.

## Required measured evidence

| Field or gate | Current state |
|---|---|
| Measured RPO seconds | PASS: 1 |
| Measured RTO seconds | PASS: 2 |
| Backup checksum and restore result | PASS |
| `DBCC CHECKDB` | PASS |
| Migration history and compatibility | PASS: 6 rows / level 160 |
| Registration invariant reconciliation | PASS: 0 violations |
| Production authorization | Not granted |

## Evidence boundary

This artifact proves the exact NFR-7 RPO/RTO recovery objective for the
non-production demo. It does not claim production authorization. T049/T050
provide the migration-rollback and previous-application rollback protocol,
but no executable migration rollback or previous-application artifact rollback
rehearsal exists in those tasks. Therefore this NFR-7 pass must not be reused
to claim that FR-5 or Gate D rollback evidence is complete.

## Reproduction

```powershell
$env:SPEC018_RUN_RECOVERY_REHEARSAL='1'
dotnet test tests/StudentRegistration.RecoveryTests/StudentRegistration.RecoveryTests.csproj --configuration Release --filter "FullyQualifiedName~RecoveryRehearsalTests.Real_sql_backup_restore_rehearsal_meets_rpo_rto_and_integrity_gates"
```
