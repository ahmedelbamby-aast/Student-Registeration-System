namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class StudentActivationPageComponentTests
{
    [Fact]
    public void Auth_03_has_validation_pending_duplicate_guard_and_secret_cleanup()
    {
        var source = IdentityPageComponentAssertions.AssertCommon(
            "src/StudentRegistration.Client/Pages/StudentActivationPage.razor",
            "/student/activate",
            "student-activation-form",
            "if (IsSubmitting || !Validate())",
            "Initial PIN / password",
            "Confirm new password",
            "ActivateStudentAsync",
            "ClearSecrets()",
            "Navigation.NavigateTo(\"/student/login\")");

        Assert.Contains("not a second factor", source, StringComparison.Ordinal);
    }
}
