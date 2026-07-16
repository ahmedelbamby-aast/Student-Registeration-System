using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec011;

internal static class Spec011ContractAssertions
{
    private const string EndpointTypeName =
        "StudentRegistration.Registration.Endpoints.Spec011Endpoints";
    private const string ContractNamespace =
        "StudentRegistration.Contracts.Registration";

    public static Type ContractType(string typeName)
    {
        var type = Assembly.Load("StudentRegistration.Contracts")
            .GetType($"{ContractNamespace}.{typeName}");
        Assert.True(
            type is not null,
            $"Missing approved DTO {ContractNamespace}.{typeName}.");
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
        var endpointType = Assembly.Load("StudentRegistration.Registration")
            .GetType(EndpointTypeName);
        Assert.True(
            endpointType is not null,
            $"Missing endpoint module {EndpointTypeName}.");
        var map = endpointType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
                candidate.Name == "MapSpec011Endpoints"
                && candidate.GetParameters().Length == 1
                && typeof(IEndpointRouteBuilder).IsAssignableFrom(
                    candidate.GetParameters()[0].ParameterType));
        Assert.True(
            map is not null,
            "MapSpec011Endpoints(IEndpointRouteBuilder) is required.");

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

    public static void RequiresCatalogueReadAvailable(RouteEndpoint endpoint)
    {
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            metadata => metadata.Policy == "Catalogue.ReadAvailable");
    }

    public static void DeclaresResponse(
        RouteEndpoint endpoint,
        int statusCode,
        string? responseTypeName = null) =>
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>(),
            metadata =>
                metadata.StatusCode == statusCode
                && (responseTypeName is null
                    || metadata.Type?.FullName == responseTypeName));

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

    public static void DeclaresErrors(
        RouteEndpoint endpoint,
        params int[] statuses)
    {
        foreach (var status in statuses)
        {
            DeclaresResponse(
                endpoint,
                status,
                "StudentRegistration.Contracts.ApiError");
        }
    }

    public static MethodInfo Handler(RouteEndpoint endpoint)
    {
        var handler = endpoint.Metadata.GetMetadata<MethodInfo>();
        Assert.NotNull(handler);
        return handler!;
    }

    public static void HasNoClientStudentIdentifier(RouteEndpoint endpoint)
    {
        var handler = Handler(endpoint);
        Assert.DoesNotContain(
            handler.GetParameters(),
            parameter =>
                parameter.Name?.Contains(
                    "student",
                    StringComparison.OrdinalIgnoreCase) == true);
    }

    public static void HasQueryParameters(
        RouteEndpoint endpoint,
        params string[] expected)
    {
        var handler = Handler(endpoint);
        var actual = handler.GetParameters()
            .Where(parameter =>
                parameter.GetCustomAttributes()
                    .Any(attribute =>
                        attribute.GetType().FullName ==
                        "Microsoft.AspNetCore.Mvc.FromQueryAttribute"))
            .Select(parameter => parameter.Name)
            .Where(name => name is not null)
            .Cast<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual);
    }

    public static void DeclaresOutcomeHeaders(
        RouteEndpoint endpoint,
        params string[] expected)
    {
        var metadata = endpoint.Metadata.SingleOrDefault(item =>
            item.GetType().FullName ==
            "StudentRegistration.Registration.Endpoints.DiscoveryOutcomeHeadersMetadata");
        Assert.NotNull(metadata);
        var property = metadata!.GetType().GetProperty("HeaderNames");
        Assert.NotNull(property);
        var actual = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            property!.GetValue(metadata));
        Assert.Equal(expected.Order(StringComparer.Ordinal), actual.Order(StringComparer.Ordinal));
    }

    public static void ContractContains(params string[] fragments)
    {
        var contract = Regex.Replace(
            RepositoryFiles.Read(
                "specs/011-eligibility-subject-discovery/contracts/api.md"),
            @"\s+",
            " ");
        RepositoryFiles.ContainsAll(contract, fragments);
    }
}
