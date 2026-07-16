using System.Reflection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec010;

internal static class Spec010ContractAssertions
{
    private const string EndpointTypeName =
        "StudentRegistration.Scheduling.Endpoints.Spec010Endpoints";
    private const string ContractNamespace =
        "StudentRegistration.Contracts.Scheduling";

    public static Type ContractType(string typeName)
    {
        var type = Assembly.Load("StudentRegistration.Contracts")
            .GetType($"{ContractNamespace}.{typeName}");
        Assert.True(type is not null, $"Missing approved DTO {ContractNamespace}.{typeName}.");
        return type!;
    }

    public static Type? OptionalContractType(string typeName) =>
        Assembly.Load("StudentRegistration.Contracts")
            .GetType($"{ContractNamespace}.{typeName}");

    public static void HasExactProperties(Type type, params string[] expected)
    {
        var actual = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal);
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual);
        Assert.Empty(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
    }

    public static void HasPropertyType(
        Type type,
        string propertyName,
        string expectedTypeName)
    {
        var property = type.GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(property);
        Assert.Equal(expectedTypeName, property!.PropertyType.FullName);
    }

    public static RouteEndpoint Endpoint(string method, string route) =>
        Assert.Single(
            Endpoints(),
            endpoint =>
                endpoint.RoutePattern.RawText == route
                && endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
                    .Contains(method, StringComparer.OrdinalIgnoreCase) == true);

    public static IReadOnlyList<RouteEndpoint> Endpoints()
    {
        var endpointType = Assembly.Load("StudentRegistration.Scheduling")
            .GetType(EndpointTypeName);
        Assert.True(endpointType is not null, $"Missing endpoint module {EndpointTypeName}.");
        var map = endpointType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
                candidate.Name == "MapSpec010Endpoints"
                && candidate.GetParameters().Length == 1
                && typeof(IEndpointRouteBuilder).IsAssignableFrom(
                    candidate.GetParameters()[0].ParameterType));
        Assert.True(map is not null, "MapSpec010Endpoints(IEndpointRouteBuilder) is required.");

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Services.AddRouting();
        builder.Services.AddAuthorization();
        builder.Services.AddAntiforgery();
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

    public static void RequiresAuthenticated(RouteEndpoint endpoint)
    {
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.NotEmpty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>());
    }

    public static void RequiresOfferingsManage(RouteEndpoint endpoint)
    {
        RequiresAuthenticated(endpoint);
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            metadata => metadata.Policy == "Offerings.Manage");
    }

    public static void RequiresOfferingDetailsRead(RouteEndpoint endpoint)
    {
        RequiresAuthenticated(endpoint);
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            metadata => metadata.Policy == "OfferingDetailsRead");
    }

    public static void RequiresAntiforgery(RouteEndpoint endpoint) =>
        Assert.True(
            endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>()?.RequiresValidation == true,
            $"{endpoint.RoutePattern.RawText} must validate antiforgery tokens.");

    public static void DeclaresResponse(
        RouteEndpoint endpoint,
        int statusCode,
        string? responseTypeName = null) =>
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>(),
            metadata =>
                metadata.StatusCode == statusCode
                && (responseTypeName is null || metadata.Type?.FullName == responseTypeName));

    public static void DeclaresPage(
        RouteEndpoint endpoint,
        int statusCode,
        string itemTypeName) =>
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>(),
            metadata =>
                metadata.StatusCode == statusCode
                && metadata.Type is { IsGenericType: true }
                && metadata.Type.GetGenericTypeDefinition().FullName ==
                    "StudentRegistration.Contracts.Page`1"
                && metadata.Type.GetGenericArguments()[0].FullName ==
                    $"{ContractNamespace}.{itemTypeName}");

    public static void DeclaresStandardErrors(
        RouteEndpoint endpoint,
        bool includesNotFound,
        bool includesConflict)
    {
        foreach (var status in new[]
                 {
                     StatusCodes.Status400BadRequest,
                     StatusCodes.Status401Unauthorized,
                     StatusCodes.Status403Forbidden,
                     StatusCodes.Status500InternalServerError,
                     StatusCodes.Status503ServiceUnavailable,
                 })
        {
            DeclaresResponse(
                endpoint,
                status,
                "StudentRegistration.Contracts.ApiError");
        }

        if (includesNotFound)
        {
            DeclaresResponse(
                endpoint,
                StatusCodes.Status404NotFound,
                "StudentRegistration.Contracts.ApiError");
        }

        if (includesConflict)
        {
            DeclaresResponse(
                endpoint,
                StatusCodes.Status409Conflict,
                "StudentRegistration.Contracts.ApiError");
        }
    }

    public static void HandlerAccepts(RouteEndpoint endpoint, string typeName)
    {
        var handler = endpoint.Metadata.GetMetadata<MethodInfo>();
        Assert.NotNull(handler);
        Assert.Contains(
            handler!.GetParameters(),
            parameter => parameter.ParameterType.FullName ==
                $"{ContractNamespace}.{typeName}");
    }

    public static void ContractContains(params string[] fragments) =>
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "specs/010-offerings-groups-resources/contracts/api.md"),
            fragments);
}
