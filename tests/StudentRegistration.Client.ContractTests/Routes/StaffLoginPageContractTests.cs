using StudentRegistration.Client.Features.Identity;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StaffLoginPageContractTests
{
    [Fact]
    public void Auth_04_binds_login_and_server_role_selection_contracts() =>
        IdentityRouteContractAssertions.AssertRoute(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec007/AUTH-04/route-contract.json",
            "src/StudentRegistration.Client/Pages/StaffLoginPage.razor",
            "AUTH-04",
            "/staff/login",
            "IdentityRouteStateMapper.StaffLoginRecord",
            ("POST", "/api/auth/staff/login", "LoginStaffAsync"),
            ("PUT", "/api/auth/session/context", "SelectRoleContextAsync"));

    [Fact]
    public void Client_cannot_promote_a_rejected_role_selection_to_success() =>
        IdentityRouteContractAssertions.AssertRejectedStateNeverBecomesSuccess(
            IdentityRouteStateMapper.StaffLoginRecord,
            "role-selection-required",
            "NO_AUTHORIZED_ROLE");
}
