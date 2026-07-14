using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Api.Operations;
using StudentRegistration.TestSupport;

namespace StudentRegistration.OperationsTests;

public sealed class ObservabilitySignalTests
{
    [Fact]
    public async Task Pipeline_provides_safe_correlation_latency_and_throughput_signals()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 14, 10, 0, 0, TimeSpan.Zero)));
        services.AddStudentRegistrationObservability();
        await using var provider = services.BuildServiceProvider();
        var builder = new ApplicationBuilder(provider);
        builder.UseStudentRegistrationObservability();
        builder.Run(context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });
        var pipeline = builder.Build();
        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };
        context.Request.Path = "/api/health";
        context.Request.Headers["X-Correlation-ID"] = "20260001";

        await pipeline(context);

        Assert.True(context.Response.Headers.TryGetValue(
            "X-Correlation-ID",
            out var correlationId));
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
        Assert.NotEqual("20260001", correlationId.ToString());
        Assert.Matches("^[A-Za-z0-9_-]{8,64}$", correlationId.ToString());
        var signals = provider.GetRequiredService<OperationalTelemetry>().Snapshot();
        Assert.Contains(signals, metric =>
            metric.Name == OperationalMetricNames.RequestLatencyMilliseconds);
        Assert.Contains(signals, metric =>
            metric.Name == OperationalMetricNames.RequestThroughput);
    }

    [Fact]
    public void Required_operational_signals_are_allow_listed_and_safe()
    {
        using var telemetry = new OperationalTelemetry(
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 14, 10, 0, 0, TimeSpan.Zero)));

        telemetry.Observe(OperationalMetricNames.RequestLatencyMilliseconds, 10);
        telemetry.Increment(OperationalMetricNames.RequestThroughput);
        telemetry.Increment(OperationalMetricNames.UnexpectedErrors);
        telemetry.Increment(
            OperationalMetricNames.BusinessRejections,
            dimensions: new Dictionary<string, string> { ["code"] = "POLICY_REJECTED" });
        telemetry.Observe(OperationalMetricNames.OptimizerDurationMilliseconds, 20);
        telemetry.Observe(OperationalMetricNames.SqlDurationMilliseconds, 4);
        telemetry.Observe(OperationalMetricNames.SqlLockWaitMilliseconds, 1);
        telemetry.Increment(OperationalMetricNames.SqlDeadlocks);
        telemetry.Increment(OperationalMetricNames.CapacityConflicts);
        telemetry.Increment(OperationalMetricNames.CounterMismatches);

        var names = telemetry.Snapshot().Select(metric => metric.Name).ToHashSet();
        Assert.Equal(10, names.Count);
        Assert.Contains(OperationalMetricNames.RequestLatencyMilliseconds, names);
        Assert.Contains(OperationalMetricNames.RequestThroughput, names);
        Assert.Contains(OperationalMetricNames.UnexpectedErrors, names);
        Assert.Contains(OperationalMetricNames.BusinessRejections, names);
        Assert.Contains(OperationalMetricNames.OptimizerDurationMilliseconds, names);
        Assert.Contains(OperationalMetricNames.SqlDurationMilliseconds, names);
        Assert.Contains(OperationalMetricNames.SqlLockWaitMilliseconds, names);
        Assert.Contains(OperationalMetricNames.SqlDeadlocks, names);
        Assert.Contains(OperationalMetricNames.CapacityConflicts, names);
        Assert.Contains(OperationalMetricNames.CounterMismatches, names);
    }

    [Fact]
    public void Concurrent_counter_updates_do_not_lose_measurements()
    {
        using var telemetry = new OperationalTelemetry(
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 14, 10, 0, 0, TimeSpan.Zero)));

        Parallel.For(
            0,
            1_000,
            _ => telemetry.Increment(OperationalMetricNames.RequestThroughput));

        var metric = Assert.Single(telemetry.Snapshot());
        Assert.Equal(1_000, metric.Value);
    }

    [Fact]
    public async Task Full_bounded_metric_buffer_does_not_break_the_request_pipeline()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 14, 10, 0, 0, TimeSpan.Zero)));
        services.AddStudentRegistrationObservability();
        await using var provider = services.BuildServiceProvider();
        var telemetry = provider.GetRequiredService<OperationalTelemetry>();
        FillToSeriesLimit(telemetry);

        var nextWasCalled = false;
        var builder = new ApplicationBuilder(provider);
        builder.UseStudentRegistrationObservability();
        builder.Run(context =>
        {
            nextWasCalled = true;
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        });

        await builder.Build()(new DefaultHttpContext
        {
            RequestServices = provider
        });

        Assert.True(nextWasCalled);
        Assert.Equal(OperationalTelemetry.MaximumSeries, telemetry.Snapshot().Count);
    }

    [Fact]
    public void Observability_source_excludes_raw_requests_and_sensitive_values()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Operations/ObservabilityExtensions.cs");

        RepositoryFiles.ContainsAll(
            source,
            "ActivitySource",
            "Meter",
            "X-Correlation-ID",
            "METRIC_CARDINALITY_LIMIT");
        Assert.DoesNotContain("Request.Body", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Request.Query", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ConnectionString", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Password", source, StringComparison.Ordinal);
        Assert.DoesNotContain("UniversityId", source, StringComparison.Ordinal);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static void FillToSeriesLimit(OperationalTelemetry telemetry)
    {
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
    }
}
