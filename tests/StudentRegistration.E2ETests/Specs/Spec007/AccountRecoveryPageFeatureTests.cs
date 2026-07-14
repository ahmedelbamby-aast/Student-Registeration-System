using StudentRegistration.TestSupport;

namespace StudentRegistration.E2ETests.Specs.Spec007;

public sealed class AccountRecoveryPageFeatureTests
{
    [Fact]
    public void Recovery_request_is_generic_and_completion_never_echoes_secrets()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/account/recovery\"",
            "RecoveryRequest",
            "RecoveryCompleteRequest",
            "If an eligible account matches",
            "Recovery code",
            "New password",
            "Confirm new password",
            "AllowSecretReveal=\"true\"",
            "CHALLENGE_INVALID",
            "PASSWORD_REJECTED",
            "IsRequesting",
            "IsCompleting");
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("response.Content.ReadAsStringAsync", page, StringComparison.Ordinal);
    }
}
