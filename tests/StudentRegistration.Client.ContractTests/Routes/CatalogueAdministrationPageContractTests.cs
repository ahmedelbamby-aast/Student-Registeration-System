namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class CatalogueAdministrationPageContractTests
{
    [Fact]
    public void Adm_05_freezes_version_validation_simulation_and_publish_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "ADM-05", "T208", "CatalogueAdministrationPage", "/admin/catalogue",
            "ListVersionsAsync", "ValidateCatalogueAsync", "SimulatePolicyAsync",
            "PublishCatalogueAsync", "STALE_VERSION");
}
