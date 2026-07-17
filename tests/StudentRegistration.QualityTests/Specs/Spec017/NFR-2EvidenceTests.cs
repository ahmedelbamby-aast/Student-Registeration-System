using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Admin;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.TestSupport;
using Testcontainers.MsSql;

namespace StudentRegistration.QualityTests.Specs.Spec017;

[CollectionDefinition(Name)]
public sealed class Spec017AuditSqlEvidenceCollection
    : ICollectionFixture<Spec017AuditSqlEvidenceFixture>
{
    public const string Name = "SPEC-017 audit SQL evidence";
}

[Collection(Spec017AuditSqlEvidenceCollection.Name)]
public sealed class NFR_2EvidenceTests(Spec017AuditSqlEvidenceFixture fixture)
{
    private const int WarmupQueries = 5;
    private const int MeasuredQueries = 30;
    private const int PageSize = 20;
    private const double MaximumP95Milliseconds = 1_000d;
    private const string ImplementationCommit =
        "e3c567f43e608597a66b9d8e5f1f0bcfc4b04594";
    private const string RawArtifactPath =
        "docs/release-evidence/SPEC-017-NFR-2-raw.json";
    private const string EvidencePath =
        "docs/release-evidence/SPEC-017-NFR-2.md";
    private const string Command =
        "dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj --no-restore --filter FullyQualifiedName~Spec017.NFR_2EvidenceTests";

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Merged_audit_first_page_meets_the_one_second_p95_demo_profile()
    {
        await using var context = fixture.CreateContext();
        var queries = new AuditEventQueries(new SqlAdminAuditReader(context));
        var query = new AdminAuditQuery(
            AdminAuditScope.All(includeIdentitySecurityEvents: true),
            Page: 1,
            PageSize: PageSize);

        for (var index = 0; index < WarmupQueries; index++)
        {
            var page = await queries.SearchAsync(query);
            Assert.Equal(PageSize, page.Items.Count);
            Assert.Equal(Spec017AuditSqlEvidenceFixture.TotalRows, page.TotalCount);
        }

        var rawSamples = new double[MeasuredQueries];
        for (var index = 0; index < rawSamples.Length; index++)
        {
            var started = Stopwatch.GetTimestamp();
            var page = await queries.SearchAsync(query);
            rawSamples[index] = Math.Round(
                Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                4,
                MidpointRounding.AwayFromZero);
            Assert.Equal(PageSize, page.Items.Count);
            Assert.Equal(Spec017AuditSqlEvidenceFixture.TotalRows, page.TotalCount);
        }

        var sortedSamples = rawSamples.Order().ToArray();
        var p95Rank = (int)Math.Ceiling(MeasuredQueries * 0.95d);
        var p95Milliseconds = sortedSamples[p95Rank - 1];
        Console.WriteLine(FormattableString.Invariant(
            $"""SPEC-017 NFR-2 rows={Spec017AuditSqlEvidenceFixture.TotalRows}; warmup={WarmupQueries}; measured={MeasuredQueries}; p95-rank={p95Rank}; p95-ms={p95Milliseconds:F4}; threshold-ms={MaximumP95Milliseconds:F0}"""));
        Assert.True(
            p95Milliseconds <= MaximumP95Milliseconds,
            $"Measured p95 {p95Milliseconds:F4} ms exceeded " +
            $"{MaximumP95Milliseconds:F0} ms.");
    }

