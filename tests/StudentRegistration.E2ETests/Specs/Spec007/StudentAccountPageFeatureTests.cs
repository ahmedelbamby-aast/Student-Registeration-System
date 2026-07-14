using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec007;

public sealed class StudentAccountPageFeatureTests
{
    [Fact]
    public void Student_account_keeps_identity_read_only_and_confirms_session_revocation()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/StudentAccountPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/student/account\"",
            "GetSessionAsync",
            "ChangePasswordRequest",
            "Current password",
            "New password",
            "AllowSecretReveal=\"true\"",
            "ConfirmationDialog",
            "RevokeAllSessionsAsync",
            "LogoutAsync",
            "CURRENT_PASSWORD_INVALID",
            "NavigateTo(\"/student/login\")");
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SecurityStamp", page, StringComparison.Ordinal);
    }
}
