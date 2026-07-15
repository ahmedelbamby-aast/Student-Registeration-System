using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Authenticated_context_is_complete_replica_neutral_and_contains_no_topology()
    {
        var context = Spec008ContractAssertions.SharedContractType("AppContextDto");

        Spec008ContractAssertions.HasExactProperties(
            context,
            "ServerTimeUtc",
            "TimeZoneId",
            "TeachingTerm",
            "RegistrationTerm",
            "RegistrationWindowState",
            "RegistrationWindow",
            "ServiceState",
            "DisplayName",
            "AuthorizedRoles",
            "ActiveRole",
            "SessionState",
            "ExpiresAtUtc",
            "SupportReferencePath");
        Spec008ContractAssertions.ExcludesProperties(
            context,
            "ReplicaId",
            "NodeId",
            "ServerId",
            "ConnectionString",
            "StudentProfile");
    }

    [Fact]
    public void Authenticated_context_contract_requires_complete_shared_replica_consistent_resolution()
    {
        RepositoryFiles.ContainsAll(
            Spec008ContractAssertions.ApiContract(),
            "02 | `GET /api/context`",
            "`200 AppContextDto`",
            "Authenticated + `Context.Read`",
            "`503 CONTEXT_UNAVAILABLE`",
            "no partial success body");
        RepositoryFiles.ContainsAll(
            Spec008ContractAssertions.Requirements(),
            "300 GET /api/context reads per second",
            "two independently addressable stateless replicas sharing SQL",
            "approved 25,000-account fixture");
    }

    [Fact]
    public void Authenticated_context_endpoint_requires_context_read_and_declares_safe_outcomes()
    {
        var endpoint = Spec008ContractAssertions.Endpoint("GET", "/api/context");

        Spec008ContractAssertions.RequiresPolicy(endpoint, "Context.Read");
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.AppContextDto");
        Spec008ContractAssertions.DeclaresResponse(endpoint, StatusCodes.Status401Unauthorized);
        Spec008ContractAssertions.DeclaresResponse(endpoint, StatusCodes.Status403Forbidden);
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status500InternalServerError,
            "StudentRegistration.Contracts.ApiError");
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status503ServiceUnavailable,
            "StudentRegistration.Contracts.ApiError");
    }
}
