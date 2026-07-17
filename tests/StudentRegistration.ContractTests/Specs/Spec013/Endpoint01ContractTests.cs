using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec013;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Recommendation_transport_is_explicit_versioned_and_has_no_aggregate_score()
    {
        Spec013ContractAssertions.HasExactProperties(
            Spec013ContractAssertions.ContractType("MeetingIntervalDto"),
            "DayOfWeek", "StartLocal", "EndLocal");
        Spec013ContractAssertions.HasExactProperties(
            Spec013ContractAssertions.ContractType("SchedulePreferencesDto"),
            "AvoidedWeekdays", "EarliestPreferredStartLocal",
            "LatestPreferredEndLocal");
        Spec013ContractAssertions.HasExactProperties(
            Spec013ContractAssertions.ContractType("ScheduleScoreComponentDto"),
            "Factor", "Value", "Message");
        var option = Spec013ContractAssertions.ContractType("ScheduleOptionDto");
        Spec013ContractAssertions.HasExactProperties(
            option,
            "OptionToken", "Rank", "Groups", "ScoreExplanation");
        Spec013ContractAssertions.ExcludesProperties(
            option,
            "Score", "Weight", "StudentId", "PlanId");
        Spec013ContractAssertions.HasCollectionItemType(
            option,
            "Groups",
            "StudentRegistration.Contracts.Scheduling.GroupDto");

        var result = Spec013ContractAssertions.ContractType(
            "OptimizationResultDto");
        Spec013ContractAssertions.HasExactProperties(
            result,
            "RequestCorrelationId", "PlanId", "PlanRowVersion",
            "AcademicContextVersion", "CatalogueVersion", "PolicySetId",
            "PolicyVersion", "OfferingVersions", "GroupVersions",
            "OptimizerConfigurationVersion", "Status", "Options",
            "Diagnostics", "EvaluatedAtUtc");
        Spec013ContractAssertions.HasDictionaryProperty(
            result,
            "OfferingVersions");
        Spec013ContractAssertions.HasDictionaryProperty(
            result,
            "GroupVersions");
        Spec013ContractAssertions.ExcludesProperties(
            result,
            "StudentId", "GroupVersionSetHash", "Score");
    }

    [Fact]
    public void Post_is_student_self_scoped_and_declares_every_finalized_outcome()
    {
        Spec013ContractAssertions.HasExactProperties(
            Spec013ContractAssertions.ContractType("RecommendScheduleRequest"),
            "ExpectedPlanRowVersion", "RequestCorrelationId", "Preferences");

        var endpoint = Spec013ContractAssertions.Endpoint(
            "POST",
            "/api/student/terms/{termId}/registration-plan/recommendations");
        Spec013ContractAssertions.RequiresStudent(endpoint);
        Spec013ContractAssertions.HasNoClientOwnedIdentifier(endpoint);
        Spec013ContractAssertions.HasRequestBody(
            endpoint,
            "RecommendScheduleRequest");
        Spec013ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "OptimizationResultDto");
        Spec013ContractAssertions.DeclaresApiErrors(
            endpoint,
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status409Conflict,
            StatusCodes.Status429TooManyRequests,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status500InternalServerError);

        Spec013ContractAssertions.ContractContains(
            "no student/plan identifier is accepted",
            "`200 OptimizationResultDto`",
            "`400 VALIDATION_ERROR`",
            "`404 REGISTRATION_CONTEXT_NOT_FOUND`",
            "`409 PLAN_CHANGED`",
            "`409 STALE_INPUT`",
            "`429 RATE_LIMITED`",
            "`503 RECOMMENDATIONS_UNAVAILABLE`",
            "`500 INTERNAL_ERROR`",
            "time-budget response never labels an incomplete option valid");
    }
}

internal static class Spec013ContractAssertions
{
    private const string ContractNamespace =
        "StudentRegistration.Contracts.Registration";
    private const string EndpointTypeName =
        "StudentRegistration.Registration.Endpoints.Spec013Endpoints";

    public static Type ContractType(string name)
    {
        var type = Assembly.Load("StudentRegistration.Contracts")
            .GetType($"{ContractNamespace}.{name}");
        Assert.True(type is not null, $"Missing approved DTO {ContractNamespace}.{name}.");
        return type!;
    }

