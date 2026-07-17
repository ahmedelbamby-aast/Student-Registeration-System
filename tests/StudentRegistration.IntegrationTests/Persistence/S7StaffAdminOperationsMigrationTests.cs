using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class S7StaffAdminOperationsMigrationTests
{
    private const string PreviousMigrationId = "20260713060000_Registration";
    private const string MigrationId = "20260713070000_StaffAdminOperations";
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec017MigrationModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";
    private const string MigrationPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713070000_StaffAdminOperations.cs";
    private const string MigrationDesignerPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713070000_StaffAdminOperations.Designer.cs";
    private const string SnapshotPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs";

    [Fact]
    public void Migration_is_incremental_seed_free_idempotent_and_matches_the_design_model()
    {
        using var context = CreateContext(ModelConnectionString);
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();

        Assert.True(RepositoryFiles.Exists(MigrationPath));
        Assert.True(RepositoryFiles.Exists(MigrationDesignerPath));
        Assert.True(RepositoryFiles.Exists(SnapshotPath));
        Assert.Contains(MigrationId, migrationsAssembly.Migrations.Keys);
        Assert.Equal(MigrationId, migrationsAssembly.Migrations.Keys.Order().Last());

        var migration = migrationsAssembly.CreateMigration(
            migrationsAssembly.Migrations[MigrationId],
            context.Database.ProviderName!);
        Assert.Equal(MigrationId,
            migration.GetType().GetCustomAttribute<MigrationAttribute>()?.Id);
        Assert.DoesNotContain(migration.UpOperations, IsSeedOperation);
        Assert.DoesNotContain(migration.DownOperations, IsSeedOperation);

        var table = Assert.Single(migration.UpOperations.OfType<CreateTableOperation>());
        Assert.Equal("ExportJobs", table.Name);
        Assert.Equal("administration", table.Schema);
        Assert.DoesNotContain(migration.UpOperations.OfType<CreateTableOperation>(), operation =>
            operation.Name is "AuditEvents" or "SecurityEvents" or "AdminSecurityGuards");

        var idempotentScript = context.GetService<IMigrator>().GenerateScript(
            PreviousMigrationId,
            MigrationId,
            MigrationsSqlGenerationOptions.Idempotent);
        Assert.Contains(MigrationId, idempotentScript, StringComparison.Ordinal);
        Assert.Contains("ExportJobs", idempotentScript, StringComparison.Ordinal);
        Assert.DoesNotContain("CREATE TABLE [audit].[AuditEvents]", idempotentScript, StringComparison.Ordinal);
        Assert.DoesNotContain("CREATE TABLE [auth].[SecurityEvents]", idempotentScript, StringComparison.Ordinal);
        Assert.DoesNotContain("CREATE TABLE [auth].[AdminSecurityGuards]", idempotentScript, StringComparison.Ordinal);

        var snapshot = migrationsAssembly.ModelSnapshot;
        Assert.NotNull(snapshot);
        var designModel = context.GetService<IDesignTimeModel>().Model;
        var snapshotModel = context.GetService<IModelRuntimeInitializer>().Initialize(
            snapshot!.Model,
            designTime: true,
            validationLogger: null);
        var modelDiffer = context.GetService<IMigrationsModelDiffer>();
        Assert.Empty(modelDiffer.GetDifferences(
            snapshotModel.GetRelationalModel(),
            designModel.GetRelationalModel()));
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Migration_supports_fresh_prior_upgrade_rollback_and_idempotent_replay()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var freshConnectionString = DatabaseConnection(
            sqlServer.ConnectionString,
            $"StudentRegistration_Test_S7_Fresh_{Guid.NewGuid():N}");
        var incrementalConnectionString = DatabaseConnection(
            sqlServer.ConnectionString,
            $"StudentRegistration_Test_S7_Incremental_{Guid.NewGuid():N}");

        await using var freshContext = CreateContext(freshConnectionString);
        await using var incrementalContext = CreateContext(incrementalConnectionString);
        try
        {
            await freshContext.GetService<IMigrator>().MigrateAsync(MigrationId);
            Assert.Equal(1, await ExportJobTableCountAsync(freshConnectionString));
            Assert.Contains(MigrationId, await freshContext.Database.GetAppliedMigrationsAsync());

            var migrator = incrementalContext.GetService<IMigrator>();
            await migrator.MigrateAsync(PreviousMigrationId);
            Assert.Equal(0, await ExportJobTableCountAsync(incrementalConnectionString));

            await migrator.MigrateAsync(MigrationId);
            Assert.Equal(1, await ExportJobTableCountAsync(incrementalConnectionString));
            Assert.True(await ExportJobConstraintCountAsync(incrementalConnectionString) >= 4);

            await migrator.MigrateAsync(PreviousMigrationId);
            Assert.Equal(0, await ExportJobTableCountAsync(incrementalConnectionString));

            var script = migrator.GenerateScript(
                PreviousMigrationId,
                MigrationId,
                MigrationsSqlGenerationOptions.Idempotent);
            await ExecuteSqlBatchesAsync(incrementalConnectionString, script);
            await ExecuteSqlBatchesAsync(incrementalConnectionString, script);
            Assert.Equal(1, await ExportJobTableCountAsync(incrementalConnectionString));
        }
        finally
        {
            await freshContext.Database.EnsureDeletedAsync();
            await incrementalContext.Database.EnsureDeletedAsync();
        }
    }

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static string DatabaseConnection(string serverConnectionString, string databaseName) =>
        new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;

    private static bool IsSeedOperation(MigrationOperation operation) =>
        operation is InsertDataOperation or UpdateDataOperation or DeleteDataOperation;

    private static Task<int> ExportJobTableCountAsync(string connectionString) =>
        ExecuteCountAsync(
            connectionString,
            """
            SELECT COUNT(*)
            FROM sys.tables AS t
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'administration' AND t.name = N'ExportJobs';
            """);

    private static Task<int> ExportJobConstraintCountAsync(string connectionString) =>
        ExecuteCountAsync(
            connectionString,
            """
            SELECT COUNT(*)
            FROM sys.check_constraints
            WHERE name IN
            (
                N'CK_ExportJobs_State',
                N'CK_ExportJobs_AttemptCount',
                N'CK_ExportJobs_LeaseShape',
                N'CK_ExportJobs_ResultShape'
            );
            """);

    private static async Task ExecuteSqlBatchesAsync(string connectionString, string script)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var batches = Regex.Split(
            script,
            @"^\s*GO\s*$",
            RegexOptions.IgnoreCase
            | RegexOptions.Multiline
            | RegexOptions.CultureInvariant
            | RegexOptions.NonBacktracking);
        foreach (var batch in batches.Where(candidate => !string.IsNullOrWhiteSpace(candidate)))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = batch;
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task<int> ExecuteCountAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
