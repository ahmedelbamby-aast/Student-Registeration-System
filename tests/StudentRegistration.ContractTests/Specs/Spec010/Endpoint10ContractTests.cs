using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint10ContractTests
{
    [Fact]
    public void Room_update_requires_version_and_atomically_records_published_impact()
    {
        var request = Spec010ContractAssertions.ContractType("UpdateRoomRequest");
        Spec010ContractAssertions.HasExactProperties(
            request,
            "ExpectedRowVersion", "Code", "Location", "Capacity", "State", "Reason");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "PUT",
            "/api/admin/rooms/{roomId}");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.RequiresAntiforgery(endpoint);
        Spec010ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Scheduling.RoomDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec010ContractAssertions.ContractContains(
            "Expected room version is required",
            "creates or refreshes durable impact alerts",
            "every room change appends a privacy-safe audit",
            "ROOM_CAPACITY_CONFLICT",
            "No room, alert, or audit effect commits partially");
    }
}
