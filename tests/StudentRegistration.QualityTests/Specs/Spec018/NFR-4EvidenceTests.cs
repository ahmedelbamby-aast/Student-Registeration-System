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

        var evidence = Spec018LoadEvidenceAssertions.ReadPending("NFR-4");
        Assert.Contains("zero overbooking", evidence, StringComparison.Ordinal);
        Assert.Contains("zero duplicate active offering enrollment", evidence, StringComparison.Ordinal);
        Assert.Contains("zero partial atomic submissions", evidence, StringComparison.Ordinal);
        Assert.Contains("target, spike, and replica-failover", evidence, StringComparison.Ordinal);
    }

    [Fact(Skip =
        "Activation condition: SPEC-014 must deliver real-SQL atomic registration and invariant reconciliation before target, spike, and replica-failover counters can be measured.")]
    public void Mandatory_profiles_record_zero_overbooking_duplicates_and_partial_submissions()
    {
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
}
