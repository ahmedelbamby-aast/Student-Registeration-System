namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class SubjectDetailsPageContractTests
{
    [Fact]
    public void Stu_03_freezes_authoritative_details_and_reason_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "STU-03", "T158", "SubjectDetailsPage", "/student/subjects/{OfferingId:guid}",
            "GetAppContextAsync", "GetEligibilityAsync", "GROUP_FULL", "SESSION_EXPIRED");
}