    [Fact]
    public void Recorded_run_has_raw_samples_correct_p95_and_reproduction_context()
    {
        var artifact = JsonSerializer.Deserialize<AuditLatencyArtifact>(
            RepositoryFiles.Read(RawArtifactPath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.Equal("1.0.0", artifact.ArtifactVersion);
        Assert.Equal("SPEC-017/NFR-2", artifact.Requirement);
        Assert.Equal(ImplementationCommit, artifact.SourceCommit);
        Assert.Equal(Command, artifact.Command);
        Assert.Equal(100_000, artifact.Fixture.TotalRows);
        Assert.Equal(50_000, artifact.Fixture.AuditEventRows);
        Assert.Equal(50_000, artifact.Fixture.SecurityEventRows);
        Assert.Equal(MeasuredQueries, artifact.RawSamplesMilliseconds.Count);
        Assert.Equal(160, artifact.Environment.CompatibilityLevel);
        Assert.Contains("2022-CU25", artifact.Environment.DatabaseImage);

        var ordered = artifact.RawSamplesMilliseconds.Order().ToArray();
        var calculated = ordered[artifact.Calculation.Rank - 1];
        Assert.Equal(29, artifact.Calculation.Rank);
        Assert.Equal(calculated, artifact.Calculation.P95Milliseconds);
        Assert.True(artifact.Calculation.P95Milliseconds <= MaximumP95Milliseconds);
        Assert.Equal("PASS", artifact.Result);

        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-017 NFR-2 Audit First-Page Latency Evidence",
            ImplementationCommit,
            RawArtifactPath,
            "100,000 merged rows",
            "30 measured queries",
            "nearest-rank",
            "47.4728 ms",
            "1,000 ms",
            "**Result: PASS.**");
    }

    private sealed record AuditLatencyArtifact(
        string ArtifactVersion,
        string Requirement,
        DateTimeOffset RecordedAtUtc,
        string SourceCommit,
        string Command,
        EvidenceEnvironment Environment,
        AuditFixture Fixture,
        IReadOnlyList<double> RawSamplesMilliseconds,
        P95Calculation Calculation,
        string Result);

    private sealed record EvidenceEnvironment(
        string OperatingSystem,
        string DotnetRuntime,
        string ProcessArchitecture,
        int LogicalProcessorCount,
        string DatabaseImage,
        string DatabaseProductVersion,
        string DatabaseEdition,
        int CompatibilityLevel);

    private sealed record AuditFixture(
        int Seed,
        int TotalRows,
        int AuditEventRows,
        int SecurityEventRows,
        int PageSize,
        int WarmupQueries,
        int MeasuredQueries);

    private sealed record P95Calculation(
        string Method,
        string Formula,
        int Rank,
        double P95Milliseconds,
        double ThresholdMilliseconds);
}

public sealed class Spec017AuditSqlEvidenceFixture : IAsyncLifetime
{
    public const int FixtureSeed = 170017;
    public const int AuditRows = 50_000;
    public const int SecurityRows = 50_000;
    public const int TotalRows = AuditRows + SecurityRows;
    public const string DatabaseImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";

    private readonly MsSqlContainer _container = new MsSqlBuilder(DatabaseImage)
        .WithPassword($"Srs!1{Guid.NewGuid():N}a")
        .Build();
    private DbContextOptions<StudentRegistrationDbContext>? _options;

    public string DatabaseProductVersion { get; private set; } = string.Empty;
    public string DatabaseEdition { get; private set; } = string.Empty;
    public int CompatibilityLevel { get; private set; }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var created = await _container.ExecScriptAsync($"""
            CREATE DATABASE [{databaseName}];
            ALTER DATABASE [{databaseName}] SET COMPATIBILITY_LEVEL = 160;
            """);
        Assert.Equal(0L, created.ExitCode);

        var connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = databaseName
        }.ConnectionString;
        _options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using (var context = CreateContext())
        {
            Assert.True(await context.Database.EnsureCreatedAsync());
        }
        await SeedAsync(connectionString);
        await ReadDatabaseEnvironmentAsync(connectionString);
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public StudentRegistrationDbContext CreateContext() => new(
        _options ?? throw new InvalidOperationException(
            "The SPEC-017 SQL evidence fixture is not initialized."));

