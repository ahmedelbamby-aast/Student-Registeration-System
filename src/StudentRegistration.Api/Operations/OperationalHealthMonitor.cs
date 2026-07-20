using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

namespace StudentRegistration.Api.Operations;

public interface IOperationalDependencyProbe
{
    Task<bool> IsSqlServerAvailableAsync(CancellationToken cancellationToken);
}

public sealed class SqlServerOperationalDependencyProbe(
    IServiceScopeFactory scopeFactory,
    ILogger<SqlServerOperationalDependencyProbe> logger)
    : IOperationalDependencyProbe
{
    public async Task<bool> IsSqlServerAvailableAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var database = scope.ServiceProvider
                .GetRequiredService<StudentRegistrationDbContext>()
                .Database;
            return await database.CanConnectAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                "The database readiness probe failed; health remains unavailable.");
            return false;
        }
    }
}

public sealed class OperationalHealthMonitor(
    OperationalHealthRegistry registry,
    IOperationalDependencyProbe dependencyProbe,
    ILogger<OperationalHealthMonitor> logger)
    : BackgroundService
{
    private static readonly TimeSpan ProbeInterval = TimeSpan.FromSeconds(15);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // No external telemetry exporter is configured in the local POC. The
        // bounded in-process metrics remain usable, while readiness honestly
        // reports degraded until a future approved exporter reports healthy.
        registry.SetTelemetryExporterAvailable(false);
        await RefreshSqlStatusAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(ProbeInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RefreshSqlStatusAsync(stoppingToken);
        }
    }

    private async Task RefreshSqlStatusAsync(CancellationToken cancellationToken)
    {
        var available = await dependencyProbe
            .IsSqlServerAvailableAsync(cancellationToken);
        registry.SetSqlServerAvailable(available);
        logger.LogDebug(
            "Database readiness probe completed with availability {Availability}.",
            available);
    }
}
