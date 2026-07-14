using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.MsSql;

namespace StudentRegistration.IntegrationTests.Infrastructure;

/// <summary>
/// Owns one isolated Testing database and its migration-seed-readiness lifecycle.
/// This fixture grants no Development, release, or production authority.
/// </summary>
public sealed class SqlServerTestDatabaseFixture : IAsyncDisposable
{
    public const string SqlServerImage = SqlServerContainerFixture.SqlServerImage;
    public const string SqlServerEdition = "Developer";
    public const int CompatibilityLevel = 160;
    public const string SeedProfileVersion = "synthetic-fixture/1.0";
    public const string TestingEnvironmentName = "Testing";
    public const string TestingDatabasePrefix = "StudentRegistration_Test_";

    private static readonly string[] RequiredContributorOwners =
        ["SPEC-007", "SPEC-008"];

    private readonly ITestSqlServerRuntime _runtime;
    private readonly ISqlServerTestDatabaseBootstrapper _bootstrapper;
    private bool _databaseCreated;
    private bool _runtimeDisposed;
    private bool _initializationAttempted;

    public SqlServerTestDatabaseFixture()
        : this(
            new TestcontainersSqlServerRuntime(),
            UnavailableCanonicalDatabaseBootstrapper.Instance)
    {
    }

    public SqlServerTestDatabaseFixture(
        ISqlServerTestDatabaseBootstrapper bootstrapper)
        : this(new TestcontainersSqlServerRuntime(), bootstrapper)
    {
    }

    internal SqlServerTestDatabaseFixture(
        ITestSqlServerRuntime runtime,
        ISqlServerTestDatabaseBootstrapper bootstrapper,
        string? runId = null)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _bootstrapper = bootstrapper
            ?? throw new ArgumentNullException(nameof(bootstrapper));
        RunId = runId ?? Guid.NewGuid().ToString("N");
        if (!Regex.IsMatch(
                RunId,
                "^[a-f0-9]{32}$",
                RegexOptions.CultureInvariant | RegexOptions.NonBacktracking))
        {
            throw new ArgumentException(
                "A 32-character lowercase hexadecimal run ID is required.",
                nameof(runId));
        }

