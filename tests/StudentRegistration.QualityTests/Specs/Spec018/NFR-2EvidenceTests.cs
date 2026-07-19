using System.Text.Json;
using StudentRegistration.LoadTesting.Spec018;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr2EvidenceTests
{
    [Fact]
    public void Publish_privacy_safe_observed_run_only_when_explicitly_requested()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("SPEC018_PUBLISH_OBSERVED_LOAD"),
                "1",
                StringComparison.Ordinal))
        {
            return;
        }

        var source = Path.Combine(
            RepositoryFiles.Root,
            ".local",
            "evidence",
            "SPEC-014-load-results.json");
        var target = Path.Combine(
            RepositoryFiles.Root,
            "docs",
            "release-evidence",
            "SPEC-018-load-results.json");
        using var evidence = JsonDocument.Parse(File.ReadAllText(source));
        Assert.Equal(0, evidence.RootElement.GetProperty("privacyViolations").GetInt32());
        Assert.Equal(0, evidence.RootElement.GetProperty("target")
            .GetProperty("unexpectedFailures").GetInt32());
        Assert.Equal(0, evidence.RootElement.GetProperty("mixedTargetReads")
            .GetProperty("unexpectedFailures").GetInt32());
        File.Copy(source, target, overwrite: true);
    }
    [Fact]
    public void Evidence_binds_the_exact_blocking_profiles_and_diagnostic_profiles()
    {
        var target = ExactLoadProfileCatalog.Target;
        var spike = ExactLoadProfileCatalog.RequiredSpike;
        var evidence = Spec018LoadEvidenceAssertions.ReadPassed("NFR-2");

        Assert.Equal(TimeSpan.FromMinutes(10), target.Duration);
        Assert.Equal((75, 300, 2), (
            target.RegistrationSubmissionsPerSecond,
            target.ReadsPerSecond,
            target.MinimumReplicaCount));
        Assert.Equal(TimeSpan.FromSeconds(60), spike.Duration);
        Assert.Equal((200, 2), (
            spike.RegistrationSubmissionsPerSecond,
            spike.MinimumReplicaCount));
        Assert.All(
            ExactLoadProfileCatalog.OptionalDiagnostics,
            profile => Assert.False(profile.IsReleaseBlocking));
        Assert.Contains("2x, 5x, and 120-minute soak", evidence, StringComparison.Ordinal);
        Assert.Contains("diagnostic only", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public void Measured_target_and_spike_profiles_complete_at_the_required_rates_and_durations()
    {
        var recorded = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence();

        Assert.Equal(600, recorded.Target.DurationSeconds);
        Assert.Equal(75, recorded.Target.ConfiguredSubmissionsPerSecond);
        Assert.Equal(45_000, recorded.Target.CompletedRequests);
        Assert.Equal(600, recorded.MixedTargetReads.DurationSeconds);
        Assert.Equal(300, recorded.MixedTargetReads.ConfiguredReadsPerSecond);
        Assert.Equal(180_000, recorded.MixedTargetReads.CompletedRequests);
        Assert.Equal(90_000, recorded.MixedTargetReads.DiscoveryReads);
        Assert.Equal(45_000, recorded.MixedTargetReads.EligibilityReads);
        Assert.Equal(27_000, recorded.MixedTargetReads.PlanAndTimetableReads);
        Assert.Equal(18_000, recorded.MixedTargetReads.RegistrationRecordReads);
        Assert.Equal(60, recorded.Spike.DurationSeconds);
        Assert.Equal(200, recorded.Spike.ConfiguredSubmissionsPerSecond);
        Assert.Equal(12_000, recorded.Spike.CompletedRequests);
    }
}
