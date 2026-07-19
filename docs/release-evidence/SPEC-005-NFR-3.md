# SPEC-005 NFR-3 Backup and Restore Evidence

**Requirement:** SPEC-005/NFR-3  
**Recorded:** 2026-07-19  
**Owner:** Ahmed ELbamby

SPEC-005 inherits the approved POC recovery objective from SPEC-018: RPO <=
300 seconds and RTO <= 3,600 seconds. The exact executed aggregate record is
`SPEC-018-NFR-7-recovery.json`.

The Docker SQL Server 2022 Developer rehearsal created and checksum-verified a
production-like backup, restored it to an isolated differently named database,
and measured the result from committed database state. The measured RPO was 1
second and the measured RTO was 2 seconds. `DBCC CHECKDB`, six restored
migration-history rows, compatibility level 160, and zero restored
overbooking/duplicate-enrollment violations all passed.

Executable gates:

- `RecoveryRehearsalTests.Real_sql_backup_restore_rehearsal_meets_rpo_rto_and_integrity_gates`
- `NFR_3EvidenceTests.Spec018_recovery_record_meets_the_inherited_rpo_rto_gate`

This proves the non-production demo recovery target and does not grant
production authority.

**Result: PASS.**
