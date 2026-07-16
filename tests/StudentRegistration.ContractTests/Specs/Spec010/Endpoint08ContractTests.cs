using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint08ContractTests
{
    [Fact]
    public void Room_list_is_bounded_filtered_authorized_and_stably_sorted()
    {
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("RoomDto"),
            "Id", "Code", "Location", "Capacity", "State", "RowVersion");

        var endpoint = Spec010ContractAssertions.Endpoint("GET", "/api/admin/rooms");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.DeclaresPage(
            endpoint,
            StatusCodes.Status200OK,
            "RoomDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, false, false);
        Spec010ContractAssertions.ContractContains(
            "Optional `query`, `state`, `minimumCapacity`, `page`, `pageSize`, `sort`",
            "Default sort `code,id`",
            "`400 PAGE_SIZE_INVALID`");
    }
}
