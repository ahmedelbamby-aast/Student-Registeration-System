using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using StudentRegistration.IdentityAccess.Application.Authorization;

namespace StudentRegistration.AuthorizationTests;

public sealed class RegistrationRecordScopeTests
{
    [Fact]
    public async Task Record_policies_require_exact_role_permission_pairs()
    {
        var services = new ServiceCollection().AddLogging().AddIdentityAuthorization();
        await using var provider = services.BuildServiceProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Student, RolePolicies.RegistrationRecordsReadOwn), null,
            RolePolicies.RegistrationRecordsReadOwn)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Student, RolePolicies.RegistrationRecordsRead), null,
            RolePolicies.RegistrationRecordsReadOwn)).Succeeded);
        Assert.True((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Admin, RolePolicies.RegistrationRecordsRead), null,
            RolePolicies.RegistrationRecordsRead)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(
            Principal(RolePolicies.Lecturer, RolePolicies.RegistrationRecordsRead), null,
            RolePolicies.RegistrationRecordsRead)).Succeeded);
    }

    private static System.Security.Claims.ClaimsPrincipal Principal(string role, string permission) =>
        new(new System.Security.Claims.ClaimsIdentity(
            [new(System.Security.Claims.ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
             new(System.Security.Claims.ClaimTypes.Role, role),
             new(RolePolicies.PermissionClaimType, permission)], "test"));
}