    private static async Task SeedAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 120;
        command.CommandText = $$"""
            WITH numbers AS
            (
                SELECT TOP ({{AuditRows}})
                    ROW_NUMBER() OVER (ORDER BY firstSet.[object_id], secondSet.[object_id]) AS [Ordinal]
                FROM sys.all_objects AS firstSet
                CROSS JOIN sys.all_objects AS secondSet
            )
            INSERT INTO [audit].[AuditEvents]
                ([Id],[ActorReference],[SubjectReference],[Action],[EntityType],[EntityId],
                 [Reason],[BeforeSummaryJson],[AfterSummaryJson],[CorrelationId],[OccurredAtUtc])
            SELECT
                CAST(HASHBYTES('MD5', CONCAT(N'audit-', [Ordinal], N'-{{FixtureSeed}}')) AS uniqueidentifier),
                CONCAT(N'admin:', ([Ordinal] % 100) + 1),
                CONCAT(N'student:', ([Ordinal] % 25000) + 1),
                CASE [Ordinal] % 3 WHEN 0 THEN N'CapacityChanged'
                    WHEN 1 THEN N'StatusChanged' ELSE N'PolicyPublished' END,
                N'SyntheticEntity',
                CONVERT(nvarchar(20), [Ordinal]),
                N'SPEC-017 NFR-2 approved demo fixture',
                CONCAT(N'{"state":"before","capacity":', [Ordinal] % 100, N'}'),
                CONCAT(N'{"state":"after","capacity":', ([Ordinal] % 100) + 1, N'}'),
                CONCAT(N'audit-correlation-', [Ordinal]),
                DATEADD(second, -[Ordinal], CAST('2026-07-17T12:00:00' AS datetime2))
            FROM numbers;

            WITH numbers AS
            (
                SELECT TOP ({{SecurityRows}})
                    ROW_NUMBER() OVER (ORDER BY firstSet.[object_id], secondSet.[object_id]) AS [Ordinal]
                FROM sys.all_objects AS firstSet
                CROSS JOIN sys.all_objects AS secondSet
            )
            INSERT INTO [auth].[SecurityEvents]
                ([Id],[ApplicationUserId],[EventType],[ActorReference],[SubjectReference],
                 [Reason],[BeforeSummaryJson],[AfterSummaryJson],[MetadataJson],
                 [CorrelationId],[OccurredAtUtc])
            SELECT
                CAST(HASHBYTES('MD5', CONCAT(N'security-', [Ordinal], N'-{{FixtureSeed}}')) AS uniqueidentifier),
                NULL,
                CASE [Ordinal] % 2 WHEN 0 THEN N'RoleChanged' ELSE N'AccountStateChanged' END,
                CONCAT(N'admin:', ([Ordinal] % 100) + 1),
                CONCAT(N'student:', ([Ordinal] % 25000) + 1),
                N'SPEC-017 NFR-2 approved demo fixture',
                N'{"roles":"Lecturer","state":"before"}',
                N'{"roles":"Admin","state":"after"}',
                N'{"source":"spec017-quality"}',
                CONCAT(N'security-correlation-', [Ordinal]),
                DATEADD(second, -[Ordinal] - 1, CAST('2026-07-17T12:00:00' AS datetime2))
            FROM numbers;

            UPDATE STATISTICS [audit].[AuditEvents] WITH FULLSCAN;
            UPDATE STATISTICS [auth].[SecurityEvents] WITH FULLSCAN;
            """;
        await command.ExecuteNonQueryAsync();
    }

    private async Task ReadDatabaseEnvironmentAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                CONVERT(nvarchar(128), SERVERPROPERTY('ProductVersion')),
                CONVERT(nvarchar(128), SERVERPROPERTY('Edition')),
                [compatibility_level]
            FROM sys.databases
            WHERE [name] = DB_NAME();
            """;
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        DatabaseProductVersion = reader.GetString(0);
        DatabaseEdition = reader.GetString(1);
        CompatibilityLevel = Convert.ToInt32(
            reader.GetValue(2),
            CultureInfo.InvariantCulture);
        Assert.Equal(160, CompatibilityLevel);
    }
}
