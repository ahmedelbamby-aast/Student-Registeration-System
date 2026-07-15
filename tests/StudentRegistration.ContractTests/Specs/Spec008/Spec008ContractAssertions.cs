using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Specs.Spec008;

internal static class Spec008ContractAssertions
{
    private const string EndpointTypeName =
        "StudentRegistration.Academics.Endpoints.Spec008Endpoints";

    private const string AcademicContractNamespace =
        "StudentRegistration.Contracts.Academics";

    public static Type SharedContractType(string typeName) =>
        ContractType("StudentRegistration.Contracts", typeName);

    public static Type AcademicContractType(string typeName) =>
        ContractType(AcademicContractNamespace, typeName);

    public static void HasExactProperties(Type type, params string[] expectedProperties)
    {
        var actual = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expected = expectedProperties.Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(expected, actual);
        Assert.Empty(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
    }

    public static void ExcludesProperties(Type type, params string[] forbiddenProperties)
    {
        var names = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .ToArray();

        Assert.All(forbiddenProperties, name => Assert.DoesNotContain(name, names));
    }

    public static PropertyInfo Property(Type type, string propertyName)
    {
        var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.True(
            property is not null,
            $"{type.FullName} must expose {propertyName}.");
        return property!;
    }

    public static Type PageItemType(Type owner, string propertyName)
    {
        var pageType = Property(owner, propertyName).PropertyType;
        Assert.True(
            pageType.IsGenericType &&
            pageType.GetGenericTypeDefinition().FullName ==
                "StudentRegistration.Contracts.Page`1",
            $"{owner.Name}.{propertyName} must use the canonical Page<T> contract.");
        return pageType.GetGenericArguments()[0];
    }

    public static RouteEndpoint Endpoint(string method, string route)
    {
        var assembly = Assembly.Load("StudentRegistration.Academics");
        var endpointType = assembly.GetType(EndpointTypeName);
        Assert.True(
            endpointType is not null,
            $"The SPEC008 endpoint module {EndpointTypeName} has not been delivered.");

        var mapMethod = endpointType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
                candidate.Name == "MapSpec008Endpoints" &&
                candidate.GetParameters().Length == 1 &&
                typeof(IEndpointRouteBuilder).IsAssignableFrom(
                    candidate.GetParameters()[0].ParameterType));
        Assert.True(
            mapMethod is not null,
            "MapSpec008Endpoints(IEndpointRouteBuilder) is required for module composition.");

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Services.AddRouting();
        builder.Services.AddAuthorization();
        var app = builder.Build();

        try
        {
            mapMethod!.Invoke(null, [app]);
            var matches = ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(source => source.Endpoints)
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

    public static void IsAnonymous(RouteEndpoint endpoint)
    {
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Empty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>());
    }

    public static void RequiresPolicy(RouteEndpoint endpoint, string policy)
    {
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == policy);
    }

    public static void DeclaresResponse(
        RouteEndpoint endpoint,
        int statusCode,
        string? responseTypeName = null)
    {
        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>(),
            metadata =>
                metadata.StatusCode == statusCode &&
                (responseTypeName is null || metadata.Type?.FullName == responseTypeName));
    }

    public static MethodInfo Handler(RouteEndpoint endpoint)
    {
        var handler = endpoint.Metadata.GetMetadata<MethodInfo>();
        Assert.True(
            handler is not null,
            $"{endpoint.RoutePattern.RawText} must retain handler method metadata.");
        return handler!;
    }

    public static string ApiContract() => RepositoryFiles.Read(
        "specs/008-academic-term-student-profile/contracts/api.md");

    public static string Requirements() => RepositoryFiles.Read(
        "specs/008-academic-term-student-profile/requirements.md");

    private static Type ContractType(string contractNamespace, string typeName)
    {
        var assembly = Assembly.Load("StudentRegistration.Contracts");
        var fullName = $"{contractNamespace}.{typeName}";
        var type = assembly.GetType(fullName);

        Assert.True(
            type is not null,
            $"The approved DTO {fullName} has not been delivered.");
        return type!;
    }
}
