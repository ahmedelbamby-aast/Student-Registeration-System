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

        var evidence = Spec018LoadEvidenceAssertions.ReadPassed("NFR-6");
        Assert.Contains("strictly below 0.1%", evidence, StringComparison.Ordinal);
        Assert.Contains("expected domain rejections are not server failures", evidence, StringComparison.Ordinal);
    }

    [Fact]
    public void Recorded_target_submission_run_has_zero_unexpected_server_failures()
    {
        var target = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence().Target;

        Assert.Equal(45_000, target.CompletedRequests);
        Assert.Equal(0, target.UnexpectedFailures);
        Assert.Equal(0m, target.UnexpectedFailureRatePercent);
        Assert.True(LoadGateEvaluator.UnexpectedFailureRatePasses(
            target.UnexpectedFailures,
            target.CompletedRequests));
    }

    [Fact]
    public void Target_run_unexpected_server_failure_rate_is_below_point_one_percent()
    {
        var recorded = Spec018LoadEvidenceAssertions.ReadRegistrationEvidence();
        var failures = recorded.Target.UnexpectedFailures +
            recorded.MixedTargetReads.UnexpectedFailures;
        var requests = recorded.Target.CompletedRequests +
            recorded.MixedTargetReads.CompletedRequests;

        Assert.Equal(225_000, requests);
        Assert.True(LoadGateEvaluator.UnexpectedFailureRatePasses(failures, requests));
    }
}
