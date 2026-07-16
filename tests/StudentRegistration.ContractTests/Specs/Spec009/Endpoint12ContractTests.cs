using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint12ContractTests
{
    [Fact]
    public void Policy_validation_requires_typed_complete_rules_and_returns_bound_preview()
    {
        var request = Spec009ContractAssertions.ContractType("PolicyValidationRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "ExpectedPolicySetRowVersion");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "POST",
            "/api/admin/policies/{policySetId}/validate");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.CatalogueValidationResult");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec009ContractAssertions.ContractContains(
            "complete typed demo rule set",
            "no executable expression",
            "valid response includes a bound preview token",
            "POLICY_NOT_VALIDATABLE");
    }
}
