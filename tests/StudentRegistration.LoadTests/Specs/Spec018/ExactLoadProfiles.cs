using StudentRegistration.LoadTesting.Spec018;

namespace StudentRegistration.LoadTests.Specs.Spec018;

public sealed class ExactLoadProfiles
{
    [Fact]
    public void Blocking_target_is_ten_minutes_at_75_submissions_and_300_reads_per_second()
    {
        var profile = ExactLoadProfileCatalog.Target;
        var counts = profile.ProjectCounts();

        Assert.Equal("required-target", profile.Name);
        Assert.Equal(TimeSpan.FromMinutes(10), profile.Duration);
        Assert.Equal(75, profile.RegistrationSubmissionsPerSecond);
        Assert.Equal(300, profile.ReadsPerSecond);
        Assert.Equal(2, profile.MinimumReplicaCount);
        Assert.True(profile.IsReleaseBlocking);
        Assert.True(profile.EnforceTargetResponseBudgets);
        Assert.Equal(45_000, counts.TotalRegistrationSubmissions);
        Assert.Equal(180_000, counts.TotalReads);
        Assert.Equal(225_000, counts.TotalRequests);
        Assert.Equal(31_500, counts.ValidUniqueSubmissions);
        Assert.Equal(9_000, counts.ExpectedDomainRejections);
        Assert.Equal(4_500, counts.IdempotentRetries);
        Assert.Equal(90_000, counts.DiscoveryReads);
        Assert.Equal(45_000, counts.EligibilityReads);
        Assert.Equal(27_000, counts.PlanAndTimetableReads);
        Assert.Equal(18_000, counts.RegistrationRecordReads);
    }

    [Fact]
    public void Blocking_spike_is_60_seconds_at_200_submissions_per_second()
    {
        var profile = ExactLoadProfileCatalog.RequiredSpike;
        var counts = profile.ProjectCounts();

        Assert.Equal("required-200-per-second-spike", profile.Name);
        Assert.Equal(TimeSpan.FromSeconds(60), profile.Duration);
        Assert.Equal(200, profile.RegistrationSubmissionsPerSecond);
        Assert.Equal(0, profile.ReadsPerSecond);
        Assert.Equal(2, profile.MinimumReplicaCount);
        Assert.True(profile.IsReleaseBlocking);
        Assert.False(profile.EnforceTargetResponseBudgets);
        Assert.Equal(12_000, counts.TotalRegistrationSubmissions);
        Assert.Equal(8_400, counts.ValidUniqueSubmissions);
        Assert.Equal(2_400, counts.ExpectedDomainRejections);
        Assert.Equal(1_200, counts.IdempotentRetries);
    }

    [Fact]
    public void Read_and_submission_mixes_are_exact_and_sum_to_100_percent()
    {
        Assert.Equal(100, ExactLoadProfileCatalog.SubmissionMix.TotalPercentage);
        Assert.Equal(70, ExactLoadProfileCatalog.SubmissionMix.ValidUniquePercentage);
        Assert.Equal(20, ExactLoadProfileCatalog.SubmissionMix.ExpectedRejectionPercentage);
        Assert.Equal(10, ExactLoadProfileCatalog.SubmissionMix.IdempotentRetryPercentage);

        Assert.Equal(100, ExactLoadProfileCatalog.ReadMix.TotalPercentage);
        Assert.Equal(50, ExactLoadProfileCatalog.ReadMix.DiscoveryPercentage);
        Assert.Equal(25, ExactLoadProfileCatalog.ReadMix.EligibilityPercentage);
        Assert.Equal(15, ExactLoadProfileCatalog.ReadMix.PlanAndTimetablePercentage);
        Assert.Equal(10, ExactLoadProfileCatalog.ReadMix.RegistrationRecordsPercentage);
    }

    [Fact]
    public void Optional_2x_5x_and_120_minute_soak_profiles_are_diagnostic_only()
    {
        var diagnostics = ExactLoadProfileCatalog.OptionalDiagnostics;

        Assert.Equal(3, diagnostics.Count);
        Assert.Contains(diagnostics, profile => profile.Name == "diagnostic-2x-target");
        Assert.Contains(diagnostics, profile => profile.Name == "diagnostic-5x-target");
        Assert.Contains(diagnostics, profile => profile.Name == "diagnostic-120-minute-soak");
        Assert.All(diagnostics, profile =>
        {
            Assert.False(profile.IsReleaseBlocking);
            Assert.Equal(LoadProfileClassification.Diagnostic, profile.Classification);
            Assert.True(profile.RequireZeroCorrectnessInvariants);
        });
    }

    [Fact]
    public void Gate_math_uses_exact_response_failure_and_correctness_thresholds()
    {
        Assert.Equal(300, LoadGateThresholds.CatalogueP95Milliseconds);
        Assert.Equal(2_000, LoadGateThresholds.CommitP95Milliseconds);
        Assert.Equal(500, LoadGateThresholds.OptimizerP95Milliseconds);
        Assert.Equal(0.001m, LoadGateThresholds.MaximumUnexpectedFailureRateExclusive);

        var passing = LoadGateEvaluator.EvaluateTarget(new LoadRunMeasurement(
            ReplicaCount: 2,
            CatalogueP95Milliseconds: 300,
            CommitP95Milliseconds: 2_000,
            OptimizerP95Milliseconds: 500,
            TotalRequests: 10_000,
            UnexpectedServerFailures: 9,
            Invariants: new LoadInvariantCounters(0, 0, 0)));
        var failing = LoadGateEvaluator.EvaluateTarget(new LoadRunMeasurement(
            ReplicaCount: 2,
            CatalogueP95Milliseconds: 301,
            CommitP95Milliseconds: 2_001,
            OptimizerP95Milliseconds: 501,
            TotalRequests: 1_000,
            UnexpectedServerFailures: 1,
            Invariants: new LoadInvariantCounters(1, 1, 1)));

        Assert.True(passing.Passed);
        Assert.False(failing.Passed);
        Assert.Equal(7, failing.Failures.Count);
    }

    [Theory]
    [InlineData(-1, 100, 100)]
    [InlineData(double.NaN, 100, 100)]
    [InlineData(double.PositiveInfinity, 100, 100)]
    [InlineData(100, -1, 100)]
    [InlineData(100, 100, double.NegativeInfinity)]
    public void Invalid_latency_measurements_fail_closed(
        double catalogueP95,
        double commitP95,
        double optimizerP95)
    {
        var evaluation = LoadGateEvaluator.EvaluateTarget(new LoadRunMeasurement(
            ReplicaCount: 2,
            CatalogueP95Milliseconds: catalogueP95,
            CommitP95Milliseconds: commitP95,
            OptimizerP95Milliseconds: optimizerP95,
            TotalRequests: 10_000,
            UnexpectedServerFailures: 0,
            Invariants: new LoadInvariantCounters(0, 0, 0)));

        Assert.False(evaluation.Passed);
    }
}
