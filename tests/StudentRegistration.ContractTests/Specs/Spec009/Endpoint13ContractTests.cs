using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint13ContractTests
{
    [Fact]
    public void Policy_simulation_is_typed_deterministic_explained_and_non_mutating()
    {
        var request = Spec009ContractAssertions.ContractType("PolicySimulationRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "PolicySetId", "StudentContextFixtureId", "RequestedCourseCodes");
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("PolicySimulationResult"),
            "Eligible", "PolicyVersion", "RuleResults");
        Spec009ContractAssertions.HasExactProperties(
            Spec009ContractAssertions.ContractType("PolicyRuleSimulationResultDto"),
            "RuleCode", "Passed", "RequiredValue", "CurrentValue",
            "SourceReference", "SourceKind", "Explanation");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "POST",
            "/api/admin/policies/{policySetId}/simulate");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.PolicySimulationResult");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec009ContractAssertions.ContractContains(
            "deterministic ordered rule results",
            "Simulation performs no durable mutation",
            "SIMULATION_FIXTURE_INVALID");
    }
}
