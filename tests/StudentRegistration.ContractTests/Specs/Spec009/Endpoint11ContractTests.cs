using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint11ContractTests
{
    [Fact]
    public void Policy_update_requires_rowversion_typed_values_source_and_preview_invalidation()
    {
        var request = Spec009ContractAssertions.ContractType(
            "PolicySetMutationRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "ExpectedPolicySetRowVersion", "Reason", "Operations");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "PUT",
            "/api/admin/policies/{policySetId}");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.PolicySetAdminDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec009ContractAssertions.ContractContains(
            "Expected policy-set rowversion is required",
            "only allow-listed typed operations",
            "old preview is invalidated",
            "No partial rule edit is committed");
    }
}
