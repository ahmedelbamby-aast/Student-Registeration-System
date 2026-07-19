namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class SubjectDiscoveryPageContractTests
{
    [Fact]
    public void Stu_02_freezes_authoritative_discovery_and_reason_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "STU-02", "T153", "SubjectDiscoveryPage", "/student/subjects",
            "GetAppContextAsync", "ListOfferingsAsync", "GROUP_FULL", "SESSION_EXPIRED");
}
