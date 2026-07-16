using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint07ContractTests
{
    [Fact]
    public void Catalogue_validation_requires_both_versions_and_returns_a_bound_preview()
    {
        var request = Spec009ContractAssertions.ContractType(
            "CatalogueValidationRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "ExpectedImportRowVersion", "ExpectedDraftRowVersion");
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("CatalogueValidationResult"),
            "Valid", "Errors", "PreviewToken", "ExpectedDraftRowVersion",
            "DependencyVersions");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "POST",
            "/api/admin/catalogue/imports/{importId}/validate");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.CatalogueValidationResult");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec009ContractAssertions.ContractContains(
            "complete normalized graph",
            "official/synthetic field provenance",
            "bound to actor, scope",
            "Invalid graph results are a `200`");
    }
}
