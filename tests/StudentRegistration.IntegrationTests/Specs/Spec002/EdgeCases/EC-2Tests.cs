using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.IntegrationTests.Specs.Spec002.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Equal_highest_priority_scope_is_rejected_for_publication_and_evaluation()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var first = PolicyRulebookCandidate.PublishedDemo("DEMO-POC-2026.1", priority: 100);
        var conflicting = PolicyRulebookCandidate.PublishedDemo(
            "DEMO-POC-2026.1-CONFLICT",
            priority: 100);

        var publication = harness.ValidatePublication(
            new PolicyPublicationDraft
            {
                CandidateRulebooks = [first, conflicting]
            });
        var decision = harness.Evaluate(new PolicyEvaluationInput
        {
            CandidateRulebooks = [first, conflicting]
        });

        Assert.False(publication.Accepted);
        Assert.Contains("POLICY_SCOPE_AMBIGUOUS", publication.RejectionCodes);
        Assert.False(decision.Eligible);
        Assert.Equal("POLICY_SCOPE_AMBIGUOUS", decision.ReasonCode);
        Assert.True(decision.AdminAlertRaised);
        Assert.False(decision.UsedFallback);
        Assert.NotEqual(Spec002PolicyTestHarness.Version, decision.PolicyVersion);
        Assert.Single(decision.Results);
    }

    [Fact]
    public void Different_priorities_select_exactly_the_highest_matching_published_rulebook()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var lower = PolicyRulebookCandidate.PublishedDemo("DEMO-LOWER", priority: 90);
        var highest = PolicyRulebookCandidate.PublishedDemo("DEMO-HIGHEST", priority: 100);

        var decision = harness.Evaluate(new PolicyEvaluationInput
        {
            CandidateRulebooks = [lower, highest]
        });

        Assert.True(decision.Eligible);
        Assert.Equal("DEMO-HIGHEST", decision.PolicyVersion);
        Assert.Equal("100", decision.InputSummary["selectedPolicyPriority"]);
    }
}
