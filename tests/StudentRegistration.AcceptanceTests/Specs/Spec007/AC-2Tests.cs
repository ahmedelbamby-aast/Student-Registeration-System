namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_2Tests
{
    [Fact]
    public void Unknown_identity_activation_is_generic_and_creates_nothing()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/StudentActivationService.cs",
            "ActivationFailed",
            "NormalizeUniversityId",
            "TryActivateAsync");
        Assert.DoesNotContain("new ApplicationUser", source, StringComparison.Ordinal);
    }
}
