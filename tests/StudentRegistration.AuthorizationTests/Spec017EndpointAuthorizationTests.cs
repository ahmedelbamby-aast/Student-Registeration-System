using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.StaffAdministration.Endpoints;

namespace StudentRegistration.AuthorizationTests;

public sealed class Spec017EndpointAuthorizationTests
{
    [Fact]
    public async Task Every_spec017_endpoint_requires_its_admin_permission_and_named_limiter()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();
        services.AddAntiforgery();
        services.AddIdentityAuthorization();
        await using var provider = services.BuildServiceProvider();
        var routes = new TestEndpointRouteBuilder(provider);
        routes.MapSpec017Endpoints();
        var endpoints = routes.DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
        var expected = new[]
        {
            ("GET", "/api/admin/operations/metrics", RolePolicies.AdminOperationsMetricsRead, "AdminOperationsRead"),
            ("GET", "/api/admin/audit", RolePolicies.AdminAuditRead, "AdminOperationsRead"),
            ("POST", "/api/admin/exports", RolePolicies.AdminAuditExport, "AdminExportCreate"),
            ("GET", "/api/admin/exports/{jobId:guid}", RolePolicies.AdminAuditExport, "AdminOperationsRead"),
            ("GET", "/api/admin/exports/{jobId:guid}/download", RolePolicies.AdminAuditExport, "AdminExportDownload")
        };
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        foreach (var (method, route, permission, limiter) in expected)
        {
            var endpoint = Assert.Single(endpoints, candidate =>
                string.Equals(candidate.RoutePattern.RawText, route, StringComparison.Ordinal)
                && candidate.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods
                    .Contains(method, StringComparer.Ordinal) == true);
            var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            Assert.Contains(authorizeData, data => data.Policy == permission);
            var policy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData);
            Assert.NotNull(policy);
            Assert.True((await authorization.AuthorizeAsync(
                Principal(permission),
                resource: null,
                policy.Requirements)).Succeeded);
            Assert.False((await authorization.AuthorizeAsync(
                Principal(permission: null),
                resource: null,
                policy.Requirements)).Succeeded);
            Assert.Equal(
                limiter,
                endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName);
        }

        var create = Assert.Single(endpoints, endpoint =>
            endpoint.RoutePattern.RawText == "/api/admin/exports"
            && endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods
                .Contains("POST", StringComparer.Ordinal) == true);
        Assert.True(
            create.Metadata.GetMetadata<IAntiforgeryMetadata>()?.RequiresValidation == true);
    }

    private static ClaimsPrincipal Principal(string? permission)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, RolePolicies.Admin)
        };
        if (permission is not null)
        {
            claims.Add(new(RolePolicies.PermissionClaimType, permission));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            "spec017-test",
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
