using StudentRegistration.Api.Operations;
using StudentRegistration.Contracts.Operations;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Admin;

public sealed class AdminMetricsTests
{
    private static readonly DateTime NowUtc =
        new(2026, 7, 17, 10, 0, 0, DateTimeKind.Utc);
    private static readonly OperationalMetricScope Scope =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Guid.Parse("20000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Complete_fresh_scope_is_live_and_threshold_breaches_have_support_references()
    {
        var reader = new StubReader(Available(CompleteMetrics(NowUtc)));
        var query = new AdminMetricsQuery(
            reader,
            new FixedTimeProvider(NowUtc),
            new(ServerFailures: 1, CapacityConflicts: 1, DataQualityAlerts: 0));

        var result = await query.ExecuteAsync(Scope);

        Assert.Equal(Scope, reader.LastScope);
        Assert.Equal(AdminMetricsAvailability.Live, result.AvailabilityState);
        Assert.Equal(NowUtc, result.ObservedAtUtc);
        Assert.Equal(AdminMetricNames.Required.Count, result.Metrics.Count);
        Assert.Empty(result.MissingMetricNames);
        Assert.Collection(
            result.Alerts.OrderBy(alert => alert.Metric, StringComparer.Ordinal),
            alert => AssertAlert(
                alert,
                AdminMetricNames.ServerFailures,
                1,
                3,
                "/admin/audit?action=registration.server-failure"),
            alert => AssertAlert(
                alert,
                AdminMetricNames.CapacityConflicts,
                1,
                2,
                "/admin/audit?action=registration.capacity-conflict"),
            alert => AssertAlert(
                alert,
                AdminMetricNames.DataQualityAlerts,
                0,
                1,
                "/admin/audit?action=registration.data-quality"));
    }

    [Fact]
    public async Task Complete_observations_older_than_sixty_seconds_are_stale_not_retimed()
    {
        var observed = NowUtc.AddSeconds(-61);
        var query = new AdminMetricsQuery(
            new StubReader(Available(CompleteMetrics(observed))),
            new FixedTimeProvider(NowUtc));

        var result = await query.ExecuteAsync(Scope);

        Assert.Equal(AdminMetricsAvailability.Stale, result.AvailabilityState);
        Assert.Equal(observed, result.ObservedAtUtc);
        Assert.All(result.Metrics, metric => Assert.Equal(observed, metric.ObservedAtUtc));
    }

    [Fact]
    public async Task Missing_series_are_degraded_and_never_fabricated_as_zero()
    {
        var traffic = Metric(AdminMetricNames.Traffic, 75, NowUtc);
        var query = new AdminMetricsQuery(
            new StubReader(Available([traffic])),
            new FixedTimeProvider(NowUtc));

        var result = await query.ExecuteAsync(Scope);

        Assert.Equal(AdminMetricsAvailability.Degraded, result.AvailabilityState);
        Assert.Same(traffic, Assert.Single(result.Metrics));
        Assert.Equal(
            AdminMetricNames.Required.Count - 1,
            result.MissingMetricNames.Count);
        Assert.DoesNotContain(result.Metrics, metric =>
            metric.Name == AdminMetricNames.FillRate && metric.Value == 0);
        Assert.DoesNotContain(result.Metrics, metric =>
            metric.Name == AdminMetricNames.Success && metric.Value == 0);
    }

    [Fact]
    public async Task Unavailable_source_preserves_last_observation_and_is_degraded()
    {
        var observed = NowUtc.AddMinutes(-5);
        var lastTraffic = Metric(AdminMetricNames.Traffic, 42, observed);
        var query = new AdminMetricsQuery(
            new StubReader(new(
                OperationalMetricSourceState.Unavailable,
                [lastTraffic])),
            new FixedTimeProvider(NowUtc));

        var result = await query.ExecuteAsync(Scope);

        Assert.Equal(AdminMetricsAvailability.Degraded, result.AvailabilityState);
        Assert.Equal(observed, result.ObservedAtUtc);
        Assert.Equal(42, Assert.Single(result.Metrics).Value);
    }

    [Fact]
    public async Task Api_adapter_returns_only_observed_privacy_safe_series()
    {
        using var telemetry = new OperationalTelemetry(new FixedTimeProvider(NowUtc));
        telemetry.Increment(OperationalMetricNames.RequestThroughput, 12);
        telemetry.Increment(OperationalMetricNames.UnexpectedErrors, 2);
        var reader = new OperationalMetricReader(telemetry);

        var result = await reader.ReadAsync(Scope);

        Assert.Equal(OperationalMetricSourceState.Available, result.SourceState);
        Assert.Equal(2, result.Metrics.Count);
        Assert.Contains(result.Metrics, metric =>
            metric.Name == AdminMetricNames.Traffic && metric.Value == 12);
        Assert.Contains(result.Metrics, metric =>
            metric.Name == AdminMetricNames.ServerFailures && metric.Value == 2);
        Assert.DoesNotContain(result.Metrics, metric => metric.Name == AdminMetricNames.FillRate);
        Assert.DoesNotContain(result.Metrics, metric => metric.Name == AdminMetricNames.Success);
    }

    [Fact]
    public void Scope_is_required_and_module_dependency_direction_stays_intact()
    {
        Assert.Throws<ArgumentException>(() => new OperationalMetricScope(Guid.Empty, Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new OperationalMetricScope(Guid.NewGuid(), Guid.Empty));

        var project = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/StudentRegistration.StaffAdministration.csproj");
        Assert.DoesNotContain("StudentRegistration.Api", project, StringComparison.Ordinal);
        Assert.DoesNotContain("StudentRegistration.Infrastructure.SqlServer", project, StringComparison.Ordinal);
        var query = RepositoryFiles.Read(
            "src/StudentRegistration.StaffAdministration/Application/AdminMetricsQuery.cs");
        Assert.DoesNotContain("StudentRegistration.Api", query, StringComparison.Ordinal);
        Assert.DoesNotContain("EntityFrameworkCore", query, StringComparison.Ordinal);
    }

    private static OperationalMetricReadResult Available(
        IReadOnlyList<OperationalMetric> metrics) => new(
        OperationalMetricSourceState.Available,
        metrics);

    private static IReadOnlyList<OperationalMetric> CompleteMetrics(DateTime observedAtUtc) =>
    [
        Metric(AdminMetricNames.Traffic, 75, observedAtUtc),
        Metric(AdminMetricNames.Success, 60, observedAtUtc),
        Metric(AdminMetricNames.ExpectedRejections, 12, observedAtUtc),
        Metric(AdminMetricNames.ServerFailures, 3, observedAtUtc),
        Metric(AdminMetricNames.FillRate, 82.5, observedAtUtc),
        Metric(AdminMetricNames.LockWaitMilliseconds, 18, observedAtUtc),
        Metric(AdminMetricNames.DataQualityAlerts, 1, observedAtUtc),
        Metric(AdminMetricNames.CapacityConflicts, 2, observedAtUtc),
    ];

    private static OperationalMetric Metric(
        string name,
        double value,
        DateTime observedAtUtc) => new(name, value, observedAtUtc);

    private static void AssertAlert(
        AdminMetricAlert alert,
        string metric,
        double threshold,
        double observed,
        string reference)
    {
        Assert.Equal(metric, alert.Metric);
        Assert.Equal(threshold, alert.Threshold);
        Assert.Equal(observed, alert.ObservedValue);
        Assert.Equal(NowUtc, alert.ObservedAtUtc);
        Assert.Equal(reference, alert.SupportReferencePath);
    }

    private sealed class StubReader(OperationalMetricReadResult result)
        : IOperationalMetricReader
    {
        public OperationalMetricScope? LastScope { get; private set; }

        public Task<OperationalMetricReadResult> ReadAsync(
            OperationalMetricScope scope,
            CancellationToken cancellationToken = default)
        {
            LastScope = scope;
            return Task.FromResult(result);
        }
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
