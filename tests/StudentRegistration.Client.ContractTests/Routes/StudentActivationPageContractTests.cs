using StudentRegistration.Client.Features.Identity;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StudentActivationPageContractTests
{
    [Fact]
    public void Auth_03_binds_the_approved_route_api_and_reason_contract() =>
        IdentityRouteContractAssertions.AssertRoute(
            "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec007/AUTH-03/route-contract.json",
            "src/StudentRegistration.Client/Pages/StudentActivationPage.razor",
            "AUTH-03",
            "/student/activate",
            "IdentityRouteStateMapper.StudentActivationRecord",
            ("POST", "/api/auth/student/activate", "ActivateStudentAsync"));

    [Fact]
    public void Rejected_activation_never_becomes_success() =>
        IdentityRouteContractAssertions.AssertRejectedStateNeverBecomesSuccess(
            IdentityRouteStateMapper.StudentActivationRecord,
            "success",
            "ACTIVATION_FAILED");
}
