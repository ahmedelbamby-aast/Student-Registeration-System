using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint04ContractTests
{
    [Fact]
    public void Draft_mutation_is_versioned_allow_listed_atomic_and_preview_invalidating()
    {
        var request = Spec009ContractAssertions.ContractType(
            "CatalogueDraftMutationRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "ExpectedDraftRowVersion", "Reason", "Source", "Operations");
        Spec009ContractAssertions.Excludes(
            Spec009ContractAssertions.ContractType("CatalogueDraftOperation"),
            "PropertyName", "NavigationGraph", "Expression");

        var endpoint = Spec009ContractAssertions.Endpoint(
            "PUT",
            "/api/admin/catalogue/drafts/{draftId}");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.CatalogueDraftDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec009ContractAssertions.ContractContains(
            "apply atomically",
            "prior preview is invalidated",
            "PROVENANCE_REQUIRED",
            "PROVENANCE_INVALID",
            "No partial operation is committed");
    }
}
