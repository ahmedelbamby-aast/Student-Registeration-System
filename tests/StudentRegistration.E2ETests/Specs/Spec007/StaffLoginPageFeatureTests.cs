using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec007;

public sealed class StaffLoginPageFeatureTests
{
    [Fact]
    public void Shared_staff_login_has_no_claimed_role_picker_or_second_factor()
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
            "INVALID_ROLE_CONFIGURATION",
            "session.Roles.Count == 1",
            "NavigateTo(\"/admin\")",
            "NavigateTo(\"/staff\")");
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"role\"", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SelectRoleContext", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Continue as", page, StringComparison.Ordinal);
        Assert.DoesNotContain("one-time code", page, StringComparison.OrdinalIgnoreCase);
    }
}
