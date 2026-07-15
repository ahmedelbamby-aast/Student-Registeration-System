using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint06ContractTests
{
    [Fact]
    public void Update_request_requires_term_and_window_versions_without_a_generic_partial_schema()
    {
        var request = Spec008ContractAssertions.AcademicContractType("UpdateTermRequest");

        Spec008ContractAssertions.HasExactProperties(
            request,
            "ExpectedTermRowVersion",
            "ExpectedWindowRowVersions",
            "Reason",
            "Source",
            "Term",
            "Windows");
        Spec008ContractAssertions.ExcludesProperties(
            request,
            "ExpectedRowVersion",
            "ClientRequestId",
            "CreationPayloadHash",
            "Operations",
            "Changes",
            "Patch",
            "PropertyName",
            "NavigationProperty");
    }

    [Fact]
    public void Frozen_update_contract_is_bounded_atomic_and_requires_refetch_after_conflict()
    {
        var contract = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/contracts/api.md");

        RepositoryFiles.ContainsAll(
            contract,
            "expectedWindowRowVersions: Record<string, string>; // maximum 20",
            "windows: TermWindowInput[]; // maximum 20",
            "must match for existing windows",
            "400 VALIDATION_ERROR",
            "authorized `404`",
            "409 STALE_VERSION/TERM_STATE_CONFLICT/WINDOW_OVERLAP",
            "ApiError.currentVersion` is only the directly contested aggregate",
            "refetch the bounded aggregate for all versions",
            "Transient failures commit no partial state");
    }

    [Fact]
    public void Update_endpoint_requires_permission_antiforgery_and_the_complete_status_matrix()
    {
        var endpoint = Spec008ContractAssertions.Endpoint(
            "PUT",
            "/api/admin/terms/{termId}");

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
            "TERM_STATE_CONFLICT",
            "WINDOW_OVERLAP");
    }
}
