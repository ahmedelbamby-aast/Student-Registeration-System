using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.AcceptanceTests.Specs.Spec002;

public sealed class AC_3Tests
{
    [Fact]
    public void Publishing_a_successor_does_not_rewrite_the_original_decision_snapshot()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var decision = harness.Evaluate(new PolicyEvaluationInput
        {
            Gpa = 2.50m,
            PlannedCredits = 18
        });
        var snapshot = harness.Capture(decision);

        var history = harness.PublishSuccessor(snapshot, "DEMO-POC-2026.2");

        Assert.Equal("DEMO-POC-2026.1", history.Original.PolicyVersion);
        Assert.Equal("DEMO-POC-2026.2", history.CurrentPolicyVersion);
        Assert.Equal(decision.InputSummary, history.Original.InputSummary);
        Assert.Equal(decision.PrimaryResult.Source, history.Original.Source);
        Assert.Equal(decision.PrimaryResult.Explanation, history.Original.Explanation);
        Assert.Equal(decision.DeterministicFingerprint, history.Original.DeterministicFingerprint);
    }

    [Fact]
    public void Decision_snapshot_summary_contains_every_used_value_and_no_identity_or_credential_data()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var decision = harness.Evaluate(new PolicyEvaluationInput
        {
            Gpa = 2.40m,
            PlannedCredits = 18,
            HasAnyHold = true,
            EarnedCredits = 96,
            RequiredEarnedCredits = 96,
            GroupCapacity = 40,
            CommittedEnrollments = 12,
            Meetings =
            [
                new(DayOfWeek.Sunday, new(9, 0, 0), new(10, 0, 0), "C-AI-101"),
                new(DayOfWeek.Sunday, new(10, 0, 0), new(11, 0, 0), "C-AI-202")
            ]
        });

        string[] expectedKeys =
        [
            "blockingHoldCount",
            "candidateRulebookCount",
            "candidateRulebooks",
            "college",
            "committedEnrollments",
            "earnedCredits",
            "gpa",
            "groupCapacity",
            "holdCount",
            "matchingPublishedPolicyCount",
            "meetings",
            "plannedCredits",
            "prerequisitesMet",
            "program",
            "repeatInterpretationRequired",
            "requiredEarnedCredits",
            "selectedPolicyPriority",
            "standingCode",
            "term",
            "withinRegistrationWindow"
        ];

        Assert.Equal(expectedKeys, decision.InputSummary.Keys);
        Assert.Equal("96", decision.InputSummary["requiredEarnedCredits"]);
        Assert.Contains("C-AI-101", decision.InputSummary["meetings"], StringComparison.Ordinal);
        Assert.DoesNotContain(
            decision.InputSummary.Keys,
            key => key.Contains("password", StringComparison.OrdinalIgnoreCase)
                || key.Contains("universityId", StringComparison.OrdinalIgnoreCase)
                || key.Contains("email", StringComparison.OrdinalIgnoreCase)
                || key.Contains("name", StringComparison.OrdinalIgnoreCase)
                || key.Contains("claim", StringComparison.OrdinalIgnoreCase));
    }
}
