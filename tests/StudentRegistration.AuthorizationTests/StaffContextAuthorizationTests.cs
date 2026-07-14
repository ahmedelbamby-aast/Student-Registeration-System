using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.IdentityAccess.Application.Authorization;

namespace StudentRegistration.AuthorizationTests;

public sealed class StaffContextAuthorizationTests
{
    [Fact]
    public async Task Staff_context_uses_available_roles_without_authorizing_unselected_roles()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityAuthorization();
        await using var provider = services.BuildServiceProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        foreach (var role in new[]
                 {
                     RolePolicies.Admin,
                     RolePolicies.Lecturer,
                     RolePolicies.TeachingAssistant
                 })
        {
            var result = await authorization.AuthorizeAsync(
                Principal([role]),
                resource: null,
                RolePolicies.StaffContext);

            Assert.True(result.Succeeded, $"Expected {role} to select a staff context.");
        }

        Assert.False((await authorization.AuthorizeAsync(
            Principal([RolePolicies.Student]),
            resource: null,
            RolePolicies.StaffContext)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            new ClaimsPrincipal(new ClaimsIdentity()),
            resource: null,
            RolePolicies.StaffContext)).Succeeded);

        var availableAdminAndLecturer = Principal(
            [RolePolicies.Admin, RolePolicies.Lecturer]);
        Assert.True((await authorization.AuthorizeAsync(
            availableAdminAndLecturer,
            resource: null,
            RolePolicies.StaffContext)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            availableAdminAndLecturer,
            resource: null,
            RolePolicies.IdentityManagement)).Succeeded);

        var lecturerContext = Principal(
            [RolePolicies.Admin, RolePolicies.Lecturer],
            activeRole: RolePolicies.Lecturer,
            includeIdentityPermission: true);
        Assert.True((await authorization.AuthorizeAsync(
            lecturerContext,
            resource: null,
            RolePolicies.Lecturer)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            lecturerContext,
            resource: null,
            RolePolicies.Admin)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            lecturerContext,
            resource: null,
            RolePolicies.IdentityManagement)).Succeeded);

        var adminContext = Principal(
            [RolePolicies.Admin, RolePolicies.Lecturer],
            activeRole: RolePolicies.Admin,
            includeIdentityPermission: true);
        Assert.True((await authorization.AuthorizeAsync(
            adminContext,
            resource: null,
            RolePolicies.IdentityManagement)).Succeeded);
    }

    private static ClaimsPrincipal Principal(
        IReadOnlyList<string> availableRoles,
        string? activeRole = null,
        bool includeIdentityPermission = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        claims.AddRange(availableRoles.Select(role =>
            new Claim(RolePolicies.AvailableRoleClaimType, role)));
        if (activeRole is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, activeRole));
        }

        if (includeIdentityPermission)
        {
            claims.Add(new Claim(
                RolePolicies.PermissionClaimType,
                RolePolicies.IdentityManagement));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
