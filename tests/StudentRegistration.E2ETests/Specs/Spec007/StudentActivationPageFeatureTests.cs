using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec007;

public sealed class StudentActivationPageFeatureTests
{
    [Fact]
    public void Student_activation_keeps_confirmation_client_side_and_handles_safe_replay_results()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentActivationPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/student/activate\"",
            "ActivateStudentRequest",
            "University ID",
            "Initial PIN / password",
            "New password",
            "Confirm new password",
            "AllowSecretReveal=\"true\"",
            "_confirmPassword",
            "ACTIVATION_FAILED",
            "PASSWORD_REJECTED",
            "IsSubmitting",
            "NavigateTo(\"/student/login\")");
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("one-time password", page, StringComparison.OrdinalIgnoreCase);
    }
}
