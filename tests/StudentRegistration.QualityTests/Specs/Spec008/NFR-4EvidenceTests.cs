using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.Academics.Endpoints;
using StudentRegistration.Api.Composition;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec008;

public sealed class NFR_4EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-008-NFR-4.md";
    private const string TestPath =
        "tests/StudentRegistration.QualityTests/Specs/Spec008/NFR-4EvidenceTests.cs";
    private const string PoliciesPath =
        "src/StudentRegistration.IdentityAccess/Application/Authorization/RolePolicies.cs";
    private const string EndpointsPath =
        "src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs";
    private static readonly Guid StudentUserId =
        Guid.Parse("00000000-0000-0000-0000-000000008401");
    private static readonly Guid OtherStudentUserId =
        Guid.Parse("00000000-0000-0000-0000-000000008402");

    [Fact]
    public async Task Student_self_scope_allows_only_the_authenticated_owner()
    {
        await using var provider = AuthorizationProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var student = Principal(
            "synthetic-student-owner",
            RolePolicies.Student,
            StudentUserId,
            RolePolicies.AcademicProfileReadOwn);

        var ownResource = new SyntheticStudentResource(StudentUserId);
        var wrongResource = new SyntheticStudentResource(OtherStudentUserId);
        Assert.True((await authorization.AuthorizeAsync(
            student,
            ownResource,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            student,
            wrongResource,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);

        var ownRoute = new DefaultHttpContext();
        ownRoute.Request.Path = "/api/students/me/academic-context";
        Assert.True((await authorization.AuthorizeAsync(
            student,
            ownRoute,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);

        var unrelatedRoute = new DefaultHttpContext();
        unrelatedRoute.Request.Path = "/api/admin/students/not-a-resource/academic-context";
        Assert.False((await authorization.AuthorizeAsync(
            student,
            unrelatedRoute,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);
    }

    [Fact]
    public async Task Student_scope_denies_unauthenticated_and_missing_permission_claims()
    {
        await using var provider = AuthorizationProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var resource = new SyntheticStudentResource(StudentUserId);
        var unauthenticated = Principal(
            "synthetic-unauthenticated-student",
            RolePolicies.Student,
            StudentUserId,
            RolePolicies.AcademicProfileReadOwn,
            authenticated: false);
        var missingPermission = Principal(
            "synthetic-student-without-permission",
            RolePolicies.Student,
            StudentUserId);

        Assert.False((await authorization.AuthorizeAsync(
            unauthenticated,
            resource,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            missingPermission,
            resource,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);
    }

    [Fact]
    public async Task Only_named_permitted_admin_can_enter_full_profile_scope()
    {
        await using var provider = AuthorizationProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var permittedAdmin = Principal(
            "synthetic-approved-admin",
            RolePolicies.Admin,
            Guid.Parse("00000000-0000-0000-0000-000000008411"),
            RolePolicies.AcademicProfilesManage);
        var adminWithoutPermission = Principal(
            "synthetic-admin-without-profile-permission",
            RolePolicies.Admin,
            Guid.Parse("00000000-0000-0000-0000-000000008412"));
        var lecturer = Principal(
            "synthetic-lecturer",
            RolePolicies.Lecturer,
            Guid.Parse("00000000-0000-0000-0000-000000008413"),
            RolePolicies.ContextRead);
        var teachingAssistant = Principal(
            "synthetic-teaching-assistant",
            RolePolicies.TeachingAssistant,
            Guid.Parse("00000000-0000-0000-0000-000000008414"),
            RolePolicies.ContextRead);

        Assert.Contains(
            RolePolicies.AcademicProfilesManage,
            RolePolicies.PermissionsForRole(RolePolicies.Admin));
        Assert.Equal(
            [RolePolicies.ContextRead, RolePolicies.RegistrationApprovalDecideAssigned],
            RolePolicies.PermissionsForRole(RolePolicies.Lecturer));
        Assert.Equal(
            [RolePolicies.ContextRead, RolePolicies.RegistrationApprovalDecideAssigned],
            RolePolicies.PermissionsForRole(RolePolicies.TeachingAssistant));

        Assert.True((await authorization.AuthorizeAsync(
            permittedAdmin,
            resource: null,
            RolePolicies.AcademicProfilesManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            adminWithoutPermission,
            resource: null,
            RolePolicies.AcademicProfilesManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            lecturer,
            resource: null,
            RolePolicies.AcademicProfilesManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            teachingAssistant,
            resource: null,
            RolePolicies.AcademicProfilesManage)).Succeeded);
    }

    [Fact]
    public async Task Endpoint_metadata_keeps_self_and_admin_profile_policies_separate()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        await using var application = builder.Build();
        application.MapSpec008Endpoints();
        var endpoints = ((IEndpointRouteBuilder)application).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();

        AssertPolicy(
            endpoints,
            "/api/students/me/academic-context",
            HttpMethods.Get,
            RolePolicies.AcademicProfileReadOwn);
        AssertPolicy(
            endpoints,
            "/api/admin/students",
            HttpMethods.Get,
            RolePolicies.AcademicProfilesManage);
        AssertPolicy(
            endpoints,
            "/api/admin/students/{studentId}/academic-context",
            HttpMethods.Get,
            RolePolicies.AcademicProfilesManage);
        AssertPolicy(
            endpoints,
            "/api/admin/students/{studentId}/academic-profile",
            HttpMethods.Patch,
            RolePolicies.AcademicProfilesManage);
    }

    [Fact]
    public void Evidence_is_source_bound_aggregate_only_and_placeholder_free()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var testSource = RepositoryFiles.Read(TestPath);
        var policySource = RepositoryFiles.Read(PoliciesPath);
        var endpointSource = RepositoryFiles.Read(EndpointsPath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-008 NFR-4 Authorization and Privacy Evidence",
            "**Owner:** Ahmed ELbamby",
            "**Result: PASS.**",
            "Student own resource: ALLOW",
            "Student wrong resource: DENY",
            "Unauthenticated Student: DENY",
            "Student missing AcademicProfile.ReadOwn: DENY",
            "Named permitted Admin with AcademicProfiles.Manage: ALLOW",
            "Admin missing AcademicProfiles.Manage: DENY",
            "Lecturer with Context.Read: DENY",
            "TeachingAssistant with Context.Read: DENY",
            "/api/students/me/academic-context",
            "/api/admin/students/{studentId}/academic-context",
            "/api/admin/students/{studentId}/academic-profile",
            "synthetic-only fixtures",
            "approved minimum scope",
            "separately approved minimum roster projections",
            "aggregate authorization outcomes only",
            "no University IDs, credentials, cookies, or security stamps",
            "no full student profile fields or values",
            "5 passed",
            "0 failed",
            "dotnet test tests/StudentRegistration.QualityTests/StudentRegistration.QualityTests.csproj",
            $"Quality test normalized-LF SHA-256: `{NormalizedSourceHash(testSource)}`",
            $"RolePolicies normalized-LF SHA-256: `{NormalizedSourceHash(policySource)}`",
            $"Spec008Endpoints normalized-LF SHA-256: `{NormalizedSourceHash(endpointSource)}`");

        Assert.DoesNotMatch(
            @"(?i)\b(?:PENDING|TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
        Assert.DoesNotMatch(@"\{\{[^}]+\}\}", evidence);
        Assert.DoesNotMatch(@"(?i)<\s*insert\b[^>]*>", evidence);
        Assert.DoesNotMatch(@"\bAI26\d{5}\b", evidence);
        Assert.DoesNotMatch(@"\b(?:AI201|CC214)\b", evidence);
        Assert.DoesNotMatch(
            @"(?im)^\s*(?:Cookie|Set-Cookie)\s*:",
            evidence);
        Assert.DoesNotMatch(
            @"__Host-StudentRegistration\.Session\s*=",
            evidence);
        Assert.DoesNotMatch(
            @"(?im)\b(?:password|credential|security[_ -]?stamp|session[_ -]?cookie)\s*[:=]\s*\S+",
            evidence);
        Assert.DoesNotMatch(
            @"(?im)\b(?:GPA|earned credits|grade)\s*[:=]\s*[0-9A-Za-z.+-]+",
            evidence);
    }

    private static ServiceProvider AuthorizationProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityAuthorization();
        services.AddStudentRegistrationAcademicModule();
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = false
        });
    }

    private static ClaimsPrincipal Principal(
        string name,
        string role,
        Guid userId,
        string? permission = null,
        bool authenticated = true)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, name),
            new(ClaimTypes.NameIdentifier, userId.ToString("D")),
            new(ClaimTypes.Role, role)
        };
        if (permission is not null)
        {
            claims.Add(new Claim(RolePolicies.PermissionClaimType, permission));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            authenticated ? "SPEC008-NFR4-Synthetic" : null,
            ClaimTypes.Name,
            ClaimTypes.Role));
    }

    private static void AssertPolicy(
        IReadOnlyList<RouteEndpoint> endpoints,
        string route,
        string method,
        string expectedPolicy)
    {
        var endpoint = Assert.Single(endpoints, candidate =>
            string.Equals(candidate.RoutePattern.RawText, route, StringComparison.Ordinal) &&
            candidate.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(
                method,
                StringComparer.Ordinal) is true);
        var authorization = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        Assert.Contains(
            authorization,
            metadata => string.Equals(
                metadata.Policy,
                expectedPolicy,
                StringComparison.Ordinal));
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }

    private static string NormalizedSourceHash(string source)
    {
        var normalized = source.Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private sealed record SyntheticStudentResource(Guid StudentUserId)
        : IStudentOwnedResource;
}
