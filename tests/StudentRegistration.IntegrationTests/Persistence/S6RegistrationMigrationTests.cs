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

public sealed class S6RegistrationMigrationTests
{
    private const string PreviousMigrationId =
        "20260713040000_DiscoveryPlanning";
    private const string MigrationId =
        "20260713060000_Registration";
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec014MigrationModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";
    private const string MigrationPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713060000_Registration.cs";
    private const string MigrationDesignerPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713060000_Registration.Designer.cs";
    private const string SnapshotPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs";

    [Fact]
    public void Migration_is_exact_seed_free_ordered_idempotent_and_matches_the_design_model()
    {
        using var context = CreateContext(ModelConnectionString);
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();

        Assert.True(RepositoryFiles.Exists(MigrationPath));
        Assert.True(RepositoryFiles.Exists(MigrationDesignerPath));
        Assert.True(RepositoryFiles.Exists(SnapshotPath));
        Assert.Contains(MigrationId, migrationsAssembly.Migrations.Keys);

        var migration = migrationsAssembly.CreateMigration(
            migrationsAssembly.Migrations[MigrationId],
            context.Database.ProviderName!);
        Assert.Equal(
            MigrationId,
            migration.GetType().GetCustomAttribute<MigrationAttribute>()?.Id);
        Assert.DoesNotContain(migration.UpOperations, IsSeedOperation);
        Assert.DoesNotContain(migration.DownOperations, IsSeedOperation);
        Assert.Contains(
            migration.UpOperations.OfType<CreateTableOperation>(),
            operation => operation.Name == "RegistrationSubmissions" &&
                operation.Schema == "registration");
        Assert.Contains(
            migration.UpOperations.OfType<CreateTableOperation>(),
            operation => operation.Name == "Enrollments" &&
                operation.Schema == "registration");
        Assert.DoesNotContain(
            migration.UpOperations.OfType<CreateTableOperation>(),
            operation => operation.Name == "DecisionSnapshots");

        var idempotentScript = context.GetService<IMigrator>().GenerateScript(
            PreviousMigrationId,
            MigrationId,
            MigrationsSqlGenerationOptions.Idempotent);
        Assert.Contains(MigrationId, idempotentScript, StringComparison.Ordinal);
        Assert.Contains("RegistrationSubmissions", idempotentScript, StringComparison.Ordinal);
        Assert.Contains("Enrollments", idempotentScript, StringComparison.Ordinal);

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
            $"StudentRegistration_Test_S6_Fresh_{Guid.NewGuid():N}");
        var incrementalConnectionString = DatabaseConnection(
            sqlServer.ConnectionString,
            $"StudentRegistration_Test_S6_Incremental_{Guid.NewGuid():N}");

        await using var freshContext = CreateContext(freshConnectionString);
        await using var incrementalContext = CreateContext(incrementalConnectionString);
        try
        {
            await freshContext.GetService<IMigrator>().MigrateAsync(MigrationId);
            Assert.Equal(2, await RegistrationTransactionTableCountAsync(freshConnectionString));
            Assert.Contains(MigrationId, await freshContext.Database.GetAppliedMigrationsAsync());

            var migrator = incrementalContext.GetService<IMigrator>();
            await migrator.MigrateAsync(PreviousMigrationId);
            Assert.Equal(0, await RegistrationTransactionTableCountAsync(
                incrementalConnectionString));

            await migrator.MigrateAsync(MigrationId);
            Assert.Equal(2, await RegistrationTransactionTableCountAsync(
                incrementalConnectionString));
            Assert.Equal(5, await RegistrationCheckConstraintCountAsync(
                incrementalConnectionString));

            await migrator.MigrateAsync(PreviousMigrationId);
            Assert.Equal(0, await RegistrationTransactionTableCountAsync(
                incrementalConnectionString));

            var script = migrator.GenerateScript(
                PreviousMigrationId,
                MigrationId,
                MigrationsSqlGenerationOptions.Idempotent);
            await ExecuteSqlBatchesAsync(incrementalConnectionString, script);
            await ExecuteSqlBatchesAsync(incrementalConnectionString, script);
            Assert.Equal(2, await RegistrationTransactionTableCountAsync(
                incrementalConnectionString));
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

    private static string DatabaseConnection(
        string serverConnectionString,
        string databaseName) =>
        new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;

    private static bool IsSeedOperation(MigrationOperation operation) =>
        operation is InsertDataOperation or UpdateDataOperation or DeleteDataOperation;

    private static async Task<int> RegistrationTransactionTableCountAsync(
        string connectionString) =>
        await ExecuteCountAsync(
            connectionString,
            """
            SELECT COUNT(*)
            FROM sys.tables AS t
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'registration'
              AND t.name IN (N'RegistrationSubmissions', N'Enrollments');
            """);

    private static async Task<int> RegistrationCheckConstraintCountAsync(
        string connectionString) =>
        await ExecuteCountAsync(
            connectionString,
            """
            SELECT COUNT(*)
            FROM sys.check_constraints
            WHERE name IN
            (
                N'CK_RegistrationSubmissions_State',
                N'CK_RegistrationSubmissions_ResultShape',
                N'CK_RegistrationSubmissions_ReceiptSnapshotJson',
                N'CK_RegistrationSubmissions_DecisionSnapshotJson',
                N'CK_Enrollments_State'
            );
            """);

    private static async Task ExecuteSqlBatchesAsync(
        string connectionString,
        string script)
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

    private static async Task<int> ExecuteCountAsync(
        string connectionString,
        string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
