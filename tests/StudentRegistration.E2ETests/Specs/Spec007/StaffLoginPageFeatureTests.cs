using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec007;

public sealed class StaffLoginPageFeatureTests
{
    [Fact]
    public void Shared_staff_login_has_no_claimed_role_or_second_factor_and_selects_only_returned_roles()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StaffLoginPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/staff/login\"",
            "StaffLoginRequest",
            "Staff username",
            "Password",
            "AllowSecretReveal=\"true\"",
            "role-selection-required",
            "SelectRoleContextRequest",
            "_availableRoles",
            "NavigateTo(\"/admin\")",
            "NavigateTo(\"/staff\")");
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"role\"", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("one-time code", page, StringComparison.OrdinalIgnoreCase);
    }
}
