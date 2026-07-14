using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public async Task Exporter_failure_degrades_health_while_bounded_fallback_keeps_requests_running()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider());
        services.AddStudentRegistrationObservability();
        await using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<OperationalHealthRegistry>();
        var telemetry = provider.GetRequiredService<OperationalTelemetry>();
        registry.SetSqlServerAvailable(true);
        registry.SetTelemetryExporterAvailable(false);

        string[] methods = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT", "OTHER"];
        string[] statuses = ["1xx", "2xx", "3xx", "4xx", "5xx"];
        string[] modules =
            ["academics", "api", "identity", "operations", "registration", "scheduling", "staff-administration"];
        for (var index = 0; index < OperationalTelemetry.MaximumSeries; index++)
        {
            telemetry.Increment(
                OperationalMetricNames.BusinessRejections,
                dimensions: new Dictionary<string, string>
                {
                    ["method"] = methods[index % methods.Length],
                    ["statusClass"] = statuses[(index / methods.Length) % statuses.Length],
                    ["module"] = modules[(index / (methods.Length * statuses.Length)) % modules.Length]
                });
        }

        var nextCalled = false;
        var builder = new ApplicationBuilder(provider);
        builder.UseStudentRegistrationObservability();
        builder.Run(context =>
        {
            nextCalled = true;
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext { RequestServices = provider };

        await builder.Build()(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        Assert.Equal(HealthSummaryStatus.Degraded, registry.GetStatus());
        Assert.Equal(OperationalTelemetry.MaximumSeries, telemetry.Snapshot().Count);
        Assert.Matches(
            "^[A-Za-z0-9_-]{8,64}$",
            context.Response.Headers["X-Correlation-ID"].ToString());
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 7, 14, 11, 0, 0, TimeSpan.Zero);
    }
}
