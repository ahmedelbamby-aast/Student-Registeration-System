namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StudentAccountPageComponentTests
{
    [Fact]
    public void Stu_08_guards_password_session_and_logout_commands_and_clears_secrets()
    {
        var source = IdentityPageComponentAssertions.AssertCommon(
            "src/StudentRegistration.Client/Pages/StudentAccountPage.razor",
            "/student/account",
            "password-change-form",
            "if (IsBusy || !ValidatePasswordChange())",
            "GetSessionAsync",
            "ChangePasswordAsync",
            "RevokeAllSessionsAsync",
            "LogoutAsync",
            "ConfirmationDialog",
            "ClearPasswords()",
            "IdentityRouteStateMapper.StudentAccountRecord");

        Assert.Contains("if (IsBusy)", source, StringComparison.Ordinal);
        Assert.Contains("disabled=\"@IsBusy\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SecurityStamp", source, StringComparison.Ordinal);
    }
}
