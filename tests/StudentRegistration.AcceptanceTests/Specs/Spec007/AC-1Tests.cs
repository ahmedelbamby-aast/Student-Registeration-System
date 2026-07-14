namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_1Tests
{
    [Fact]
    public void Activated_student_login_reaches_only_the_student_context()
    {
        var source = Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/StudentAuthenticationService.cs",
            "NormalizeUniversityId",
            "VerifyHashedPassword",
            "Student",
            "AuthenticationSucceeded");
        Assert.DoesNotContain("Admin", source, StringComparison.Ordinal);
    }
}
