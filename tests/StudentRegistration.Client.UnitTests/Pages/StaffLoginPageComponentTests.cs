namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StaffLoginPageComponentTests
{
    [Fact]
    public void Auth_04_has_password_only_login_and_rejects_multi_role_configuration()
    {
        var source = IdentityPageComponentAssertions.AssertCommon(
            "src/StudentRegistration.Client/Pages/StaffLoginPage.razor",
            "/staff/login",
            "staff-login-form",
            "if (IsSubmitting || !Validate())",
            "password-only login",
            "LoginStaffAsync",
            "role-selection-required",
            "INVALID_ROLE_CONFIGURATION",
            "session.Roles.Count == 1",
            "ApplyUnsupportedRoleConfiguration");

        Assert.DoesNotContain("<select", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SelectRoleContextAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Choose an authorized staff context", source, StringComparison.Ordinal);
        Assert.DoesNotContain("OTP", source, StringComparison.OrdinalIgnoreCase);
    }
}
