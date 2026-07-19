namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class RegistrationReviewPageContractTests
{
    [Fact]
    public void Stu_05_freezes_authoritative_validation_submission_and_recovery_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "STU-05", "T168", "RegistrationReviewPage", "/student/review",
            "GetRegistrationPlanAsync", "ValidateRegistrationPlanAsync",
            "SubmitRegistrationAsync", "LookupRegistrationAsync", "WINDOW_CLOSED");
}
