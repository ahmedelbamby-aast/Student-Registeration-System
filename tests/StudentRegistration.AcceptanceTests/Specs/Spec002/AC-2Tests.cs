using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.AcceptanceTests.Specs.Spec002;

public sealed class AC_2Tests
{
    [Fact]
    public void Waitlist_or_capacity_override_conflict_requires_a_separate_policy_amendment()
    {
        var harness = Spec002PolicyTestHarness.Load();

        var validation = harness.ValidatePublication(new PolicyPublicationDraft
        {
            WaitlistEnabled = true,
            CapacityOverrideEnabled = true
        });

        Assert.False(validation.Accepted);
        Assert.True(validation.RequiresPolicyAmendment);
        Assert.Contains("WAITLIST_NOT_APPROVED", validation.RejectionCodes);
        Assert.Contains("CAPACITY_OVERRIDE_NOT_APPROVED", validation.RejectionCodes);
        Assert.False(validation.StoredExecutableContent);
    }
}
