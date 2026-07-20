using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Infrastructure.SqlServer.Scheduling;
using StudentRegistration.IntegrationTests.Infrastructure;
using StudentRegistration.Scheduling.Application;

namespace StudentRegistration.IntegrationTests.Scheduling;

public sealed class SqlOfferingStoreTests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Create_load_list_and_update_round_trip_through_real_sql()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_OfferingStore_{Guid.NewGuid():N}"
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var context = new StudentRegistrationDbContext(options);
        try
        {
            Assert.True(await context.Database.EnsureCreatedAsync());
            await SeedTermAndCourseAsync(connectionString);
            var store = new SqlOfferingStore(context);

            var created = await store.CreateAsync(
                new(
                    TermId,
                    CourseId,
                    [new("G01", 30)],
                    "Create real SQL offering"),
                CancellationToken.None);

            Assert.Equal(TermId, created.TermId);
            Assert.Equal(CourseId, created.CourseId);
            Assert.Equal("CS101", created.CourseCode);
            Assert.Equal("Introduction", created.CourseTitle);
            Assert.Equal("draft", created.State);
            Assert.NotEmpty(created.RowVersion);
            var createdGroup = Assert.Single(created.Groups);
            Assert.Equal(30, createdGroup.Capacity);
            Assert.NotEmpty(createdGroup.RowVersion);

            var loaded = await store.LoadAsync(created.Id, CancellationToken.None);
            Assert.NotNull(loaded);
            Assert.Equal(created.Id, loaded.Id);

            var page = await store.ListAsync(
                new(TermId, "draft", "CS101", 1, 20, "courseCode,id"),
                CancellationToken.None);
            Assert.Equal(1, page.TotalCount);
            Assert.Equal(created.Id, Assert.Single(page.Items).Id);

            var updated = await store.UpdateGroupAsync(
                new(
                    createdGroup.Id,
                    created.RowVersion,
                    createdGroup.RowVersion,
                    "G02",
                    24,
                    true,
                    [],
                    [],
                    "admin-test",
                    "Update group"),
                CancellationToken.None);

            Assert.Equal("G02", updated.GroupCode);
            Assert.Equal(24, updated.Capacity);
            Assert.True(updated.RegistrationPaused);
            Assert.NotEqual(createdGroup.RowVersion, updated.RowVersion);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static readonly Guid TermId = Guid.Parse("21000000-0000-0000-0000-000000000001");
    private static readonly Guid CourseId = Guid.Parse("31000000-0000-0000-0000-000000000001");

    private static async Task SeedTermAndCourseAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO [academics].[AcademicTerms]
                ([Id], [Code], [CreationClientRequestId], [CreationPayloadHash],
                 [DisplayName], [TeachingStartsOn], [TeachingEndsOn], [TimeZoneId], [State])
            VALUES
                ('21000000-0000-0000-0000-000000000001', N'2026-FALL',
                 '22000000-0000-0000-0000-000000000001', N'hash', N'Fall 2026',
                 '2026-09-01', '2026-12-31', N'Africa/Cairo', 'draft');
            INSERT INTO [academics].[CatalogueDrafts]
                ([Id], [ScopeCode], [BasedOnVersionId], [CanonicalContentHash],
                 [ContentJson], [ValidationSummaryJson], [State])
            VALUES
                ('32000000-0000-0000-0000-000000000001', N'AI', NULL, N'hash',
                 N'{}', N'{}', 'validated');
            INSERT INTO [academics].[CatalogueVersions]
                ([Id], [SourceDraftId], [SupersedesId], [ScopeCode], [VersionCode],
                 [SourceReference], [EffectiveFromUtc], [PublishedAtUtc], [PublishedBy], [State])
            VALUES
                ('33000000-0000-0000-0000-000000000001',
                 '32000000-0000-0000-0000-000000000001', NULL, N'AI', N'2026.1',
                 N'synthetic', '2026-07-01', '2026-07-01', N'admin', 'published');
            INSERT INTO [academics].[Courses]
                ([Id], [CatalogueVersionId], [Code], [Title], [Credits], [IsActive],
                 [ProvenanceSourceReference], [ProvenanceAccessedOn],
                 [ProvenanceSourceKind], [ProvenanceSyntheticFieldsJson])
            VALUES
                ('31000000-0000-0000-0000-000000000001',
                 '33000000-0000-0000-0000-000000000001', N'CS101', N'Introduction',
                 3, 1, N'synthetic', '2026-07-01', N'synthetic-demo', N'[]');
            """;
        await command.ExecuteNonQueryAsync();
    }
}
