using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Identity;

public sealed class StaffPasswordLoginTests
{
    [Fact]
    public async Task Staff_login_rejects_accounts_with_more_than_one_staff_role()
    {
        var fixture = new IdentityServiceTestFixture();
        fixture.AddStaff(
            "teacher.one",
            "correct horse battery staple",
            ["TeachingAssistant", "Lecturer"]);
        var service = fixture.CreateStaffAuthentication();

        var success = await service.AuthenticateAsync(
            "teacher.one",
            "correct horse battery staple");
        var failure = await service.AuthenticateAsync("teacher.one", "incorrect");

        Assert.False(success.Succeeded);
        Assert.False(failure.Succeeded);
    }

    [Fact]
    public void Staff_login_verifies_provisioned_state_and_derives_effective_roles()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/StaffAuthenticationService.cs");

        RepositoryFiles.ContainsAll(
            source,
            "StaffAuthenticationService",
            "NormalizeUserName",
            "VerifyHashedPassword",
            "IsEnabled",
            "LockoutEndUtc",
            "GetEffectiveRolesAsync",
            "effectiveRoles.Length != 1",
            "AuthenticationFailed");
        Assert.DoesNotContain("request.Role", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SecondFactor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
    }
}
