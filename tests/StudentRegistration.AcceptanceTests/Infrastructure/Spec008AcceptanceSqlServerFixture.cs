using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.MsSql;

namespace StudentRegistration.AcceptanceTests.Infrastructure;

/// <summary>
/// Isolated non-production SQL Server runtime for SPEC-008 acceptance tests.
/// Migration and synthetic seed orchestration are supplied by their owner task.
/// </summary>
public sealed class Spec008AcceptanceSqlServerFixture : IAsyncDisposable
{
    public const string SqlServerImage =
        "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:e07b9699a2b749969f19d86563ceeea22bd3a69f7f1db85a8d1ac4bdaf0c6f56";

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(5);
    private readonly MsSqlContainer _container;

    public Spec008AcceptanceSqlServerFixture()
    {
        var password =
            $"Srs!1{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}a";
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

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
