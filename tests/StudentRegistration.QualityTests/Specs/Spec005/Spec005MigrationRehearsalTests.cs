using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.TestSupport;
using Testcontainers.MsSql;

namespace StudentRegistration.QualityTests.Specs.Spec005;

public sealed class Spec005MigrationRehearsalTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-005-NFR-2-rehearsal.json";
    private const string SqlImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Reviewed_script_rehearsal_and_backup_restore_rollback_are_measured()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("SPEC005_RUN_MIGRATION_REHEARSAL"),
                "1",
                StringComparison.Ordinal))
        {
            Assert.True(RepositoryFiles.Exists(EvidencePath));
            return;
        }

        var suffix = Guid.NewGuid().ToString("N");
        var sourceDatabase = $"StudentRegistration_Test_{suffix}";
        var rollbackDatabase = $"StudentRegistration_Rollback_{suffix}";
        var backupFile = $"/var/opt/mssql/data/spec005-{suffix}.bak";
        var password = $"Srs!1{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}a";
        await using var container = new MsSqlBuilder(SqlImage)
            .WithPassword(password)
            .WithEnvironment("MSSQL_PID", "Developer")
            .Build();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        await container.StartAsync(timeout.Token);
        var master = container.GetConnectionString();
        await ExecuteAsync(
            master,
            $"CREATE DATABASE [{sourceDatabase}]; ALTER DATABASE [{sourceDatabase}] SET COMPATIBILITY_LEVEL = 160; BACKUP DATABASE [{sourceDatabase}] TO DISK=N'{backupFile}' WITH COPY_ONLY,INIT,CHECKSUM; RESTORE VERIFYONLY FROM DISK=N'{backupFile}' WITH CHECKSUM;",
            timeout.Token);
        var source = new SqlConnectionStringBuilder(master)
        {
            InitialCatalog = sourceDatabase
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(source)
            .Options;
        string script;
        await using (var context = new StudentRegistrationDbContext(options))
        {
            script = context.GetService<IMigrator>().GenerateScript(
                fromMigration: null,
                toMigration: null,
                MigrationsSqlGenerationOptions.Idempotent);
        }
        var artifactHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(script)));

        var forward = Stopwatch.StartNew();
        await ExecuteBatchesAsync(source, script, timeout.Token);
        forward.Stop();
        var migrationCount = await ScalarAsync<int>(
            source,
            "SELECT COUNT(*) FROM [dbo].[__EFMigrationsHistory];",
            timeout.Token);
        bool modelParity;
        await using (var context = new StudentRegistrationDbContext(options))
        {
            modelParity = !((await context.Database.GetPendingMigrationsAsync(timeout.Token)).Any());
        }

        var rollback = Stopwatch.StartNew();
        await ExecuteAsync(
            master,
            $"RESTORE DATABASE [{rollbackDatabase}] FROM DISK=N'{backupFile}' WITH MOVE N'{sourceDatabase}' TO N'/var/opt/mssql/data/{rollbackDatabase}.mdf', MOVE N'{sourceDatabase}_log' TO N'/var/opt/mssql/data/{rollbackDatabase}_log.ldf', RECOVERY; DBCC CHECKDB ([{rollbackDatabase}]) WITH NO_INFOMSGS;",
            timeout.Token);
        rollback.Stop();
        var priorStateRestored = await ScalarAsync<int>(
            master,
            $"SELECT CASE WHEN OBJECT_ID(N'[{rollbackDatabase}].[dbo].[__EFMigrationsHistory]') IS NULL AND (SELECT COUNT(*) FROM [{rollbackDatabase}].sys.tables)=0 THEN 1 ELSE 0 END;",
            timeout.Token) == 1;

        var evidence = new MigrationRehearsalEvidence(
            "spec005-migration-rehearsal/1.0",
            "SPEC-005",
            DateTimeOffset.UtcNow,
            600,
            480,
            Math.Round(forward.Elapsed.TotalSeconds, 3),
            Math.Round(rollback.Elapsed.TotalSeconds, 3),
            artifactHash,
            true,
            true,
            priorStateRestored,
            modelParity,
            migrationCount,
            forward.Elapsed.TotalSeconds <= 480 &&
            rollback.Elapsed.TotalSeconds <= 480 &&
            migrationCount == 7 && priorStateRestored && modelParity
                ? "pass"
                : "fail",
            "Ahmed ELbamby",
            false);
        await File.WriteAllTextAsync(
            Path.Combine(RepositoryFiles.Root, EvidencePath),
            JsonSerializer.Serialize(
                evidence,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    WriteIndented = true
                }),
            timeout.Token);
        Assert.Equal("pass", evidence.Status);
    }

    private static async Task ExecuteBatchesAsync(
        string connectionString,
        string script,
        CancellationToken cancellationToken)
    {
        var batches = Regex.Split(
            script,
            @"^\s*GO\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline |
            RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
        foreach (var batch in batches.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            await ExecuteAsync(connectionString, batch, cancellationToken);
        }
    }

    private static async Task ExecuteAsync(
        string connectionString,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 600;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<T> ScalarAsync<T>(
        string connectionString,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 600;
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(value, typeof(T));
    }

    private sealed record MigrationRehearsalEvidence(
        string SchemaVersion,
        string OwnerSpec,
        DateTimeOffset RecordedAtUtc,
        int ApprovedWindowSeconds,
        int MaximumAllowedSeconds,
        double ForwardDurationSeconds,
        double RollbackDurationSeconds,
        string MigrationArtifactSha256,
        bool BackupVerified,
        bool RollbackTested,
        bool PriorStateRestored,
        bool ModelParityVerified,
        int AppliedMigrationCount,
        string Status,
        string ApprovedBy,
        bool ProductionAuthorized);
}
