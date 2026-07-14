using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Infrastructure;

public sealed class NonProductionSeedLifecycleTests
{
    private const string LifecyclePath =
        "tests/StudentRegistration.IntegrationTests/Infrastructure/non-production-lifecycle.json";

    [Fact]
    public void Contract_separates_disposable_testing_from_persistent_development()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(LifecyclePath));
        var root = document.RootElement;
        var testing = root.GetProperty("testing");
        var development = root.GetProperty("development");

        Assert.Equal("non-production-demo", root.GetProperty("scope").GetString());
        Assert.False(root.GetProperty("productionAuthorized").GetBoolean());
        Assert.Equal("Testing", testing.GetProperty("environment").GetString());
        Assert.True(testing.GetProperty("uniquePerRun").GetBoolean());
        Assert.True(testing.GetProperty("disposeAfterRun").GetBoolean());
        Assert.Equal("Development", development.GetProperty("environment").GetString());
        Assert.Equal(
            NonProductionDatabaseGuard.DevelopmentDatabaseName,
            development.GetProperty("databaseName").GetString());
        Assert.True(development.GetProperty("persistsUntilExplicitReset").GetBoolean());
        Assert.True(development.GetProperty("resetRequiresEnvironmentAndTargetGuard").GetBoolean());
    }

    [Theory]
    [InlineData("Production", "StudentRegistration_Production")]
    [InlineData("Staging", "StudentRegistration_Staging")]
    [InlineData("Development", "StudentRegistration_Test_deadbeefdeadbeefdeadbeefdeadbeef")]
    [InlineData("Testing", "StudentRegistration_Development")]
    [InlineData("Testing", "StudentRegistration_Test_shared")]
    public void Seed_and_reset_guards_reject_unapproved_environment_or_target(
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

    [Fact]
    public void Only_testing_is_disposed_and_development_requires_explicit_reset()
    {
        const string testingDatabase =
            "StudentRegistration_Test_deadbeefdeadbeefdeadbeefdeadbeef";

        NonProductionDatabaseGuard.EnsureSeedAllowed(
            "Testing",
            testingDatabase);
        NonProductionDatabaseGuard.EnsureSeedAllowed(
            "Development",
            NonProductionDatabaseGuard.DevelopmentDatabaseName);
        NonProductionDatabaseGuard.EnsureExplicitResetAllowed(
            "Development",
            NonProductionDatabaseGuard.DevelopmentDatabaseName);

        Assert.True(NonProductionDatabaseGuard.ShouldDisposeAfterRun(
            "Testing",
            testingDatabase));
        Assert.False(NonProductionDatabaseGuard.ShouldDisposeAfterRun(
            "Development",
            NonProductionDatabaseGuard.DevelopmentDatabaseName));
    }

    [Fact]
    public void Seed_profile_is_versioned_synthetic_idempotent_and_owner_composed()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(LifecyclePath));
        var seed = document.RootElement.GetProperty("seed");

        Assert.Equal(
            SqlServerTestDatabaseFixture.SeedProfileVersion,
            seed.GetProperty("profileVersion").GetString());
        Assert.Equal("synthetic-only", seed.GetProperty("dataClassification").GetString());
        Assert.True(seed.GetProperty("deterministicLogicalData").GetBoolean());
        Assert.True(seed.GetProperty("idempotentReseed").GetBoolean());
        Assert.Equal(
            ["SPEC-007", "SPEC-008"],
            seed.GetProperty("contributorOwnerSpecs")
                .EnumerateArray()
                .Select(item => item.GetString()!));
        Assert.Equal(
            "ASP.NET Identity hash only",
            seed.GetProperty("durableCredentialMaterial").GetString());
    }

    [Fact]
    public void Local_credentials_logs_and_exports_are_git_ignored_and_expire_after_seven_days()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(LifecyclePath));
        var lifecycle = document.RootElement.GetProperty("localArtifacts");
        var gitIgnore = RepositoryFiles.Read(".gitignore");

        Assert.Equal(LocalArtifactLifecycle.MaximumAgeDays, 7);
        Assert.Equal(7, lifecycle.GetProperty("maximumAgeDays").GetInt32());
        Assert.True(lifecycle.GetProperty("gitIgnored").GetBoolean());
        foreach (var root in LocalArtifactLifecycle.Roots)
        {
            Assert.Contains($"{root}/", gitIgnore, StringComparison.Ordinal);
        }

        var now = new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);
        Assert.True(LocalArtifactLifecycle.IsExpired(now.AddDays(-7), now));
        Assert.False(LocalArtifactLifecycle.IsExpired(now.AddDays(-7).AddTicks(1), now));
    }

    [Fact]
    public void Upstream_readiness_matches_the_executable_artifact_state()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(LifecyclePath));
        var root = document.RootElement;
        var readiness = root.GetProperty("upstreamReadiness");

        Assert.False(root.GetProperty("productionAuthorized").GetBoolean());
        var missingPaths = readiness.GetProperty("requiredPaths")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .Where(path => !RepositoryFiles.Exists(path))
            .ToArray();
        Assert.Equal(
            missingPaths.Length is 0 ? "ready" : "blocked",
            readiness.GetProperty("status").GetString());
        if (missingPaths.Length is not 0)
        {
            Assert.Contains(
                "No migration, seed, reset, release, or production claim is made",
                readiness.GetProperty("reason").GetString(),
                StringComparison.Ordinal);
        }
    }
}
