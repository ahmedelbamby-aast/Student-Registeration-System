using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Reflection;
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

public sealed class S1IdentityAcademicFoundationMigrationTests
{
    private const string MigrationId =
        "20260713010000_IdentityAcademicFoundation";
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec008MigrationModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";
    private const string InfrastructureProjectPath =
        "src/StudentRegistration.Infrastructure.SqlServer/StudentRegistration.Infrastructure.SqlServer.csproj";
    private const string MigrationPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713010000_IdentityAcademicFoundation.cs";
    private const string MigrationDesignerPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713010000_IdentityAcademicFoundation.Designer.cs";
    private const string SnapshotPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/StudentRegistrationDbContextModelSnapshot.cs";

    [Fact]
    public void Migration_metadata_is_exact_seed_free_and_matches_the_design_model()
    {
        using var context = CreateContext(ModelConnectionString);
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();

        Assert.Contains(MigrationId, migrationsAssembly.Migrations.Keys);

        var migration = migrationsAssembly.CreateMigration(
            migrationsAssembly.Migrations[MigrationId],
            context.Database.ProviderName!);
        Assert.Equal(
            MigrationId,
            migration.GetType().GetCustomAttribute<MigrationAttribute>()?.Id);
        Assert.DoesNotContain(migration.UpOperations, IsSeedOperation);
        Assert.DoesNotContain(migration.DownOperations, IsSeedOperation);

        var snapshot = migrationsAssembly.ModelSnapshot;
        Assert.NotNull(snapshot);

        var designTimeModel = context.GetService<IDesignTimeModel>().Model;
        var snapshotModel = context.GetService<IModelRuntimeInitializer>().Initialize(
            snapshot.Model,
            designTime: true,
            validationLogger: null);
        var modelDiffer = context.GetService<IMigrationsModelDiffer>();
        Assert.False(
            modelDiffer.HasDifferences(
                snapshotModel.GetRelationalModel(),
                designTimeModel.GetRelationalModel()),
            "The checked-in EF snapshot must have parity with the canonical design-time model.");
    }

    [Fact]
    public void Infrastructure_keeps_the_pinned_design_time_tooling_private()
    {
        Assert.True(RepositoryFiles.Exists(MigrationPath));
        Assert.True(RepositoryFiles.Exists(MigrationDesignerPath));
        Assert.True(RepositoryFiles.Exists(SnapshotPath));

        var project = XDocument.Load(RepositoryFiles.PathTo(InfrastructureProjectPath));
        var designReference = project.Descendants("PackageReference").SingleOrDefault(
            reference => string.Equals(
                (string?)reference.Attribute("Include"),
                "Microsoft.EntityFrameworkCore.Design",
                StringComparison.Ordinal));

        Assert.NotNull(designReference);
        Assert.Equal("10.0.10", (string?)designReference.Attribute("Version"));
        Assert.Equal("all", (string?)designReference.Attribute("PrivateAssets"));
        Assert.Contains(
            "runtime",
            (string?)designReference.Attribute("IncludeAssets") ?? string.Empty,
            StringComparison.Ordinal);
        Assert.Contains(
            "buildtransitive",
            (string?)designReference.Attribute("IncludeAssets") ?? string.Empty,
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Empty_database_supports_forward_update_rollback_and_idempotent_replay()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();

        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var scriptDatabaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var connectionString = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;
        var scriptConnectionString = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = scriptDatabaseName
        }.ConnectionString;

        await using var context = CreateContext(connectionString);
        try
        {
            var migrator = context.GetService<IMigrator>();

            await migrator.MigrateAsync(MigrationId);
            Assert.Equal(
                [MigrationId],
                await context.Database.GetAppliedMigrationsAsync());

            // Reapplying this migration slice is intentionally a no-op even
            // after later incremental migrations have been added.
            await migrator.MigrateAsync(MigrationId);
            Assert.Equal(
                [MigrationId],
                await context.Database.GetAppliedMigrationsAsync());

            await migrator.MigrateAsync("0");
            Assert.Empty(await context.Database.GetAppliedMigrationsAsync());

            var script = migrator.GenerateScript(
                "0",
                MigrationId,
                MigrationsSqlGenerationOptions.Idempotent);
            Assert.Contains(MigrationId, script, StringComparison.Ordinal);

            await CreateDatabaseAsync(fixture.ConnectionString, scriptDatabaseName);
            await ExecuteSqlBatchesAsync(scriptConnectionString, script);
            await ExecuteSqlBatchesAsync(scriptConnectionString, script);

            await AssertFoundationTablesAsync(scriptConnectionString);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
            await DropDatabaseAsync(fixture.ConnectionString, scriptDatabaseName);
        }
    }

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static bool IsSeedOperation(MigrationOperation operation) =>
        operation is InsertDataOperation or UpdateDataOperation or DeleteDataOperation;

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

    private static async Task CreateDatabaseAsync(
        string serverConnectionString,
        string databaseName)
    {
        await using var connection = new SqlConnection(
            new SqlConnectionStringBuilder(serverConnectionString)
            {
                InitialCatalog = "master"
            }.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE [{databaseName}];";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task AssertFoundationTablesAsync(string connectionString)
    {
        string[] expectedTables =
        [
            "auth.ApplicationUsers",
            "academics.AcademicTerms",
            "academics.RegistrationWindows",
            "academics.Students",
            "academics.StudentTermAcademicStates",
            "academics.TranscriptAttempts",
            "academics.StudentHolds"
        ];

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        foreach (var table in expectedTables)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT CASE WHEN OBJECT_ID(@tableName, 'U') IS NULL THEN 0 ELSE 1 END;";
            command.Parameters.AddWithValue("@tableName", table);
            Assert.Equal(1, Convert.ToInt32(await command.ExecuteScalarAsync()));
        }
    }

    private static async Task DropDatabaseAsync(
        string serverConnectionString,
        string databaseName)
    {
        SqlConnection.ClearAllPools();
        await using var connection = new SqlConnection(
            new SqlConnectionStringBuilder(serverConnectionString)
            {
                InitialCatalog = "master"
            }.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(N'{databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{databaseName}];
            END;
            """;
        await command.ExecuteNonQueryAsync();
    }
}
