namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class SC_1OutcomeTests
{
    [Fact]
    public void Successful_login_exposes_only_permitted_contexts()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/AuthenticationResult.cs",
            "AuthorizedRoles",
            "ActiveRole",
            "RoleSelectionRequired");
        Assert.DoesNotContain("ClaimsPrincipal", source, StringComparison.Ordinal);
    }
}
