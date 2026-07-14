using StudentRegistration.LoadTesting.Spec018;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr5EvidenceTests
{
    [Fact]
    public void Required_profiles_use_two_stateless_replicas_and_optional_profiles_cannot_block()
    {
        Assert.All(
            ExactLoadProfileCatalog.RequiredProfiles,
            profile =>
            {
                Assert.True(profile.MinimumReplicaCount >= 2);
                Assert.True(profile.RequireStatelessReplicas);
                Assert.True(profile.IsReleaseBlocking);
            });
        Assert.All(
            ExactLoadProfileCatalog.OptionalDiagnostics,
            profile => Assert.False(profile.IsReleaseBlocking));

        var evidence = Spec018LoadEvidenceAssertions.ReadPending("NFR-5");
        Assert.Contains("at least two stateless API replicas", evidence, StringComparison.Ordinal);
        Assert.Contains("optional diagnostics", evidence, StringComparison.Ordinal);
        Assert.Contains("cannot block POC completion", evidence, StringComparison.Ordinal);
    }

    [Fact(Skip =
        "Activation condition: two independently addressable API replicas plus SPEC-007 through SPEC-014 runtime paths must exist before mandatory target, spike, and failover execution evidence can be recorded.")]
    public void Target_spike_and_failover_execute_across_two_stateless_replicas()
    {
    }
}
