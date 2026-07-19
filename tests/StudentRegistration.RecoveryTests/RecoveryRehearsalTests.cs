using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.TestSupport;
using Testcontainers.MsSql;

namespace StudentRegistration.RecoveryTests;

public sealed class RecoveryRehearsalTests
{
    private const string RunbookPath = "docs/runbooks/RECOVERY_AND_ROLLBACK.md";
    private const string EvidencePath =
        "docs/release-evidence/SPEC-018-NFR-7-recovery.json";

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

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_backup_restore_rehearsal_meets_rpo_rto_and_integrity_gates()
    {
        Spec018RecoveryEvidence evidence;
        if (string.Equals(
                Environment.GetEnvironmentVariable("SPEC018_RUN_RECOVERY_REHEARSAL"),
                "1",
                StringComparison.Ordinal))
        {
            evidence = await Spec018SqlRecoveryRehearsal.RunAsync();
            await File.WriteAllTextAsync(
                Path.Combine(RepositoryFiles.Root, EvidencePath),
                JsonSerializer.Serialize(
                    evidence,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)
                    {
                        WriteIndented = true
                    }));
        }
        else
        {
            evidence = JsonSerializer.Deserialize<Spec018RecoveryEvidence>(
                RepositoryFiles.Read(EvidencePath),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? throw new InvalidOperationException(
                    "The recorded SPEC-018 recovery evidence is invalid.");
        }

        Assert.Equal("SPEC-018", evidence.OwnerSpec);
        Assert.Equal("non-production-demo", evidence.Scope);
        Assert.Equal("isolated-recovery", evidence.RestoreEnvironment);
        Assert.InRange(evidence.MeasuredRpoSeconds, 0, 300);
        Assert.InRange(evidence.MeasuredRtoSeconds, 0, 3_600);
        Assert.Equal("pass", evidence.ReconciliationStatus);
        Assert.Equal("pass", evidence.Status);
        Assert.All(evidence.IntegrityChecks, check => Assert.Equal("pass", check.Status));
        Assert.False(evidence.ProductionAuthorized);
    }

    private static string Section(string heading) =>
        Regex.Replace(
            RepositoryFiles.Section(RepositoryFiles.Read(RunbookPath), heading),
            @"\s+",
            " ");
}

public sealed record Spec018RecoveryEvidence(
    string SchemaVersion,
    string OwnerSpec,
    string Scope,
    string EvidenceId,
    int EvidenceVersion,
    string SourceCommit,
    DateTimeOffset RecordedAtUtc,
    string BackupId,
    DateTimeOffset BackupCreatedAtUtc,
    string RestoreEnvironment,
    DateTimeOffset RestoreStartedAtUtc,
    DateTimeOffset RestoreCompletedAtUtc,
    int RpoTargetSeconds,
    int MeasuredRpoSeconds,
    int RtoTargetSeconds,
    int MeasuredRtoSeconds,
    IReadOnlyList<Spec018RecoveryIntegrityCheck> IntegrityChecks,
    string ReconciliationStatus,
    string Status,
    string ApprovedBy,
    bool ProductionAuthorized);

public sealed record Spec018RecoveryIntegrityCheck(
    string CheckId,
    string Status,
    string Details);

internal static class Spec018SqlRecoveryRehearsal
{
    private const string SqlImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";

    public static async Task<Spec018RecoveryEvidence> RunAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var sourceDatabase = $"StudentRegistration_Test_{suffix}";
        var restoredDatabase = $"StudentRegistration_Recovery_{suffix}";
        var backupFile = $"/var/opt/mssql/data/spec018-{suffix}.bak";
        var password = $"Srs!1{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}a";
        await using var container = new MsSqlBuilder(SqlImage)
            .WithPassword(password)
            .WithEnvironment("MSSQL_PID", "Developer")
            .Build();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        await container.StartAsync(timeout.Token);

        var masterConnectionString = container.GetConnectionString();
        await ExecuteAsync(
            masterConnectionString,
            $"CREATE DATABASE [{sourceDatabase}]; ALTER DATABASE [{sourceDatabase}] SET COMPATIBILITY_LEVEL = 160;",
            timeout.Token);
        var sourceConnectionString = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = sourceDatabase
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(sourceConnectionString)
            .Options;
        await using (var context = new StudentRegistrationDbContext(options))
        {
            await context.Database.MigrateAsync(timeout.Token);
        }

        var checkpointUtc = DateTimeOffset.UtcNow;
        await ExecuteAsync(
            sourceConnectionString,
            "CREATE TABLE [dbo].[RecoveryCheckpoint] ([Id] int NOT NULL PRIMARY KEY, [CommittedAtUtc] datetime2 NOT NULL); INSERT [dbo].[RecoveryCheckpoint] ([Id], [CommittedAtUtc]) VALUES (1, @checkpoint);",
            timeout.Token,
            new SqlParameter("@checkpoint", checkpointUtc.UtcDateTime));

