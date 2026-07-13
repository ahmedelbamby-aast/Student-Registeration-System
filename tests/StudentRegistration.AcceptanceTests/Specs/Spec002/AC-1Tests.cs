using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.AcceptanceTests.Specs.Spec002;

public sealed class AC_1Tests
{
    [Fact]
    public void Probation_plan_above_twelve_credits_is_blocked_with_governing_evidence()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var decision = harness.Evaluate(new PolicyEvaluationInput
        {
            Gpa = 1.99m,
            PlannedCredits = 13
        });

        Assert.False(decision.Eligible);
        Assert.Equal("PROBATION_LOAD_EXCEEDED", decision.ReasonCode);
        Assert.Equal(12, decision.MaximumCredits);
        Assert.Equal("DEMO-POC-2026.1", decision.PolicyVersion);
        Assert.Equal("SRC-GENERAL-2016", decision.PrimaryResult.Source.Id);
        Assert.Contains("12", decision.PrimaryResult.Explanation, StringComparison.Ordinal);
        Assert.False(decision.PrimaryResult.OverridePossible);
    }
}
