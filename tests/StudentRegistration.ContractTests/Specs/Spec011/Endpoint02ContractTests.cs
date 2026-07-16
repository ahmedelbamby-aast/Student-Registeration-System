using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec011;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Eligibility_detail_is_complete_authorized_and_refresh_only()
    {
        var endpoint = Spec011ContractAssertions.Endpoint(
            "GET",
            "/api/student/offerings/{offeringId}/eligibility");

        Spec011ContractAssertions.RequiresCatalogueReadAvailable(endpoint);
        Spec011ContractAssertions.HasNoClientStudentIdentifier(endpoint);
        Spec011ContractAssertions.HasQueryParameters(endpoint);
        Spec011ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Registration.OfferingEligibilityDto");
        Spec011ContractAssertions.DeclaresErrors(
            endpoint,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status500InternalServerError);
        Spec011ContractAssertions.ContractContains(
            "`200 OfferingEligibilityDto`",
            "`404 OFFERING_NOT_FOUND_OR_OUTSIDE_CONTEXT`",
            "`503 DISCOVERY_UNAVAILABLE`",
            "returns `selectable=false`, `GROUP_FULL`, zero seats, and the new rowversion",
            "It does not reserve or submit a seat",
            "final submission belongs to SPEC-014");
    }
}
