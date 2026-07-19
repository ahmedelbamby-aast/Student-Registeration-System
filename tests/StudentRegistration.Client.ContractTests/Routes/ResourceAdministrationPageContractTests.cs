namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class ResourceAdministrationPageContractTests
{
    [Fact]
    public void Adm_07_freezes_room_availability_and_impact_revalidation_contracts() =>
        Spec003RouteContractAssertions.AssertRoute(
            "ADM-07", "T218", "ResourceAdministrationPage", "/admin/resources",
            "ListRoomsAsync", "ListAvailabilityAsync", "RevalidateAlertAsync",
            "ResolveAlertAsync", "STALE_VERSION");
}
