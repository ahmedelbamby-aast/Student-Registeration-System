using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec012;

public sealed class Endpoint01ContractTests
{
    [Fact]
    public void Plan_response_is_exact_fixed_18_and_server_explained()
    {
        var plan = Spec012ContractAssertions.ContractType("RegistrationPlanDto");
        Spec012ContractAssertions.HasExactProperties(
            plan,
            "Id", "TermId", "RowVersion", "SelectedGroups", "TotalCredits",
            "DefaultTargetCredits", "MaximumAllowedCredits", "LoadReasons",
            "SelectionIssues", "Conflicts", "Validation", "ReviewBlocked");
        Spec012ContractAssertions.HasPropertyType<decimal>(
            plan,
            "DefaultTargetCredits");
        Spec012ContractAssertions.HasPropertyType<decimal>(
            plan,
            "MaximumAllowedCredits");
        Spec012ContractAssertions.HasCollectionItemType(
            plan,
            "LoadReasons",
            "LoadPolicyReasonDto");
        Spec012ContractAssertions.ExcludesProperties(
            plan,
            "StudentId", "PlanOwnerId", "Gpa", "OverloadAllowed",
            "CapacityReservation");

        Spec012ContractAssertions.HasExactProperties(
            Spec012ContractAssertions.ContractType("LoadPolicyReasonDto"),
            "Code", "Blocking", "Message", "RequiredValue", "CurrentValue",
            "PolicySetId", "PolicyVersion", "SourceReference");
        Spec012ContractAssertions.ContractContains(
            "`spec012-credit-load/1.0`",
            "`defaultTargetCredits=18`",
            "`maximumAllowedCredits=18`",
            "server-authored `loadReasons`",
            "policy/source provenance",
            "No browser GPA calculation or overload path");
    }

    [Fact]
    public void Get_is_student_term_scoped_private_read_with_no_seat_effect()
    {
        var endpoint = Spec012ContractAssertions.Endpoint(
            "GET",
            "/api/student/terms/{termId}/registration-plan");

        Spec012ContractAssertions.RequiresStudent(endpoint);
        Spec012ContractAssertions.HasNoClientOwnedIdentifier(endpoint);
        Spec012ContractAssertions.HasNoRequestBody(endpoint);
        Spec012ContractAssertions.DeclaresResponse(
            endpoint,
            StatusCodes.Status200OK,
            "RegistrationPlanDto");
        Spec012ContractAssertions.DeclaresApiErrors(
            endpoint,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status500InternalServerError);
        Spec012ContractAssertions.ContractContains(
            "The server derives the Student identifier",
            "No route or request accepts a Student identifier or client-owned plan identifier",
            "A current empty plan is a normal success",
            "`totalCredits=0`",
            "fixed 18/18 credit fields and sourced load reasons",
            "The read has no durable mutation and no seat-allocation side effect");
    }
}

internal static class Spec012ContractAssertions
{
    private const string ContractNamespace =
        "StudentRegistration.Contracts.Registration";
    private const string EndpointTypeName =
        "StudentRegistration.Registration.Endpoints.Spec012Endpoints";

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

    public static void HasPropertyType<T>(Type type, string name)
    {
        var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(property);
        Assert.Equal(typeof(T), property.PropertyType);
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
            argument => argument.FullName == $"{ContractNamespace}.{itemTypeName}");
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

    public static void HasNoRequestBody(RouteEndpoint endpoint) =>
        Assert.DoesNotContain(
            Handler(endpoint).GetParameters(),
            parameter => parameter.ParameterType.Namespace == ContractNamespace
                && parameter.ParameterType.Name.EndsWith("Request", StringComparison.Ordinal));

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
                "specs/012-schedule-builder-conflicts/contracts/api.md"),
            @"\s+",
            " ");
        RepositoryFiles.ContainsAll(normalized, fragments);
    }

    private static IReadOnlyList<RouteEndpoint> Endpoints()
    {
        var endpointType = Assembly.Load("StudentRegistration.Registration")
            .GetType(EndpointTypeName);
        Assert.True(endpointType is not null, $"Missing endpoint module {EndpointTypeName}.");
        var map = endpointType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(method => method.Name == "MapSpec012Endpoints"
                && method.GetParameters().Length == 1
                && typeof(IEndpointRouteBuilder).IsAssignableFrom(
                    method.GetParameters()[0].ParameterType));
        Assert.True(map is not null, "MapSpec012Endpoints(IEndpointRouteBuilder) is required.");

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
