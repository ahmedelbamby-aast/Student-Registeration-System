using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint03ContractTests
{
    [Fact]
    public void Admin_offering_list_is_bounded_filtered_authorized_and_stably_sorted()
    {
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("CourseOfferingSummaryDto"),
            "Id", "TermId", "CourseId", "CourseCode", "CourseTitle", "State",
            "GroupCount", "RowVersion");

        var endpoint = Spec010ContractAssertions.Endpoint("GET", "/api/admin/offerings");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.DeclaresPage(
            endpoint,
            StatusCodes.Status200OK,
            "CourseOfferingSummaryDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, false, false);
        Spec010ContractAssertions.ContractContains(
            "Optional `termId`, `state`, `query`, `page`, `pageSize`, `sort`",
            "Default sort `courseCode,id`",
            "`400 PAGE_SIZE_INVALID`",
            "Nested groups are not returned by this list");
    }
}
