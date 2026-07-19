using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public async Task Compatibility_mismatch_stops_before_migration_seed_or_readiness()
    {
        var events = new List<string>();
        var bootstrapper = new RecordingBootstrapper(events);
        await using var fixture = new SqlServerTestDatabaseFixture(
            new CompatibilityMismatchRuntime(events),
            bootstrapper,
            "deadbeefdeadbeefdeadbeefdeadbeef");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.InitializeAsync());

        Assert.Contains("compatibility", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            [
                "bootstrap:availability",
                "runtime:start",
                "runtime:compatibility-mismatch",
                "runtime:dispose"
            ],
            events);
        Assert.Equal(0, bootstrapper.MigrationCalls);
        Assert.Equal(0, bootstrapper.SeedCalls);
        Assert.Equal(0, bootstrapper.ReadinessCalls);
        Assert.False(fixture.IsReady);
    }

    [Theory]
    [InlineData("Production", "StudentRegistration_Production")]
    [InlineData("Development", "StudentRegistration_Test_deadbeefdeadbeefdeadbeefdeadbeef")]
    [InlineData("Testing", "StudentRegistration_Development")]
    [InlineData("Testing", "StudentRegistration_Test_shared")]
    public void Environment_and_connection_target_must_both_validate_before_mutation(
        string environmentName,
        string databaseName)
    {
        var seedMutations = 0;
        var resetMutations = 0;

        Assert.Throws<InvalidOperationException>(() =>
        {
            NonProductionDatabaseGuard.EnsureSeedAllowed(environmentName, databaseName);
            seedMutations++;
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            NonProductionDatabaseGuard.EnsureExplicitResetAllowed(
                environmentName,
                databaseName);
            resetMutations++;
        });

        Assert.Equal(0, seedMutations);
        Assert.Equal(0, resetMutations);
    }

    private sealed class CompatibilityMismatchRuntime(List<string> events)
        : ITestSqlServerRuntime
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            events.Add("runtime:start");
            return Task.CompletedTask;
        }

        public Task CreateDatabaseAsync(
            string databaseName,
            int compatibilityLevel,
            CancellationToken cancellationToken)
        {
            Assert.Equal(SqlServerTestDatabaseFixture.CompatibilityLevel, compatibilityLevel);
            events.Add("runtime:compatibility-mismatch");
            throw new InvalidOperationException(
                "Injected SQL Server compatibility verification mismatch.");
        }

        public string BuildDatabaseConnectionString(string databaseName) =>
            throw new InvalidOperationException("A mismatched target cannot expose a connection.");

        public Task DropDatabaseAsync(
            string databaseName,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("No destructive cleanup is allowed before creation.");

        public ValueTask DisposeAsync()
        {
            events.Add("runtime:dispose");
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingBootstrapper(List<string> events)
        : ISqlServerTestDatabaseBootstrapper
    {
        public string ProfileVersion => SqlServerTestDatabaseFixture.SeedProfileVersion;

        public string DataClassification => "synthetic-only";

        public IReadOnlyList<string> ContributorOwnerSpecs => ["SPEC-007", "SPEC-008"];

        public int MigrationCalls { get; private set; }

        public int SeedCalls { get; private set; }

        public int ReadinessCalls { get; private set; }

        public Task EnsureAvailableAsync(CancellationToken cancellationToken)
        {
            events.Add("bootstrap:availability");
            return Task.CompletedTask;
        }

        public Task ApplyMigrationsAsync(
            string connectionString,
            CancellationToken cancellationToken)
        {
            MigrationCalls++;
            return Task.CompletedTask;
        }

        public Task<SyntheticSeedResult> SeedAsync(
            string connectionString,
            CancellationToken cancellationToken)
        {
            SeedCalls++;
            return Task.FromResult(new SyntheticSeedResult(
                ProfileVersion,
                "must-not-run",
                IsSyntheticOnly: true));
        }

        public Task<SqlServerTestDatabaseReadiness> VerifyReadinessAsync(
            string connectionString,
            CancellationToken cancellationToken)
        {
            ReadinessCalls++;
            return Task.FromResult(new SqlServerTestDatabaseReadiness(
                MigrationsApplied: false,
                SeedComplete: false,
                ProfileVersion,
                "must-not-run"));
        }
    }
}
