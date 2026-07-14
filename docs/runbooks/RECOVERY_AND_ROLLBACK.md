# Recovery and Rollback Runbook

**Owner:** Ahmed ELbamby<br>
**Scope:** SPEC-018 non-production design-capability demo<br>
**Requirements:** SPEC-018 FR-5, NFR-7, AC-3, and SC-3<br>
**Last reviewed:** 2026-07-14

This runbook defines the rehearsal protocol for backup/restore, migration
rollback, and application rollback. Publishing the protocol is not measured
recovery evidence. Gate D remains blocked until the procedure is executed
against an approved production-like backup in a clean recovery target and its
immutable evidence passes every gate below.

## Authority and environment safeguards

Ahmed ELbamby is the sole human approver for this non-production demo
rehearsal. His demo approval is not production authorization and must not be
presented as official AASTMT approval. Actual production execution requires
recorded institutional production authority for the operator, source,
recovery target, SQL topology/edition, backup and encryption custody,
certificate/key custody, retention, cutover window, and rollback decision.

Before any command runs, perform positive environment and connection-target
validation using trusted server configuration rather than a caller-supplied
Boolean. The allowed demo targets are an isolated database explicitly labeled
Development or Testing, or a clean `isolated-recovery` target created for the
rehearsal. Seed and reset remain limited to Development/Testing. A
production-like backup may be restored only into the separately authorized
isolated recovery target; that does not authorize seed or reset there.

Record and independently compare the source and target server/database
identities, then prove the source and target are different. Confirm the target
is isolated from normal application traffic and contains no database that can
be overwritten. No seed, reset, restore, migration, rollback, or cutover action starts
until the run ID, operator, approval scope, source, target, environment,
connection target, backup custody, and recovery artifact versions all pass
validation.

Secrets and connection strings come from the approved secret input at runtime;
they never enter commands copied into evidence. Backup media must be encrypted,
access-controlled, checksum-verified, and readable only by the authorized
rehearsal identity. Stop immediately if an identity, target, artifact, key,
certificate, or approval is missing or ambiguous. Production readiness remains fail closed
until the unresolved institutional production inputs and a measured rehearsal
are approved.

## Recovery objectives and measurement

- **RPO <= 5 minutes (300 seconds).** Record `incidentCutoffUtc` as the last
  instant whose committed state should be recoverable and
  `restoredRecoveryPointUtc` as the latest committed instant verified in the
  restored database. Calculate `measuredRpoSeconds = incidentCutoffUtc -
  restoredRecoveryPointUtc`; negative or estimated values are invalid.
- **RTO <= 1 hour (3,600 seconds).** Record `recoveryDeclaredAtUtc` when the
  authorized recovery decision starts the clock and
  `serviceRestorationVerifiedAtUtc` only after restore, compatible application
  deployment, health checks, and the integrity/reconciliation gate pass.
  Calculate `measuredRtoSeconds = serviceRestorationVerifiedAtUtc -
  recoveryDeclaredAtUtc`.

All instants use server UTC from the controlled rehearsal clock. Also capture
`restoreStartedAtUtc` and `restoreCompletedAtUtc` for diagnostic timing, but do
not substitute restore duration for end-to-end RTO. A run can pass only when
the measured values are within both targets and every integrity,
reconciliation, security, and application check passes. A missing timestamp,
unverified recovery point, threshold breach, or clock anomaly makes the result
`blocked` or `fail`, never an estimate-based pass.

## Backup and restore rehearsal

### Preconditions

- Select an encrypted, access-controlled production-like backup and its
  available differential/log chain. Record its immutable ID, creation time,
  source commit, expected migration, retention classification, and custody.
- Verify the backup checksum and chain before beginning the RTO clock. A corrupt,
  incomplete, expired, unauthorized, or unverified artifact blocks the run.
- Provision a clean isolated recovery environment, validate its SQL
  compatibility and capacity, and prove the source and target are different.
- Confirm the approved compatible application artifact, configuration, shared
  Data Protection key repository, and certificate are available without
  placing their secrets in evidence.

