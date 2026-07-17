using System.Text.Json;
using StudentRegistration.Contracts.Operations;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec017;

public sealed class NFR_1EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-017-NFR-1.md";
    private const string RawMeasurementsPath =
        "docs/release-evidence/SPEC-017-NFR-1-measurements.json";

    [Fact]
    public async Task Recorded_measurements_match_the_real_freshness_boundary()
    {
        var artifact = JsonSerializer.Deserialize<MeasurementArtifact>(
            RepositoryFiles.Read(RawMeasurementsPath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.Equal("spec017-nfr1/1.0", artifact.SchemaVersion);
        Assert.Equal("SPEC-017 NFR-1", artifact.Requirement);
        Assert.Matches("^[0-9a-f]{40}$", artifact.MeasuredCommit);
        Assert.Equal(4, artifact.Fixture.CaseCount);
        Assert.Equal(AdminMetricNames.Required.Count, artifact.Fixture.MetricSeriesPerCase);
        Assert.Equal(32, artifact.Fixture.TotalMetricObservations);
        Assert.Equal(artifact.Fixture.CaseCount, artifact.Measurements.Count);

        foreach (var measurement in artifact.Measurements)
        {
            var observedAtUtc = artifact.Fixture.ClockUtc
                .AddSeconds(-measurement.ObservationAgeSeconds)
                .UtcDateTime;
            var metrics = AdminMetricNames.Required
                .Order(StringComparer.Ordinal)
                .Select((name, index) => new OperationalMetric(
                    name,
                    index + 1,
                    observedAtUtc))
                .ToArray();
            var reader = new FixedMetricReader(
                Enum.Parse<OperationalMetricSourceState>(measurement.SourceState),
                metrics);
            var query = new AdminMetricsQuery(
                reader,
                new FixedTimeProvider(artifact.Fixture.ClockUtc));

            var result = await query.ExecuteAsync(
                new OperationalMetricScope(
                    Guid.Parse("00000000-0000-0000-0000-000000000017"),
                    Guid.Parse("00000000-0000-0000-0000-000000000092")));

            Assert.Equal(measurement.ExpectedAvailability, result.AvailabilityState.ToString());
            Assert.Equal(measurement.ActualAvailability, result.AvailabilityState.ToString());
            Assert.Equal(measurement.ExpectedObservedAtUtc.UtcDateTime, result.ObservedAtUtc);
            Assert.Equal(measurement.ActualObservedAtUtc.UtcDateTime, result.ObservedAtUtc);
            Assert.Equal(observedAtUtc, result.ObservedAtUtc);
            Assert.Equal(AdminMetricNames.Required.Count, result.Metrics.Count);
        }

        var liveAges = artifact.Measurements
            .Where(item => item.ActualAvailability == nameof(AdminMetricsAvailability.Live))
            .Select(item => item.ObservationAgeSeconds)
            .ToArray();
        var staleAges = artifact.Measurements
            .Where(item => item.ActualAvailability == nameof(AdminMetricsAvailability.Stale))
            .Select(item => item.ObservationAgeSeconds)
            .ToArray();

        Assert.Equal(artifact.Calculation.MaxAgePresentedAsLiveSeconds, liveAges.Max());
        Assert.Equal(artifact.Calculation.FirstAgeClassifiedStaleSeconds, staleAges.Min());
        Assert.True(liveAges.Max() <= artifact.Calculation.LimitSeconds);
        Assert.True(artifact.Calculation.TimestampsPreserved);
        Assert.Equal("PASS", artifact.Calculation.Result);
    }

    [Fact]
    public void Release_report_records_reproducible_measurement_context()
    {
        var artifact = JsonSerializer.Deserialize<MeasurementArtifact>(
            RepositoryFiles.Read(RawMeasurementsPath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var report = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            report,
            "SPEC-017 NFR-1",
            artifact.MeasuredCommit,
            artifact.Command,
            RawMeasurementsPath,
            "4 deterministic cases",
            "8 required metric series per case",
            "32 metric",
            "observations in total",
            "max(live observation age) = 60 seconds <= 60 seconds",
            "first stale observation age = 61 seconds",
            "observation timestamps preserved = true",
            "**Result: PASS.**");
    }

    private sealed class FixedMetricReader(
        OperationalMetricSourceState sourceState,
        IReadOnlyList<OperationalMetric> metrics) : IOperationalMetricReader
    {
        public Task<OperationalMetricReadResult> ReadAsync(
            OperationalMetricScope scope,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OperationalMetricReadResult(sourceState, metrics));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed record MeasurementArtifact(
        string SchemaVersion,
        string Requirement,
        string MeasuredCommit,
        DateTimeOffset MeasuredAtUtc,
        string Command,
        MeasurementEnvironment Environment,
        MeasurementFixture Fixture,
        IReadOnlyList<Measurement> Measurements,
        ThresholdCalculation Calculation);

    private sealed record MeasurementEnvironment(
        string OperatingSystem,
        string Architecture,
        string DotnetSdk,
        string TargetFramework,
        string TimeZone,
        string ExecutionMode);

    private sealed record MeasurementFixture(
        DateTimeOffset ClockUtc,
        int CaseCount,
        int MetricSeriesPerCase,
        int TotalMetricObservations);

    private sealed record Measurement(
        string Id,
        string SourceState,
        int ObservationAgeSeconds,
        string ExpectedAvailability,
        string ActualAvailability,
        DateTimeOffset ExpectedObservedAtUtc,
        DateTimeOffset ActualObservedAtUtc);

    private sealed record ThresholdCalculation(
        int LimitSeconds,
        int MaxAgePresentedAsLiveSeconds,
        int FirstAgeClassifiedStaleSeconds,
        bool TimestampsPreserved,
        string Result);
}
