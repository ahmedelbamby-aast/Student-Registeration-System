using System.Text.Json;
using Microsoft.AspNetCore.Http;
using StudentRegistration.Api.Endpoints;
using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.OperationsTests.Specs.Spec018;

public sealed class Endpoint01BehaviorTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 14, 9, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Healthy_probe_is_public_bounded_and_uses_authoritative_time()
    {
        var registry = new OperationalHealthRegistry();
        registry.SetSqlServerAvailable(true);
        registry.SetTelemetryExporterAvailable(true);

        var result = Spec018Endpoints.CreateHealthResult(
            registry,
            new FixedTimeProvider(Now));

        Assert.Equal(
            StatusCodes.Status200OK,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        var summary = Assert.IsType<HealthSummary>(
            Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Equal(HealthSummaryStatus.Healthy, summary.Status);
        Assert.Equal(Now.UtcDateTime, summary.TimestampUtc);
        Assert.False(string.IsNullOrWhiteSpace(summary.Version));

        var payload = JsonSerializer.Serialize(summary);
        Assert.DoesNotContain("sql", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("host", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("replica", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("certificate", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sql_unavailable_is_unhealthy_while_exporter_unavailable_is_degraded()
    {
        var registry = new OperationalHealthRegistry();
        registry.SetSqlServerAvailable(true);
        registry.SetTelemetryExporterAvailable(false);

        var degraded = Spec018Endpoints.CreateHealthResult(
            registry,
            new FixedTimeProvider(Now));
        Assert.Equal(
            StatusCodes.Status200OK,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(degraded).StatusCode);
        Assert.Equal(
            HealthSummaryStatus.Degraded,
            Assert.IsType<HealthSummary>(
                Assert.IsAssignableFrom<IValueHttpResult>(degraded).Value).Status);

        registry.SetSqlServerAvailable(false);
        var unhealthy = Spec018Endpoints.CreateHealthResult(
            registry,
            new FixedTimeProvider(Now));
        Assert.Equal(
            StatusCodes.Status503ServiceUnavailable,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(unhealthy).StatusCode);
        Assert.Equal(
            HealthSummaryStatus.Unhealthy,
            Assert.IsType<HealthSummary>(
                Assert.IsAssignableFrom<IValueHttpResult>(unhealthy).Value).Status);
    }

    [Fact]
    public void Unknown_startup_dependencies_fail_readiness_closed()
    {
        var result = Spec018Endpoints.CreateHealthResult(
            new OperationalHealthRegistry(),
            new FixedTimeProvider(Now));

        Assert.Equal(
            StatusCodes.Status503ServiceUnavailable,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal(
            HealthSummaryStatus.Unhealthy,
            Assert.IsType<HealthSummary>(
                Assert.IsAssignableFrom<IValueHttpResult>(result).Value).Status);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
