using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec007;

public sealed class StudentLoginPageFeatureTests
{
    [Fact]
    public void Student_login_is_single_submit_accessible_and_server_routed()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentLoginPage.razor");
        var feedback = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Identity/IdentityPageFeedback.cs");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/student/login\"",
            "StudentLoginRequest",
            "University ID",
            "Password",
            "autocomplete=\"username\"",
            "autocomplete=\"current-password\"",
            "AllowSecretReveal=\"true\"",
            "IsSubmitting",
            "NavigateTo(\"/student\")");
        RepositoryFiles.ContainsAll(feedback, "AUTHENTICATION_FAILED", "RATE_LIMITED");
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("role-picker", page, StringComparison.OrdinalIgnoreCase);
    }
}
