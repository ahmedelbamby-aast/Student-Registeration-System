using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

public sealed class AC_2Tests
{
    [Fact]
    public async Task Earned_credit_failure_explains_required_current_policy_and_source()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            EarnedCredits = 95m,
            MinimumEarnedCredits = 96m
        };

        var offering = Assert.Single((await Spec011AcceptanceSupport.Service(fixture)
            .EvaluateTermAsync(fixture.ApplicationUserId, fixture.TermId)).Items);

        Assert.False(offering.Eligible);
        var reason = Assert.Single(
            offering.Reasons,
            item => item.Code == "MINIMUM_EARNED_CREDITS_NOT_MET");
        Assert.False(reason.Passed);
        Assert.True(reason.Blocking);
        Assert.Equal("96", reason.RequiredValue);
        Assert.Equal("95", reason.CurrentValue);
        Assert.Equal("DEMO-POC-2026.1", reason.PolicyVersion);
        Assert.Equal("SRC-DATA-SCIENCE", reason.SourceReference);
        Assert.False(string.IsNullOrWhiteSpace(reason.Message));
    }
}
