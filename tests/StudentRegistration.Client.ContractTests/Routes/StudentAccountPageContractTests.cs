using StudentRegistration.Client.Features.Identity;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StudentAccountPageContractTests
{
    [Fact]
    public void Stu_08_binds_session_password_revoke_and_logout_contracts() =>
        IdentityRouteContractAssertions.AssertRoute(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec007/STU-08/route-contract.json",
            "src/StudentRegistration.Client/Pages/StudentAccountPage.razor",
            "STU-08",
            "/student/account",
            "IdentityRouteStateMapper.StudentAccountRecord",
            ("GET", "/api/auth/session", "GetSessionAsync"),
            ("POST", "/api/auth/password/change", "ChangePasswordAsync"),
            ("POST", "/api/auth/sessions/revoke-all", "RevokeAllSessionsAsync"),
            ("POST", "/api/auth/logout", "LogoutAsync"));

    [Theory]
    [InlineData("success", "SESSION_EXPIRED")]
    [InlineData("unknown-state", "STU08_UNKNOWN")]
    public void Rejected_account_result_never_becomes_success(
        string state,
        string reason) =>
        IdentityRouteContractAssertions.AssertRejectedStateNeverBecomesSuccess(
            IdentityRouteStateMapper.StudentAccountRecord,
            state,
            reason);
}
