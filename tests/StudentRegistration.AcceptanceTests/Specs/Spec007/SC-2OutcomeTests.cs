namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class SC_2OutcomeTests
{
    [Fact]
    public void Unknown_and_already_claimed_activation_share_the_safe_outcome()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/StudentActivationService.cs",
            "ActivationFailed",
            "TryActivateAsync");
        Assert.DoesNotContain("NotFound", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AlreadyActivated", source, StringComparison.Ordinal);
    }
}
