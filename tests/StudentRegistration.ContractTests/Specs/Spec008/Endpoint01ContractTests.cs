using Microsoft.AspNetCore.Http;

namespace StudentRegistration.ContractTests.Specs.Spec008;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Public_context_is_exactly_the_six_field_privacy_safe_shared_contract()
    {
        var context = Spec008ContractAssertions.SharedContractType("PublicContextDto");

        Spec008ContractAssertions.HasExactProperties(
            context,
            "ServerTimeUtc",
            "TimeZoneId",
            "TeachingTermLabel",
            "RegistrationTermLabel",
            "RegistrationWindowState",
            "ServiceState");
        Spec008ContractAssertions.ExcludesProperties(
            context,
            "DisplayName",
            "UniversityId",
            "StudentId",
            "Roles",
            "SessionState",
            "RegistrationWindow",
            "RowVersion",
            "ReplicaId",
            "DiagnosticDetails");
        Assert.Equal(
            typeof(DateTime),
            Spec008ContractAssertions.Property(context, "ServerTimeUtc").PropertyType);
        Assert.Null(
            context.Assembly.GetType(
                "StudentRegistration.Contracts.Academics.PublicAcademicContextDto"));
    }

    [Fact]
    public void Public_context_contract_remains_the_frozen_anonymous_server_time_read()
    {
        RepositoryFiles.ContainsAll(
            Spec008ContractAssertions.ApiContract(),
            "`PublicAcademicContextDto` is an alias for the exact six-field",
            "All instants are normalized to UTC",
            "01 | `GET /api/public/context`",
            "No input; `200 PublicContextDto`",
            "Anonymous",
            "`503 CONTEXT_UNAVAILABLE`; `500 INTERNAL_ERROR`");
    }

    [Fact]
    public void Public_context_endpoint_is_anonymous_and_declares_safe_complete_outcomes()
    {
        var endpoint = Spec008ContractAssertions.Endpoint(
            "GET",
            "/api/public/context");

        Spec008ContractAssertions.IsAnonymous(endpoint);
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "StudentRegistration.Contracts.PublicContextDto");
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status500InternalServerError,
            "StudentRegistration.Contracts.ApiError");
        Spec008ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status503ServiceUnavailable,
            "StudentRegistration.Contracts.ApiError");

        Assert.DoesNotContain(
            Spec008ContractAssertions.Handler(endpoint).GetParameters(),
            parameter => parameter.ParameterType.Assembly.GetName().Name ==
                "StudentRegistration.Contracts" &&
                parameter.ParameterType.Name.EndsWith("Request", StringComparison.Ordinal));
    }
}
