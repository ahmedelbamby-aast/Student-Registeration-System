namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StudentAccountPageComponentTests
{
    [Fact]
    public void Stu_08_renders_authenticated_landmarks_and_operates_the_workspace_menu() =>
        StudentPageRenderHarness.AssertAuthenticatedShellAndMenuOperate("StudentAccountPage", "STU-08");

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
        Assert.Contains("Disabled=\"@IsBusy\"", source, StringComparison.Ordinal);
        Assert.Contains("<AuthenticatedPage", source, StringComparison.Ordinal);
        Assert.Contains("Context=\"@_shellContext\"", source, StringComparison.Ordinal);
        Assert.Contains("AcademicApi.GetAppContextAsync", source, StringComparison.Ordinal);
        Assert.Contains("RegistrationPageSupport.ToShellContext", source, StringComparison.Ordinal);
        Assert.Contains("CurrentRouteId=\"STU-08\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SecurityStamp", source, StringComparison.Ordinal);
    }
}
