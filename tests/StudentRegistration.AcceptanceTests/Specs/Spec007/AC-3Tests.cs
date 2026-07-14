namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_3Tests
{
    [Fact]
    public void Shared_staff_login_derives_roles_without_a_second_factor()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/StaffAuthenticationService.cs",
            "VerifyHashedPassword",
            "GetEffectiveRolesAsync",
            "TeachingAssistant");
        Assert.DoesNotContain("SecondFactor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("request.Role", source, StringComparison.Ordinal);
    }
}
