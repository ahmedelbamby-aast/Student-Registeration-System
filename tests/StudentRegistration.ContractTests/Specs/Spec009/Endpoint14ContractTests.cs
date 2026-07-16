using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec009;

public sealed class Endpoint14ContractTests
{
    [Fact]
    public void Policy_publish_has_one_winner_immutable_version_audit_and_replay()
    {
        var request = Spec009ContractAssertions.ContractType("PolicyPublishRequest");
        Spec009ContractAssertions.HasExactProperties(
            request,
            "ExpectedPolicySetRowVersion", "PreviewToken", "ClientRequestId");
        var endpoint = Spec009ContractAssertions.Endpoint(
            "POST",
            "/api/admin/policies/{policySetId}/publish");
        Spec009ContractAssertions.RequiresPolicy(endpoint);
        Spec009ContractAssertions.RequiresAntiforgery(endpoint);
        Spec009ContractAssertions.HandlerAccepts(endpoint, request.Name);
        Spec009ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status201Created,
            "StudentRegistration.Contracts.Academics.PolicySetAdminDto");
        Spec009ContractAssertions.DeclaresStandardErrors(endpoint, true, true);
        Spec009ContractAssertions.ContractContains(
            "creates one immutable published version",
            "appends one audit fact atomically",
            "Concurrent confirmations have one winner",
            "same-payload replay returns the stored result");
    }
}
