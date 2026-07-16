using StudentRegistration.Client.Features.Identity;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class AccountRecoveryPageContractTests
{
    [Fact]
    public void Auth_05_binds_request_and_completion_contracts() =>
        IdentityRouteContractAssertions.AssertRoute(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec007/AUTH-05/route-contract.json",
            "src/StudentRegistration.Client/Pages/AccountRecoveryPage.razor",
            "AUTH-05",
            "/account/recovery",
            "IdentityRouteStateMapper.AccountRecoveryRecord",
            ("POST", "/api/auth/recovery/request", "RequestRecoveryAsync"),
            ("POST", "/api/auth/recovery/complete", "CompleteRecoveryAsync"));

    [Fact]
    public void Rejected_recovery_never_becomes_success() =>
        IdentityRouteContractAssertions.AssertRejectedStateNeverBecomesSuccess(
            IdentityRouteStateMapper.AccountRecoveryRecord,
            "success",
            "CHALLENGE_INVALID");
}