    public static void HasExactProperties(Type type, params string[] expected)
    {
        var actual = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal);
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual);
        Assert.Empty(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
    }

    public static void ExcludesProperties(Type type, params string[] names)
    {
        foreach (var name in names)
        {
            Assert.Null(type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public));
        }
    }

    public static void HasCollectionItemType(
        Type type,
        string propertyName,
        string itemTypeName)
    {
        var property = type.GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(property);
        Assert.True(property.PropertyType.IsGenericType);
        Assert.Contains(
            property.PropertyType.GetGenericArguments(),
            argument => argument.FullName == itemTypeName);
    }

    public static void HasDictionaryProperty(Type type, string propertyName)
    {
        var property = type.GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(property);
        Assert.True(property.PropertyType.IsGenericType);
        Assert.Equal(
            typeof(IReadOnlyDictionary<,>),
            property.PropertyType.GetGenericTypeDefinition());
        Assert.Equal(
            [typeof(string), typeof(string)],
            property.PropertyType.GetGenericArguments());
    }

    public static RouteEndpoint Endpoint(string method, string route) =>
        Assert.Single(
            Endpoints(),
            endpoint => endpoint.RoutePattern.RawText == route
                && endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
                    .Contains(method, StringComparer.OrdinalIgnoreCase) == true);

    public static void RequiresStudent(RouteEndpoint endpoint)
    {
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == "Student");
    }

    public static void HasNoClientOwnedIdentifier(RouteEndpoint endpoint) =>
        Assert.DoesNotContain(
            Handler(endpoint).GetParameters(),
            parameter => parameter.Name is not null
                && (parameter.Name.Equals("studentId", StringComparison.OrdinalIgnoreCase)
                    || parameter.Name.Equals("planId", StringComparison.OrdinalIgnoreCase)));

    public static void HasRequestBody(RouteEndpoint endpoint, string requestTypeName) =>
        Assert.Contains(
            Handler(endpoint).GetParameters(),
            parameter => parameter.ParameterType.FullName ==
                $"{ContractNamespace}.{requestTypeName}");

    public static void DeclaresResponse(
        RouteEndpoint endpoint,
        int status,
        string responseTypeName) =>
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>(),
            metadata => metadata.StatusCode == status
                && metadata.Type?.FullName == $"{ContractNamespace}.{responseTypeName}");

    public static void DeclaresApiErrors(RouteEndpoint endpoint, params int[] statuses)
    {
        foreach (var status in statuses)
        {
            Assert.Contains(
                endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>(),
                metadata => metadata.StatusCode == status
                    && metadata.Type?.FullName == "StudentRegistration.Contracts.ApiError");
        }
    }

    public static void ContractContains(params string[] fragments)
    {
        var normalized = Regex.Replace(
            RepositoryFiles.Read(
                "specs/013-schedule-recommendations/contracts/api.md"),
            @"\s+",
            " ");
        RepositoryFiles.ContainsAll(normalized, fragments);
    }

    private static IReadOnlyList<RouteEndpoint> Endpoints()
    {
        var endpointType = Assembly.Load("StudentRegistration.Registration")
            .GetType(EndpointTypeName);
        Assert.True(
            endpointType is not null,
            $"Missing future endpoint module {EndpointTypeName}.");
        var map = endpointType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(method => method.Name == "MapSpec013Endpoints"
                && method.GetParameters().Length == 1
                && typeof(IEndpointRouteBuilder).IsAssignableFrom(
                    method.GetParameters()[0].ParameterType));
        Assert.True(map is not null, "MapSpec013Endpoints(IEndpointRouteBuilder) is required.");

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Services.AddRouting();
        builder.Services.AddAuthorization();
        var app = builder.Build();
        try
        {
            map!.Invoke(null, [app]);
            return ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(source => source.Endpoints)
                .OfType<RouteEndpoint>()
                .ToArray();
        }
        finally
        {
            app.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    private static MethodInfo Handler(RouteEndpoint endpoint)
    {
        var method = endpoint.Metadata.GetMetadata<MethodInfo>();
        Assert.NotNull(method);
        return method!;
    }
}
