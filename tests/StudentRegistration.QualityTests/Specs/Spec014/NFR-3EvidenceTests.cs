namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class NFR_3EvidenceTests
{
    [Fact]
    public async Task Two_replica_spike_sustains_200_submissions_per_second_for_60_seconds_without_invariant_loss()
    {
        var evidence = await Spec014EvidenceGate.GetAsync();
        var spike = evidence.Spike;

        Assert.Equal(2, evidence.LogicalApplicationReplicaCount);
        Assert.Equal(2, spike.ReplicaCount);
        Assert.Equal(200, spike.ConfiguredSubmissionsPerSecond);
        Assert.Equal(60, spike.DurationSeconds);
        Assert.Equal(12_000, spike.ScheduledRequests);
        Assert.Equal(spike.ScheduledRequests, spike.CompletedRequests);
        Assert.Equal(0, spike.UnexpectedFailures);
        Assert.Equal(2, spike.ReplicaRequestCounts.Count);
        Assert.All(spike.ReplicaRequestCounts.Values, count => Assert.True(count > 0));
        Assert.Equal(0, spike.Invariants.TotalViolations);
        Assert.Equal(100, evidence.Collision.ConcurrentRequests);
        Assert.Equal(30, evidence.Collision.GroupCapacity);
        Assert.Equal(30, evidence.Collision.AcceptedRequests);
        Assert.Equal(70, evidence.Collision.ExpectedConflictRequests);
        Assert.Equal(30, evidence.Collision.ActiveEnrollments);
        Assert.Equal(30, evidence.Collision.FinalEnrolledCount);
        Assert.Equal(2, evidence.Collision.ReplicaCount);
    }
}
