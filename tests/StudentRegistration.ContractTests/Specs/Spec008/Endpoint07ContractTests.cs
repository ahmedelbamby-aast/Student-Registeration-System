using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint07ContractTests
{
    [Fact]
    public void Publish_request_requires_both_contested_versions_and_no_idempotency_payload()
    {
        var request = Spec008ContractAssertions.AcademicContractType(
            "PublishRegistrationWindowRequest");

        Spec008ContractAssertions.HasExactProperties(
            request,
            "ExpectedTermRowVersion",
            "ExpectedWindowRowVersion",
            "Reason",
            "Source");
        Spec008ContractAssertions.ExcludesProperties(
            request,
            "ExpectedRowVersion",
            "ExpectedWindowRowVersions",
            "ClientRequestId",
            "CreationPayloadHash",
            "Windows",
            "LifecycleState");
    }

    [Fact]
    public void Frozen_publish_contract_requires_stable_locking_conservative_overlap_and_bounded_safe_errors()
    {
        var contract = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/contracts/api.md");
        var requirements = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/requirements.md");

        RepositoryFiles.ContainsAll(
            contract,
            "Publication forbids any overlap between Published windows in the same term",
            "regardless of scope",
            "expectedTermRowVersion",
            "expectedWindowRowVersion",
            "409 STALE_VERSION/WINDOW_OVERLAP",
            "ApiError.fieldErrors` contains at most 20 field keys",
            "5 messages per key",
            "256 characters per message",
            "one atomic transaction",
            "second idempotency entity");
        RepositoryFiles.ContainsAll(
            requirements,
            "lock the AcademicTerm and then",
            "every candidate/existing RegistrationWindow in stable ID order",
            "recheck",
            "inside the transaction",
            "reason, source/provenance, and audit");
    }

    [Fact]
    public void Publish_endpoint_requires_permission_antiforgery_and_the_complete_status_matrix()
    {
        var endpoint = Spec008ContractAssertions.Endpoint(
            "POST",
            "/api/admin/terms/{termId}/registration-windows/{windowId}/publish");

        Spec008ContractAssertions.RequiresPolicy(endpoint, "AcademicTerms.Manage");
        Assert.True(
            endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>()?.RequiresValidation == true);
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.Academics.AdminTermDto");
        foreach (var status in new[]
        {
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
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
            "STALE_VERSION",
            "WINDOW_OVERLAP");
    }
}
