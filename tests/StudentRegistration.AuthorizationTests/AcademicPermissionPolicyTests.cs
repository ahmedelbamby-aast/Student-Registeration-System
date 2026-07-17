using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.IdentityAccess.Application.Authorization;

namespace StudentRegistration.AuthorizationTests;

public sealed class AcademicPermissionPolicyTests
{
    private const string CatalogueReadAvailable = "Catalogue.ReadAvailable";
    private const string OfferingsManage = "Offerings.Manage";
    private const string OfferingDetailsRead = "OfferingDetailsRead";

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

        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, RolePolicies.CataloguePolicyManage),
            resource: null,
            RolePolicies.CataloguePolicyManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin),
            resource: null,
            RolePolicies.CataloguePolicyManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Lecturer, RolePolicies.CataloguePolicyManage),
            resource: null,
            RolePolicies.CataloguePolicyManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, RolePolicies.AcademicTermsManage),
            resource: null,
            RolePolicies.CataloguePolicyManage)).Succeeded);

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
    [InlineData(
        RolePolicies.Student,
        RolePolicies.ContextRead,
        RolePolicies.AcademicProfileReadOwn,
        RolePolicies.CatalogueReadAvailable,
        RolePolicies.RegistrationSubmitOwn,
        RolePolicies.RegistrationRecordsReadOwn)]
    [InlineData(
        RolePolicies.Admin,
        RolePolicies.IdentityAccessManage,
        RolePolicies.ContextRead,
        RolePolicies.AcademicTermsManage,
        RolePolicies.AcademicProfilesManage,
        RolePolicies.CataloguePolicyManage,
        RolePolicies.OfferingsManage,
        RolePolicies.RegistrationRecordsRead)]
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

    [Fact]
    public async Task Scheduling_policies_require_the_governed_role_and_permission_pairs()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityAuthorization();
        await using var provider = services.BuildServiceProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, OfferingsManage),
            resource: null,
            OfferingsManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin),
            resource: null,
            OfferingsManage)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Student, OfferingsManage),
            resource: null,
            OfferingsManage)).Succeeded);

        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Student, CatalogueReadAvailable),
            resource: null,
            OfferingDetailsRead)).Succeeded);
        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, OfferingsManage),
            resource: null,
            OfferingDetailsRead)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Student),
            resource: null,
            OfferingDetailsRead)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, CatalogueReadAvailable),
            resource: null,
            OfferingDetailsRead)).Succeeded);
    }

    [Fact]
    public void Scheduling_permissions_are_derived_only_for_the_governed_roles()
    {
        Assert.Contains(
            CatalogueReadAvailable,
            RolePolicies.PermissionsForRole(RolePolicies.Student));
        Assert.Contains(
            OfferingsManage,
            RolePolicies.PermissionsForRole(RolePolicies.Admin));
        Assert.DoesNotContain(
            OfferingsManage,
            RolePolicies.PermissionsForRole(RolePolicies.Student));
        Assert.DoesNotContain(
            CatalogueReadAvailable,
            RolePolicies.PermissionsForRole(RolePolicies.Admin));
        Assert.DoesNotContain(
            OfferingsManage,
            RolePolicies.PermissionsForRole(RolePolicies.Lecturer));
        Assert.DoesNotContain(
            OfferingsManage,
            RolePolicies.PermissionsForRole(RolePolicies.TeachingAssistant));
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
