using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint11ContractTests
{
    [Fact]
    public void Staff_availability_view_is_bounded_private_complete_and_read_only()
    {
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("StaffTermAvailabilityDto"),
            "Id", "StaffId", "StaffName", "TermId", "DeadlineUtc",
            "RowVersion", "Ranges");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "GET",
            "/api/admin/staff-availability");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.DeclaresPage(
            endpoint,
            StatusCodes.Status200OK,
            "StaffTermAvailabilityDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, false);
        Spec010ContractAssertions.ContractContains(
            "Required `termId`",
            "The complete range set is read-only",
            "Admin does not receive `Availability.ManageOwn`",
            "`400 PAGE_SIZE_INVALID`",
            "raw availability payload");
    }
}
