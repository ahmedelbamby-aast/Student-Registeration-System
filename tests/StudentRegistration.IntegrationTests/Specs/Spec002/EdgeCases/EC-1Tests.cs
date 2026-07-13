using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.IntegrationTests.Specs.Spec002.EdgeCases;

public sealed class EC_1Tests
{
    [Fact]
    public void Missing_approved_effective_policy_fails_closed_and_alerts_admin_without_guessing()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var decision = harness.Evaluate(new PolicyEvaluationInput
        {
            CandidateRulebooks = []
        });

        Assert.False(decision.Eligible);
        Assert.Equal("POLICY_UNAVAILABLE", decision.ReasonCode);
        Assert.Equal("POLICY_UNAVAILABLE", decision.PrimaryResult.ReasonCode);
        Assert.True(decision.AdminAlertRaised);
        Assert.False(decision.UsedFallback);
        Assert.Single(decision.Results);
        Assert.NotEqual(Spec002PolicyTestHarness.Version, decision.PolicyVersion);
        Assert.Null(decision.MaximumCredits);
        Assert.Equal("0", decision.InputSummary["candidateRulebookCount"]);
        Assert.Equal("0", decision.InputSummary["matchingPublishedPolicyCount"]);
        Assert.NotEqual("SRC-GENERAL-2016", decision.PrimaryResult.Source.Id);
    }
}
