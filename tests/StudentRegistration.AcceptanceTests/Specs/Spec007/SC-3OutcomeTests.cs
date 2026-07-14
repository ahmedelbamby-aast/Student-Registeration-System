namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class SC_3OutcomeTests
{
    [Fact]
    public void Account_enumeration_resistance_uses_one_public_failure()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/AuthenticationResult.cs",
            "AuthenticationFailed");
        Assert.DoesNotContain("UserNotFound", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WrongPassword", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AccountDisabled", source, StringComparison.Ordinal);
    }
}
