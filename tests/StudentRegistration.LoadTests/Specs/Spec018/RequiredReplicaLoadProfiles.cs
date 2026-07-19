using System.Text.Json;
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

    [Fact]
    public void Recorded_ten_minute_submission_target_completed_across_two_replicas()
    {
        using var evidence = ReadRecordedEvidence();
        AssertRecordedProfile(
            evidence.RootElement.GetProperty("target"),
            expectedDurationSeconds: 600,
            expectedRate: 75,
            expectedRequests: 45_000);
    }

    [Fact]
    public void Recorded_200_per_second_spike_completed_across_two_replicas()
    {
        using var evidence = ReadRecordedEvidence();
        AssertRecordedProfile(
            evidence.RootElement.GetProperty("spike"),
            expectedDurationSeconds: 60,
            expectedRate: 200,
            expectedRequests: 12_000);
    }

    [Fact]
    public void Execute_required_replica_failover_profile()
    {
        using var evidence = ReadRecordedEvidence();
        var reads = evidence.RootElement.GetProperty("mixedTargetReads");
        var target = evidence.RootElement.GetProperty("target");

        Assert.Equal(300, reads.GetProperty("failoverAtSecond").GetInt32());
        Assert.True(reads.GetProperty("firstReplicaRemoved").GetBoolean());
        Assert.True(evidence.RootElement
            .GetProperty("firstReplicaRestartVerified")
            .GetBoolean());
        Assert.Equal(0, target.GetProperty("invariants")
            .GetProperty("totalViolations")
            .GetInt32());
    }

    private static JsonDocument ReadRecordedEvidence()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null &&
               !File.Exists(Path.Combine(root.FullName, "StudentRegistration.slnx")))
        {
            root = root.Parent;
        }

        Assert.NotNull(root);
        var path = Path.Combine(
            root.FullName,
            "docs",
            "release-evidence",
            "SPEC-018-load-results.json");
        var evidence = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal(
            "SPEC014-SQL-REGISTRATION-1.0.0",
            evidence.RootElement.GetProperty("profileVersion").GetString());
        Assert.Equal(
            2,
            evidence.RootElement
                .GetProperty("logicalApplicationReplicaCount")
                .GetInt32());
        return evidence;
    }

    private static void AssertRecordedProfile(
        JsonElement profile,
        int expectedDurationSeconds,
        int expectedRate,
        int expectedRequests)
    {
        Assert.Equal(expectedDurationSeconds, profile.GetProperty("durationSeconds").GetInt32());
        Assert.Equal(
            expectedRate,
            profile.GetProperty("configuredSubmissionsPerSecond").GetInt32());
        Assert.Equal(2, profile.GetProperty("replicaCount").GetInt32());
        Assert.Equal(expectedRequests, profile.GetProperty("scheduledRequests").GetInt32());
        Assert.Equal(expectedRequests, profile.GetProperty("completedRequests").GetInt32());

        var replicas = profile.GetProperty("replicaRequestCounts")
            .EnumerateObject()
            .ToArray();
        Assert.Equal(2, replicas.Length);
        Assert.All(replicas, replica => Assert.True(replica.Value.GetInt32() > 0));
        Assert.Equal(
            expectedRequests,
            replicas.Sum(replica => replica.Value.GetInt32()));
    }
}
