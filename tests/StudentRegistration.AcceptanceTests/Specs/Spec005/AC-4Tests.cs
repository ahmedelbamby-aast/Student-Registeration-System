using StudentRegistration.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

[Collection(Spec005SqlAcceptanceCollection.Name)]
public sealed class AC_4Tests(Spec005SqlAcceptanceDatabase database)
{
    [Fact]
    public void Migration_first_isolated_fixture_proves_erd_provenance_and_hash_only_seed()
    {
        var sqlProof = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Infrastructure/Spec008SqlServerTestDatabaseBootstrapperTests.cs");
        var lifecycle = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerTestDatabaseFixture.cs");
        var modelProof = RepositoryFiles.Read(
            "tests/StudentRegistration.SpecificationTests/Spec005/RelationalInvariantContractTests.cs");

        RepositoryFiles.ContainsAll(
            sqlProof,
            "Testing_database_migrates_then_seeds_is_idempotent_ready_and_disposable",
            "SELECT [PasswordHash]",
            "credential.Secret");
        RepositoryFiles.ContainsAll(
            lifecycle,
            "ApplyMigrationsAsync",
            "SeedAsync",
            "DropDatabaseAsync",
            "synthetic-only",
            "StudentRegistration_Test_",
            "CompatibilityLevel = 160");
        RepositoryFiles.ContainsAll(
            modelProof,
            "rowversion",
            "Alternate key",
            "foreign keys");
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Composed_database_is_migrated_compatibility_160_and_synthetic_hash_only()
    {
        var graph = await database.SeedAsync(1, 0, 1);
        await using var context = database.CreateContext();
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.Equal(
            (byte)160,
            await context.Database.SqlQuery<byte>($"""
                SELECT [compatibility_level] AS [Value]
                FROM sys.databases WHERE [name]=DB_NAME()
                """).SingleAsync());
        var credential = await context.Database.SqlQuery<string>($"""
            SELECT [PasswordHash] AS [Value] FROM [auth].[ApplicationUsers]
            WHERE [Id]=(SELECT [ApplicationUserId] FROM [academics].[Students] WHERE [Id]={graph.StudentIds[0]})
            """).SingleAsync();
        Assert.StartsWith("AQAAAA", credential, StringComparison.Ordinal);
        Assert.Equal(
            "synthetic",
            await context.Database.SqlQuery<string>($"""
                SELECT [Source] AS [Value] FROM [academics].[Students] WHERE [Id]={graph.StudentIds[0]}
                """).SingleAsync());
    }
}