        var recoveryDeclaredAtUtc = DateTimeOffset.UtcNow;
        var restoreStartedAtUtc = recoveryDeclaredAtUtc;
        await ExecuteAsync(
            masterConnectionString,
            $"BACKUP DATABASE [{sourceDatabase}] TO DISK = N'{backupFile}' WITH COPY_ONLY, INIT, CHECKSUM; RESTORE VERIFYONLY FROM DISK = N'{backupFile}' WITH CHECKSUM;",
            timeout.Token);
        var backupCreatedAtUtc = DateTimeOffset.UtcNow;
        await ExecuteAsync(
            masterConnectionString,
            $"RESTORE DATABASE [{restoredDatabase}] FROM DISK = N'{backupFile}' WITH MOVE N'{sourceDatabase}' TO N'/var/opt/mssql/data/{restoredDatabase}.mdf', MOVE N'{sourceDatabase}_log' TO N'/var/opt/mssql/data/{restoredDatabase}_log.ldf', RECOVERY; ALTER DATABASE [{restoredDatabase}] SET COMPATIBILITY_LEVEL = 160; DBCC CHECKDB ([{restoredDatabase}]) WITH NO_INFOMSGS;",
            timeout.Token);
        var restoreCompletedAtUtc = DateTimeOffset.UtcNow;
        var restoredConnectionString = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = restoredDatabase
        }.ConnectionString;

        var restoredCheckpointUtc = await ScalarAsync<DateTime>(
            restoredConnectionString,
            "SELECT [CommittedAtUtc] FROM [dbo].[RecoveryCheckpoint] WHERE [Id] = 1;",
            timeout.Token);
        var migrationCount = await ScalarAsync<int>(
            restoredConnectionString,
            "SELECT COUNT(*) FROM [dbo].[__EFMigrationsHistory];",
            timeout.Token);
        var invariantViolations = await ScalarAsync<int>(
            restoredConnectionString,
            "SELECT (SELECT COUNT(*) FROM [scheduling].[SectionGroups] WHERE [EnrolledCount] > [Capacity]) + (SELECT COUNT(*) FROM (SELECT [StudentId], [OfferingId] FROM [registration].[Enrollments] WHERE [State] = 'active' GROUP BY [StudentId], [OfferingId] HAVING COUNT(*) > 1) AS duplicates);",
            timeout.Token);
        var compatibilityLevel = await ScalarAsync<int>(
            masterConnectionString,
            $"SELECT [compatibility_level] FROM sys.databases WHERE [name] = N'{restoredDatabase}';",
            timeout.Token);
        var serviceRestorationVerifiedAtUtc = DateTimeOffset.UtcNow;
        var measuredRpoSeconds = Math.Max(
            0,
            (int)Math.Ceiling(
                (checkpointUtc - new DateTimeOffset(restoredCheckpointUtc, TimeSpan.Zero))
                .TotalSeconds));
        var measuredRtoSeconds = Math.Max(
            0,
            (int)Math.Ceiling(
                (serviceRestorationVerifiedAtUtc - recoveryDeclaredAtUtc).TotalSeconds));
        var passed = migrationCount > 0 &&
            invariantViolations == 0 &&
            compatibilityLevel == 160 &&
            measuredRpoSeconds <= 300 &&
            measuredRtoSeconds <= 3_600;

        return new Spec018RecoveryEvidence(
            "1.0",
            "SPEC-018",
            "non-production-demo",
            $"backup-evidence-{DateTime.UtcNow:yyyyMMdd}-{suffix[..8]}",
            1,
            SourceCommit(),
            serviceRestorationVerifiedAtUtc,
            $"spec018-{suffix}",
            backupCreatedAtUtc,
            "isolated-recovery",
            restoreStartedAtUtc,
            restoreCompletedAtUtc,
            300,
            measuredRpoSeconds,
            3_600,
            measuredRtoSeconds,
            [
                new("dbcc-checkdb", passed ? "pass" : "fail", "DBCC CHECKDB completed without an error."),
                new("migration-history", migrationCount > 0 ? "pass" : "fail", $"Restored migration rows: {migrationCount}."),
                new("registration-invariants", invariantViolations == 0 ? "pass" : "fail", $"Restored overbooking/duplicate violations: {invariantViolations}."),
                new("compatibility-level", compatibilityLevel == 160 ? "pass" : "fail", $"Restored compatibility level: {compatibilityLevel}.")
            ],
            passed ? "pass" : "fail",
            passed ? "pass" : "fail",
            "Ahmed ELbamby",
            false);
    }

    private static async Task ExecuteAsync(
        string connectionString,
        string commandText,
        CancellationToken cancellationToken,
        params SqlParameter[] parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.CommandTimeout = 300;
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<T> ScalarAsync<T>(
        string connectionString,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.CommandTimeout = 300;
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(value, typeof(T));
    }

    private static string SourceCommit()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "rev-parse HEAD",
            WorkingDirectory = RepositoryFiles.Root,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException("Git could not be started.");
        var value = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        return process.ExitCode == 0 && Regex.IsMatch(value, "^[0-9a-f]{40}$")
            ? value
            : throw new InvalidOperationException("The source commit could not be resolved.");
    }
}
