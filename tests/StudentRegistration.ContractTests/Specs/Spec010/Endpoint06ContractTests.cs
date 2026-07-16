using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint06ContractTests
{
    [Fact]
    public void Offering_validation_enforces_bundle_staffing_resources_and_bound_preview()
    {
        var request = Spec010ContractAssertions.ContractType("ValidateOfferingRequest");
        Spec010ContractAssertions.HasExactProperties(
            request,
            "ExpectedOfferingRowVersion", "ExpectedGroupRowVersions",
            "ExpectedRoomRowVersions", "ExpectedStaffTermAvailabilityRowVersions");
        Spec010ContractAssertions.HasExactProperties(
            Spec010ContractAssertions.ContractType("OfferingValidationResult"),
            "Valid", "Reasons", "PreviewToken", "DependencyVersions");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "POST",
            "/api/admin/offerings/{offeringId}/validate");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.RequiresAntiforgery(endpoint);
        Spec010ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Scheduling.OfferingValidationResult");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec010ContractAssertions.ContractContains(
            "Staffing, capacity, availability, and conflict failures are `200 valid=false`",
            "MISSING_LECTURE",
            "MISSING_TEACHING_ASSISTANT",
            "ROOM_CONFLICT",
            "STAFF_UNAVAILABLE",
            "signed preview bound to actor",
            "`OFFERING_NOT_VALIDATABLE` is lifecycle/state-only",
            "`Section`");
    }
}
