using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

public sealed class SC_1OutcomeTests
{
    [Fact]
    public async Task Available_and_unavailable_results_have_understandable_stable_reasons()
    {
        var availableFixture = new Spec011ScenarioBuilder();
        var unavailableFixture = new Spec011ScenarioBuilder
        {
            EarnedCredits = 95m,
            MinimumEarnedCredits = 96m
        };

        var available = Assert.Single((await Spec011AcceptanceSupport.Service(availableFixture)
            .EvaluateTermAsync(
                availableFixture.ApplicationUserId,
                availableFixture.TermId)).Items);
        var unavailable = Assert.Single((await Spec011AcceptanceSupport.Service(unavailableFixture)
            .EvaluateTermAsync(
                unavailableFixture.ApplicationUserId,
                unavailableFixture.TermId)).Items);

        Assert.True(available.Eligible);
        Assert.Contains(available.Reasons, reason =>
            reason.Passed && !string.IsNullOrWhiteSpace(reason.Message));
        Assert.False(unavailable.Eligible);
        Assert.Contains(unavailable.Reasons, reason =>
            reason.Blocking
            && reason.Code == "MINIMUM_EARNED_CREDITS_NOT_MET"
            && !string.IsNullOrWhiteSpace(reason.Message));
    }
}
