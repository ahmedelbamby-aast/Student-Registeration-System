namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class AccountRecoveryPageComponentTests
{
    [Fact]
    public void Auth_05_has_two_guarded_steps_enumeration_safe_result_and_secret_cleanup()
    {
        var source = IdentityPageComponentAssertions.AssertCommon(
            "src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor",
            "/account/recovery",
            "recovery-request-form",
            "if (IsRequesting || IsCompleting || !ValidateRequest())",
            "data-testid=\"recovery-complete-form\"",
            "RequestRecoveryAsync",
            "CompleteRecoveryAsync",
            "GenericRequestConfirmation",
            "ClearSecrets()",
            "if (IsRequesting || IsCompleting || !ValidateCompletion())");

        Assert.Contains("without revealing whether an account exists", source, StringComparison.Ordinal);
    }
}
