using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint04ContractTests
{
    private const string ServiceSourcePath =
        "src/StudentRegistration.Academics/Application/AdminAcademicManagementService.cs";

    [Fact]
    public void Term_list_response_has_the_exact_bounded_admin_shape()
    {
        Spec008ContractAssertions.HasExactProperties(
            Spec008ContractAssertions.AcademicContractType("AdminTermDto"),
            "Id", "Code", "DisplayName", "TimeZoneId", "TeachingStartsOn",
            "TeachingEndsOn", "State", "RowVersion", "Windows");
        Spec008ContractAssertions.HasExactProperties(
            Spec008ContractAssertions.AcademicContractType("AdminRegistrationWindowDto"),
            "Id", "ScopeType", "ScopeValue", "OpensAtUtc", "ClosesAtUtc",
            "LifecycleState", "ComputedState", "RowVersion");
    }

    [Fact]
    public void Frozen_contract_requires_bounded_filtering_and_a_deterministic_id_tie_breaker()
    {
        var contract = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/contracts/api.md");

        RepositoryFiles.ContainsAll(
            contract,
            "GET /api/admin/terms",
            "Optional query (3..50), page, pageSize, state and allow-listed sort",
            "200 Page<AdminTermDto>",
            "Default sort is `code,id`",
            "allowed primary sorts are code, teaching start, and state",
            "always ending in ID",
            "400 PAGE_SIZE_INVALID/VALIDATION_ERROR",
            "conflict is not applicable");

        var service = RepositoryFiles.Read(ServiceSourcePath);
        RepositoryFiles.ContainsAll(
            service,
            "DefaultPageSize = 20",
            "MaximumPageSize = 100",
            "MinimumQueryLength = 3",
            "MaximumQueryLength = 50",
            "TeachingStartsOn",
            "ThenBy",
            ".Id");
    }

    [Fact]
    public void Term_list_requires_permission_and_declares_the_complete_denial_matrix()
    {
        var endpoint = Spec008ContractAssertions.Endpoint("GET", "/api/admin/terms");

        Spec008ContractAssertions.RequiresPolicy(endpoint, "AcademicTerms.Manage");
        Spec008ContractAssertions.DeclaresResponse(endpoint, StatusCodes.Status200OK);
        var successType = endpoint.Metadata
            .GetOrderedMetadata<Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata>()
            .Single(metadata => metadata.StatusCode == StatusCodes.Status200OK)
            .Type;
        Assert.True(successType?.IsGenericType == true);
        Assert.Equal(
            "StudentRegistration.Contracts.Page`1",
            successType!.GetGenericTypeDefinition().FullName);
        Assert.Equal(
            "StudentRegistration.Contracts.Academics.AdminTermDto",
            successType.GetGenericArguments().Single().FullName);
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

        var parameters = Spec008ContractAssertions.Handler(endpoint)
            .GetParameters()
            .Select(parameter => parameter.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("query", parameters);
        Assert.Contains("page", parameters);
        Assert.Contains("pageSize", parameters);
        Assert.Contains("state", parameters);
        Assert.Contains("sort", parameters);
    }
}
