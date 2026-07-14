using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.IdentityAccess.Application;

public sealed class IdentityCookieAuthenticationEvents(
    IIdentityAccountStore store) : CookieAuthenticationEvents
{
    public override Task RedirectToLogin(
        RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(
        RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var presentedStamp = context.Principal?.FindFirstValue(
            IdentityAuthenticationDefaults.SecurityStampClaim);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            string.IsNullOrWhiteSpace(presentedStamp))
        {
            await RejectAsync(context);
            return;
        }

        var user = await store.FindByIdAsync(userId, context.HttpContext.RequestAborted);
        if (user is null ||
            !user.IsEnabled ||
            !CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(user.SecurityStamp),
                System.Text.Encoding.UTF8.GetBytes(presentedStamp)))
        {
            await RejectAsync(context);
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(
            IdentityAuthenticationDefaults.AuthenticationScheme);
    }
}
