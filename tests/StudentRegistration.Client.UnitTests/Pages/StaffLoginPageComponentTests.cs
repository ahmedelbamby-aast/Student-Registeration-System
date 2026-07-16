namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StaffLoginPageComponentTests
{
    [Fact]
    public void Auth_04_has_password_only_login_and_server_returned_role_selection()
    {
        var source = IdentityPageComponentAssertions.AssertCommon(
            "src/StudentRegistration.Client/Pages/StaffLoginPage.razor",
            "/staff/login",
            "staff-login-form",
            "if (IsSubmitting || !Validate())",
            "password-only login",
            "LoginStaffAsync",
            "role-selection-required",
            "SelectRoleContextAsync",
            "_availableRoles.Contains(role",
            "IsSelectingRole");

        Assert.DoesNotContain("<select", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("OTP", source, StringComparison.OrdinalIgnoreCase);
    }
}