### Procedure

1. Create the rehearsal/evidence ID, record `recoveryDeclaredAtUtc`, freeze the
   artifact set, and capture the authorized participants and communication
   channel.
2. Re-run the authority/environment guard immediately before SQL access. Do not overwrite
   the source database or any existing target database. Do not enable normal
   application traffic.
3. Record `restoreStartedAtUtc`; restore the verified full backup and each
   required differential/log artifact into a newly named target. Apply the
   chain only to the selected point in time and retain the restore output.
4. Record `restoredRecoveryPointUtc` from restored committed data, not from a
   filename or operator estimate. Record `restoreCompletedAtUtc` after SQL
   reports the target online.
5. Do not auto-apply a newer migration. Confirm the restored
   `__EFMigrationsHistory` matches the frozen application/migration plan.
6. Run the complete integrity and reconciliation gate. Any failure keeps the
   target isolated and marks the run failed.
7. Deploy the compatible immutable application artifact to at least two
   stateless replicas, point only those rehearsal replicas at the restored
   target, and run health, authentication, context, read, and safe synthetic
   registration smoke tests.
8. Record `serviceRestorationVerifiedAtUtc` only after all checks pass. Keep
   the target isolated unless the separately authorized cutover decision is
   recorded.

Preserve privacy-safe command/result hashes and timestamps, then dispose or
retain the rehearsal target according to the approved non-production policy.
Never copy credentials, keys, connection strings, or full student records into
the evidence package.

## Migration rollback rehearsal

Every production candidate migration requires a reviewed migration bundle,
an independently verified pre-migration backup, a compatibility decision, and
a rollback strategy before deployment. Migrations never run automatically on application startup.

1. Build a clean clone from the pre-migration backup, positively validate the
   clone target, and record the current application version, migration ID,
   schema fingerprint, and `__EFMigrationsHistory`.
2. Apply the exact reviewed migration bundle as a controlled operation. Record
   start/end UTC, bundle hash, resulting migration history, validation output,
   and application compatibility; never substitute a locally regenerated
   script.
3. If the change is explicitly classified reversible and data-preserving,
   execute its reviewed rollback script on the clean clone, then verify schema
   parity and data reconciliation.
4. If rollback is destructive, ambiguous, non-transactional, or not proven,
   do not improvise a down migration. Restore the pre-migration backup to a
   new clean target and deploy the previous application version instead.
5. Run the integrity/reconciliation and application smoke gates for both the
   rollback-script and restore paths that the release plan may use. Capture
   the selected method, migration before/after, backup ID, timestamps, result,
   and reviewer decision.

No operator may mutate a production source during the demo rehearsal. A
missing bundle hash, backup, compatibility proof, safe reverse path, or review
causes the migration release decision to fail closed. The previous version is
not restored to traffic until its database compatibility and the recovery
objectives have been proved.

## Application rollback rehearsal

The release package must retain the previous immutable application artifact,
its build/source identifier, configuration contract, dependency manifest, and
database compatibility decision. The target must also have access to the same
approved shared Data Protection keys and generated local certificate used by
the non-production POC; secret material remains outside Git and evidence.

1. Validate the environment, connection target, current/previous artifact
   hashes, configuration version, database migration, key repository, and
   certificate. If database compatibility with the previous build is not
   proven, first execute the approved migration rollback or restore path.
2. Record `rollbackStartedAtUtc`, remove the failing build from new traffic,
   and deploy the previous artifact to at least two replicas without changing
   durable business state or requiring sticky sessions.
3. Start the replicas against the validated database and shared key store.
   Run health and smoke checks for safe health, public/authenticated context,
   cross-replica authentication, authorized reads, and one reversible
   synthetic command/replay scenario.
4. Confirm correlation, error, and dependency signals contain no secrets or
   full profiles. Record `rollbackCompletedAtUtc` only after readiness and
   smoke checks pass, then make the separately authorized routing decision.

