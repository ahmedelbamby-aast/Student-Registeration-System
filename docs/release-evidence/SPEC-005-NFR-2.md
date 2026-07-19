# SPEC-005 NFR-2 Controlled Migration Evidence

**Requirement:** SPEC-005/NFR-2  
**Recorded:** 2026-07-19  
**Owner:** Ahmed ELbamby  
**Scope:** non-production POC rehearsal

Ahmed ELbamby, the repository's sole approval authority, approved a numeric
**600 seconds** deployment window in the Operations review perspective for
this non-production POC rehearsal. The mandatory 80% ceiling is therefore
**480 seconds**. This is not official AASTMT production authorization; a
missing, changed, or expired window continues to fail closed.

The executable SQL Server 2022 Developer rehearsal generated the complete
idempotent EF migration script from the reviewed model, hashed that exact
artifact, verified a checksum-protected pre-migration backup, applied the
script as an explicit controlled operation, verified all seven migrations and
model parity, and performed a tested rollback by restoring the verified backup
to a separately named database. `DBCC CHECKDB` passed and the prior empty
schema state was restored. Application startup remains migration-free.

The immutable measurements and artifact hash are recorded in
`SPEC-005-NFR-2-rehearsal.json`. Both measured forward and rollback durations
are below 480 seconds: the seven-migration forward rehearsal completed in
1.146 seconds and the verified-backup rollback completed in 1.714 seconds.

Executable gates:

- `Spec005MigrationRehearsalTests.Reviewed_script_rehearsal_and_backup_restore_rollback_are_measured`
- `NFR_2EvidenceTests.Recorded_rehearsal_is_inside_threshold_and_restores_prior_state`
- `MigrationBundleTests.Controlled_migration_contract_is_reviewed_ordered_and_never_runs_at_startup`

**Result: PASS.**
