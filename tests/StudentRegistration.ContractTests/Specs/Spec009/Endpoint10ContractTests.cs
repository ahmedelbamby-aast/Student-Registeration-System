using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint10ContractTests
{
    [Fact]
    public void Policy_create_uses_typed_operations_and_payload_bound_idempotency()
    {
        var request = Spec009ContractAssertions.ContractType("CreatePolicySetRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "Scope", "Version", "TermId", "ProgramId", "EffectiveFromUtc",
            "EffectiveToUtc", "Reason", "Operations", "ClientRequestId");
        Spec009ContractAssertions.Excludes(
            Spec009ContractAssertions.ContractType("PolicySetOperation"),
            "Expression", "Script", "PropertyName");
        var endpoint = Spec009ContractAssertions.Endpoint("POST", "/api/admin/policies");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status201Created,
            "StudentRegistration.Contracts.Academics.PolicySetAdminDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, false, true);
        Spec009ContractAssertions.ContractContains(
            "canonical typed payload",
            "UNKNOWN_RULE_TYPE",
            "RULE_VALUE_TYPE_MISMATCH",
            "IDEMPOTENCY_KEY_REUSED");
    }
}
