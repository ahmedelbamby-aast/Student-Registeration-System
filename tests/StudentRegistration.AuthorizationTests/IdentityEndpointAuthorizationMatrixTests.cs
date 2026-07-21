using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.IdentityAccess.Endpoints;

namespace StudentRegistration.AuthorizationTests;

public sealed class IdentityEndpointAuthorizationMatrixTests
{
    private static readonly (string Method, string Route)[] ProtectedEndpoints =
    [
        ("POST", "/api/auth/logout"),
        ("POST", "/api/auth/password/change"),
        ("POST", "/api/auth/sessions/revoke-all"),
        ("GET", "/api/auth/session"),
        ("GET", "/api/admin/users"),
        ("POST", "/api/admin/users/imports"),
        ("GET", "/api/admin/users/imports/{importId}"),
        ("POST", "/api/admin/users/imports/{importId}/publish"),
        ("PATCH", "/api/admin/users/{userId}/status"),
        ("PUT", "/api/admin/users/{userId}/roles")
    ];

    [Fact]
    public async Task Every_protected_endpoint_executes_its_positive_and_negative_policy_pair()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();
        services.AddIdentityAuthorization();
        await using var provider = services.BuildServiceProvider();
        var routes = new TestEndpointRouteBuilder(provider);
        routes.MapSpec007Endpoints();
        var endpoints = routes.DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        foreach (var (method, route) in ProtectedEndpoints)
        {
            var endpoint = Assert.Single(endpoints, candidate =>
                string.Equals(
                    $"/{candidate.RoutePattern.RawText?.TrimStart('/')}",
                    route,
                    StringComparison.Ordinal)
                && candidate.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods
                    .Contains(method, StringComparer.Ordinal) == true);
            var authorizeData = endpoint.Metadata
                .GetOrderedMetadata<IAuthorizeData>();
            Assert.NotEmpty(authorizeData);
            var policy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData);
            Assert.NotNull(policy);
            var (positive, negative) = PrincipalsFor(authorizeData);

            Assert.True(
                (await authorization.AuthorizeAsync(
                    positive,
                    resource: null,
                    policy.Requirements)).Succeeded,
                $"Expected the positive principal to access {method} {route}.");
            Assert.False(
                (await authorization.AuthorizeAsync(
                    negative,
                    resource: null,
                    policy.Requirements)).Succeeded,
                $"Expected the negative principal to be denied for {method} {route}.");
        }

        Assert.Equal(10, ProtectedEndpoints.Length);
    }

    private static (ClaimsPrincipal Positive, ClaimsPrincipal Negative) PrincipalsFor(
        IReadOnlyList<IAuthorizeData> authorizeData)
    {
        if (authorizeData.Any(data => string.Equals(
                data.Policy,
                RolePolicies.IdentityManagement,
                StringComparison.Ordinal)))
        {
            return (
                Principal(
                    [
                        new Claim(ClaimTypes.Role, RolePolicies.Admin),
                        new Claim(
                            RolePolicies.PermissionClaimType,
                            RolePolicies.IdentityAccessManage)
                    ]),
                Principal(
                    [
                        new Claim(ClaimTypes.Role, RolePolicies.Lecturer),
                        new Claim(
                            RolePolicies.PermissionClaimType,
                            RolePolicies.IdentityAccessManage)
                    ]));
        }

        if (authorizeData.Any(data => string.Equals(
                data.Policy,
                RolePolicies.StaffContext,
                StringComparison.Ordinal)))
        {
            return (
                Principal(
                    [new Claim(
                        RolePolicies.AvailableRoleClaimType,
                        RolePolicies.Lecturer)]),
                Principal([new Claim(ClaimTypes.Role, RolePolicies.Student)]));
        }

        return (
            Principal([new Claim(ClaimTypes.Role, RolePolicies.Student)]),
            new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private static ClaimsPrincipal Principal(IEnumerable<Claim> claims)
    {
        var allClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        allClaims.AddRange(claims);
        return new ClaimsPrincipal(new ClaimsIdentity(
            allClaims,
            "spec007-matrix",
            ClaimTypes.Name,
            ClaimTypes.Role));
    }

    private sealed class TestEndpointRouteBuilder(IServiceProvider serviceProvider)
        : IEndpointRouteBuilder
    {
        public IServiceProvider ServiceProvider { get; } = serviceProvider;

        public ICollection<EndpointDataSource> DataSources { get; } =
            new List<EndpointDataSource>();

        public IApplicationBuilder CreateApplicationBuilder() =>
            new ApplicationBuilder(ServiceProvider);
    }
}
