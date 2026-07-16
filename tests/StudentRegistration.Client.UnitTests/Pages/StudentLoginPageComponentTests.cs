namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StudentLoginPageComponentTests
{
    [Fact]
    public void Auth_02_has_validation_pending_duplicate_guard_and_server_navigation()
    {
        var source = IdentityPageComponentAssertions.AssertCommon(
            "src/StudentRegistration.Client/Pages/StudentLoginPage.razor",
            "/student/login",
            "student-login-form",
            "if (IsSubmitting || !Validate())",
            "University ID",
            "Password",
            "LoginStudentAsync",
            "Navigation.NavigateTo(\"/student\")",
            "_password = string.Empty");

        Assert.DoesNotContain("role picker", source, StringComparison.OrdinalIgnoreCase);
    }
}
