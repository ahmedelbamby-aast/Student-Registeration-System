using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint04ContractTests
{
    [Fact]
    public void Offering_create_is_draft_grouped_validated_and_payload_bound()
    {
        var request = Spec010ContractAssertions.ContractType("CreateOfferingRequest");
        Spec010ContractAssertions.HasExactProperties(
            request,
            "TermId", "CourseId", "Groups", "ClientRequestId");

        var endpoint = Spec010ContractAssertions.Endpoint("POST", "/api/admin/offerings");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.RequiresAntiforgery(endpoint);
        Spec010ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status201Created,
            "StudentRegistration.Contracts.Scheduling.CourseOfferingDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec010ContractAssertions.ContractContains(
            "one or more initial Draft groups",
            "Group codes are unique after canonicalization",
            "server-canonical payload",
            "Create canonicalization and replay",
            "deterministic rejection observed by the first completed attempt",
            "audit append commit atomically",
            "IDEMPOTENCY_KEY_REUSED",
            "rolls back the claim, create, and audit");
    }
}
