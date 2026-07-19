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

        var evidence = Spec018LoadEvidenceAssertions.ReadPassed("NFR-5");
        Assert.Contains("at least two stateless API replicas", evidence, StringComparison.Ordinal);
        Assert.Contains("optional diagnostics", evidence, StringComparison.Ordinal);
        Assert.Contains("cannot block POC completion", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public void Recorded_target_and_spike_are_complete_and_evenly_distributed_across_two_replicas()
    {
        var recorded = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence();

        AssertProfile(recorded.Target, 600, 75, 45_000, evenlyDistributed: false);
        AssertProfile(recorded.Spike, 60, 200, 12_000, evenlyDistributed: true);
    }

    [Fact]
    public void Target_spike_and_failover_execute_across_two_stateless_replicas()
    {
        var recorded = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence();

        Assert.Equal(2, recorded.LogicalApplicationReplicaCount);
        Assert.Equal(2, recorded.MixedTargetReads.ReplicaCount);
        Assert.Equal(2, recorded.MixedTargetReads.ReplicaRequestCounts.Count);
        Assert.True(recorded.MixedTargetReads.FirstReplicaRemoved);
        Assert.True(recorded.FirstReplicaRestartVerified);
        Assert.Equal(2, recorded.Spike.ReplicaCount);
        Assert.Equal(2, recorded.Spike.ReplicaRequestCounts.Count);
    }

    private static void AssertProfile(
        RecordedLoadProfile profile,
        int expectedDurationSeconds,
        int expectedRate,
        int expectedRequests,
        bool evenlyDistributed)
    {
        Assert.Equal(expectedDurationSeconds, profile.DurationSeconds);
        Assert.Equal(expectedRate, profile.ConfiguredSubmissionsPerSecond);
        Assert.Equal(2, profile.ReplicaCount);
        Assert.Equal(expectedRequests, profile.ScheduledRequests);
        Assert.Equal(expectedRequests, profile.CompletedRequests);
        Assert.Equal(2, profile.ReplicaRequestCounts.Count);
        Assert.Equal(expectedRequests, profile.ReplicaRequestCounts.Values.Sum());
        Assert.All(profile.ReplicaRequestCounts.Values, count => Assert.True(count > 0));
        if (evenlyDistributed)
        {
            Assert.All(
                profile.ReplicaRequestCounts.Values,
                count => Assert.Equal(expectedRequests / 2, count));
        }
    }
}
