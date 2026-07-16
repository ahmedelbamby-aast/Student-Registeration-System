using StudentRegistration.Client.Features.Identity;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StudentLoginPageContractTests
{
    [Fact]
    public void Auth_02_binds_the_approved_route_api_and_reason_contract() =>
        IdentityRouteContractAssertions.AssertRoute(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec007/AUTH-02/route-contract.json",
            "src/StudentRegistration.Client/Pages/StudentLoginPage.razor",
            "AUTH-02",
            "/student/login",
            "IdentityRouteStateMapper.StudentLoginRecord",
            ("POST", "/api/auth/student/login", "LoginStudentAsync"));

    [Theory]
    [InlineData("success", "INVALID_CREDENTIALS")]
    [InlineData("unknown-state", "AUTH02_UNKNOWN")]
    public void Rejected_server_result_never_becomes_login_success(
        string state,
        string reason) =>
        IdentityRouteContractAssertions.AssertRejectedStateNeverBecomesSuccess(
            IdentityRouteStateMapper.StudentLoginRecord,
            state,
            reason);
}
