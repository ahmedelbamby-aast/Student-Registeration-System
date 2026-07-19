namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class OfferingAdministrationPageContractTests
{
    [Fact]
    public void Adm_06_freezes_offering_validation_and_publish_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "ADM-06", "T213", "OfferingAdministrationPage", "/admin/offerings",
            "ListOfferingsAsync", "UpdateGroupAsync", "ValidateOfferingAsync",
            "PublishOfferingAsync", "STALE_VERSION");
}
