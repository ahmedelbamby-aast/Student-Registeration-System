namespace StudentRegistration.AcceptanceTests.Specs.Spec007;

public sealed class AC_9Tests
{
    [Fact]
    public void Authentication_quality_configuration_and_authorization_are_pinned()
    {
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/IdentitySecurityOptions.cs",
            "100_000",
            "MinimumPasswordLength = 15",
            "MaximumFailures = 5",
            "Production");
        Spec007AcceptanceAssertions.Source(
            "src/StudentRegistration.IdentityAccess/Application/Authorization/RolePolicies.cs",
            "Student",
            "Admin",
            "Lecturer",
            "TeachingAssistant");
    }
}
