namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class RegistrationResultPageContractTests
{
    [Fact]
    public void Stu_06_freezes_owned_result_receipt_and_lost_response_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "STU-06", "T173", "RegistrationResultPage", "/student/registration/result/{Id:guid}",
            "GetRegistrationDetailAsync", "LookupRegistrationAsync",
            "REGISTRATION_RESULT_UNAVAILABLE", "NoPartialRegistration");
}
