global using StudentRegistration.TestSupport;

using System.Reflection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace StudentRegistration.ContractTests.Specs.Spec007;

internal static class Spec007ContractAssertions
{
    private const string EndpointTypeName =
        "StudentRegistration.IdentityAccess.Endpoints.Spec007Endpoints";
    private const string EndpointSourcePath =
        "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs";
    private const string ContractNamespace =
        "StudentRegistration.Contracts.Identity";

    public static Type ContractType(string typeName)
    {
        var assembly = Assembly.Load("StudentRegistration.Contracts");
        var type = assembly.GetType($"{ContractNamespace}.{typeName}");

        Assert.True(
            type is not null,
            $"The shared identity DTO {ContractNamespace}.{typeName} has not been delivered.");
        return type!;
    }

    public static void HasExactProperties(Type type, params string[] expectedProperties)
    {
        var actualProperties = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expected = expectedProperties
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actualProperties);
        Assert.Empty(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
    }

    public static void ExcludesProperties(Type type, params string[] forbiddenProperties)
    {
        var names = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .ToArray();

        Assert.All(forbiddenProperties, forbidden => Assert.DoesNotContain(forbidden, names));
    }

    public static RouteEndpoint Endpoint(string method, string route)
    {
        var assembly = Assembly.Load("StudentRegistration.IdentityAccess");
        var endpointType = assembly.GetType(EndpointTypeName);
        Assert.True(
            endpointType is not null,
            $"The SPEC007 endpoint module {EndpointTypeName} has not been delivered.");

        var mapMethod = endpointType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
                candidate.Name == "MapSpec007Endpoints" &&
                candidate.GetParameters().Length == 1 &&
                typeof(IEndpointRouteBuilder).IsAssignableFrom(
                    candidate.GetParameters()[0].ParameterType));
        Assert.True(
            mapMethod is not null,
            "MapSpec007Endpoints(IEndpointRouteBuilder) is required for module composition.");

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Services.AddRouting();
        builder.Services.AddAntiforgery();
        builder.Services.AddAuthorization();
        builder.Services.AddRateLimiter(_ => { });
        var app = builder.Build();

        try
        {
            mapMethod!.Invoke(null, [app]);
            var matches = ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .OfType<RouteEndpoint>()
                .Where(endpoint =>
                    string.Equals(endpoint.RoutePattern.RawText, route, StringComparison.Ordinal) &&
                    endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods
                        .Contains(method, StringComparer.OrdinalIgnoreCase) == true)
                .ToArray();

            Assert.True(
                matches.Length == 1,
                $"Expected exactly one {method} {route} endpoint, found {matches.Length}.");
            return matches[0];
        }
        finally
        {
            app.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    public static void RequiresAntiforgery(RouteEndpoint endpoint)
    {
        var metadata = endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>();
        Assert.True(
            metadata?.RequiresValidation == true,
            $"{endpoint.RoutePattern.RawText} must validate antiforgery tokens.");
    }

    public static void RequiresNamedRateLimit(RouteEndpoint endpoint)
    {
        var metadata = endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>();
        Assert.True(
            !string.IsNullOrWhiteSpace(metadata?.PolicyName),
            $"{endpoint.RoutePattern.RawText} must use a named shared rate-limit policy.");
    }

    public static void IsAnonymous(RouteEndpoint endpoint)
    {
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Empty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>());
    }

    public static void IsProtected(RouteEndpoint endpoint)
    {
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.NotEmpty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>());
    }

    public static void DeclaresResponseStatus(RouteEndpoint endpoint, int statusCode)
    {
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>(),
            metadata => metadata.StatusCode == statusCode);
    }

    public static string EndpointSource() => RepositoryFiles.Read(EndpointSourcePath);

    public static string ApiContract() => RepositoryFiles.Read(
        "specs/007-identity-account-lifecycle/contracts/api.md");

    public static string Requirements() => RepositoryFiles.Read(
        "specs/007-identity-account-lifecycle/requirements.md");
}
