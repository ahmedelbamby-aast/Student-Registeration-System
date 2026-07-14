using System.Text.RegularExpressions;
using StudentRegistration.TestSupport;

namespace StudentRegistration.RecoveryTests;

public sealed class RecoveryRehearsalTests
{
    private const string RunbookPath = "docs/runbooks/RECOVERY_AND_ROLLBACK.md";

    [Fact]
    public void Runbook_declares_exact_recovery_objectives_and_measurements()
    {
        var objectives = Section("Recovery objectives and measurement");

        RepositoryFiles.ContainsAll(
            objectives,
            "RPO <= 5 minutes (300 seconds)",
            "RTO <= 1 hour (3,600 seconds)",
            "measuredRpoSeconds",
            "incidentCutoffUtc",
            "restoredRecoveryPointUtc",
            "measuredRtoSeconds",
            "recoveryDeclaredAtUtc",
            "serviceRestorationVerifiedAtUtc");
    }

    [Fact]
    public void Backup_restore_rehearsal_is_isolated_authorized_and_verifiable()
    {
        var rehearsal = Section("Backup and restore rehearsal");

        RepositoryFiles.ContainsAll(
            rehearsal,
            "production-like backup",
            "clean isolated recovery environment",
            "source and target are different",
            "backup checksum",
            "encrypted",
            "access-controlled",
            "restoreStartedAtUtc",
            "restoreCompletedAtUtc",
            "Do not overwrite",
            "integrity and reconciliation gate");
    }

    [Fact]
    public void Migration_rollback_is_rehearsed_without_startup_or_unreviewed_schema_changes()
    {
        var rehearsal = Section("Migration rollback rehearsal");

        RepositoryFiles.ContainsAll(
            rehearsal,
            "reviewed migration bundle",
            "pre-migration backup",
            "clean clone",
            "reviewed rollback script",
            "Restore the pre-migration backup",
            "previous application version",
            "Migrations never run automatically on application startup",
            "fail closed");
    }

    [Fact]
    public void Application_rollback_is_compatible_bounded_and_health_checked()
    {
        var rehearsal = Section("Application rollback rehearsal");

        RepositoryFiles.ContainsAll(
            rehearsal,
            "previous immutable application artifact",
            "database compatibility",
            "shared Data Protection keys",
            "generated local certificate",
            "at least two replicas",
            "health and smoke checks",
            "rollbackStartedAtUtc",
            "rollbackCompletedAtUtc",
            "fail closed");
    }

    [Fact]
    public void Integrity_and_reconciliation_cover_database_history_and_registration_invariants()
    {
        var gate = Section("Integrity and reconciliation gate");

        RepositoryFiles.ContainsAll(
            gate,
            "DBCC CHECKDB",
            "__EFMigrationsHistory",
            "zero overbooking",
            "zero duplicate active offering enrollment",
            "zero partial atomic submission",
            "idempotency",
            "audit history",
            "logical IDs",
            "rowversion",
            "password-hash bytes",
            "reconciliationStatus = pass");
    }

    [Fact]
    public void Evidence_is_complete_immutable_safe_and_release_blocking()
    {
        var evidence = Section("Evidence record");

        string[] requiredFields =
        [
            "evidenceId",
            "evidenceVersion",
            "sourceCommit",
            "recordedAtUtc",
            "backupId",
            "backupCreatedAtUtc",
            "restoreEnvironment",
            "restoreStartedAtUtc",
            "restoreCompletedAtUtc",
            "rpoTargetSeconds",
            "measuredRpoSeconds",
            "rtoTargetSeconds",
            "measuredRtoSeconds",
            "integrityChecks",
            "reconciliationStatus",
            "status",
            "approvedBy",
            "productionAuthorized"
        ];

        RepositoryFiles.ContainsAll(evidence, requiredFields);
        RepositoryFiles.ContainsAll(
            evidence,
            "immutable after sign-off",
            "superseding evidence version",
            "no credentials, connection strings, secrets, or full student profiles",
            "status = blocked",
            "productionAuthorized = false");
    }

    [Fact]
    public void Production_and_environment_authority_fail_closed()
    {
        var safeguards = Section("Authority and environment safeguards");

        RepositoryFiles.ContainsAll(
            safeguards,
            "Ahmed ELbamby",
            "non-production demo rehearsal",
            "not production authorization",
            "institutional production authority",
            "positive environment and connection-target validation",
            "Development or Testing",
            "isolated-recovery",
            "No seed, reset, restore, migration, rollback, or cutover action starts",
            "Production readiness remains fail closed");
    }

    private static string Section(string heading) =>
        Regex.Replace(
            RepositoryFiles.Section(RepositoryFiles.Read(RunbookPath), heading),
            @"\s+",
            " ");
}
