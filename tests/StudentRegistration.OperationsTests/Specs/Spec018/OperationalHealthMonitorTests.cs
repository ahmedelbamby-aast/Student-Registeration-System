using Microsoft.Extensions.Logging.Abstractions;
using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.OperationsTests.Specs.Spec018;

public sealed class OperationalHealthMonitorTests
{
    [Fact]
    public async Task Startup_probe_marks_reachable_sql_ready_before_serving_requests()
    {
        var registry = new OperationalHealthRegistry();
        var probe = new StubProbe(isAvailable: true);
        using var monitor = new OperationalHealthMonitor(
            registry,
            probe,
            NullLogger<OperationalHealthMonitor>.Instance);

        await monitor.StartAsync(CancellationToken.None);
        try
        {
            Assert.Equal(1, probe.CallCount);
            Assert.Equal(HealthSummaryStatus.Degraded, registry.GetStatus());
        }
        finally
        {
            await monitor.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Startup_probe_keeps_unreachable_sql_unhealthy()
    {
        var registry = new OperationalHealthRegistry();
        var probe = new StubProbe(isAvailable: false);
        using var monitor = new OperationalHealthMonitor(
            registry,
            probe,
            NullLogger<OperationalHealthMonitor>.Instance);

        await monitor.StartAsync(CancellationToken.None);
        try
        {
            Assert.Equal(HealthSummaryStatus.Unhealthy, registry.GetStatus());
        }
        finally
        {
            await monitor.StopAsync(CancellationToken.None);
        }
    }

    private sealed class StubProbe(bool isAvailable) : IOperationalDependencyProbe
    {
        public int CallCount { get; private set; }

        public Task<bool> IsSqlServerAvailableAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(isAvailable);
        }
    }
}
