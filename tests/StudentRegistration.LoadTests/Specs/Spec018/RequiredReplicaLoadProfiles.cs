using StudentRegistration.LoadTesting.Spec018;

namespace StudentRegistration.LoadTests.Specs.Spec018;

public sealed class RequiredReplicaLoadProfiles
{
    [Fact]
    public void Every_required_profile_is_blocking_stateless_and_uses_at_least_two_replicas()
    {
        Assert.All(ExactLoadProfileCatalog.RequiredProfiles, profile =>
        {
            Assert.True(profile.IsReleaseBlocking);
            Assert.True(profile.RequireStatelessReplicas);
            Assert.True(profile.MinimumReplicaCount >= 2);
            Assert.True(profile.RequireZeroCorrectnessInvariants);
        });
    }

    [Fact]
    public void Replica_failover_reuses_the_exact_target_mix_and_drops_one_replica_at_midpoint()
    {
        var target = ExactLoadProfileCatalog.Target;
        var failover = ExactLoadProfileCatalog.ReplicaFailoverTarget;

        Assert.Equal(target.Duration, failover.Duration);
        Assert.Equal(
            target.RegistrationSubmissionsPerSecond,
            failover.RegistrationSubmissionsPerSecond);
        Assert.Equal(target.ReadsPerSecond, failover.ReadsPerSecond);
        Assert.Equal(TimeSpan.FromMinutes(5), failover.FailureInjectionAt);
        Assert.Equal(1, failover.ReplicasToRemove);
        Assert.Equal(2, failover.MinimumReplicaCount);
    }

    [Fact(Skip =
        "Activation condition: SPEC-007 through SPEC-014 must deliver executable authentication, discovery, optimizer, and atomic registration endpoints plus the migrated 25,000-account fixture before the mandatory 10-minute target can run.")]
    public void Execute_required_ten_minute_target_across_two_replicas()
    {
    }

    [Fact(Skip =
        "Activation condition: SPEC-007 through SPEC-014 must deliver executable authentication and atomic registration endpoints plus the migrated 25,000-account fixture before the mandatory 200-per-second spike can run.")]
    public void Execute_required_200_per_second_spike_across_two_replicas()
    {
    }

    [Fact(Skip =
        "Activation condition: the required two-replica runtime and SPEC-007 through SPEC-014 correctness paths must exist before one replica can be removed under the exact target mix.")]
    public void Execute_required_replica_failover_profile()
    {
    }
}
