using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.MsSql;

namespace StudentRegistration.IntegrationTests.Infrastructure;

/// <summary>
/// Non-production SQL Server fixture for isolated demo tests. It does not
/// define or approve a production SQL Server edition or topology.
/// </summary>
public sealed class SqlServerContainerFixture : IAsyncDisposable
{
    public const string SqlServerImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";
    public const string VerificationMarker = "SPEC004_SQL_RUNTIME_OK";

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(5);
    private const string VerificationScript = """
        IF DB_ID(N'StudentRegistration_Test_Spec004') IS NOT NULL
        BEGIN
            ALTER DATABASE [StudentRegistration_Test_Spec004]
                SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [StudentRegistration_Test_Spec004];
        END;
        GO
        CREATE DATABASE [StudentRegistration_Test_Spec004];
        GO
        ALTER DATABASE [StudentRegistration_Test_Spec004]
            SET COMPATIBILITY_LEVEL = 160;
        GO
        IF TRY_CONVERT(int, SERVERPROPERTY('ProductMajorVersion')) <> 16
        BEGIN
            THROW 51000, 'Expected SQL Server 2022 major version 16.', 1;
        END;

        IF CONVERT(nvarchar(128), SERVERPROPERTY('Edition'))
            NOT LIKE N'Developer Edition%'
        BEGIN
            THROW 51001, 'Expected SQL Server Developer Edition.', 1;
        END;

        IF (SELECT compatibility_level
            FROM sys.databases
            WHERE [name] = N'StudentRegistration_Test_Spec004') <> 160
        BEGIN
            THROW 51002, 'Expected compatibility level 160.', 1;
        END;

        SELECT
            N'SPEC004_SQL_RUNTIME_OK' AS VerificationMarker,
            SERVERPROPERTY('ProductMajorVersion') AS ProductMajorVersion,
            SERVERPROPERTY('Edition') AS Edition,
            compatibility_level AS CompatibilityLevel
        FROM sys.databases
        WHERE [name] = N'StudentRegistration_Test_Spec004';
        GO
        ALTER DATABASE [StudentRegistration_Test_Spec004]
            SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        DROP DATABASE [StudentRegistration_Test_Spec004];
        GO
        """;

    private readonly MsSqlContainer _container;

    public SqlServerContainerFixture()
    {
        var password = $"Srs!1{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}a";
        _container = new MsSqlBuilder(SqlServerImage)
            .WithPassword(password)
            .WithEnvironment("MSSQL_PID", "Developer")
            .WithLogger(NullLogger.Instance)
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        timeout.CancelAfter(StartupTimeout);
        await _container.StartAsync(timeout.Token).ConfigureAwait(false);
    }

    public async Task<SqlServerRuntimeEvidence> VerifyApprovedRuntimeAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _container.ExecScriptAsync(
            VerificationScript,
            cancellationToken).ConfigureAwait(false);
        return new SqlServerRuntimeEvidence(
            result.ExitCode,
            result.Stdout,
            result.Stderr);
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}

public sealed record SqlServerRuntimeEvidence(
    long? ExitCode,
    string Stdout,
    string Stderr);

public sealed class SqlServerContainerRuntimeTests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Pinned_sql_2022_developer_runs_at_compatibility_160()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();
        using var verificationTimeout = new CancellationTokenSource(
            TimeSpan.FromMinutes(1));

        var evidence = await fixture.VerifyApprovedRuntimeAsync(
            verificationTimeout.Token);

        Assert.Equal<long?>(0L, evidence.ExitCode);
        Assert.Contains(
            SqlServerContainerFixture.VerificationMarker,
            evidence.Stdout,
            StringComparison.Ordinal);
        Assert.True(
            string.IsNullOrWhiteSpace(evidence.Stderr),
            evidence.Stderr);
    }
}
