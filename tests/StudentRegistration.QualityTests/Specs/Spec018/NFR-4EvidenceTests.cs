using StudentRegistration.LoadTesting.Spec018;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr4EvidenceTests
{
    [Fact]
    public void Correctness_gate_rejects_each_nonzero_invariant_counter()
    {
        Assert.False(Evaluate(new LoadInvariantCounters(1, 0, 0)).Passed);
        Assert.False(Evaluate(new LoadInvariantCounters(0, 1, 0)).Passed);
        Assert.False(Evaluate(new LoadInvariantCounters(0, 0, 1)).Passed);
        Assert.True(Evaluate(new LoadInvariantCounters(0, 0, 0)).Passed);

        var evidence = Spec018LoadEvidenceAssertions.ReadPassed("NFR-4");
        Assert.Contains("zero overbooking", evidence, StringComparison.Ordinal);
        Assert.Contains("zero duplicate active offering enrollment", evidence, StringComparison.Ordinal);
        Assert.Contains("zero partial atomic submissions", evidence, StringComparison.Ordinal);
        Assert.Contains("target, spike, and replica-failover", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public void Recorded_target_spike_and_collision_runs_have_zero_invariant_violations()
    {
        var recorded = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence();

        AssertZero(recorded.Target.Invariants);
        AssertZero(recorded.Spike.Invariants);
        AssertZero(recorded.Collision.Invariants);
        Assert.Equal(100, recorded.Collision.ConcurrentRequests);
        Assert.Equal(30, recorded.Collision.GroupCapacity);
        Assert.Equal(30, recorded.Collision.AcceptedRequests);
        Assert.Equal(30, recorded.Collision.ActiveEnrollments);
        Assert.Equal(30, recorded.Collision.FinalEnrolledCount);
    }

    [Fact]
    public void Mandatory_profiles_record_zero_overbooking_duplicates_and_partial_submissions()
    {
        var recorded = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence();

        AssertZero(recorded.Target.Invariants);
        AssertZero(recorded.Spike.Invariants);
        Assert.True(recorded.MixedTargetReads.FirstReplicaRemoved);
        Assert.Equal(300, recorded.MixedTargetReads.FailoverAtSecond);
        Assert.True(recorded.FirstReplicaRestartVerified);
    }

    private static LoadGateEvaluation Evaluate(LoadInvariantCounters invariants) =>
        LoadGateEvaluator.EvaluateTarget(new LoadRunMeasurement(
            ReplicaCount: 2,
            CatalogueP95Milliseconds: 300,
            CommitP95Milliseconds: 2_000,
            OptimizerP95Milliseconds: 500,
            TotalRequests: 10_000,
            UnexpectedServerFailures: 0,
            Invariants: invariants));

    private static void AssertZero(RecordedInvariantCounters counters)
    {
        Assert.Equal(0, counters.OverbookedGroups);
        Assert.Equal(0, counters.DuplicateActiveOfferingEnrollments);
        Assert.Equal(0, counters.PartialScheduleCommits);
        Assert.Equal(0, counters.TotalViolations);
    }
}
