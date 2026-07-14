using StudentRegistration.LoadTesting.Spec018;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr2EvidenceTests
{
    [Fact]
    public void Evidence_binds_the_exact_blocking_profiles_and_diagnostic_profiles()
    {
        var target = ExactLoadProfileCatalog.Target;
        var spike = ExactLoadProfileCatalog.RequiredSpike;
        var evidence = Spec018LoadEvidenceAssertions.ReadPending("NFR-2");

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

    [Fact(Skip =
        "Activation condition: SPEC-007 through SPEC-014 must expose the authenticated read mix and atomic registration endpoints against the migrated production-like synthetic fixture before target and spike evidence can be recorded.")]
    public void Measured_target_and_spike_profiles_complete_at_the_required_rates_and_durations()
    {
    }
}
