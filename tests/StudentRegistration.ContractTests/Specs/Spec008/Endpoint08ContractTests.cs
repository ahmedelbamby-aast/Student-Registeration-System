using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint08ContractTests
{
    [Fact]
    public void Admin_student_locator_is_the_exact_bounded_minimal_shape()
    {
        var locator = Spec008ContractAssertions.AcademicContractType(
            "AdminStudentLocatorDto");

        Spec008ContractAssertions.HasExactProperties(
            locator,
            "StudentId",
            "UniversityId",
            "ProgramCode",
            "Cohort",
            "Standing",
            "DataVersion");
        Spec008ContractAssertions.ExcludesProperties(
            locator,
            "CurrentGpa",
            "EarnedCredits",
            "TranscriptAttempts",
            "ActiveHolds",
            "Provenance",
            "ApplicationUserId",
            "PasswordHash",
            "RowVersion");
    }

    [Fact]
    public void Frozen_locator_contract_requires_term_bounded_query_and_deterministic_sort()
    {
        RepositoryFiles.ContainsAll(
            Spec008ContractAssertions.ApiContract(),
            "08 | `GET /api/admin/students`",
            "Required `termId` and query (3..50)",
            "`200 Page<AdminStudentLocatorDto>`",
            "`AcademicProfiles.Manage`",
            "Default sort is `universityId,studentId`",
            "allowed primary sorts are University ID, program, cohort, and standing",
            "always ending in Student ID",
            "locator always requires both `termId` and a nonblank query",
            "unrestricted student dump");
    }

    [Fact]
    public void Locator_endpoint_requires_profile_management_and_declares_every_safe_outcome()
    {
        var endpoint = Spec008ContractAssertions.Endpoint(
            "GET",
            "/api/admin/students");

        Spec008ContractAssertions.RequiresPolicy(endpoint, "AcademicProfiles.Manage");
        Assert.Null(endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>());
        AssertPageResponseItem(endpoint, "AdminStudentLocatorDto");
        foreach (var status in new[]
        {
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status500InternalServerError,
            StatusCodes.Status503ServiceUnavailable
        })
        {
            Spec008ContractAssertions.DeclaresResponse(endpoint, status);
        }
    }

    [Fact]
    public void Locator_handler_cannot_enumerate_without_explicit_term_and_query()
    {
        var endpoint = Spec008ContractAssertions.Endpoint(
            "GET",
            "/api/admin/students");
        var parameters = Spec008ContractAssertions.Handler(endpoint).GetParameters();
        var names = parameters
            .Select(parameter => parameter.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("termId", names);
        Assert.Contains("query", names);
        Assert.Contains("page", names);
        Assert.Contains("pageSize", names);
        Assert.Contains("sort", names);
        Assert.DoesNotContain(
            parameters,
            parameter => parameter.ParameterType.Name.Contains("IdentityUser", StringComparison.Ordinal));
    }

    private static void AssertPageResponseItem(
        Microsoft.AspNetCore.Routing.RouteEndpoint endpoint,
        string expectedItemName)
    {
        var responseType = endpoint.Metadata
            .GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Single(metadata => metadata.StatusCode == StatusCodes.Status200OK)
            .Type;
        Assert.NotNull(responseType);
        Assert.True(
            responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition().FullName ==
                "StudentRegistration.Contracts.Page`1");
        Assert.Equal(expectedItemName, responseType.GetGenericArguments()[0].Name);
    }
}
