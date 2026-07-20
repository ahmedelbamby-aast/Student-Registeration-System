using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Infrastructure.SqlServer.Academics;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Academics;

public sealed class SqlCatalogueAdministrationStoreTests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Draft_import_validation_publication_and_reads_round_trip_through_real_sql()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_CatalogueStore_{Guid.NewGuid():N}"
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var context = new StudentRegistrationDbContext(options);
        try
        {
            Assert.True(await context.Database.EnsureCreatedAsync());
            var content = CataloguePublicationService.CreateDemoCurriculum();
            var validation = new CataloguePublicationService().Validate(content);
            var draft = new CatalogueDraft(
                Guid.NewGuid(),
                content.ScopeCode,
                basedOnVersionId: null,
                validation.CanonicalContentHash,
                JsonSerializer.Serialize(content),
                string.Empty,
                CatalogueDraftState.Editing);
            context.Add(draft);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var store = new SqlCatalogueAdministrationStore(
                context,
                new CataloguePublicationService(),
                TimeProvider.System);
            var loaded = await store.GetDraftAsync(draft.Id, CancellationToken.None);
            Assert.NotNull(loaded);
            Assert.Equal(19, loaded.Courses.Count);

            var invalid = await store.UpdateDraftAsync(
                draft.Id,
                new(
                    loaded.RowVersion,
                    "Exercise exact-credit validation",
                    "integration-test",
                    [new("upsert-course", Course: loaded.Courses[0] with { Credits = 4m })]),
                CancellationToken.None);
            var invalidImport = await store.CreateImportAsync(
                new(
                    draft.Id,
                    "integration-test",
                    new DateOnly(2026, 7, 20),
                    "sha256:INVALID",
                    ["Credits"],
                    Guid.NewGuid()),
                CancellationToken.None);
            var invalidResult = await store.ValidateImportAsync(
                new(
                    invalidImport.Id,
                    invalidImport.RowVersion,
                    invalid.RowVersion,
                    "admin-test"),
                CancellationToken.None);
            Assert.False(invalidResult.Valid);
            Assert.Contains(invalidResult.Errors, error => error.Code == "INVALID_CREDITS");

            var repaired = await store.UpdateDraftAsync(
                draft.Id,
                new(
                    invalidResult.ExpectedDraftRowVersion,
                    "Repair exact-credit validation",
                    "integration-test",
                    [new("upsert-course", Course: invalid.Courses[0] with { Credits = 3m })]),
                CancellationToken.None);
            var import = await store.CreateImportAsync(
                new(
                    draft.Id,
                    "integration-test",
                    new DateOnly(2026, 7, 20),
                    "sha256:VALID",
                    ["Credits"],
                    Guid.NewGuid()),
                CancellationToken.None);
            var validResult = await store.ValidateImportAsync(
                new(import.Id, import.RowVersion, repaired.RowVersion, "admin-test"),
                CancellationToken.None);
            Assert.True(validResult.Valid);
            Assert.NotNull(validResult.PreviewToken);

            var published = await store.PublishImportAsync(
                new(
                    import.Id,
                    validResult.ExpectedDraftRowVersion,
                    validResult.PreviewToken!,
                    Guid.NewGuid(),
                    "admin-test"),
                CancellationToken.None);
            Assert.Equal("published", published.State);

            var versions = await store.ListVersionsAsync(
                new(null, null, 1, 20, null),
                CancellationToken.None);
            Assert.Equal(1, versions.TotalCount);
            Assert.Equal(published.Id, Assert.Single(versions.Items).Id);

            var programs = await store.ListProgramsAsync(
                new(null, true, 1, 20, null),
                CancellationToken.None);
            Assert.Equal(1, programs.TotalCount);
            Assert.Equal("AI-DS", Assert.Single(programs.Items).Code);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}
