using StudentRegistration.Client.Features.Identity;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StaffLoginPageContractTests
{
    [Fact]
    public void Auth_04_binds_single_role_staff_login_contract() =>
        IdentityRouteContractAssertions.AssertRoute(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec007/AUTH-04/route-contract.json",
            "src/StudentRegistration.Client/Pages/StaffLoginPage.razor",
            "AUTH-04",
            "/staff/login",
            "IdentityRouteStateMapper.StaffLoginRecord",
            ("POST", "/api/auth/staff/login", "LoginStaffAsync"));

    [Fact]
    public void Client_cannot_promote_an_invalid_role_configuration_to_success() =>
        IdentityRouteContractAssertions.AssertRejectedStateNeverBecomesSuccess(
            IdentityRouteStateMapper.StaffLoginRecord,
            "service-error",
            "INVALID_ROLE_CONFIGURATION");
}
