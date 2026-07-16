using System.Reflection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec009;

internal static class Spec009ContractAssertions
{
    private const string EndpointTypeName =
        "StudentRegistration.Academics.Endpoints.Spec009Endpoints";
    private const string ContractNamespace =
        "StudentRegistration.Contracts.Academics";

    public static Type ContractType(string typeName)
    {
        var type = Assembly.Load("StudentRegistration.Contracts")
            .GetType($"{ContractNamespace}.{typeName}");
        Assert.True(type is not null, $"Missing approved DTO {ContractNamespace}.{typeName}.");
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

    public static void Excludes(Type type, params string[] forbidden)
    {
        var properties = type.GetProperties().Select(property => property.Name).ToArray();
        Assert.All(forbidden, property => Assert.DoesNotContain(property, properties));
    }

    public static RouteEndpoint Endpoint(string method, string route)
    {
        var endpointType = Assembly.Load("StudentRegistration.Academics")
            .GetType(EndpointTypeName);
        Assert.True(endpointType is not null, $"Missing endpoint module {EndpointTypeName}.");
        var map = endpointType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
                candidate.Name == "MapSpec009Endpoints"
                && candidate.GetParameters().Length == 1
                && typeof(IEndpointRouteBuilder).IsAssignableFrom(
                    candidate.GetParameters()[0].ParameterType));
        Assert.True(map is not null, "MapSpec009Endpoints(IEndpointRouteBuilder) is required.");

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Services.AddRouting();
        builder.Services.AddAuthorization();
        var app = builder.Build();
        try
        {
            map!.Invoke(null, [app]);
            return Assert.Single(
                ((IEndpointRouteBuilder)app).DataSources
                    .SelectMany(source => source.Endpoints)
                    .OfType<RouteEndpoint>(),
                endpoint =>
                    endpoint.RoutePattern.RawText == route
                    && endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
                        .Contains(method, StringComparer.OrdinalIgnoreCase) == true);
        }
        finally
        {
            app.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    public static void RequiresPolicy(RouteEndpoint endpoint)
    {
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            metadata => metadata.Policy == "CataloguePolicy.Manage");
    }

    public static void RequiresAntiforgery(RouteEndpoint endpoint) =>
        Assert.True(
            endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>()?.RequiresValidation == true);

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
            DeclaresResponse(endpoint, status);
        }

        if (includesNotFound)
        {
            DeclaresResponse(endpoint, StatusCodes.Status404NotFound);
        }

        if (includesConflict)
        {
            DeclaresResponse(endpoint, StatusCodes.Status409Conflict);
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
                "specs/009-catalog-prerequisites-policy-admin/contracts/api.md"),
            fragments);
}
