using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Disabled_and_locked_accounts_share_the_generic_denial()
    {
        var student = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StudentAuthenticationService.cs");
        var staff = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StaffAuthenticationService.cs");
        RepositoryFiles.ContainsAll(student, "IsEnabled", "LockoutEndUtc", "AuthenticationFailed");
        RepositoryFiles.ContainsAll(staff, "IsEnabled", "LockoutEndUtc", "AuthenticationFailed");
        Assert.DoesNotContain("AccountDisabled", student, StringComparison.Ordinal);
        Assert.DoesNotContain("AccountLocked", staff, StringComparison.Ordinal);
    }
}
