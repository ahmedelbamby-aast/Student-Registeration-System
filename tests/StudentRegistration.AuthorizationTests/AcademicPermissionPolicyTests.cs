using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.IdentityAccess.Application.Authorization;

namespace StudentRegistration.AuthorizationTests;

public sealed class AcademicPermissionPolicyTests
{
    [Fact]
    public async Task Academic_policies_require_the_exact_permission_and_role_or_owner_scope()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityAuthorization();
        await using var provider = services.BuildServiceProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var studentUserId = Guid.NewGuid();

        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, RolePolicies.AcademicTermsManage),
            resource: null,
            RolePolicies.AcademicTermsManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin),
            resource: null,
            RolePolicies.AcademicTermsManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Lecturer, RolePolicies.AcademicTermsManage),
            resource: null,
            RolePolicies.AcademicTermsManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, RolePolicies.AcademicProfilesManage),
            resource: null,
            RolePolicies.AcademicTermsManage)).Succeeded);

        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, RolePolicies.AcademicProfilesManage),
            resource: null,
            RolePolicies.AcademicProfilesManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin),
            resource: null,
            RolePolicies.AcademicProfilesManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Lecturer, RolePolicies.AcademicProfilesManage),
            resource: null,
            RolePolicies.AcademicProfilesManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, RolePolicies.AcademicTermsManage),
            resource: null,
            RolePolicies.AcademicProfilesManage)).Succeeded);

        var ownedProfile = new StudentResource(studentUserId);
        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Student, RolePolicies.AcademicProfileReadOwn, studentUserId),
            ownedProfile,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Student, userId: studentUserId),
            ownedProfile,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Student, RolePolicies.AcademicProfileReadOwn),
            ownedProfile,
            RolePolicies.AcademicProfileReadOwn)).Succeeded);

        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.TeachingAssistant, RolePolicies.ContextRead),
            resource: null,
            RolePolicies.ContextRead)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.TeachingAssistant),
            resource: null,
            RolePolicies.ContextRead)).Succeeded);
    }

    [Theory]
    [InlineData(RolePolicies.Student, RolePolicies.ContextRead, RolePolicies.AcademicProfileReadOwn)]
    [InlineData(
        RolePolicies.Admin,
        RolePolicies.IdentityAccessManage,
        RolePolicies.ContextRead,
        RolePolicies.AcademicTermsManage,
        RolePolicies.AcademicProfilesManage)]
    [InlineData(RolePolicies.Lecturer, RolePolicies.ContextRead)]
    [InlineData(RolePolicies.TeachingAssistant, RolePolicies.ContextRead)]
    public void Permission_claims_are_derived_only_from_the_effective_role(
        string role,
        params string[] expectedPermissions)
    {
        Assert.Equal(
            expectedPermissions.Order(StringComparer.Ordinal),
            RolePolicies.PermissionsForRole(role).Order(StringComparer.Ordinal));
        Assert.Empty(RolePolicies.PermissionsForRole("Unknown"));
    }

    private static ClaimsPrincipal Principal(
        string role,
        string? permission = null,
        Guid? userId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()),
            new(ClaimTypes.Role, role)
        };
        if (permission is not null)
        {
            claims.Add(new Claim(RolePolicies.PermissionClaimType, permission));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private sealed record StudentResource(Guid StudentUserId) : IStudentOwnedResource;
}
