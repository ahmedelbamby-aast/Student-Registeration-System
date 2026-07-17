namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class NFR_2EvidenceTests
{
    [Fact]
    public async Task Target_profile_sustains_75_submissions_per_second_for_10_minutes_under_two_seconds_p95()
    {
        var evidence = await Spec014EvidenceGate.GetAsync();
        var target = evidence.Target;

        Assert.Equal(25_000, evidence.SyntheticAccountCount);
        Assert.Equal(5_000, evidence.LogicalSessionCount);
        Assert.Equal(75, target.ConfiguredSubmissionsPerSecond);
        Assert.Equal(600, target.DurationSeconds);
        Assert.Equal(45_000, target.ScheduledRequests);
        Assert.Equal(target.ScheduledRequests, target.CompletedRequests);
        Assert.InRange(target.SubmissionP95Milliseconds, 0, 2_000);
        Assert.Equal(1, evidence.Boundary.AuthenticatedServiceSubmissions);
        Assert.Equal(1, evidence.Boundary.CoordinatorBoundaryExecutions);
        Assert.True(evidence.EndpointSmokeExecuted);
    }
}
