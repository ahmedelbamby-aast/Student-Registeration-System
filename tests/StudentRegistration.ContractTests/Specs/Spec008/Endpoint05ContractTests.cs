using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint05ContractTests
{
    private const string ServiceSourcePath =
        "src/StudentRegistration.Academics/Application/RegistrationWindowService.cs";

    [Fact]
    public void Create_term_request_has_the_exact_payload_and_no_client_payload_hash()
    {
        var request = Spec008ContractAssertions.AcademicContractType("CreateTermRequest");

        Spec008ContractAssertions.HasExactProperties(
            request,
            "ClientRequestId", "Reason", "Source", "Term", "Windows");
        Spec008ContractAssertions.ExcludesProperties(
            request,
            "CreationPayloadHash",
            "PayloadHash",
            "ExpectedRowVersion",
            "ExpectedTermRowVersion",
            "ExpectedWindowRowVersions",
            "Role",
            "ServerTimeUtc");
        Spec008ContractAssertions.HasExactProperties(
            Spec008ContractAssertions.AcademicContractType("TermInput"),
            "Code", "DisplayName", "TimeZoneId", "TeachingStartsOn",
            "TeachingEndsOn", "State");
        Spec008ContractAssertions.HasExactProperties(
            Spec008ContractAssertions.AcademicContractType("TermWindowInput"),
            "Id", "ScopeType", "ScopeValue", "OpensAtUtc", "ClosesAtUtc",
            "LifecycleState");
    }

    [Fact]
    public void Create_semantics_are_durable_payload_bound_and_atomic()
    {
        var contract = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "nonblank payload-bound",
            "globally unique",
            "CreationClientRequestId",
            "server-canonical `CreationPayloadHash`",
            "same transaction as the term, windows, and audit",
            "Same-key/same-payload",
            "replays the created aggregate",
            "same-key/different-payload returns",
            "409 IDEMPOTENCY_KEY_REUSED");

        var service = RepositoryFiles.Read(ServiceSourcePath);
        RepositoryFiles.ContainsAll(
            service,
            "CreationClientRequestId",
            "CreationPayloadHash",
            "IDEMPOTENCY_KEY_REUSED",
            "TERM_CODE_EXISTS",
            "TERM_STATE_CONFLICT");
    }

    [Fact]
    public void Create_endpoint_requires_permission_antiforgery_and_the_complete_status_matrix()
    {
        var endpoint = Spec008ContractAssertions.Endpoint("POST", "/api/admin/terms");

        Spec008ContractAssertions.RequiresPolicy(endpoint, "AcademicTerms.Manage");
        Assert.True(
            endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>()?.RequiresValidation == true);
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status201Created,
            "StudentRegistration.Contracts.Academics.AdminTermDto");
        foreach (var status in new[]
        {
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status409Conflict,
            StatusCodes.Status500InternalServerError,
            StatusCodes.Status503ServiceUnavailable
        })
        {
            Spec008ContractAssertions.DeclaresResponse(endpoint, status);
        }

        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs"),
            "TERM_CODE_EXISTS",
            "TERM_STATE_CONFLICT",
            "IDEMPOTENCY_KEY_REUSED");
    }
}