        DatabaseName = $"{TestingDatabasePrefix}{RunId}";
        NonProductionDatabaseGuard.EnsureSeedAllowed(
            TestingEnvironmentName,
            DatabaseName);
    }

    public string RunId { get; }

    public string EnvironmentName => TestingEnvironmentName;

    public string DatabaseName { get; }

    public string? ConnectionString { get; private set; }

    public bool IsReady { get; private set; }

    public string? LogicalFingerprint { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_runtimeDisposed, this);
        if (_initializationAttempted)
        {
            throw new InvalidOperationException(
                "A database fixture can be initialized only once.");
        }

        _initializationAttempted = true;
        ValidateBootstrapperContract();

        // Availability is checked before a container or database is mutated.
        await _bootstrapper.EnsureAvailableAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            await _runtime.StartAsync(cancellationToken).ConfigureAwait(false);
            await _runtime.CreateDatabaseAsync(
                    DatabaseName,
                    CompatibilityLevel,
                    cancellationToken)
                .ConfigureAwait(false);
            _databaseCreated = true;
            ConnectionString = _runtime.BuildDatabaseConnectionString(DatabaseName);

            await _bootstrapper.ApplyMigrationsAsync(
                    ConnectionString,
                    cancellationToken)
                .ConfigureAwait(false);
            var seed = await _bootstrapper.SeedAsync(
                    ConnectionString,
                    cancellationToken)
                .ConfigureAwait(false);
            ValidateSeed(seed);
            var readiness = await _bootstrapper.VerifyReadinessAsync(
                    ConnectionString,
                    cancellationToken)
                .ConfigureAwait(false);
            ValidateReadiness(seed, readiness);

            LogicalFingerprint = seed.LogicalFingerprint;
            IsReady = true;
        }
        catch
        {
            IsReady = false;
            LogicalFingerprint = null;
            try
            {
                await CleanupAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // Preserve the bootstrap failure; cleanup remains best effort.
            }

            throw;
        }
    }

    public async Task<string> ReseedAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsReady || ConnectionString is null || LogicalFingerprint is null)
        {
            throw new InvalidOperationException(
                "Only a fully ready Testing database can be re-seeded.");
        }

        var originalFingerprint = LogicalFingerprint;
        var seed = await _bootstrapper.SeedAsync(
                ConnectionString,
                cancellationToken)
            .ConfigureAwait(false);
        ValidateSeed(seed);
        var readiness = await _bootstrapper.VerifyReadinessAsync(
                ConnectionString,
                cancellationToken)
            .ConfigureAwait(false);

        try
        {
            ValidateReadiness(seed, readiness);
            if (!string.Equals(
                    originalFingerprint,
                    seed.LogicalFingerprint,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Idempotent re-seed changed the logical fixture fingerprint.");
            }
        }
        catch
        {
            IsReady = false;
            LogicalFingerprint = null;
            throw;
        }

        return seed.LogicalFingerprint;
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupAsync(CancellationToken.None).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private void ValidateBootstrapperContract()
    {
        if (!string.Equals(
                _bootstrapper.ProfileVersion,
                SeedProfileVersion,
                StringComparison.Ordinal)
            || !string.Equals(
                _bootstrapper.DataClassification,
                "synthetic-only",
                StringComparison.Ordinal)
            || !_bootstrapper.ContributorOwnerSpecs.SequenceEqual(
                RequiredContributorOwners,
                StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "The Testing bootstrapper must use the approved versioned synthetic profile and canonical SPEC-007/SPEC-008 contributor order.");
        }
    }

    private static void ValidateSeed(SyntheticSeedResult seed)
    {
        ArgumentNullException.ThrowIfNull(seed);
        if (!seed.IsSyntheticOnly
            || !string.Equals(
                seed.ProfileVersion,
                SeedProfileVersion,
                StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(seed.LogicalFingerprint))
        {
            throw new InvalidOperationException(
                "The seed result is incomplete, unversioned, or not synthetic-only.");
        }
    }

    private static void ValidateReadiness(
        SyntheticSeedResult seed,
        SqlServerTestDatabaseReadiness readiness)
    {
        ArgumentNullException.ThrowIfNull(readiness);
        if (!readiness.MigrationsApplied
            || !readiness.SeedComplete
            || !string.Equals(
                readiness.ProfileVersion,
                SeedProfileVersion,
                StringComparison.Ordinal)
            || !string.Equals(
                readiness.LogicalFingerprint,
                seed.LogicalFingerprint,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The Testing database failed migration, seed, or logical fingerprint readiness verification.");
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        if (_runtimeDisposed)
        {
            IsReady = false;
            return;
        }

        try
        {
            if (_databaseCreated)
            {
                await _runtime.DropDatabaseAsync(DatabaseName, cancellationToken)
                    .ConfigureAwait(false);
                _databaseCreated = false;
            }
        }
        finally
        {
            await _runtime.DisposeAsync().ConfigureAwait(false);
            _runtimeDisposed = true;
            ConnectionString = null;
            LogicalFingerprint = null;
            IsReady = false;
        }
    }
}

public interface ISqlServerTestDatabaseBootstrapper
{
    string ProfileVersion { get; }

    string DataClassification { get; }

    IReadOnlyList<string> ContributorOwnerSpecs { get; }

    Task EnsureAvailableAsync(CancellationToken cancellationToken);

    Task ApplyMigrationsAsync(
        string connectionString,
        CancellationToken cancellationToken);

    Task<SyntheticSeedResult> SeedAsync(
        string connectionString,
        CancellationToken cancellationToken);

    Task<SqlServerTestDatabaseReadiness> VerifyReadinessAsync(
        string connectionString,
        CancellationToken cancellationToken);
}

public sealed record SyntheticSeedResult(
    string ProfileVersion,
    string LogicalFingerprint,
    bool IsSyntheticOnly);

public sealed record SqlServerTestDatabaseReadiness(
    bool MigrationsApplied,
    bool SeedComplete,
    string ProfileVersion,
    string LogicalFingerprint);

public sealed class UnavailableCanonicalDatabaseBootstrapper
    : ISqlServerTestDatabaseBootstrapper
{
    public static UnavailableCanonicalDatabaseBootstrapper Instance { get; } = new();

    private const string BlockedMessage =
        "The migration and seed owners are not executable; no SQL mutation or readiness claim is allowed.";

    private UnavailableCanonicalDatabaseBootstrapper()
    {
    }

    public string ProfileVersion => SqlServerTestDatabaseFixture.SeedProfileVersion;

    public string DataClassification => "synthetic-only";

    public IReadOnlyList<string> ContributorOwnerSpecs => ["SPEC-007", "SPEC-008"];

    public Task EnsureAvailableAsync(CancellationToken cancellationToken) =>
        Task.FromException(new InvalidOperationException(BlockedMessage));

    public Task ApplyMigrationsAsync(
        string connectionString,
        CancellationToken cancellationToken) =>
        Task.FromException(new InvalidOperationException(BlockedMessage));

    public Task<SyntheticSeedResult> SeedAsync(
        string connectionString,
        CancellationToken cancellationToken) =>
        Task.FromException<SyntheticSeedResult>(
            new InvalidOperationException(BlockedMessage));

    public Task<SqlServerTestDatabaseReadiness> VerifyReadinessAsync(
        string connectionString,
        CancellationToken cancellationToken) =>
        Task.FromException<SqlServerTestDatabaseReadiness>(
            new InvalidOperationException(BlockedMessage));
}

public static class NonProductionDatabaseGuard
{
    public const string DevelopmentDatabaseName = "StudentRegistration_Development";

    private static readonly Regex TestingDatabaseNamePattern = new(
        "^StudentRegistration_Test_[a-f0-9]{32}$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    public static void EnsureSeedAllowed(
        string environmentName,
        string databaseName) =>
        EnsureAllowedTarget(environmentName, databaseName, "Seed");

    public static void EnsureExplicitResetAllowed(
        string environmentName,
        string databaseName) =>
        EnsureAllowedTarget(environmentName, databaseName, "Explicit reset");

    public static bool ShouldDisposeAfterRun(
        string environmentName,
        string databaseName)
    {
        EnsureAllowedTarget(environmentName, databaseName, "Disposal");
        return string.Equals(environmentName, "Testing", StringComparison.Ordinal);
    }

    private static void EnsureAllowedTarget(
        string environmentName,
        string databaseName,
        string operation)
    {
        var developmentTarget =
            string.Equals(environmentName, "Development", StringComparison.Ordinal)
            && string.Equals(
                databaseName,
                DevelopmentDatabaseName,
                StringComparison.Ordinal);
        var testingTarget =
            string.Equals(environmentName, "Testing", StringComparison.Ordinal)
            && databaseName is not null
            && TestingDatabaseNamePattern.IsMatch(databaseName);

        if (!developmentTarget && !testingTarget)
        {
            throw new InvalidOperationException(
                $"{operation} requires an exact Development or unique per-run Testing environment/database target.");
        }
    }
}

public static class LocalArtifactLifecycle
{
    public const int MaximumAgeDays = 7;

    public static IReadOnlyList<string> Roots { get; } =
        [".local", "credentials", "logs", "exports"];

    public static bool IsExpired(
        DateTimeOffset createdAtUtc,
        DateTimeOffset nowUtc) =>
        createdAtUtc <= nowUtc.AddDays(-MaximumAgeDays);
}

internal interface ITestSqlServerRuntime : IAsyncDisposable
{
    Task StartAsync(CancellationToken cancellationToken);

    Task CreateDatabaseAsync(
        string databaseName,
        int compatibilityLevel,
        CancellationToken cancellationToken);

    string BuildDatabaseConnectionString(string databaseName);

    Task DropDatabaseAsync(
        string databaseName,
        CancellationToken cancellationToken);
}

internal sealed class TestcontainersSqlServerRuntime : ITestSqlServerRuntime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(5);
    private readonly MsSqlContainer _container;

    public TestcontainersSqlServerRuntime()
    {
        var password =
            $"Srs!1{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}a";
        _container = new MsSqlBuilder(SqlServerTestDatabaseFixture.SqlServerImage)
            .WithPassword(password)
            .WithEnvironment("MSSQL_PID", SqlServerTestDatabaseFixture.SqlServerEdition)
            .WithLogger(NullLogger.Instance)
            .Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        timeout.CancelAfter(StartupTimeout);
        await _container.StartAsync(timeout.Token).ConfigureAwait(false);
    }

    public async Task CreateDatabaseAsync(
        string databaseName,
        int compatibilityLevel,
        CancellationToken cancellationToken)
    {
        NonProductionDatabaseGuard.EnsureSeedAllowed(
            SqlServerTestDatabaseFixture.TestingEnvironmentName,
            databaseName);
        if (compatibilityLevel is not SqlServerTestDatabaseFixture.CompatibilityLevel)
        {
            throw new InvalidOperationException(
                "Only SQL Server compatibility level 160 is approved for this fixture.");
        }

        var script = $"""
            IF TRY_CONVERT(int, SERVERPROPERTY('ProductMajorVersion')) <> 16
                THROW 51010, 'Expected SQL Server 2022 major version 16.', 1;
            IF CONVERT(nvarchar(128), SERVERPROPERTY('Edition'))
                NOT LIKE N'Developer Edition%'
                THROW 51011, 'Expected SQL Server Developer Edition.', 1;
            GO
            IF DB_ID(N'{databaseName}') IS NOT NULL
                THROW 51012, 'Per-run Testing database already exists.', 1;
            GO
            CREATE DATABASE [{databaseName}];
            GO
            ALTER DATABASE [{databaseName}]
                SET COMPATIBILITY_LEVEL = {compatibilityLevel};
            GO
            IF (SELECT compatibility_level FROM sys.databases
                WHERE [name] = N'{databaseName}') <> {compatibilityLevel}
                THROW 51013, 'Testing database compatibility verification failed.', 1;
            GO
            """;
        var result = await _container.ExecScriptAsync(script, cancellationToken)
            .ConfigureAwait(false);
        if (result.ExitCode is not 0)
        {
            throw new InvalidOperationException(
                $"SQL Server Testing database creation failed with exit code {result.ExitCode?.ToString() ?? "unknown"}.");
        }
    }

    public string BuildDatabaseConnectionString(string databaseName)
    {
        NonProductionDatabaseGuard.EnsureSeedAllowed(
            SqlServerTestDatabaseFixture.TestingEnvironmentName,
            databaseName);
        return new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = databaseName
        }.ConnectionString;
    }

    public async Task DropDatabaseAsync(
        string databaseName,
        CancellationToken cancellationToken)
    {
        NonProductionDatabaseGuard.EnsureExplicitResetAllowed(
            SqlServerTestDatabaseFixture.TestingEnvironmentName,
            databaseName);
        var script = $"""
            IF DB_ID(N'{databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{databaseName}]
                    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{databaseName}];
            END;
            """;
        var result = await _container.ExecScriptAsync(script, cancellationToken)
            .ConfigureAwait(false);
        if (result.ExitCode is not 0)
        {
            throw new InvalidOperationException(
                $"SQL Server Testing database disposal failed with exit code {result.ExitCode?.ToString() ?? "unknown"}.");
        }
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