A missing artifact, incompatible schema, unavailable key/certificate,
unhealthy replica, failed authorization check, or integrity mismatch causes
application rollback to fail closed. Keep traffic on the last known safe
version or in maintenance; never report restoration from a partially verified
single replica.

## Integrity and reconciliation gate

The restored/rolled-back target remains isolated until all applicable checks
are recorded as pass:

- Run SQL Server physical/logical integrity validation, including `DBCC CHECKDB`,
  and retain its privacy-safe result.
- Compare `__EFMigrationsHistory`, schema/model fingerprint, compatibility
  level, and required constraints/indexes with the frozen release manifest.
- Reconcile table/aggregate counts and critical logical IDs against the backup
  manifest. Deterministic comparison excludes SQL `rowversion` and salted
  password-hash bytes; it must not expose credential material or full profiles.
- Query the restored invariants and prove zero overbooking, zero duplicate active offering enrollment,
  and zero partial atomic submission. Verify registration guard ownership,
  enrollment/offering/group relationships, capacity counters, and immutable
  submission results.
- Replay representative `idempotency` keys and confirm the stored deterministic
  outcome is returned without allocating another seat.
- Verify transcript/policy/decision/audit history remains immutable and
  temporally consistent, including audit-event linkage for sensitive changes.
- Verify server UTC/term context, authorized role/data scope, shared-key
  cross-replica authentication, health, and privacy-safe telemetry.
- Reconcile the latest committed transaction timestamp to the declared
  recovery point and recompute RPO/RTO from recorded instants.

Set `reconciliationStatus = pass` only when every required item passes. Any
missing, failed, stale, or unexplained result sets reconciliation to `fail` or
`blocked`, prevents cutover, and blocks release.

## Evidence record

Write one versioned record conforming to
`docs/release-evidence/schemas/backup-evidence.schema.json`. At minimum it
contains:

- `evidenceId`, `evidenceVersion`, `sourceCommit`, and `recordedAtUtc`;
- `backupId`, `backupCreatedAtUtc`, `restoreEnvironment`,
  `restoreStartedAtUtc`, and `restoreCompletedAtUtc`;
- `rpoTargetSeconds = 300`, `measuredRpoSeconds`,
  `rtoTargetSeconds = 3600`, and `measuredRtoSeconds`;
- `integrityChecks`, `reconciliationStatus`, `status`, and `approvedBy`;
- `productionAuthorized = false` for every demo rehearsal.

The supporting rehearsal record also captures `incidentCutoffUtc`,
`restoredRecoveryPointUtc`, `recoveryDeclaredAtUtc`,
`serviceRestorationVerifiedAtUtc`, environment/connection-target validation,
backup checksum and custody, source/target identifiers, migration bundle and
rollback method/hash, application artifacts before/after,
`rollbackStartedAtUtc`, `rollbackCompletedAtUtc`, operator/reviewer, defect or
incident references, and privacy-safe command/result hashes.

Evidence is immutable after sign-off. A correction creates a superseding evidence version
with its predecessor ID and rationale; signed evidence is never edited in
place. Store no credentials, connection strings, secrets, or full student profiles.
Local exports remain Git-ignored and are purged within seven days.

Use `status = blocked` when approval, authority, target validation, artifact,
measurement, or required check is missing; use `fail` when an executed check or
threshold fails. `pass` requires RPO/RTO within target,
`reconciliationStatus = pass`, all three rehearsal paths passing, and Ahmed
ELbamby's demo approval in `approvedBy`. A demo pass does not change
`productionAuthorized = false` and does not satisfy institutional production
approval.

## Release decision

The runbook itself never satisfies FR-5, NFR-7, AC-3, or Gate D. Release proof
requires a dated execution in the approved production-like environment, the
immutable evidence record, passing automated evidence checks, and Ahmed
ELbamby's release review perspectives. Block release for an absent/stale
rehearsal, RPO or RTO breach, failed rollback path, integrity or reconciliation
failure, unresolved critical/high security issue, unsafe evidence, or missing
approval. Production execution remains separately fail closed until the
institutional authorities and deployment-specific decisions are recorded.
