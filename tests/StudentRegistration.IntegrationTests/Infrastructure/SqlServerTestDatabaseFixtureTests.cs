namespace StudentRegistration.IntegrationTests.Infrastructure;

public sealed class SqlServerTestDatabaseFixtureTests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Pinned_testcontainers_runtime_creates_and_disposes_the_unique_database()
    {
        await using var fixture = new SqlServerTestDatabaseFixture(
            new FakeBootstrapper());

        await fixture.InitializeAsync();

        Assert.True(fixture.IsReady);
        Assert.NotNull(fixture.ConnectionString);
        Assert.Contains(fixture.DatabaseName, fixture.ConnectionString);
    }

    [Fact]
    public void Fixture_pins_testcontainers_sql_2022_developer_and_a_unique_testing_target()
    {
        var first = CreateFixture(new List<string>(), new FakeBootstrapper());
        var second = CreateFixture(new List<string>(), new FakeBootstrapper());

        Assert.StartsWith(
            "mcr.microsoft.com/mssql/server:2022-",
            SqlServerTestDatabaseFixture.SqlServerImage,
            StringComparison.Ordinal);
        Assert.Contains("@sha256:", SqlServerTestDatabaseFixture.SqlServerImage);
        Assert.Equal("Developer", SqlServerTestDatabaseFixture.SqlServerEdition);
        Assert.Equal(160, SqlServerTestDatabaseFixture.CompatibilityLevel);
        Assert.Equal("Testing", first.EnvironmentName);
        Assert.Matches(
            "^StudentRegistration_Test_[a-f0-9]{32}$",
            first.DatabaseName);
        Assert.NotEqual(first.DatabaseName, second.DatabaseName);
        Assert.DoesNotContain("Development", first.DatabaseName, StringComparison.Ordinal);
        Assert.DoesNotContain("Production", first.DatabaseName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Initialization_migrates_then_seeds_then_verifies_before_readiness_and_disposes()
    {
        var events = new List<string>();
        var bootstrapper = new FakeBootstrapper(events: events);
        await using var fixture = CreateFixture(events, bootstrapper);

        await fixture.InitializeAsync();

        Assert.True(fixture.IsReady);
        Assert.Equal("logical-fixture-v1", fixture.LogicalFingerprint);
        Assert.Equal(
            [
                "bootstrap:availability",
                "runtime:start",
                "runtime:create:160",
                "bootstrap:migrate",
                "bootstrap:seed",
                "bootstrap:verify"
            ],
            events);

        await fixture.DisposeAsync();

        Assert.Equal("runtime:drop", events[^2]);
        Assert.Equal("runtime:dispose", events[^1]);
        Assert.False(fixture.IsReady);
    }

    [Fact]
    public async Task Idempotent_reseed_preserves_the_logical_fingerprint()
    {
        var events = new List<string>();
        var bootstrapper = new FakeBootstrapper(events: events);
        await using var fixture = CreateFixture(events, bootstrapper);
        await fixture.InitializeAsync();

        var fingerprint = await fixture.ReseedAsync();

        Assert.Equal("logical-fixture-v1", fingerprint);
        Assert.Equal(2, bootstrapper.SeedCalls);
        Assert.True(fixture.IsReady);
        Assert.Equal("bootstrap:seed", events[^2]);
        Assert.Equal("bootstrap:verify", events[^1]);
    }

    [Fact]
    public async Task Independent_rebuilds_preserve_the_versioned_logical_fingerprint()
    {
        await using var first = CreateFixture([], new FakeBootstrapper());
        await using var second = CreateFixture([], new FakeBootstrapper());

        await first.InitializeAsync();
        await second.InitializeAsync();

        Assert.NotEqual(first.DatabaseName, second.DatabaseName);
        Assert.Equal(first.LogicalFingerprint, second.LogicalFingerprint);
        Assert.Equal("logical-fixture-v1", first.LogicalFingerprint);
    }

    [Fact]
    public async Task Partial_bootstrap_never_becomes_ready_and_is_cleaned_up()
    {
        var events = new List<string>();
        var bootstrapper = new FakeBootstrapper(
            events: events,
            failDuringSeed: true);
        var fixture = CreateFixture(events, bootstrapper);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.InitializeAsync());

        Assert.False(fixture.IsReady);
        Assert.Null(fixture.LogicalFingerprint);
        Assert.Contains("runtime:drop", events);
        Assert.Equal("runtime:dispose", events[^1]);
    }

    [Fact]
    public async Task Missing_canonical_migrations_or_contributors_fail_before_sql_mutation()
    {
        var events = new List<string>();
        await using var fixture = CreateFixture(
            events,
            UnavailableCanonicalDatabaseBootstrapper.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.InitializeAsync());

        Assert.Contains("migration and seed owners are not executable", exception.Message);
        Assert.DoesNotContain("runtime:start", events);
        Assert.DoesNotContain(events, item => item.StartsWith("runtime:create", StringComparison.Ordinal));
        Assert.False(fixture.IsReady);
    }

    private static SqlServerTestDatabaseFixture CreateFixture(
        List<string> events,
        ISqlServerTestDatabaseBootstrapper bootstrapper) =>
        new(new FakeRuntime(events), bootstrapper);

    private sealed class FakeRuntime(List<string> events) : ITestSqlServerRuntime
    {
        private bool _disposed;

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
            events.Add($"runtime:create:{compatibilityLevel}");
            return Task.CompletedTask;
        }

        public string BuildDatabaseConnectionString(string databaseName) =>
            $"Server=synthetic-test;Database={databaseName};";

        public Task DropDatabaseAsync(
            string databaseName,
            CancellationToken cancellationToken)
        {
            events.Add("runtime:drop");
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                events.Add("runtime:dispose");
                _disposed = true;
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeBootstrapper(
        List<string>? events = null,
        bool failDuringSeed = false) : ISqlServerTestDatabaseBootstrapper
    {
        private readonly List<string> _events = events ?? [];

        public string ProfileVersion => SqlServerTestDatabaseFixture.SeedProfileVersion;

        public string DataClassification => "synthetic-only";

        public IReadOnlyList<string> ContributorOwnerSpecs => ["SPEC-007", "SPEC-008"];

        public int SeedCalls { get; private set; }

        public Task EnsureAvailableAsync(CancellationToken cancellationToken)
        {
            _events.Add("bootstrap:availability");
            return Task.CompletedTask;
        }

        public Task ApplyMigrationsAsync(
            string connectionString,
            CancellationToken cancellationToken)
        {
            _events.Add("bootstrap:migrate");
            return Task.CompletedTask;
        }

        public Task<SyntheticSeedResult> SeedAsync(
            string connectionString,
            CancellationToken cancellationToken)
        {
            _events.Add("bootstrap:seed");
            SeedCalls++;
            if (failDuringSeed)
            {
                throw new InvalidOperationException("Synthetic partial-bootstrap failure.");
            }

            return Task.FromResult(new SyntheticSeedResult(
                ProfileVersion,
                "logical-fixture-v1",
                IsSyntheticOnly: true));
        }

        public Task<SqlServerTestDatabaseReadiness> VerifyReadinessAsync(
            string connectionString,
            CancellationToken cancellationToken)
        {
            _events.Add("bootstrap:verify");
            return Task.FromResult(new SqlServerTestDatabaseReadiness(
                MigrationsApplied: true,
                SeedComplete: true,
                ProfileVersion,
                "logical-fixture-v1"));
        }
    }
}
