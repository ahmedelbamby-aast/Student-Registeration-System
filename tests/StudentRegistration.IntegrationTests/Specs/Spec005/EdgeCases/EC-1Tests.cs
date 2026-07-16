using Microsoft.Data.SqlClient;
using StudentRegistration.IntegrationTests.Infrastructure;
using Testcontainers.MsSql;

namespace StudentRegistration.IntegrationTests.Specs.Spec005.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Migrated_but_unseeded_database_is_not_ready()
    {
        await using var container = new MsSqlBuilder(
                SqlServerTestDatabaseFixture.SqlServerImage)
            .WithPassword($"Srs!1{Guid.NewGuid():N}a")
            .Build();
        await container.StartAsync();

        var databaseName =
            $"{SqlServerTestDatabaseFixture.TestingDatabasePrefix}{Guid.NewGuid():N}";
        var createResult = await container.ExecScriptAsync($"""
            CREATE DATABASE [{databaseName}];
            ALTER DATABASE [{databaseName}]
                SET COMPATIBILITY_LEVEL = {SqlServerTestDatabaseFixture.CompatibilityLevel};
            """);
        Assert.Equal<long?>(0L, createResult.ExitCode);

        var connectionString = new SqlConnectionStringBuilder(
            container.GetConnectionString())
        {
            InitialCatalog = databaseName
        }.ConnectionString;
        var bootstrapper = new Spec008SqlServerTestDatabaseBootstrapper(
            studentCount: 1);

        await bootstrapper.ApplyMigrationsAsync(
            connectionString,
            CancellationToken.None);
        var readiness = await bootstrapper.VerifyReadinessAsync(
            connectionString,
            CancellationToken.None);

        Assert.True(readiness.MigrationsApplied);
        Assert.False(readiness.SeedComplete);
        Assert.NotEmpty(readiness.LogicalFingerprint);
    }

    [Theory]
    [InlineData("Production", "StudentRegistration_Production")]
    [InlineData("Staging", "StudentRegistration_Development")]
    [InlineData("Testing", "StudentRegistration_Test_shared")]
    public void Seed_and_reset_guards_reject_forbidden_targets_before_mutation(
        string environmentName,
        string databaseName)
    {
        Assert.Throws<InvalidOperationException>(() =>
            NonProductionDatabaseGuard.EnsureSeedAllowed(
                environmentName,
                databaseName));
        Assert.Throws<InvalidOperationException>(() =>
            NonProductionDatabaseGuard.EnsureExplicitResetAllowed(
                environmentName,
                databaseName));
    }
}
