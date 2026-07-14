using StudentRegistration.LoadTesting.Spec018;

namespace StudentRegistration.QualityTests.Specs.Spec018;

public sealed class Nfr6EvidenceTests
{
    [Fact]
    public void Unexpected_failure_gate_is_strictly_below_point_one_percent()
    {
        Assert.Equal(0.001m, LoadGateThresholds.MaximumUnexpectedFailureRateExclusive);
        Assert.True(LoadGateEvaluator.UnexpectedFailureRatePasses(0, 1));
        Assert.True(LoadGateEvaluator.UnexpectedFailureRatePasses(9, 10_000));
        Assert.False(LoadGateEvaluator.UnexpectedFailureRatePasses(1, 1_000));
        Assert.False(LoadGateEvaluator.UnexpectedFailureRatePasses(10, 10_000));
        Assert.False(LoadGateEvaluator.UnexpectedFailureRatePasses(0, 0));

        var evidence = Spec018LoadEvidenceAssertions.ReadPending("NFR-6");
        Assert.Contains("strictly below 0.1%", evidence, StringComparison.Ordinal);
        Assert.Contains("expected domain rejections are not server failures", evidence, StringComparison.Ordinal);
    }

    [Fact(Skip =
        "Activation condition: the required target profile must execute against SPEC-007 through SPEC-014 runtime endpoints before unexpected server failures and total requests can be measured.")]
    public void Target_run_unexpected_server_failure_rate_is_below_point_one_percent()
    {
    }
}
