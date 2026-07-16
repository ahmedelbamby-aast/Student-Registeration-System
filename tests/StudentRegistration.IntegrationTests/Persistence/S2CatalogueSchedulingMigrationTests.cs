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

public sealed class S2CatalogueSchedulingMigrationTests
{
    private const string FoundationMigrationId =
        "20260713010000_IdentityAcademicFoundation";
    private const string MigrationId =
        "20260713020000_CatalogueScheduling";
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec010MigrationModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";
    private const string MigrationPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713020000_CatalogueScheduling.cs";
    private const string MigrationDesignerPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713020000_CatalogueScheduling.Designer.cs";
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
    [Trait("Dependency", "Docker")]
    public async Task S1_database_supports_incremental_update_rollback_and_idempotent_replay()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();

        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var scriptDatabaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var connectionString = DatabaseConnection(fixture.ConnectionString, databaseName);
        var scriptConnectionString = DatabaseConnection(
            fixture.ConnectionString,
            scriptDatabaseName);

        await using var context = CreateContext(connectionString);
        try
        {
            var migrator = context.GetService<IMigrator>();

            await migrator.MigrateAsync(FoundationMigrationId);
            Assert.Equal(
                [FoundationMigrationId],
                await context.Database.GetAppliedMigrationsAsync());
            Assert.Equal(0, await SchedulingTableCountAsync(connectionString));

            await migrator.MigrateAsync(MigrationId);
            Assert.Equal(
                [FoundationMigrationId, MigrationId],
                await context.Database.GetAppliedMigrationsAsync());
            Assert.Equal(8, await SchedulingTableCountAsync(connectionString));
            await AssertParentVersionTriggersAsync(connectionString);

            await migrator.MigrateAsync(FoundationMigrationId);
            Assert.Equal(
                [FoundationMigrationId],
                await context.Database.GetAppliedMigrationsAsync());
            Assert.Equal(0, await SchedulingTableCountAsync(connectionString));
            Assert.Equal(0, await CatalogueTableCountAsync(connectionString));

            var script = migrator.GenerateScript(
                FoundationMigrationId,
                MigrationId,
                MigrationsSqlGenerationOptions.Idempotent);
            Assert.Contains(MigrationId, script, StringComparison.Ordinal);

            await CreateDatabaseAsync(fixture.ConnectionString, scriptDatabaseName);
            await using (var scriptContext = CreateContext(scriptConnectionString))
            {
                await scriptContext.GetService<IMigrator>()
                    .MigrateAsync(FoundationMigrationId);
            }

            await ExecuteSqlBatchesAsync(scriptConnectionString, script);
            await ExecuteSqlBatchesAsync(scriptConnectionString, script);
            Assert.Equal(8, await SchedulingTableCountAsync(scriptConnectionString));
            Assert.Equal(9, await CatalogueTableCountAsync(scriptConnectionString));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
            await DropDatabaseAsync(fixture.ConnectionString, scriptDatabaseName);
        }
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Child_schedule_mutations_advance_the_owning_group_rowversion()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();

        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var connectionString = DatabaseConnection(fixture.ConnectionString, databaseName);
        await using var context = CreateContext(connectionString);
        try
        {
            await context.Database.MigrateAsync();
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await SeedParentVersionGraphAsync(connection);

            var initial = await ReadVersionAsync(connection);
            await ExecuteAsync(
                connection,
                """
                UPDATE [scheduling].[MeetingSlots]
                SET [StartLocal] = '09:15', [EndLocal] = '10:15'
                WHERE [Id] = '50000000-0000-0000-0000-000000000002';
                """);
            var afterMeeting = await ReadVersionAsync(connection);
            Assert.False(initial.SequenceEqual(afterMeeting));

            await ExecuteAsync(
                connection,
                """
                INSERT INTO [scheduling].[GroupStaffAssignments]
                    ([GroupId], [MeetingSlotId], [ActivityType], [StaffId], [TeachingRole])
                VALUES
                    ('50000000-0000-0000-0000-000000000001',
                     '50000000-0000-0000-0000-000000000002',
                     'lecture',
                     '70000000-0000-0000-0000-000000000001',
                     'lecturer');
                """);
            var afterAssignment = await ReadVersionAsync(connection);
            Assert.False(afterMeeting.SequenceEqual(afterAssignment));
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

    private static string DatabaseConnection(
        string serverConnectionString,
        string databaseName) =>
        new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;

    private static bool IsSeedOperation(MigrationOperation operation) =>
        operation is InsertDataOperation or UpdateDataOperation or DeleteDataOperation;

    private static async Task<int> SchedulingTableCountAsync(string connectionString) =>
        await ExecuteCountAsync(
            connectionString,
            """
            SELECT COUNT(*)
            FROM sys.tables AS t
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'scheduling'
              AND t.name IN
              (
                  N'CourseOfferings', N'SectionGroups', N'MeetingSlots', N'Rooms',
                  N'GroupStaffAssignments', N'StaffTermAvailabilities',
                  N'StaffAvailabilities', N'ScheduleImpactAlerts'
              );
            """);

    private static async Task<int> CatalogueTableCountAsync(string connectionString) =>
        await ExecuteCountAsync(
            connectionString,
            """
            SELECT COUNT(*)
            FROM sys.tables AS t
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'academics'
              AND t.name IN
              (
                  N'CatalogueDrafts', N'CatalogueVersions', N'Programs', N'Courses',
                  N'CurriculumCourses', N'CoursePrerequisites', N'PolicySets',
                  N'PolicyRules', N'ImportBatches'
              );
            """);

    private static async Task AssertParentVersionTriggersAsync(string connectionString)
    {
        Assert.Equal(
            2,
            await ExecuteCountAsync(
                connectionString,
                """
                SELECT COUNT(*)
                FROM sys.triggers
                WHERE name IN
                    (N'TR_MeetingSlots_AdvanceSectionGroupVersion',
                     N'TR_GroupStaffAssignments_AdvanceSectionGroupVersion');
                """));
    }

    private static async Task SeedParentVersionGraphAsync(SqlConnection connection)
    {
        await ExecuteAsync(
            connection,
            """
            INSERT INTO [auth].[ApplicationUsers]
                ([Id], [UserName], [NormalizedUserName], [UniversityId], [PasswordHash],
                 [SecurityStamp], [IsEnabled], [AccessFailedCount], [LockoutEndUtc])
            VALUES
                ('71000000-0000-0000-0000-000000000001',
                 N'lecturer@example.test', N'LECTURER@EXAMPLE.TEST', NULL,
                 N'not-a-real-hash', N'stamp', 1, 0, NULL);
            INSERT INTO [auth].[Staff]
                ([Id], [ApplicationUserId], [StaffNumber], [DisplayName], [IsActive])
            VALUES
                ('70000000-0000-0000-0000-000000000001',
                 '71000000-0000-0000-0000-000000000001',
                 N'S-001', N'Lecturer One', 1);
            INSERT INTO [academics].[AcademicTerms]
                ([Id], [Code], [CreationClientRequestId], [CreationPayloadHash],
                 [DisplayName], [TeachingStartsOn], [TeachingEndsOn], [TimeZoneId], [State])
            VALUES
                ('20000000-0000-0000-0000-000000000001',
                 N'2026-FALL', '22000000-0000-0000-0000-000000000001',
                 N'hash', N'Fall 2026', '2026-09-01', '2026-12-31',
                 N'Africa/Cairo', 'draft');
            INSERT INTO [academics].[CatalogueDrafts]
                ([Id], [ScopeCode], [BasedOnVersionId], [CanonicalContentHash],
                 [ContentJson], [ValidationSummaryJson], [State])
            VALUES
                ('31000000-0000-0000-0000-000000000001',
                 N'AI', NULL, N'hash', N'{}', N'{}', 'validated');
            INSERT INTO [academics].[CatalogueVersions]
                ([Id], [SourceDraftId], [SupersedesId], [ScopeCode], [VersionCode],
                 [SourceReference], [EffectiveFromUtc], [PublishedAtUtc], [PublishedBy], [State])
            VALUES
                ('32000000-0000-0000-0000-000000000001',
                 '31000000-0000-0000-0000-000000000001', NULL, N'AI', N'2026.1',
                 N'synthetic', '2026-07-01', '2026-07-01', N'admin', 'published');
            INSERT INTO [academics].[Courses]
                ([Id], [CatalogueVersionId], [Code], [Title], [Credits], [IsActive],
                 [ProvenanceSourceReference], [ProvenanceAccessedOn],
                 [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('30000000-0000-0000-0000-000000000001',
                 '32000000-0000-0000-0000-000000000001',
                 N'CS101', N'Introduction', 3, 1,
                 N'synthetic', '2026-07-01', N'synthetic-demo', N'[]');
            INSERT INTO [scheduling].[CourseOfferings]
                ([Id], [TermId], [CourseId], [State])
            VALUES
                ('10000000-0000-0000-0000-000000000001',
                 '20000000-0000-0000-0000-000000000001',
                 '30000000-0000-0000-0000-000000000001',
                 'draft');
            INSERT INTO [scheduling].[Rooms]
                ([Id], [Code], [Location], [Capacity], [AvailabilityState])
            VALUES
                ('40000000-0000-0000-0000-000000000001',
                 N'R101', N'Main Building', 35, 'available');
            INSERT INTO [scheduling].[SectionGroups]
                ([Id], [OfferingId], [GroupCode], [Capacity], [EnrolledCount], [State], [RegistrationPaused])
            VALUES
                ('50000000-0000-0000-0000-000000000001',
                 '10000000-0000-0000-0000-000000000001',
                 N'G01', 30, 0, 'draft', 0);
            INSERT INTO [scheduling].[MeetingSlots]
                ([Id], [GroupId], [RoomId], [ActivityType], [DayOfWeek], [StartLocal], [EndLocal])
            VALUES
                ('50000000-0000-0000-0000-000000000002',
                 '50000000-0000-0000-0000-000000000001',
                 '40000000-0000-0000-0000-000000000001',
                 'lecture', 1, '09:00', '10:00');
            """);
    }

    private static async Task<byte[]> ReadVersionAsync(SqlConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT [Version] FROM [scheduling].[SectionGroups] WHERE [Id] = '50000000-0000-0000-0000-000000000001';";
        return (byte[])(await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("SectionGroup version is missing."));
    }

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
            await ExecuteAsync(connection, batch);
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

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
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
        await ExecuteAsync(connection, $"CREATE DATABASE [{databaseName}];");
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
        await ExecuteAsync(
            connection,
            $"""
            IF DB_ID(N'{databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{databaseName}];
            END;
            """);
    }
}
