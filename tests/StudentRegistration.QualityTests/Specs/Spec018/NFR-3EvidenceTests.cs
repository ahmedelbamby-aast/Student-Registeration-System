using StudentRegistration.LoadTesting.Spec018;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr3EvidenceTests
{
    [Fact]
    public void Evidence_contract_has_the_exact_target_p95_budgets()
    {
        var evidence = Spec018LoadEvidenceAssertions.ReadPending("NFR-3");

        Assert.Equal(300, LoadGateThresholds.CatalogueP95Milliseconds);
        Assert.Equal(2_000, LoadGateThresholds.CommitP95Milliseconds);
        Assert.Equal(500, LoadGateThresholds.OptimizerP95Milliseconds);
        Assert.True(ExactLoadProfileCatalog.Target.EnforceTargetResponseBudgets);
        Assert.False(ExactLoadProfileCatalog.RequiredSpike.EnforceTargetResponseBudgets);
        Assert.Contains("Catalogue p95 <= 300 ms", evidence, StringComparison.Ordinal);
        Assert.Contains("commit p95 <= 2,000 ms", evidence, StringComparison.Ordinal);
        Assert.Contains("optimizer p95 <= 500 ms", evidence, StringComparison.Ordinal);
    }

    [Fact(Skip =
        "Activation condition: SPEC-009 through SPEC-014 must deliver catalogue, optimizer, and atomic commit runtime endpoints before target-load p95 measurements can be captured.")]
    public void Target_run_meets_catalogue_commit_and_optimizer_p95_budgets()
    {
    }
}
