using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.IdentityAccess.Application.Authorization;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AuthorizationTests;

public sealed class IdentityResourceScopeTests
{
    [Fact]
    public async Task Executable_policy_matrix_requires_server_role_permission_and_matching_resource()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityAuthorization();
        await using var provider = services.BuildServiceProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var userId = Guid.NewGuid();

        var authorizedAdmin = Principal(
            userId,
            RolePolicies.Admin,
            includeIdentityPermission: true);
        var adminWithoutPermission = Principal(userId, RolePolicies.Admin);
        var lecturerWithPermission = Principal(
            userId,
            RolePolicies.Lecturer,
            includeIdentityPermission: true);
        Assert.True((await authorization.AuthorizeAsync(
            authorizedAdmin,
            null,
            RolePolicies.IdentityManagement)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            adminWithoutPermission,
            null,
            RolePolicies.IdentityManagement)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            lecturerWithPermission,
            null,
            RolePolicies.IdentityManagement)).Succeeded);

        var student = Principal(userId, RolePolicies.Student);
        Assert.True((await authorization.AuthorizeAsync(
            student,
            new StudentResource(userId),
            RolePolicies.OwnStudentResource)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            student,
            new StudentResource(Guid.NewGuid()),
            RolePolicies.OwnStudentResource)).Succeeded);

        var teachingResource = new TeachingResource(userId);
        Assert.True((await authorization.AuthorizeAsync(
            Principal(userId, RolePolicies.Lecturer),
            teachingResource,
            RolePolicies.AssignedTeachingResource)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(Guid.NewGuid(), RolePolicies.TeachingAssistant),
            teachingResource,
            RolePolicies.AssignedTeachingResource)).Succeeded);
    }

    [Fact]
    public void Role_policies_define_positive_negative_and_resource_scope_boundaries()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/Authorization/RolePolicies.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public static class RolePolicies",
            "Student",
            "Admin",
            "Lecturer",
            "TeachingAssistant",
            "IdentityManagement",
            "OwnStudentResource",
            "AssignedTeachingResource",
            "RequireAuthenticatedUser",
            "RequireRole");
        Assert.DoesNotContain("route", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("client", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Identity_management_permission_is_issued_only_from_the_effective_admin_role()
    {
        var endpoints = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Endpoints/Spec007Endpoints.cs");

        RepositoryFiles.ContainsAll(
            endpoints,
            "result.AuthorizedRoles.Contains(RolePolicies.Admin",
            "RolePolicies.PermissionClaimType",
            "RolePolicies.IdentityManagement");
        Assert.DoesNotContain("request.Permission", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("request.RoleClaims", endpoints, StringComparison.Ordinal);
    }

    private static ClaimsPrincipal Principal(
        Guid userId,
        string role,
        bool includeIdentityPermission = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };
        if (includeIdentityPermission)
        {
            claims.Add(new Claim(
                RolePolicies.PermissionClaimType,
                RolePolicies.IdentityManagement));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private sealed record StudentResource(Guid StudentUserId) : IStudentOwnedResource;

    private sealed record TeachingResource(Guid AssignedUserId) : ITeachingAssignedResource
    {
        public bool IsAssignedTo(Guid staffUserId) => staffUserId == AssignedUserId;
    }
}
