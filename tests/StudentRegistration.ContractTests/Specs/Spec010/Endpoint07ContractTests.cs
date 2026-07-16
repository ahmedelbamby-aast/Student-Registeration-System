using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec010;

public sealed class Endpoint07ContractTests
{
    [Fact]
    public void Offering_publish_uses_stable_locks_revalidation_audit_and_replay()
    {
        var request = Spec010ContractAssertions.ContractType("PublishOfferingRequest");
        Spec010ContractAssertions.HasExactProperties(
            request,
            "ExpectedOfferingRowVersion", "ExpectedGroupRowVersions",
            "ExpectedRoomRowVersions", "ExpectedStaffTermAvailabilityRowVersions",
            "PreviewToken", "ClientRequestId", "Reason");

        var endpoint = Spec010ContractAssertions.Endpoint(
            "POST",
            "/api/admin/offerings/{offeringId}/publish");
        Spec010ContractAssertions.RequiresOfferingsManage(endpoint);
        Spec010ContractAssertions.RequiresAntiforgery(endpoint);
        Spec010ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec010ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Scheduling.CourseOfferingDto");
        Spec010ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec010ContractAssertions.ContractContains(
            "Locks CourseOffering, sorted groups, sorted rooms, then sorted staff-term roots",
            "appends audit atomically",
            "only endpoint that transitions Draft offerings/groups to Published",
            "`OFFERING_NOT_VALIDATABLE` is lifecycle/state-only",
            "STALE_PREVIEW",
            "RESOURCE_CONFLICT",
            "Same-key/same-payload replays",
            "deadlock retry reruns the whole idempotent transaction");
    }
}
