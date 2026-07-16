using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint08ContractTests
{
    [Fact]
    public void Catalogue_publish_is_preview_bound_idempotent_and_atomic()
    {
        var request = Spec009ContractAssertions.ContractType("PublishVersionRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "ExpectedDraftRowVersion", "PreviewToken", "ClientRequestId");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "POST",
            "/api/admin/catalogue/imports/{importId}/publish");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status201Created,
            "StudentRegistration.Contracts.Academics.CatalogueVersionSummaryDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec009ContractAssertions.ContractContains(
            "locks normalized catalogue scope",
            "appends one audit fact in one transaction",
            "STALE_PREVIEW",
            "Same-key/same-payload replays");
    }
}
