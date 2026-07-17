namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class NFR_5EvidenceTests
{
    [Fact]
    public async Task Expected_conflicts_are_classified_separately_and_target_unexpected_failure_rate_is_below_point_one_percent()
    {
        var evidence = await Spec014EvidenceGate.GetAsync();
        var target = evidence.Target;

        Assert.True(target.ExpectedConflictRequests > 0);
        Assert.Equal(
            target.ScheduledRequests,
            target.AcceptedRequests +
            target.ExpectedConflictRequests +
            target.IdempotentReplayRequests +
            target.InProgressRequests +
            target.UnexpectedFailures);
        Assert.InRange(target.UnexpectedFailureRatePercent, 0, 0.099999999);
    }
}
