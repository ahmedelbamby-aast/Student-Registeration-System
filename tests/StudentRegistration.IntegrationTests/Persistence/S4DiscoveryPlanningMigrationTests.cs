using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using StudentRegistration.Infrastructure.SqlServer.Migrations;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class S4DiscoveryPlanningMigrationTests
{
    private const string PreviousMigrationId =
        "20260713020000_CatalogueScheduling";
    private const string MigrationId =
        "20260713040000_DiscoveryPlanning";
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec012MigrationModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";
    private const string MigrationPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713040000_DiscoveryPlanning.cs";
    private const string MigrationDesignerPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713040000_DiscoveryPlanning.Designer.cs";
    private const string SnapshotPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs";

    [Fact]
    public void Migration_metadata_is_exact_seed_free_and_matches_the_design_model()
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
            operation => operation.Name == "RegistrationPlans" &&
                operation.Schema == "registration");
        Assert.Contains(
            migration.UpOperations.OfType<CreateTableOperation>(),
            operation => operation.Name == "RegistrationPlanItems" &&
                operation.Schema == "registration");

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
    public async Task S2_database_supports_incremental_update_rollback_and_idempotent_replay()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_S4_{Guid.NewGuid():N}"
        }.ConnectionString;
        await using var context = CreateContext(connectionString);

        try
        {
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync(PreviousMigrationId);
            Assert.DoesNotContain(MigrationId, await context.Database.GetAppliedMigrationsAsync());

            await migrator.MigrateAsync(MigrationId);
            Assert.Contains(MigrationId, await context.Database.GetAppliedMigrationsAsync());
            Assert.Equal(2, await RegistrationTableCountAsync(connectionString));

            await migrator.MigrateAsync(PreviousMigrationId);
            Assert.DoesNotContain(MigrationId, await context.Database.GetAppliedMigrationsAsync());
            Assert.Equal(0, await RegistrationTableCountAsync(connectionString));

            var script = migrator.GenerateScript(
                PreviousMigrationId,
                MigrationId,
                MigrationsSqlGenerationOptions.Idempotent);
            Assert.Contains(MigrationId, script, StringComparison.Ordinal);
            Assert.Contains("RegistrationPlans", script, StringComparison.Ordinal);

            await context.Database.CreateExecutionStrategy().ExecuteAsync(
                () => migrator.MigrateAsync(MigrationId));
            Assert.Equal(2, await RegistrationTableCountAsync(connectionString));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static async Task<int> RegistrationTableCountAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sys.tables AS t
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'registration'
              AND t.name IN (N'RegistrationPlans', N'RegistrationPlanItems');
            """;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static bool IsSeedOperation(MigrationOperation operation) =>
        operation is InsertDataOperation or UpdateDataOperation or DeleteDataOperation;
}
