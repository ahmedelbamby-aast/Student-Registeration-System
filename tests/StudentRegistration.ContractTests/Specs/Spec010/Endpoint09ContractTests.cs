using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint09ContractTests
{
    [Fact]
    public void Room_create_normalizes_code_and_uses_payload_bound_idempotency()
    {
        var request = Spec010ContractAssertions.ContractType("CreateRoomRequest");
        Spec010ContractAssertions.HasExactProperties(
            request,
            "Code", "Location", "Capacity", "State", "ClientRequestId");

        var endpoint = Spec010ContractAssertions.Endpoint("POST", "/api/admin/rooms");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.RequiresAntiforgery(endpoint);
        Spec010ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status201Created,
            "StudentRegistration.Contracts.Scheduling.RoomDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, false, true);
        Spec010ContractAssertions.ContractContains(
            "Unicode NFKC-normalized and trimmed",
            "codes are then uppercased",
            "actor plus institutional scope",
            "Room creation and its privacy-safe audit append commit atomically",
            "recorded success or deterministic rejection",
            "ROOM_CODE_EXISTS",
            "IDEMPOTENCY_KEY_REUSED",
            "Response loss after commit is recovered");
    }
}
